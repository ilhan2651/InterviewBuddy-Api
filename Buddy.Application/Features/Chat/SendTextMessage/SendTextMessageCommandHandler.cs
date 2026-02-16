using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Chat.SendTextMessage
{
    public class SendTextMessageCommandHandler : IRequestHandler<SendTextMessageCommand, SendTextMessageResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILLMService _openAIService;
        private readonly ICurrentUserService _currentUserService;

        public SendTextMessageCommandHandler(IUnitOfWork unitOfWork, ILLMService openAIService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _openAIService = openAIService;
            _currentUserService = currentUserService;
        }

        public async Task<SendTextMessageResponse> Handle(SendTextMessageCommand request, CancellationToken cancellationToken)
        {
            // 1. Find or create conversation
            var conversation = await _unitOfWork.Conversations.GetBySessionIdWithMessagesAsync(request.SessionId, cancellationToken);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    UserId = _currentUserService.UserId,
                    AnonymousId = request.AnonymousId,
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                    StartedAt = DateTime.UtcNow
                };
                await _unitOfWork.Conversations.AddAsync(conversation);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 2. Save User Message
            var userMsg = new Message
            {
                ConversationId = conversation.Id,
                Type = MessageType.User,
                TextContent = request.Message,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Messages.AddAsync(userMsg);

            // 3. Get History for GPT
            var history = conversation.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            // 4. Generate AI Response
            var aiTextResponse = await _openAIService.GenerateChatResponseAsync(request.Message, history);

            // 5. Generate TTS
            var audioStream = await _openAIService.TextToSpeechAsync(aiTextResponse);

            // 6. Save AI Message
            var aiMsg = new Message
            {
                ConversationId = conversation.Id,
                Type = MessageType.AI,
                TextContent = aiTextResponse,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Messages.AddAsync(aiMsg);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Save Audio File
            var audioFileName = $"{aiMsg.Id}.mp3";
            var relativePath = Path.Combine("audio", "ai", audioFileName);
            // Note: In a real app, you'd use IWebHostEnvironment or a Storage Service. 
            // For this task, we follow the instruction to save to wwwroot/audio/ai/.
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Buddy.Api", "wwwroot", "audio", "ai", audioFileName);

            var directoryPath = Path.GetDirectoryName(absolutePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath!);
            }

            using (var fileStream = File.Create(absolutePath))
            {
                await audioStream.CopyToAsync(fileStream, cancellationToken);
            }

            aiMsg.AudioPath = relativePath.Replace("\\", "/");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SendTextMessageResponse
            {
                SessionId = conversation.SessionId,
                UserMessage = request.Message,
                AIResponse = aiTextResponse,
                AIAudioUrl = aiMsg.AudioPath
            };
        }
    }
}
