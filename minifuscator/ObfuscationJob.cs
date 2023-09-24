using minifuscator.Protections.Settings;

namespace minifuscator;

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
    /// The settings for the protections.
    /// </summary>
    public ProtectionSettings Settings { get; set; } = new ProtectionSettings();
}
