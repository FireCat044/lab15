using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    // Метод для надсилання повідомлень конкретному клієнту
    public async Task SendMessage(string user, string message)
    {
        await Clients.User(user).SendAsync("ReceiveMessage", message);
    }

    // Метод для надсилання повідомлень всім клієнтам
    public async Task BroadcastMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
