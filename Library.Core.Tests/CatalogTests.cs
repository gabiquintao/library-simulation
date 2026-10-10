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

    [Fact]
    public void TryLoan_UnknownUserAndUnavailableBook_ReturnsUserNotFound()
    {
        _catalog.TryLoan(_book0, _ana, T0, out _);

        bool ok = _catalog.TryLoan(_book0, new UserId(99), T0, out LoanError error);

        Assert.False(ok);
        Assert.Equal(LoanError.UserNotFound, error);
    }

    [Fact]
    public void AddBook_BeyondInitialCapacity_KeepsBooksAndActiveLoans()
    {
        _catalog.TryLoan(_book0, _ana, T0, out _);

        BookId last = default;
        for (int i = 0; i < 40; i++)
            last = _catalog.AddBook(9780000000000 + i, $"Book {i}", "Author", 2000, new CategoryId(0));

        Assert.Equal("Book 39", _catalog.GetTitle(last));
        Assert.Equal("Dom Casmurro", _catalog.GetTitle(_book0));

        bool ok = _catalog.TryLoan(_book0, _rui, T0, out LoanError error);
        Assert.False(ok);
        Assert.Equal(LoanError.BookUnavailable, error);

        Assert.True(_catalog.TryLoan(last, _ana, T0, out _));
        Assert.True(_catalog.TryReturn(last, _ana, T0 + Day, out _));
    }

    [Fact]
    public void AddUser_BeyondInitialCapacity_KeepsAllUsers()
    {
        UserId last = default;
        for (int i = 0; i < 40; i++)
            last = _catalog.AddUser($"User {i}");

        Assert.Equal("User 39", _catalog.GetUserName(last));
        Assert.Equal("Ana", _catalog.GetUserName(_ana));
    }

    [Fact]
    public void AddCategory_BeyondInitialCapacity_KeepsAllCategories()
    {
        CategoryId last = default;
        for (int i = 0; i < 40; i++)
            last = _catalog.AddCategory($"Category {i}");

        Assert.Equal("Category 39", _catalog.GetCategoryName(last));
        Assert.Equal("Ficção", _catalog.GetCategoryName(new CategoryId(0)));
    }

    [Fact]
    public void LoanCycles_BeyondInitialCapacity_KeepWorking()
    {
        for (int i = 0; i < 40; i++)
        {
            long now = T0 + i * 2 * Day;

            Assert.True(_catalog.TryLoan(_book0, _ana, now, out _));
            Assert.True(_catalog.TryReturn(_book0, _ana, now + Day, out _));
        }
    }

    [Fact]
    public void AddBook_NullTitle_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => _catalog.AddBook(1, null!, "Author", 2000, new CategoryId(0)));
    }

    [Fact]
    public void AddBook_NullAuthor_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => _catalog.AddBook(1, "Title", null!, 2000, new CategoryId(0)));
    }

    [Fact]
    public void AddBook_NonExistentCategory_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _catalog.AddBook(1, "Title", "Author", 2000, new CategoryId(99)));
    }

    [Fact]
    public void AddUser_NullName_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _catalog.AddUser(null!));
    }

    [Fact]
    public void AddCategory_NullName_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _catalog.AddCategory(null!));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void GetTitle_InvalidId_Throws(int id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _catalog.GetTitle(new BookId(id)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void GetUserName_InvalidId_Throws(int id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _catalog.GetUserName(new UserId(id)));
    }

    [Fact]
    public void GetCategoryName_InvalidId_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _catalog.GetCategoryName(new CategoryId(99)));
    }
}