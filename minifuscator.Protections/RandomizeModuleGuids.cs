using minifuscator.Shared;

namespace minifuscator.Protections;

/// <summary>
/// Applies randomization of the module GUIDs (Mvid, EncId and EncBaseId) to the assembly.
/// </summary>
public class Miscellaneous : Protection
{
  public override int Priority => 100;

  public override void Execute(ProtectionContext context)
  {
    if (!context.Settings.RandomizeModuleGUIDs)
      return;

    // Randomize the module GUIDs.
    context.Module.Mvid = Guid.NewGuid();
    context.Module.EncId = Guid.NewGuid();
    context.Module.EncBaseId = Guid.NewGuid();

    Logger.Info("MiscObf", $"Mvid: {context.Module.Mvid}");
    Logger.Info("MiscObf", $"EncId: {context.Module.EncId}");
    Logger.Info("MiscObf", $"EncBaseId: {context.Module.EncBaseId}");
    Logger.Success("MiscObf", "Randomized module GUIDs.");
  }
}
