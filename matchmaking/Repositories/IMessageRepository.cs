using System;
using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IMessageRepository
{
    IReadOnlyList<Message> GetByChatId(int chatId, DateTime? visibleAfter = null);
    void Add(Message message);
    void MarkAsRead(int chatId, int readerId);
}
