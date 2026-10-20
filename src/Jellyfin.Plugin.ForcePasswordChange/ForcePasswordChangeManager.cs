using System;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Data.Queries;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ForcePasswordChange;

public class ForcePasswordChangeManager
{
    private readonly IUserManager _userManager;
    private readonly IDeviceManager _deviceManager;
    private readonly ISessionManager _sessionManager;
    private readonly PasswordChangeStore _store;
    private readonly ILogger<ForcePasswordChangeManager> _logger;

    public ForcePasswordChangeManager(
        IUserManager userManager,
        IDeviceManager deviceManager,
        ISessionManager sessionManager,
        PasswordChangeStore store,
        ILogger<ForcePasswordChangeManager> logger)
    {
        _userManager = userManager;
        _deviceManager = deviceManager;
        _sessionManager = sessionManager;
        _store = store;
        _logger = logger;
    }

    public void RequirePasswordChange(Guid userId)
    {
        if (_userManager.GetUserById(userId) is null)
        {
            throw new ArgumentException("User does not exist.", nameof(userId));
        }

        if (_store.Require(userId))
        {
            _logger.LogInformation("Password change required for user {UserId}", userId);
        }
    }

    public void ClearPasswordChangeRequirement(Guid userId)
    {
        if (_store.Clear(userId))
        {
            _logger.LogInformation("Password change requirement cleared for user {UserId}", userId);
        }
    }

    public bool IsRequired(Guid userId) => _store.IsRequired(userId);

    public Guid[] GetRequiredUserIds() => _store.GetRequiredUsers();

    public async Task HandleSessionStartedAsync(object? sender, SessionEventArgs e)
    {
        var session = e.SessionInfo;
        if (session is null || session.UserId == Guid.Empty || !IsRequired(session.UserId))
        {
            return;
        }

        if (Plugin.Instance?.Configuration.BlockSessionsUntilChanged != true)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(session.DeviceId))
        {
            _logger.LogWarning(
                "Unable to block session for user {UserId}: the session has no device ID",
                session.UserId);
            return;
        }

        var devices = _deviceManager.GetDevices(new DeviceQuery
        {
            UserId = session.UserId,
            DeviceId = session.DeviceId
        }).Items;

        if (devices.Count == 0)
        {
            _logger.LogWarning(
                "Unable to block session for user {UserId}: no matching authentication device was found",
                session.UserId);
            return;
        }

        _logger.LogInformation(
            "Blocking session for user {UserId} until the password is changed",
            session.UserId);

        foreach (var device in devices)
        {
            await _sessionManager.Logout(device).ConfigureAwait(false);
        }
    }
}
