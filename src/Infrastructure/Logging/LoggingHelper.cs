using Microsoft.Extensions.Logging;

namespace WipeoutRewrite.Infrastructure.Logging;

/// <summary>
/// Centralized logging helper for consistent log formatting across the application.
/// Provides standardized logging methods with consistent prefixes and formatting.
/// </summary>
public static class LoggingHelper
{
    /// <summary>
    /// Logs an informational message with a component prefix.
    /// </summary>
    public static void LogComponentInfo(ILogger logger, string component, string message, params object?[] args)
    {
        var formattedMessage = $"[{component}] {message}";
        logger.LogInformation(formattedMessage, args);
    }

    /// <summary>
    /// Logs a debug message with a component prefix.
    /// </summary>
    public static void LogComponentDebug(ILogger logger, string component, string message, params object?[] args)
    {
        var formattedMessage = $"[{component}] {message}";
        logger.LogDebug(formattedMessage, args);
    }

    /// <summary>
    /// Logs a warning message with a component prefix.
    /// </summary>
    public static void LogComponentWarning(ILogger logger, string component, string message, params object?[] args)
    {
        var formattedMessage = $"[{component}] {message}";
        logger.LogWarning(formattedMessage, args);
    }

    /// <summary>
    /// Logs an error message with a component prefix.
    /// </summary>
    public static void LogComponentError(ILogger logger, string component, string message, params object?[] args)
    {
        var formattedMessage = $"[{component}] {message}";
        logger.LogError(formattedMessage, args);
    }
}
