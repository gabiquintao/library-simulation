using Library.Core;
using Library.Core.Models;

namespace Library.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var catalog = new Catalog();
            var fiction = catalog.AddCategory("Fiction");

            BookId last = default;
            for (int i = 0; i < 20; i++)
                last = catalog.AddBook(9788574801407 + i, $"Book {i}", "Author", 2000, fiction);

            Console.WriteLine(last.Value);
            Console.WriteLine(catalog.GetTitle(last));
            Console.WriteLine(catalog.GetCategoryName(fiction));
        }
    }
}