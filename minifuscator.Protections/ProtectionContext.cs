using AsmResolver.DotNet;
using minifuscator.Protections.Settings;

namespace minifuscator.Protections;

/// <summary>
/// Represents the context of the protection. This is passed to all protections and can be used to access relevant data.
/// </summary>
public class ProtectionContext
{
  /// <summary>
  /// The module to be obfuscated.
  /// </summary>
  public ModuleDefinition Module { get; }

  /// <summary>
  /// The protection settings.
  /// </summary>
  public ProtectionSettings Settings { get; }

  /// <summary>
  /// Creates a new <seealso cref="ProtectionContext"/> instance with the specified target module and settings.
  /// </summary>
  /// <param name="module">The module to be obfuscated.</param>
  /// <param name="settings">The obfuscation settings.</param>
  public ProtectionContext(ModuleDefinition module, ProtectionSettings settings)
  {
    Module = module;
    Settings = settings;
  }
}
