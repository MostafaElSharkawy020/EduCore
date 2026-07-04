namespace EduCore.ViewModels
{
    // ── Taking a quiz/exam ──
    public class TakeAssessmentViewModel
    {
        public string Title { get; set; }
        public bool IsExam { get; set; }
        public int AssessmentId { get; set; }   // quiz id or exam id
        public int BackId { get; set; }          // class id (quiz) or course id (exam)
        public int DurationMinutes { get; set; } // 0 = no time limit
        public List<TakeQuestion> Questions { get; set; } = new();
    }

    public class TakeQuestion
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public List<TakeChoice> Choices { get; set; } = new();
    }

    public class TakeChoice
    {
        public int ChoiceId { get; set; }
        public string Text { get; set; }
    }

    // ── Result / feedback ──
    public class AssessmentResultViewModel
    {
        public string Title { get; set; }
        public bool IsExam { get; set; }
        public int AssessmentId { get; set; }   // for "retake"
        public int BackId { get; set; }          // class id (quiz) or course id (exam)
        public int Score { get; set; }
        public int Total { get; set; }
        public List<ResultItem> Items { get; set; } = new();
    }

    public class ResultItem
    {
        public string QuestionText { get; set; }
        public bool IsCorrect { get; set; }
        public List<ResultChoice> Choices { get; set; } = new();
    }

    public class ResultChoice
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsSelected { get; set; }
    }
}
