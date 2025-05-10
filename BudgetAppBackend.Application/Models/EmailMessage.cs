namespace BudgetAppBackend.Application.Models
{
    public class EmailMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ToName { get; set; }

        public EmailMessage(string to, string subject, string body, string toName)
        {
            To = to;
            Subject = subject;
            Body = body;
            ToName = toName;
        }
    }
}
