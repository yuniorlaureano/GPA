using GPA.Common.DTOs.Inventory;

namespace GPA.Dtos.Inventory
{
    public class TransactionsFilterDto
    {
        public string Term { get; set; } = string.Empty;
        public DetailedDate? From { get; set; }
        public DetailedDate? To { get; set; }
        public int Status { get; set; } = -1;
        public int TransactionType { get; set; } = -1;
        public int Reason { get; set; } = -1;
    }
}
