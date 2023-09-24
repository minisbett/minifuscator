namespace minifuscator.Protections.Settings;

/// <summary>
/// Represents the settings for the different obfuscations applied to the assembly.
/// </summary>
public class ProtectionSettings
{
  /// <summary>
  /// Settings for the name obfuscation.
  /// </summary>
  public RenamerSettings Renamer { get; set; } = new RenamerSettings();

  /// <summary>
  /// Settings for the string obfuscation.
  /// </summary>
  public UnmanagedStringsSettings UnmanagedStrings { get; set; } = new UnmanagedStringsSettings();

  /// <summary>
  /// Settings for the number obfuscation.
  /// </summary>
  public UnmanagedNumbersSettings UnmanagedNumbers { get; set; } = new UnmanagedNumbersSettings();

  /// <summary>
  /// Bool whether the module GUIDs (MVID, EncId and EncBaseId) should be randomized.
  /// </summary>
  public bool RandomizeModuleGUIDs { get; set; } = false;
}
