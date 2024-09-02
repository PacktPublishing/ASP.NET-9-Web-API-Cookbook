using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

class BasicSignalRClient
{
    private static string _bearerToken = string.Empty;
    private static HubConnection? _connection;
    private static readonly HttpClient _httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Basic SignalR Messaging Client");

        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Register");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Send Message");
            Console.WriteLine("4. Exit");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await Register();
                    break;
                case "2":
                    await Login();
                    break;
                case "3":
                    await SendMessage();
                    break;
                case "4":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again.");
                    break;
            }
        }
    }

    static async Task Register()
    {
        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        var response = await _httpClient.PostAsJsonAsync("https://localhost:5001/api/auth/register", new { username, password });
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Registration successful!");
        }
        else
        {
            Console.WriteLine($"Registration failed: {await response.Content.ReadAsStringAsync()}");
        }
    }

    static async Task Login()
    {
        Console.Write("Enter username: ");
        var username = Console.ReadLine();
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        var response = await _httpClient.PostAsJsonAsync("https://localhost:5001/api/auth/login", new { username, password });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(result);
             if (json.TryGetProperty("token", out var tokenProperty))
            {
                _bearerToken = tokenProperty.GetString() ?? string.Empty;
                Console.WriteLine("Login successful!");
            }
            else
            {
                Console.WriteLine("Login failed: Token not found in response.");
            }

            try {
                await ConnectToHub();
            } catch (Exception ex) {
                Console.WriteLine($"Could not connect to hub. Exception: {ex}.");
            }
        }
        else
        {
            Console.WriteLine($"Login failed: {await response.Content.ReadAsStringAsync()}");
        }
    }

    static async Task ConnectToHub()
    {
        if (string.IsNullOrEmpty(_bearerToken))
        {
            Console.WriteLine("Please login first.");
            return;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/messagingHub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_bearerToken)!;
            })
            .Build();

        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Console.WriteLine($"{user}: {message}");
        });

        try
        {
            await _connection.StartAsync();
            Console.WriteLine("Connected to hub");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to hub: {ex.Message}");
        }
    }

    static async Task SendMessage()
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            Console.WriteLine("Not connected to hub. Please connect first.");
            return;
        }

        Console.Write("Enter message: ");
        var message = Console.ReadLine();
        await _connection.InvokeAsync("SendMessage", message);
        Console.WriteLine("Message sent.");
    }
}
