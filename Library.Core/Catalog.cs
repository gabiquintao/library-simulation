using Library.Core.Models;
using Library.Core.Storage;

namespace Library.Core
{
    public sealed class Catalog
    {
        private Book[] _books = new Book[16];
        private User[] _users = new User[16];
        private Category[] _categories = new Category[16];
        private Loan[] _loans = new Loan[16];
        private int[] _activeLoans = new int[16];
        private int _bookCount;
        private int _userCount;
        private int _categoryCount;
        private int _loanCount;
        private readonly TextStore _texts = new TextStore();

        public BookId AddBook(long isbn, string title, string author, ushort year, CategoryId categoryId)
        {
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(author);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(categoryId.Value, _categoryCount);

            if (_bookCount == _books.Length)
            {
                Array.Resize(ref _books, _books.Length * 2);
                Array.Resize(ref _activeLoans, _activeLoans.Length * 2);
            }

            var (titleOffset, titleLength) = _texts.Add(title);
            var (authorOffset, authorLength) = _texts.Add(author);
            var id = new BookId(_bookCount);

            var book = new Book
            {
                Id = id,
                Isbn = isbn,
                TitleOffset = titleOffset,
                TitleLength = titleLength,
                AuthorOffset = authorOffset,
                AuthorLength = authorLength,
                Year = year,
                CategoryId = categoryId
            };

            _books[_bookCount] = book;
            _bookCount++;
            return id;
        }

        private Book GetBook(BookId id)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id.Value);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(id.Value, _bookCount);
            return _books[id.Value];
        }

        public string GetTitle(BookId id)
        {
            var book = GetBook(id);
            var title = _texts.Get(book.TitleOffset, book.TitleLength);
            return title;
        }

        public string GetAuthor(BookId id)
        {
            var book = GetBook(id);
            var author = _texts.Get(book.AuthorOffset, book.AuthorLength);
            return author;
        }

        public UserId AddUser(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (_userCount == _users.Length)
                Array.Resize(ref _users, _users.Length * 2);

            var (nameOffset, nameLength) = _texts.Add(name);
            var id = new UserId(_userCount);

            var user = new User
            {
                Id = id,
                NameOffset = nameOffset,
                NameLength = nameLength
            };

            _users[_userCount] = user;
            _userCount++;
            return id;
        }

        private User GetUser(UserId id)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id.Value);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(id.Value, _userCount);
            return _users[id.Value];
        }

        public string GetUserName(UserId id)
        {
            var user = GetUser(id);
            var name = _texts.Get(user.NameOffset, user.NameLength);
            return name;
        }

        public CategoryId AddCategory(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(_categoryCount, ushort.MaxValue);

            if (_categoryCount == _categories.Length)
                Array.Resize(ref _categories, _categories.Length * 2);

            var (nameOffset, nameLength) = _texts.Add(name);
            var id = new CategoryId((ushort)_categoryCount);    

            var category = new Category
            {
                Id = id,
                NameOffset = nameOffset,
                NameLength = nameLength
            };

            _categories[_categoryCount] = category;
            _categoryCount++;
            return id;
        }

        private Category GetCategory(CategoryId id)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(id.Value, _categoryCount);
            return _categories[id.Value];
        }

        public string GetCategoryName(CategoryId id)
        {
            var category = GetCategory(id);
            var name = _texts.Get(category.NameOffset, category.NameLength);
            return name;
        }

        private int AddLoan(long loanedAt, long dueAt, BookId bookId, UserId userId)
        {
            if (_loanCount == _loans.Length)
                Array.Resize(ref _loans, _loans.Length * 2);

            var loan = new Loan
            {
                LoanedAt = loanedAt,
                DueAt = dueAt,
                ReturnedAt = 0,
                BookId = bookId,
                UserId = userId,
                State = LoanState.Active
            };

            _loans[_loanCount] = loan;
            int index = _loanCount;
            _loanCount++;
            return index;
        }

        public bool TryLoan(BookId bookId, UserId userId, long now, out LoanError error)
        {
            if (bookId.Value < 0 || bookId.Value >= _bookCount)
            {
                error = LoanError.BookNotFound;
                return false;
            }

            if (userId.Value < 0 || userId.Value >= _userCount)
            {
                error = LoanError.UserNotFound;
                return false;
            }

            if (_activeLoans[bookId.Value] != 0)
            {
                error = LoanError.BookUnavailable;
                return false;
            }

            long dueDate = now + 30 * 24 * 60 * 60;
            int index = AddLoan(now, dueDate, bookId, userId);
            _activeLoans[bookId.Value] = index + 1;
            error = LoanError.None;
            return true;
        }

        public bool TryReturn(BookId bookId, UserId userId, long now, out LoanError error)
        {
            if (bookId.Value < 0 || bookId.Value >= _bookCount)
            {
                error = LoanError.BookNotFound;
                return false;
            }

            if (userId.Value < 0 || userId.Value >= _userCount)
            {
                error = LoanError.UserNotFound;
                return false;
            }

            if (_activeLoans[bookId.Value] == 0)
            {
                error = LoanError.BookNotLoaned;
                return false;
            }

            int loanIndex = _activeLoans[bookId.Value] - 1;

            if (_loans[loanIndex].UserId != userId)
            {
                error = LoanError.UserMismatch;
                return false;
            }

            _loans[loanIndex] = _loans[loanIndex] with
            {
                ReturnedAt = now,
                State = LoanState.Returned
            };
            _activeLoans[bookId.Value] = 0;

            error = LoanError.None; 
            return true;
        }
    }
}
