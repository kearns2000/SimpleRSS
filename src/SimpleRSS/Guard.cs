namespace SimpleRSS;

internal static class Guard
{
    public static void NotNull<T>(T? value, string paramName) where T : class =>
        ArgumentNullException.ThrowIfNull(value, paramName);

    public static void NotNullOrWhiteSpace(string? value, string paramName) =>
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
}
