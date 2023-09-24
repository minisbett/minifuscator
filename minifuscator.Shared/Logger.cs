namespace minifuscator.Shared;

/// <summary>
/// Provides logging methods for the console.
/// </summary>
public static class Logger
{
  /// <summary>
  /// Outputs a success message.
  /// </summary>
  /// <param name="source">The source of the message.</param>
  /// <param name="msg">The message.</param>
  public static void Success(string source, string msg) => WriteLine(source, "SUCCESS", ConsoleColor.DarkGreen, msg);

  /// <summary>
  /// Outputs an info message.
  /// </summary>
  /// <param name="source">The source of the message.</param>
  /// <param name="msg">The message.</param>
  public static void Info(string source, string msg) => WriteLine(source, "INFO", ConsoleColor.Gray, msg);

  /// <summary>
  /// Outputs a warning message.
  /// </summary>
  /// <param name="source">The source of the message.</param>
  /// <param name="msg">The message.</param>
  public static void Warn(string source, string msg) => WriteLine(source, "WARN", ConsoleColor.Yellow, msg);

  /// <summary>
  /// Outputs an error message.
  /// </summary>
  /// <param name="source">The source of the message.</param>
  /// <param name="msg">The message.</param>
  public static void Error(string source, string msg) => WriteLine(source, "ERROR", ConsoleColor.Red, msg);

  /// <summary>
  /// Outputs a critical message.
  /// </summary>
  /// <param name="source">The source of the message.</param>
  /// <param name="msg">The message.</param>
  public static void Critical(string source, string msg) => WriteLine(source, "CRITICAL", ConsoleColor.DarkRed, msg);

  /// <summary>
  /// Writes the specified message with the specified severity and color to the console.
  /// </summary>
  /// <param name="source">The source of the message.</param>
  /// <param name="severity">The severity of the message.</param>
  /// <param name="msg">The message.</param>
  /// <param name="severityColor">The color of the severity.</param>
  private static void WriteLine(string source, string severity, ConsoleColor severityColor, string msg)
  {
    // Write the source to the console.
    Console.Write($"[{source}/");

    // Write the severity in the specified color while preserving the original color.
    ConsoleColor before = Console.ForegroundColor;
    Console.ForegroundColor = severityColor;
    Console.Write(severity);
    Console.ForegroundColor = before;

    // Write the message to the console.
    Console.WriteLine($"] {msg}");
  }
}
