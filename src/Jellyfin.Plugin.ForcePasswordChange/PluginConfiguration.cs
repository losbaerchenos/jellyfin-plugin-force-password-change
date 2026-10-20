using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ForcePasswordChange;

public class PluginConfiguration : BasePluginConfiguration
{
    public bool BlockSessionsUntilChanged { get; set; } = true;

    public int MinimumPasswordLength { get; set; } = 12;
}
