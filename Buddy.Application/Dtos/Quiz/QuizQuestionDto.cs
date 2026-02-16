using System.Collections.Generic;

namespace Buddy.Application.Dtos.Quiz
{
    public class QuizQuestionDto
    {
        public int Number { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> ExpectedKeywords { get; set; } = new List<string>();
    }
}
