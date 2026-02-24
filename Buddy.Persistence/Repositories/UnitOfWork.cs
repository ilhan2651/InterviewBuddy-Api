using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using Buddy.Persistence.Context;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private Hashtable? _repositories;

        private IInterviewSessionRepository? _interviewSessions;
        private IInterviewQuestionRepository? _interviewQuestions;
        private IInterviewAnswerRepository? _interviewAnswers;
        private IQuizRepository? _quizzes;
        private IQuizQuestionRepository? _quizQuestions;
        private IQuizAnswerRepository? _quizAnswers;
        private IUserRepository? _users;
        private IConversationRepository? _conversations;
        private IMessageRepository? _messages;
        private IUserApiKeyRepository? _userApiKeys;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IInterviewSessionRepository InterviewSessions => _interviewSessions ??= new InterviewSessionRepository(_context);
        public IInterviewQuestionRepository InterviewQuestions => _interviewQuestions ??= new InterviewQuestionRepository(_context);
        public IInterviewAnswerRepository InterviewAnswers => _interviewAnswers ??= new InterviewAnswerRepository(_context);
        public IQuizRepository Quizzes => _quizzes ??= new QuizRepository(_context);
        public IQuizQuestionRepository QuizQuestions => _quizQuestions ??= new QuizQuestionRepository(_context);
        public IQuizAnswerRepository QuizAnswers => _quizAnswers ??= new QuizAnswerRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IConversationRepository Conversations => _conversations ??= new ConversationRepository(_context);
        public IMessageRepository Messages => _messages ??= new MessageRepository(_context);
        public IUserApiKeyRepository UserApiKeys => _userApiKeys ??= new UserApiKeyRepository(_context);

        public IGenericRepository<T> GetRepository<T>() where T : BaseEntity
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IGenericRepository<T>)_repositories[type]!;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
