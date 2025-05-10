namespace BudgetAppBackend.Application.DTOs.PagingsDTOs
{
    public class PagingDTO
    {
        private int rowCount1 = 4;
        public int rowCount { get => rowCount1; set => rowCount1 = Math.Min(20, value); }
        public int pageNumber { get; set; } = 1;
    }
}
