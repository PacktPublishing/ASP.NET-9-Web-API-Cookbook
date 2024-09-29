using Bogus;
using books.Models;
using Microsoft.EntityFrameworkCore;

namespace books.Data;

public static class DatabaseSeeder
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new AppDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
        {
            if (context.Books.Any())
            {
                return; 
            }

            var faker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3, 3))
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.PublicationDate, f => f.Date.Past(100))
                .RuleFor(b => b.ISBN, f => f.Random.Replace("###-#-##-####"))
                .RuleFor(b => b.Genre, f => f.PickRandom(new[] { "Fiction", "Non-fiction", "Science Fiction", "Mystery", "Romance", "Thriller" }))
                .RuleFor(b => b.Summary, f => f.Lorem.Paragraph(3));

            var books = faker.Generate(1000);  // Generate 100 fake books

            context.Books.AddRange(books);
            context.SaveChanges();
        }
    }
}
