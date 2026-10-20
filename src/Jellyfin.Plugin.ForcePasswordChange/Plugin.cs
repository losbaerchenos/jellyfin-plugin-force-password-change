using System;
using System.Collections.Generic;
using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ForcePasswordChange;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        IServerApplicationHost applicationHost)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        ApplicationHost = applicationHost;
    }

    public static Plugin? Instance { get; private set; }

    public IServerApplicationHost ApplicationHost { get; }

    public override string Name => "Force Password Change";

    public override string Description =>
        "Ermöglicht Administratoren, Benutzer zu zwingen, beim nächsten Login ein neues Passwort zu setzen.";

    public override Guid Id => new("6f4c70b2-93a1-4c37-9d0f-52b7d0a1c911");

    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = "ForcePasswordChange",
            EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.configPage.html"
        };
    }
}
