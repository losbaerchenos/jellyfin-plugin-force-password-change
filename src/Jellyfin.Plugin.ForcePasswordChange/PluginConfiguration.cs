using System.ComponentModel.DataAnnotations;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ForcePasswordChange;

public class PluginConfiguration : BasePluginConfiguration
{
    public bool BlockSessionsUntilChanged { get; set; }

    [Range(8, 128)]
    public int MinimumPasswordLength { get; set; } = 12;
}
