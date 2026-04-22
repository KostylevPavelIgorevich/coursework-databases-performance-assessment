namespace BackendCore;

public static class GreetingService
{
    public static string Greet(string name)
    {
        var safeName = string.IsNullOrWhiteSpace(name) ? "friend" : name.Trim();
        return $"Hello, {safeName}! You've been greeted from C#!";
    }
}
