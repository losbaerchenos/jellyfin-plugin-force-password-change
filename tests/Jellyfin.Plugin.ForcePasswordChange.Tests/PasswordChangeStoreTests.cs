using System.Reflection;
using Jellyfin.Plugin.ForcePasswordChange;
using Xunit;

namespace Jellyfin.Plugin.ForcePasswordChange.Tests;

public sealed class PasswordChangeStoreTests
{
    [Fact]
    public void SessionBlockingIsDisabledByDefault()
    {
        Assert.False(new PluginConfiguration().BlockSessionsUntilChanged);
    }

    [Fact]
    public void PasswordChangePageIsEmbedded()
    {
        var pluginAssembly = Assembly.LoadFrom(
            Path.Combine(AppContext.BaseDirectory, "Jellyfin.Plugin.ForcePasswordChange.dll"));
        using var page = pluginAssembly.GetManifestResourceStream(
            "Jellyfin.Plugin.ForcePasswordChange.Configuration.passwordChangePage.html");

        Assert.NotNull(page);
        using var reader = new StreamReader(page!);
        var html = reader.ReadToEnd();
        Assert.Contains("/Users/AuthenticateByName", html);
        Assert.Contains("/ChangePassword", html);
    }

    [Fact]
    public void RequiredUsersSurviveStoreRestart()
    {
        using var testDirectory = new TemporaryDirectory();
        var userId = Guid.NewGuid();
        var store = new PasswordChangeStore(testDirectory.StateFilePath);

        Assert.True(store.Require(userId));
        Assert.False(store.Require(userId));

        var restartedStore = new PasswordChangeStore(testDirectory.StateFilePath);
        Assert.True(restartedStore.IsRequired(userId));
        Assert.Equal(new[] { userId }, restartedStore.GetRequiredUsers());
    }

    [Fact]
    public void ClearedRequirementStaysClearedAfterRestart()
    {
        using var testDirectory = new TemporaryDirectory();
        var userId = Guid.NewGuid();
        var store = new PasswordChangeStore(testDirectory.StateFilePath);
        store.Require(userId);

        Assert.True(store.Clear(userId));
        Assert.False(store.Clear(userId));

        var restartedStore = new PasswordChangeStore(testDirectory.StateFilePath);
        Assert.False(restartedStore.IsRequired(userId));
        Assert.Empty(restartedStore.GetRequiredUsers());
    }

    [Fact]
    public void EmptyUserIdIsRejected()
    {
        using var testDirectory = new TemporaryDirectory();
        var store = new PasswordChangeStore(testDirectory.StateFilePath);

        Assert.Throws<ArgumentException>(() => store.Require(Guid.Empty));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
            StateFilePath = System.IO.Path.Combine(Path, "pending-users.json");
        }

        public string Path { get; }

        public string StateFilePath { get; }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
