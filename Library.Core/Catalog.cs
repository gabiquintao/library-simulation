using Library.Core.Models;
using Library.Core.Storage;

namespace Library.Core
{
    public sealed class Catalog
    {
        private Book[] _books = new Book[16];
        private User[] _users = new User[16];
        private Category[] _categories = new Category[16];
        private int _bookCount;
        private int _userCount;
        private int _categoryCount;
        private readonly TextStore _texts = new TextStore();

        public BookId AddBook(long isbn, string title, string author, ushort year, CategoryId categoryId)
        {
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(author);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(categoryId.Value, _categoryCount);

            if (_bookCount == _books.Length)
                Array.Resize(ref _books, _books.Length * 2);

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
    }
}
