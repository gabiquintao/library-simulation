using Library.Core.Models;

namespace Library.Core.Tests;

public class CatalogTests
{
    private const long Day = 24 * 60 * 60;
    private const long T0 = 1_791_504_000;

    private readonly Catalog _catalog = new();
    private readonly BookId _book0;
    private readonly BookId _book1;
    private readonly UserId _ana;
    private readonly UserId _rui;

    public CatalogTests()
    {
        var fiction = _catalog.AddCategory("Ficção");
        _book0 = _catalog.AddBook(9788574801407, "Dom Casmurro", "Machado de Assis", 1899, fiction);
        _book1 = _catalog.AddBook(9789720049582, "Os Maias", "Eça de Queirós", 1888, fiction);
        _ana = _catalog.AddUser("Ana");
        _rui = _catalog.AddUser("Rui");
    }

    [Fact]
    public void TryLoan_AvailableBook_Succeeds()
    {
        bool ok = _catalog.TryLoan(_book0, _ana, T0, out LoanError error);

        Assert.True(ok);
        Assert.Equal(LoanError.None, error);
    }

    [Fact]
    public void TryLoan_AlreadyLoanedBook_ReturnsBookUnavailable()
    {
        _catalog.TryLoan(_book0, _ana, T0, out _);

        bool ok = _catalog.TryLoan(_book0, _rui, T0, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.BookUnavailable, error);
    }

    [Fact]
    public void TryLoan_NonExistentBook_ReturnsBookNotFound()
    {
        bool ok = _catalog.TryLoan(new BookId(99), _ana, T0, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.BookNotFound, error);
    }

    [Fact]
    public void TryLoan_NonExistentUser_ReturnsUserNotFound()
    {
        bool ok = _catalog.TryLoan(_book1, new UserId(99), T0, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.UserNotFound, error);
    }

    [Fact]
    public void TryReturn_BookNeverLoaned_ReturnsBookNotLoaned()
    {
        bool ok = _catalog.TryReturn(_book1, _ana, T0 + Day, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.BookNotLoaned, error);
    }

    [Fact]
    public void TryReturn_BorrowerMismatch_ReturnsUserMismatch()
    {
        _catalog.TryLoan(_book0, _ana, T0, out _);

        bool ok = _catalog.TryReturn(_book0, _rui, T0 + Day, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.UserMismatch, error);
    }

    [Fact]
    public void TryReturn_NonExistentBook_ReturnsBookNotFound()
    {
        bool ok = _catalog.TryReturn(new BookId(99), _ana, T0 + Day, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.BookNotFound, error);
    }

    [Fact]
    public void TryReturn_NonExistentUser_ReturnsUserNotFound()
    {
        bool ok = _catalog.TryReturn(_book0, new UserId(99), T0 + Day, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.UserNotFound, error);
    }

    [Fact]
    public void TryReturn_OnTime_Succeeds()
    {
        _catalog.TryLoan(_book0, _ana, T0, out _);

        bool ok = _catalog.TryReturn(_book0, _ana, T0 + 10 * Day, out LoanError error);

        Assert.True(ok);
        Assert.Equal(LoanError.None, error);
    }

    [Fact]
    public void TryReturn_AlreadyReturnedBook_ReturnsBookNotLoaned()
    {
        _catalog.TryLoan(_book0, _ana, T0, out _);
        _catalog.TryReturn(_book0, _ana, T0 + 10 * Day, out _);

        bool ok = _catalog.TryReturn(_book0, _ana, T0 + 11 * Day, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.BookNotLoaned, error);
    }

    [Fact]
    public void TryLoan_AfterBookReturned_Succeeds()
    {
        _catalog.TryLoan(_book0, _ana, T0, out _);
        _catalog.TryReturn(_book0, _ana, T0 + 10 * Day, out _);

        bool ok = _catalog.TryLoan(_book0, _rui, T0 + 12 * Day, out LoanError error);

        Assert.True(ok);
        Assert.Equal(LoanError.None, error);
    }

    [Fact]
    public void TryReturn_Overdue_Succeeds()
    {
        _catalog.TryLoan(_book0, _rui, T0, out _);

        bool ok = _catalog.TryReturn(_book0, _rui, T0 + 50 * Day, out LoanError error);

        Assert.True(ok);
        Assert.Equal(LoanError.None, error);
    }
}