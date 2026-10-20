using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediaBrowser.Controller.Authentication;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ForcePasswordChange.Api;

[ApiController]
[Route("Plugin/ForcePasswordChange")]
public class ForcePasswordChangeController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly ForcePasswordChangeManager _manager;
    private readonly ILogger<ForcePasswordChangeController> _logger;

    public ForcePasswordChangeController(
        IUserManager userManager,
        ForcePasswordChangeManager manager,
        ILogger<ForcePasswordChangeController> logger)
    {
        _userManager = userManager;
        _manager = manager;
        _logger = logger;
    }

    [Authorize(Policy = "RequiresElevation")]
    [HttpPost("User/{userId}/Require")]
    public IActionResult Require([FromRoute] Guid userId)
    {
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

    [Authorize]
    [HttpGet("Status")]
    public IActionResult Status()
    {
        var userId = Guid.Parse(User.FindFirst("Jellyfin-UserId")!.Value);
        return Ok(new { passwordChangeRequired = _manager.IsRequired(userId) });
    }

    [Authorize]
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword(
        [FromBody, Required] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("Jellyfin-UserId")!.Value);

        if (!_manager.IsRequired(userId))
        {
            return BadRequest(new { error = "Für diesen Benutzer ist kein Passwortwechsel erforderlich." });
        }

        var minLength = Plugin.Instance?.Configuration.MinimumPasswordLength ?? 12;

        if (string.IsNullOrWhiteSpace(request.NewPassword) ||
            request.NewPassword.Length < minLength)
        {
            return BadRequest(new
            {
                error = $"Das neue Passwort muss mindestens {minLength} Zeichen lang sein."
            });
        }

        var user = _userManager.GetUserById(userId)
            ?? throw new InvalidOperationException("Benutzer nicht gefunden.");

        await _userManager.ChangePassword(user, request.NewPassword);

        _manager.ClearPasswordChangeRequirement(userId);

        _logger.LogInformation("User {UserId} changed their required password", userId);

        return NoContent();
    }
}

public record ChangePasswordRequest(string NewPassword);
