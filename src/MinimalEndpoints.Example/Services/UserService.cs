using System.Collections.Concurrent;
using MinimalEndpoints.Example.Models;

namespace MinimalEndpoints.Example.Services;

public interface IUserService
{
    User Create(string name);
    User? Get(int id);
    User? Update(int id, string name);
    bool Delete(int id);
}

public class UserService : IUserService
{
    private static readonly ConcurrentDictionary<int, User> Users = new();
    private static int _nextId = 0;

    public User Create(string name)
    {
        var id = Interlocked.Increment(ref _nextId);
        var user = new User(id, name);
        Users.TryAdd(id, user);
        return user;
    }

    public User? Get(int id)
    {
        return Users.TryGetValue(id, out var user) ? user : null;
    }

    public User? Update(int id, string name)
    {
        if (Users.TryGetValue(id, out var user))
        {
            var updated = user with { Name = name };
            Users.TryUpdate(id, updated, user);
            return updated;
        }
        return null;
    }

    public bool Delete(int id)
    {
        return Users.TryRemove(id, out _);
    }
}
