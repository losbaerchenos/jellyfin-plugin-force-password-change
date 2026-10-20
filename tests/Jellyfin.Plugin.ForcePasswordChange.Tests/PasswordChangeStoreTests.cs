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
    public void PasswordChangePageUsesExpectedEndpoints()
    {
        var html = ReadConfigurationPage("passwordChangePage.html");
        Assert.Contains("/Users/AuthenticateByName", html);
        Assert.Contains("/ChangePassword", html);
        Assert.Contains("headers.set('Authorization', getAuthorizationHeader())", html);
        Assert.Contains("values.push(`Token=\"${accessToken}\"`)", html);
        Assert.DoesNotContain("X-Emby-Authorization", html);
        Assert.DoesNotContain("X-Emby-Token", html);
    }

    [Fact]
    public void ConfigurationPageDisplaysHttpErrorDetails()
    {
        var html = ReadConfigurationPage("configPage.html");

        Assert.Contains("error?.responseJSON", html);
        Assert.Contains("error?.responseText", html);
        Assert.Contains("'HTTP ' + error.status", html);
        Assert.Contains("user list could not be refreshed", html);
        Assert.Contains("'/Plugin/ForcePasswordChange/User/' + encodeURIComponent(userId) + '/' + action", html);
        Assert.DoesNotContain("`${", html);
    }

    private static string ReadConfigurationPage(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Configuration", fileName);
        return File.ReadAllText(path);
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
