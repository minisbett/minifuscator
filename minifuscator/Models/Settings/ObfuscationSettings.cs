namespace minifuscator.Models.Settings;

/// <summary>
/// Represents the settings for the different obfuscations applied to the assembly.
/// </summary>
public class ObfuscationSettings
{
  /// <summary>
  /// Settings for the name obfuscation.
  /// </summary>
  public NameObfuscationSettings NameObfuscation { get; set; } = new NameObfuscationSettings();

  /// <summary>
  /// Settings for the string obfuscation.
  /// </summary>
  public StringObfuscationSettings StringObfuscation { get; set; } = new StringObfuscationSettings();

  /// <summary>
  /// Settings for the number obfuscation.
  /// </summary>
  public NumberObfuscationSettings NumberObfuscation { get; set; } = new NumberObfuscationSettings();

  /// <summary>
  /// Bool whether the module GUIDs (MVID, EncId and EncBaseId) should be randomized.
  /// </summary>
  public bool RandomizeModuleGUIDs { get; set; } = false;
}
