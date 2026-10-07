using Library.Core;
using Library.Core.Models;

namespace Library.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var catalog = new Catalog();

            BookId last = default;
            for (int i = 0; i < 20; i++)
                last = catalog.AddBook(9788574801407 + i, $"Book {i}", "Author", 2000, new CategoryId(0));

            Console.WriteLine(last.Value);
            Console.WriteLine(catalog.GetTitle(last));

            var user = catalog.AddUser("User");
            Console.WriteLine(catalog.GetName(user));
        }
    }
}