using TableTop.Core.Entities;

namespace TableTop.Core.Interfaces;

public interface IChatMessageRepository
{
    Task AddAsync(ChatMessage message, CancellationToken ct = default);
    Task<List<ChatMessage>> GetLastAsync(Guid roomId, int count, CancellationToken ct = default);
}