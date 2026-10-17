using System;

namespace WinFormsAppbai2.Models
{
    public class LoanRecord
    {
        public string TicketId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Device { get; set; }
        public int Quantity { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string Status
        {
            get
            {
                if (ReturnDate.HasValue)
                    return "Đã trả";
                if (DueDate.Date < DateTime.Today)
                    return "Quá hạn";
                return "Đang mượn";
            }
        }

        // For CSV export convenience
        public string ReturnDateDisplay => ReturnDate?.ToString("yyyy-MM-dd") ?? string.Empty;
    }
}
