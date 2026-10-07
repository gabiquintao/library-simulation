using Library.Core.Models;
using Library.Core.Storage;

namespace Library.Core
{
    public sealed class Catalog
    {
        private Book[] _books = new Book[16];
        private User[] _users = new User[16];
        private int _bookCount;
        private int _userCount;
        private readonly TextStore _texts = new TextStore();

        public BookId AddBook(long isbn, string title, string author, ushort year, CategoryId categoryId)
        {
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

            if (_bookCount == _books.Length)
                Array.Resize(ref _books, _books.Length * 2);

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
            var (nameOffset, nameLength) = _texts.Add(name);
            var id = new UserId(_userCount);

            var user = new User
            {
                Id = id,
                NameOffset = nameOffset,
                NameLength = nameLength
            };

            if (_userCount == _users.Length)
                Array.Resize(ref _users, _users.Length * 2);

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

        public string GetName(UserId id)
        {
            var user = GetUser(id);
            var name = _texts.Get(user.NameOffset, user.NameLength);
            return name;
        }
    }
}
