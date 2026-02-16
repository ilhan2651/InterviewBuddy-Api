using Buddy.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IInterviewSessionRepository InterviewSessions { get; }
        IInterviewQuestionRepository InterviewQuestions { get; }
        IInterviewAnswerRepository InterviewAnswers { get; }
        IQuizRepository Quizzes { get; }
        IQuizQuestionRepository QuizQuestions { get; }
        IQuizAnswerRepository QuizAnswers { get; }
        IUserRepository Users { get; }
        IConversationRepository Conversations { get; }
        IMessageRepository Messages { get; }

        IGenericRepository<T> GetRepository<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
