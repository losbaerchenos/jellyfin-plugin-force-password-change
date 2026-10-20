using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ForcePasswordChange.Api;

[ApiController]
[Route("Plugin/ForcePasswordChange")]
public class ForcePasswordChangeController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly ISessionManager _sessionManager;
    private readonly ForcePasswordChangeManager _manager;
    private readonly ILogger<ForcePasswordChangeController> _logger;

    public ForcePasswordChangeController(
        IUserManager userManager,
        ISessionManager sessionManager,
        ForcePasswordChangeManager manager,
        ILogger<ForcePasswordChangeController> logger)
    {
        _userManager = userManager;
        _sessionManager = sessionManager;
        _manager = manager;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("PasswordChangePage")]
    [Produces("text/html")]
    public IActionResult PasswordChangePage()
    {
        const string resourceName =
            "Jellyfin.Plugin.ForcePasswordChange.Configuration.passwordChangePage.html";
        using var stream = typeof(ForcePasswordChangeController).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        Response.Headers.CacheControl = "no-store";
        return Content(reader.ReadToEnd(), "text/html; charset=utf-8");
    }

    [Authorize(Policy = "RequiresElevation")]
    [HttpPost("User/{userId}/Require")]
    public IActionResult Require([FromRoute] Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        if (_userManager.GetUserById(userId) is null)
        {
            return NotFound();
        }

        _manager.RequirePasswordChange(userId);
        return NoContent();
    }

    [Authorize(Policy = "RequiresElevation")]
    [HttpPost("User/{userId}/Clear")]
    public IActionResult Clear([FromRoute] Guid userId)
    {
        _manager.ClearPasswordChangeRequirement(userId);
        return NoContent();
    }

    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("RequiredUsers")]
    public IActionResult RequiredUsers()
    {
        return Ok(_manager.GetRequiredUserIds());
    }

    [Authorize]
    [HttpGet("Status")]
    public IActionResult Status()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(new
        {
            passwordChangeRequired = _manager.IsRequired(userId),
            minimumPasswordLength = Math.Clamp(Plugin.Instance?.Configuration.MinimumPasswordLength ?? 12, 8, 128)
        });
    }

    [Authorize]
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword(
        [FromBody, Required] ChangePasswordRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        if (!_manager.IsRequired(userId))
        {
            return BadRequest(new { error = "No password change is required for this user." });
        }

        var minLength = Math.Clamp(Plugin.Instance?.Configuration.MinimumPasswordLength ?? 12, 8, 128);

        if (request is null ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            request.NewPassword.Length < minLength)
        {
            return BadRequest(new
            {
                error = $"The new password must be at least {minLength} characters long."
            });
        }

        if (_userManager.GetUserById(userId) is null)
        {
            return NotFound();
        }

        await _userManager.ChangePassword(userId, request.NewPassword).ConfigureAwait(false);

        _manager.ClearPasswordChangeRequirement(userId);

        var currentAccessToken = User.FindFirst("Jellyfin-Token")?.Value ?? string.Empty;
        await _sessionManager.RevokeUserTokens(userId, currentAccessToken).ConfigureAwait(false);

        _logger.LogInformation("User {UserId} changed their required password", userId);

        return NoContent();
    }

    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirst("Jellyfin-UserId")?.Value, out userId) && userId != Guid.Empty;
    }
}

public sealed record ChangePasswordRequest(string? NewPassword);
