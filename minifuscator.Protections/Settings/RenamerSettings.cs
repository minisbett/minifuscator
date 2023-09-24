namespace minifuscator.Protections.Settings;

/// <summary>
/// Represents the settings for the renamer protection.
/// </summary>
public class RenamerSettings
{
  /// <summary>
  /// Bool whether renamer is being executed.
  /// </summary>
  public bool Enabled { get; set; } = false;

  /// <summary>
  /// The length of the randomly generated names.
  /// </summary>
  public int Length { get; set; } = 32;

  /// <summary>
  /// The character set to use for the randomly generated names.
  /// Default is 1-65535.
  /// </summary>
  public char[] CharSet { get; set; } = Enumerable.Range(1, ushort.MaxValue).Select(x => (char)x).ToArray();

  /// <summary>
  /// Bool whether module names should be renamed.
  /// </summary>
  public bool Module { get; set; } = false;

  /// <summary>
  /// Bool whether namespace names should be renamed.
  /// Note: Consistency between types in a namespace is ensured.
  /// </summary>
  public bool Namespaces { get; set; } = false;

  /// <summary>
  /// Bool whether type names should be renamed.
  /// </summary>
  public bool Types { get; set; } = false;

  /// <summary>
  /// Bool whether method names should be renamed.
  /// </summary>
  public bool Methods { get; set; } = false;

  /// <summary>
  /// Bool whether parameter names should be renamed.
  /// </summary>
  public bool Parameters { get; set; } = false;

  /// <summary>
  /// Bool whether property names should be renamed.
  /// </summary>
  public bool Properties { get; set; } = false;

  /// <summary>
  /// Bool whether field names should be renamed.
  /// </summary>
  public bool Fields { get; set; } = false;

  /// <summary>
  /// Bool whether event names should be renamed.
  /// </summary>
  public bool Events { get; set; } = false;
}
