using System.Xml;
using System.Text.Json;

namespace books.Middleware;


public class XmlFormatterMiddleware : IMiddleware
{
    private readonly ILogger<XmlFormatterMiddleware> _logger;

    public XmlFormatterMiddleware(ILogger<XmlFormatterMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var originalBodyStream = context.Response.Body;
        
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await next(context);

            if (context.Response.StatusCode == 200 && 
                context.Request.Headers["Accept"].FirstOrDefault()?.Contains("application/xml") == true)
            {
                context.Response.ContentType = "application/xml";
                
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
                _logger.LogInformation("Original response content: {Content}", responseContent);

                var jsonDocument = JsonDocument.Parse(responseContent);

                responseBody.SetLength(0);

                using (var xmlWriter = XmlWriter.Create(responseBody, new XmlWriterSettings { Indent = true }))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("root");

                    WriteElement(xmlWriter, jsonDocument.RootElement);

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                }
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            context.Response.ContentLength = responseBody.Length;
            await responseBody.CopyToAsync(originalBodyStream);
        }
        
        context.Response.Body = originalBodyStream;
    }

    private void WriteElement(XmlWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    writer.WriteStartElement(property.Name);
                    WriteElement(writer, property.Value);
                    writer.WriteEndElement();
                }
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    writer.WriteStartElement("item");
                    WriteElement(writer, item);
                    writer.WriteEndElement();
                }
                break;

            case JsonValueKind.String:
                writer.WriteString(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteString(element.GetRawText());
                break;

            case JsonValueKind.True:
                writer.WriteString("true");
                break;

            case JsonValueKind.False:
                writer.WriteString("false");
                break;

            case JsonValueKind.Null:
                writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
                break;
        }
    }
}
