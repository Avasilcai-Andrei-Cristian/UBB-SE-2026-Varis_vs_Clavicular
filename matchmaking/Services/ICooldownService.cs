using System;

namespace matchmaking.Services
{
    public interface ICooldownService
    {
        bool IsOnCooldown(int userId, int jobId, DateTime utcNow);
    }
}