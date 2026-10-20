using System;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ForcePasswordChange;

public class SessionEventHandler : IHostedService
{
    private readonly ForcePasswordChangeManager _manager;
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<SessionEventHandler> _logger;

    public SessionEventHandler(
        ForcePasswordChangeManager manager,
        ISessionManager sessionManager,
        ILogger<SessionEventHandler> logger)
    {
        _manager = manager;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sessionManager.SessionStarted += OnSessionStarted;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _sessionManager.SessionStarted -= OnSessionStarted;
        return Task.CompletedTask;
    }

    private async Task OnSessionStarted(object? sender, SessionEventArgs e)
    {
        try
        {
            await _manager.HandleSessionStartedAsync(sender, e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle SessionStarted");
        }
    }
}
