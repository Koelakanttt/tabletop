using Microsoft.EntityFrameworkCore;
using TableTop.Core.Entities;
using TableTop.Core.Interfaces;
using TableTop.Infrastructure.Data;

namespace TableTop.Infrastructure.Repositories;

public class ChatMessageRepository(AppDbContext db) : IChatMessageRepository
{
    public async Task AddAsync(ChatMessage message, CancellationToken ct = default)
    {
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<ChatMessage>> GetLastAsync(Guid roomId, int count, CancellationToken ct = default)
    {
        var lastDescending = await db.ChatMessages
            .AsNoTracking()
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.SentAt)
            .Take(count)
            .ToListAsync(ct);

        lastDescending.Reverse(); // хронологический порядок для отображения
        return lastDescending;
    }
}