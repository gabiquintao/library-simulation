namespace Library.Core.Models
{
    public enum LoanState : byte { Active, Returned }

    public readonly record struct Loan
    {
        public long LoanedAt { get; init; }
        public long DueAt { get; init; }
        public long ReturnedAt { get; init; }
        public BookId BookId { get; init; }
        public UserId UserId { get; init; }
        public LoanState State { get; init; }

    }
}
