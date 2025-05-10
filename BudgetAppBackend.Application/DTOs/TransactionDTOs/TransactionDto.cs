namespace BudgetAppBackend.Application.DTOs.TransactionDTOs
{
    public class TransactionDto
    {
        public TransactionDto(Guid id, DateTime date, decimal amount, string payee, IReadOnlyList<string> categories)
        {
            TransactionId = id;
            TransactionDate = date;
            Amount = amount;
            Payee = payee;
            Categories = new List<string>(categories);
        }

        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public List<string> Categories { get; set; } = new();
        public string Payee { get; set; } = string.Empty;
    }
}
