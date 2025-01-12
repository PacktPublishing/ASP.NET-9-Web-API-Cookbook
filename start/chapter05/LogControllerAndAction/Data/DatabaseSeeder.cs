using Bogus;
using Books.Models;
using Microsoft.EntityFrameworkCore;

namespace Books.Data;

public static class DatabaseSeeder
{
    public static void Initialize(AppDbContext context)
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

            var books = faker.Generate(1000); 

            context.Books.AddRange(books);
            context.SaveChanges();
    }
}
