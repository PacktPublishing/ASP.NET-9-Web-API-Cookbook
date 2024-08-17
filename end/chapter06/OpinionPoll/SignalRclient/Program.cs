using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

string url = string.Empty;

while (String.IsNullOrWhiteSpace(url)) {

    Console.WriteLine("Please enter the URL of your SignalR hub and press enter");
    Console.WriteLine("Please use this format http://myUrl.com/myHub");

    string input = Console.ReadLine()!;
    if (!String.IsNullOrEmpty(input)) {
        url = input.Trim();
    }
}

var hubConnection = new HubConnectionBuilder()
                         .WithUrl(url)
                         .Build();

hubConnection.On<string>("ReceiveMessage",
    message => Console.WriteLine($"SignalR Hub Message: {message}"));

hubConnection.On<object>("ReceiveVoteResults", results => {
    var jsonElement = (JsonElement)results;
    Console.WriteLine("Current Results:");

    foreach (var result in jsonElement.EnumerateArray())
    {
        int choice = result.GetProperty("choice").GetInt32();
        int count = result.GetProperty("count").GetInt32();

        switch (choice)
        {
            case 1:
                Console.WriteLine($"Super Nintendo: {count} votes");
                break;
            case 2:
                Console.WriteLine($"Sega Genesis: {count} votes");
                break;
            default:
                Console.WriteLine($"Unknown choice {choice}: {count} votes");
                break;
        }
    }
});

try
{
    await hubConnection.StartAsync();
    Console.WriteLine("Connected to the SignalR hub.");

    while (true)
    {
        Console.WriteLine("Please specify the action:");
        Console.WriteLine("1 - Cast vote for Super Nintendo");
        Console.WriteLine("2 - Cast vote for Sega Genesis");
        Console.WriteLine("r - Get current results");
        Console.WriteLine("q - Exit the program");

        var action = Console.ReadLine();

        if (action == "exit")
            break;

        if (action == "r")
        {
            await hubConnection.InvokeAsync("GetCurrentResults");
        }
        else if (action == "1" || action == "2")
        {
            await hubConnection.InvokeAsync("Vote", int.Parse(action));
        }
        else
        {
            Console.WriteLine("Invalid action. Please try again.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}


