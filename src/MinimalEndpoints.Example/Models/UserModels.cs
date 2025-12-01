namespace MinimalEndpoints.Example.Models;

public record User(int Id, string Name);
public record CreateUserRequest(string Name);
public record UpdateUserRequest(string Name);