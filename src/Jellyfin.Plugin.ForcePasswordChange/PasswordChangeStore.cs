using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Jellyfin.Plugin.ForcePasswordChange;

public sealed class PasswordChangeStore
{
    private readonly string _filePath;
    private readonly object _syncLock = new();
    private HashSet<Guid> _pendingUsers;

    public PasswordChangeStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
        _pendingUsers = Load();
    }

    public bool IsRequired(Guid userId)
    {
        lock (_syncLock)
        {
            return _pendingUsers.Contains(userId);
        }
    }

    public Guid[] GetRequiredUsers()
    {
        lock (_syncLock)
        {
            return _pendingUsers.Order().ToArray();
        }
    }

    public bool Require(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A user ID is required.", nameof(userId));
        }

        lock (_syncLock)
        {
            if (_pendingUsers.Contains(userId))
            {
                return false;
            }

            var updatedUsers = new HashSet<Guid>(_pendingUsers) { userId };
            Save(updatedUsers);
            _pendingUsers = updatedUsers;
            return true;
        }
    }

    public bool Clear(Guid userId)
    {
        lock (_syncLock)
        {
            if (!_pendingUsers.Contains(userId))
            {
                return false;
            }

            var updatedUsers = new HashSet<Guid>(_pendingUsers);
            updatedUsers.Remove(userId);
            Save(updatedUsers);
            _pendingUsers = updatedUsers;
            return true;
        }
    }

    private HashSet<Guid> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var userIds = JsonSerializer.Deserialize<Guid[]>(File.ReadAllText(_filePath))
            ?? throw new InvalidDataException("The password change state file contains no user list.");

        if (userIds.Any(userId => userId == Guid.Empty))
        {
            throw new InvalidDataException("The password change state file contains an empty user ID.");
        }

        return new HashSet<Guid>(userIds);
    }

    private void Save(HashSet<Guid> userIds)
    {
        var directory = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("The password change state file must have a parent directory.");
        Directory.CreateDirectory(directory);

        var temporaryFilePath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
        File.WriteAllText(temporaryFilePath, JsonSerializer.Serialize(userIds.Order().ToArray()));
        File.Move(temporaryFilePath, _filePath, overwrite: true);
    }
}
