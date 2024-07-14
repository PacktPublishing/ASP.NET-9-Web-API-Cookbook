using System.Text.Json;
using books.Models;

namespace books.Middleware;

public class CsvFormatterMiddleware : IResponseFormatterMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var originalBodyStream = context.Response.Body;
        
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await next(context);

            if (context.Response.StatusCode == 200 && context.Response.ContentType.StartsWith("application/json"))
            {
                context.Response.ContentType = "text/csv";
                
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
                var books = JsonSerializer.Deserialize<List<BookDTO>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                responseBody.SetLength(0);

                using (var writer = new StreamWriter(responseBody, leaveOpen: true))
                {
                    await writer.WriteLineAsync("Id,Title,Author,PublicationDate,ISBN,Genre,Summary");

                    foreach (var book in books)
                    {
                        var csvLine = $"{book.Id},{EscapeCsvField(book.Title)},{EscapeCsvField(book.Author)},{book.PublicationDate:yyyy-MM-dd},{book.ISBN},{EscapeCsvField(book.Genre)},{EscapeCsvField(book.Summary)}";
                        await writer.WriteLineAsync(csvLine);
                    }
                }
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            context.Response.ContentLength = responseBody.Length;
            await responseBody.CopyToAsync(originalBodyStream);
        }
        
        context.Response.Body = originalBodyStream;
    }
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    public string GetContentType() => "text/csv";
}
