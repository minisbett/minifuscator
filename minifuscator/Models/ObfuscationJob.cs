using minifuscator.Models.Settings;

namespace minifuscator.Models;

/// <summary>
/// Represents an obfuscation job to be passed to the obfuscation engine.
/// </summary>
public class ObfuscationJob
{
  /// <summary>
  /// The target input and output assembly of the obfuscation.
  /// </summary>
  public required ObfuscationTarget Target { get; init; }

  /// <summary>
  /// The settings for the obfuscations.
  /// </summary>
  public ObfuscationSettings Settings { get; set; } = new ObfuscationSettings();
}
