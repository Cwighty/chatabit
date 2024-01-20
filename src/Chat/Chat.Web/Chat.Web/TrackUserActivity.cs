using System.Diagnostics.Metrics;
using Chat.Data.Entities;

namespace Chat.Web;

public class UserActivityTracker
{
    private readonly HashSet<string> _activeUsers = [];
    private DateTime _lastUpdate = DateTime.UtcNow.Date;
    private readonly Counter<int> _dailyActiveUsersCounter;
    
    public UserActivityTracker(Meter meter)
    {
        _dailyActiveUsersCounter = meter.CreateCounter<int>("chatapi.daily_active_users", null, "Number of active users per day");
    }

    public void TrackUserActivity(ChatMessage message)
    {
        var today = DateTime.UtcNow.Date;
        if (!_activeUsers.Add(message.UserName))
        {
            if (_lastUpdate != today)
            {
                _dailyActiveUsersCounter.Add(_activeUsers.Count);
                _activeUsers.Clear();
                _lastUpdate = today;
            }
        }
        _dailyActiveUsersCounter.Add(1);
    }
}
