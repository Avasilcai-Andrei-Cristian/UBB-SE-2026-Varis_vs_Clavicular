using matchmaking.Domain.Session;

namespace matchmaking;

public static class App
{
    public static SessionContext Session { get; set; } = new SessionContext();
}
