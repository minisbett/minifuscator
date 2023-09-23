namespace minifuscator.Utils;

/// <summary>
/// Provides utility methods for strings.
/// </summary>
internal static class StringUtils
{
  /// <summary>
  /// Static random instance for the class.
  /// </summary>
  private static readonly Random _random = new Random();

  /// <summary>
  /// Returns a random string of the specified length using the specified character set.
  /// </summary>
  /// <param name="length">The length of the string.</param>
  /// <param name="charSet">The character set used to randomize the string.</param>
  /// <returns></returns>
  public static string GetRandomString(int length, char[] charSet) =>
    string.Concat(Enumerable.Range(0, length).Select(x => charSet[_random.Next(0, charSet.Length)]));
}
