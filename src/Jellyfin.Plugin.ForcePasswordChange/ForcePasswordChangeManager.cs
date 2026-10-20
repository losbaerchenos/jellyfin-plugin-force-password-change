using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ForcePasswordChange;

public class ForcePasswordChangeManager
{
    private readonly IUserManager _userManager;
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<ForcePasswordChangeManager> _logger;

    // In-Memory ist bewusst einfach. Für Neustart-Überleben:
    // später in PluginConfiguration bzw. eine eigene JSON-Datei serialisieren.
    private readonly ConcurrentDictionary<Guid, DateTime> _pendingUsers = new();

    public ForcePasswordChangeManager(
        IUserManager userManager,
        ISessionManager sessionManager,
        ILogger<ForcePasswordChangeManager> logger)
    {
        _userManager = userManager;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public void RequirePasswordChange(Guid userId)
    {
        _pendingUsers[userId] = DateTime.UtcNow;
        _logger.LogInformation("Password change required for user {UserId}", userId);
    }

    public void ClearPasswordChangeRequirement(Guid userId)
    {
        _pendingUsers.TryRemove(userId, out _);
        _logger.LogInformation("Password change requirement cleared for user {UserId}", userId);
    }

    public bool IsRequired(Guid userId) => _pendingUsers.ContainsKey(userId);

    public async Task HandleSessionStartedAsync(object? sender, SessionEventArgs e)
    {
        var userId = e.SessionInfo?.UserId ?? Guid.Empty;

        if (userId == Guid.Empty || !IsRequired(userId))
        {
            return;
        }

        if (Plugin.Instance?.Configuration.BlockSessionsUntilChanged == true)
        {
            _logger.LogInformation(
                "Blocking new session for user {UserId} until password is changed",
                userId);

            await _sessionManager.Logout(userId);
        }
    }
}
