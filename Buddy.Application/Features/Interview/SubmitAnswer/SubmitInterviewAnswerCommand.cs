using MediatR;
using Buddy.Application.Features.Interview.StartInterview; // For InterviewQuestionDto

namespace Buddy.Application.Features.Interview.SubmitAnswer
{
    public class SubmitInterviewAnswerCommand : IRequest<SubmitInterviewAnswerResponse>
    {
        public int InterviewSessionId { get; set; }
        public int QuestionId { get; set; }
        public string? AnswerText { get; set; }
        public string? AudioPath { get; set; } // If from audio file
        // Note: Logic for handling audio stream upload will be similar to Chat features, 
        // likely handled in Controller then passed here as text or path, or Stream handled here.
        // For simplicity, assuming text or pre-uploaded path for now, or we can add Stream property.
    }

    public class SubmitInterviewAnswerResponse
    {
        public bool IsCompleted { get; set; }
        public string? Feedback { get; set; } // Immediate feedback on the submitted answer
        public InterviewQuestionDto? NextQuestion { get; set; }
        public string? CompletionMessage { get; set; }
        
        public bool RetryRequired { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
