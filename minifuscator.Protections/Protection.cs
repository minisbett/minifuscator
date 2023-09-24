namespace minifuscator.Protections;

/// <summary>
/// Base class for protections.
/// </summary>
public abstract class Protection
{
  /// <summary>
  /// The priority of the protection. The lower the number, the earlier it will be executed.
  /// </summary>
  public abstract int Priority { get; }

  /// <summary>
  /// Entry point for the protection to execute.
  /// </summary>
  public abstract void Execute(ProtectionContext context);
}
