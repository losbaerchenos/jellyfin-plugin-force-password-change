using System.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ForcePasswordChange;

public sealed class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton(serviceProvider =>
        {
            var applicationPaths = serviceProvider.GetRequiredService<IApplicationPaths>();
            var statePath = Path.Combine(
                applicationPaths.PluginConfigurationsPath,
                "Jellyfin.Plugin.ForcePasswordChange.pending-users.json");
            return new PasswordChangeStore(statePath);
        });
        serviceCollection.AddSingleton<ForcePasswordChangeManager>();
        serviceCollection.AddHostedService<SessionEventHandler>();
    }
}
