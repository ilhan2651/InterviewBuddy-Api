using Buddy.Application.Dtos.Interview;
using Buddy.Application.Dtos.Quiz;
using Buddy.Domain.Entities;
using Buddy.Domain.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Buddy.Application.Services
{
    /// <summary>
    /// Monolithic interface maintained for backward compatibility. 
    /// New features should use IInterviewLLMService or IQuizLLMService.
    /// </summary>
    public interface ILLMService : IInterviewLLMService, IQuizLLMService
    {
    }
}

