namespace minifuscator.Modules;

/// <summary>
/// Base class for obfuscations, providing the assembly, module and settings.
/// </summary>
internal abstract class ObfuscationBase
{
  /// <summary>
  /// The obfuscation settings, injected on initialization.
  /// </summary>
  public ObfuscationSettings Settings { get; set; } = null!;

  /// <summary>
  /// The targetted assembly, injected on initialization.
  /// </summary>
  public Assembly Assembly { get; set; } = null!;

  /// <summary>
  /// The main module of the targetted assembly.
  /// </summary>
  public Module Module => Assembly.ManifestModule!;

  /// <summary>
  /// The priority of the obfuscation. The lower the number, the earlier it will be executed.
  /// </summary>
  public abstract int Priority { get; }

  /// <summary>
  /// Entry point for the obfuscation to execute.
  /// </summary>
  public abstract void Execute();
}
