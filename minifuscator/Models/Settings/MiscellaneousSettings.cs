namespace minifuscator.Models.Settings;

/// <summary>
/// Represents miscellaneous settings of smaller obfuscations.
/// </summary>
public class MiscellaneousSettings
{
  /// <summary>
  /// Bool whether the module GUIDs (MVID, EncId and EncBaseId) should be randomized.
  /// </summary>
  public bool RandomizeModuleGUIDs { get; set; } = false;
}
