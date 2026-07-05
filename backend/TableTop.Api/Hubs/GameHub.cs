using Microsoft.AspNetCore.SignalR;
using TableTop.Core.Entities;
using TableTop.Core.Interfaces;
using TableTop.Core.Services;

namespace TableTop.Api.Hubs;

public class GameHub(
    DiceService diceService,
    IChatMessageRepository chatMessages,
    RoomService roomService) : Hub
{
    public async Task JoinRoom(string joinCode, string playerName)
    {
        var room = await roomService.FindByJoinCodeAsync(joinCode);
        if (room is null)
        {
            await Clients.Caller.SendAsync("MessageReceived", "⚠️", "Комната не найдена", DateTimeOffset.UtcNow);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, joinCode);
        Context.Items["room"] = joinCode;
        Context.Items["roomId"] = room.Id;
        Context.Items["name"] = playerName;

        var history = await chatMessages.GetLastAsync(room.Id, 50);
        await Clients.Caller.SendAsync("History",
            history.Select(m => new { playerName = m.PlayerName, text = m.Text, sentAt = m.SentAt }));

        await Clients.OthersInGroup(joinCode).SendAsync("PlayerJoined", playerName);
    }

    public async Task SendMessage(string text)
    {
        if (Context.Items["room"] is not string joinCode ||
            Context.Items["roomId"] is not Guid roomId ||
            Context.Items["name"] is not string playerName)
            return;

        if (text.StartsWith("/roll ", StringComparison.OrdinalIgnoreCase))
        {
            await HandleRoll(joinCode, playerName, text["/roll ".Length..]);
            return;
        }

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            PlayerName = playerName,
            Text = text,
            SentAt = DateTimeOffset.UtcNow
        };
        await chatMessages.AddAsync(message);

        await Clients.Group(joinCode)
            .SendAsync("MessageReceived", playerName, text, message.SentAt);
    }

    private async Task HandleRoll(string joinCode, string playerName, string notation)
    {
        try
        {
            var result = diceService.Roll(notation);
            await Clients.Group(joinCode).SendAsync("DiceRolled", playerName, new
            {
                notation = notation.Trim(),
                rolls = result.Rolls,
                modifier = result.Modifier,
                total = result.Total
            });
        }
        catch (FormatException ex)
        {
            await Clients.Caller.SendAsync("MessageReceived", "🎲", ex.Message, DateTimeOffset.UtcNow);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items["room"] is string joinCode &&
            Context.Items["name"] is string playerName)
        {
            await Clients.OthersInGroup(joinCode)
                .SendAsync("PlayerLeft", playerName);
        }

        await base.OnDisconnectedAsync(exception);
    }
}