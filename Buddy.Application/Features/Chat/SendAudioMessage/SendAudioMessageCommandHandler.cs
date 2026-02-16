using Buddy.Application.Common.Interfaces;
using Buddy.Application.Features.Chat.SendTextMessage;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Chat.SendAudioMessage
{
    public class SendAudioMessageCommandHandler : IRequestHandler<SendAudioMessageCommand, SendTextMessageResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILLMService _openAIService;
        private readonly IUnitOfWork _unitOfWork;

        public SendAudioMessageCommandHandler(IMediator mediator, ILLMService openAIService, IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _openAIService = openAIService;
            _unitOfWork = unitOfWork;
        }

        public async Task<SendTextMessageResponse> Handle(SendAudioMessageCommand request, CancellationToken cancellationToken)
        {
            // 1. Save User Audio File
            var uniqueFileName = $"{Guid.NewGuid()}_{request.FileName}";
            var relativePath = Path.Combine("audio", "user", uniqueFileName);
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Buddy.Api", "wwwroot", "audio", "user", uniqueFileName);

            var directoryPath = Path.GetDirectoryName(absolutePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath!);
            }

            // Using MemoryStream to allow seeking for Whisper API if needed, 
            // though OpenAIService implementation currently handles the stream directly.
            using (var fileStream = File.Create(absolutePath))
            {
                await request.AudioStream.CopyToAsync(fileStream, cancellationToken);
            }

            // 2. Transcribe Audio using Whisper
            // We need a fresh stream since the previous one was consumed by File.Create
            using var fileStreamForTranscription = File.OpenRead(absolutePath);
            var transcribedText = await _openAIService.TranscribeAudioAsync(fileStreamForTranscription);

            if (string.IsNullOrWhiteSpace(transcribedText))
            {
                transcribedText = "[Ses anlaşılamadı]";
            }

            // 3. Call SendTextMessageCommand
            var textCommand = new SendTextMessageCommand
            {
                AnonymousId = request.AnonymousId,
                SessionId = request.SessionId,
                Message = transcribedText
            };

            var response = await _mediator.Send(textCommand, cancellationToken);

            // 4. Update the User Message with AudioPath
            // We find the last user message in this session to attach the audio path
            var lastUserMessage = await _unitOfWork.Messages.GetLastUserMessageInSessionAsync(response.SessionId, cancellationToken);

            if (lastUserMessage != null)
            {
                lastUserMessage.AudioPath = relativePath.Replace("\\", "/");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return response;
        }
    }
}
