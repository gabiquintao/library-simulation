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
            {
                if (!catalog.TryAddBook(9788574801407 + i, $"Book {i}", "Author", 2000, fiction,
                                        out last, out AddBookError error))
                    Console.WriteLine($"Could not add book {i}: {error}");
            }

            Console.WriteLine(last.Value);
            Console.WriteLine(catalog.GetTitle(last));
            Console.WriteLine(catalog.GetCategoryName(fiction));

            Console.WriteLine();

            long known = 9788574801407 + 5;
            if (catalog.TryGetBookIdByIsbn(known, out BookId found))
                Console.WriteLine($"ISBN {known} -> {catalog.GetTitle(found)}");
            else
                Console.WriteLine($"ISBN {known} not found");

            long unknown = 1234567890123;
            if (catalog.TryGetBookIdByIsbn(unknown, out found))
                Console.WriteLine($"ISBN {unknown} -> {catalog.GetTitle(found)}");
            else
                Console.WriteLine($"ISBN {unknown} not found");

            bool added = catalog.TryAddBook(known, "Duplicate", "Author", 2000, fiction, out _, out AddBookError duplicateError);
            Console.WriteLine($"Duplicate ISBN: added = {added}, error = {duplicateError}");
        }
    }
}