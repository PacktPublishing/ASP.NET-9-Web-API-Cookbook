using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

class Program
{
    private static string _bearerToken = string.Empty;
    private static HubConnection _connection = CreateDummyConnection();
    private static string _currentConnectionId = string.Empty;

    private static HubConnection CreateDummyConnection()
    {
        return new HubConnectionBuilder().WithUrl("http://dummy").Build();
    }

    private static bool EnsureConnected()
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            Console.WriteLine("Not connected to hub. Please log in first.");
            return false;
        }
        return true;
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("SignalR Messaging Hub Test Client");

        while (true)
        {
            if (string.IsNullOrEmpty(_bearerToken))
            {
                await Login();
            }
            else
            {
                await ShowMainMenu();
            }
        }
    }

    static async Task Login()
    {
        Console.WriteLine("Please log in:");
        Console.Write("Username: ");
        var username = Console.ReadLine();
        Console.Write("Password: ");
        var password = Console.ReadLine();

        using (var client = new HttpClient())
        {
            var response = await client.PostAsJsonAsync("https://localhost:5001/api/auth/login", new { username, password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(result);
                _bearerToken = json.GetProperty("token").GetString()!;
                Console.WriteLine("Login successful!");
                await ConnectToHub();
            }
            else
            {
                Console.WriteLine("Login failed. Please try again.");
            }
        }

    }

    static async Task ConnectToHub()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/messagingHub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_bearerToken)!;
            })
            .Build();

        _connection.On<string>("ReceiveConnectionId", (connectionId) =>
        {
            _currentConnectionId = connectionId;
            Console.WriteLine($"Your connection ID: {connectionId}");
        });
        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Console.WriteLine($"{user}: {message}");
        });
        _connection.On<string, string>("ReceiveDirectMessage", (user, message) =>
        {
            Console.WriteLine($"[Direct] {user}: {message}");
        });
        _connection.On<string, string, string>("ReceiveGroupMessage", (user, group, message) =>
        {
            Console.WriteLine($"{user} in {group}: {message}");
        });

        _connection.On<string>("UserConnected", (user) =>
        {
            Console.WriteLine($"{user} connected");
        });

        _connection.On<string>("UserDisconnected", (user) =>
        {
            Console.WriteLine($"{user} disconnected");
        });

        try
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync("GetConnectionId");
            Console.WriteLine("Connected to hub");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to hub: {ex.Message}");
            _connection = CreateDummyConnection();
        }
    }

    static async Task ShowMainMenu()
    {
        await DisplayConnectionId();

        Console.WriteLine("\nChoose an option:");
        Console.WriteLine("1. Send message to all");
        Console.WriteLine("2. Send direct message");
        Console.WriteLine("3. Send group message");
        Console.WriteLine("4. Add user to group (Admin only)");
        Console.WriteLine("5. Remove user from group (Admin only)");
        Console.WriteLine("6. Logout");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                await SendMessageToAll();
                break;
            case "2":
                await SendDirectMessage();
                break;
            case "3":
                await SendGroupMessage();
                break;
            case "4":
                await AddUserToGroup();
                break;
            case "5":
                await RemoveUserFromGroup();
                break;
            case "6":
                Logout();
                break;
            default:
                Console.WriteLine("Invalid option, please try again.");
                break;
        }

            await DisplayConnectionId();
    }

     static async Task DisplayConnectionId()
     {
        if (EnsureConnected())
        {
            try
            {
                _currentConnectionId = await _connection.InvokeAsync<string>("GetConnectionId");
                Console.WriteLine($"\nCurrent Connection ID: {_currentConnectionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting connection ID: {ex.Message}");
            }
        }
    }


    static async Task SendMessageToAll()
    {
        if (!EnsureConnected()) return;

        Console.Write("Enter message: ");
        var message = Console.ReadLine();
        await _connection.InvokeAsync("SendToAll", message);
    }

    static async Task SendDirectMessage()
    {
        if (!EnsureConnected()) return;

        Console.Write("Enter recipient username: ");
        var recipient = Console.ReadLine();
        Console.Write("Enter message: ");
        var message = Console.ReadLine();
        await _connection.InvokeAsync("SendToIndividual", recipient, message);
    }

    static async Task SendGroupMessage()
    {
        if (!EnsureConnected()) return;

        Console.Write("Enter group name: ");
        var group = Console.ReadLine();
        Console.Write("Enter message: ");
        var message = Console.ReadLine();
        await _connection.InvokeAsync("SendToGroup", group, message);
    }

    static async Task AddUserToGroup()
    {
        if (!EnsureConnected()) return;

        Console.Write("Enter username to add: ");
        var username = Console.ReadLine();
        Console.Write("Enter group name: ");
        var group = Console.ReadLine();

        try
        {
            await _connection.InvokeAsync("AddUserToGroup", username, group);
            Console.WriteLine($"User {username} has been added to group {group}");
        }
        catch (HubException ex) when (ex.Message.Contains("user is unauthorized"))
        {
            Console.WriteLine("Error: You are not authorized to add users to groups. This action requires admin privileges.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while adding user to group: {ex.Message}");
        }
    }

    static async Task RemoveUserFromGroup()
    {
        if (!EnsureConnected()) return;

        Console.Write("Enter username to remove: ");
        var username = Console.ReadLine();
        Console.Write("Enter group name: ");
        var group = Console.ReadLine();

        try
        {
            await _connection.InvokeAsync("RemoveUserFromGroup", username, group);
            Console.WriteLine($"User {username} has been removed from group {group}");
        }
        catch (HubException ex) when (ex.Message.Contains("user is unauthorized"))
        {
            Console.WriteLine("Error: You are not authorized to remove users from groups. This action requires admin privileges.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while removing user from group: {ex.Message}");
        }
    }


    static void Logout()
    {
        _bearerToken = string.Empty;
        _connection.DisposeAsync().AsTask().Wait();
        _connection = CreateDummyConnection();
        _currentConnectionId = string.Empty;
        Console.WriteLine("Logged out successfully.");
    }
}
