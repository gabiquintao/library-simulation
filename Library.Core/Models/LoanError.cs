namespace Library.Core.Models
{
    public enum LoanError : byte
    {
        None,
        BookNotFound,
        UserNotFound,
        BookUnavailable,
        BookNotLoaned,
        UserMismatch
    }
}
