namespace minifuscator.Obfuscations;

/// <summary>
/// Applies miscellaneous obfuscations to the assembly.
/// </summary>
internal class Miscellaneous : ObfuscationBase
{
  public override int Priority => 100;

  public override void Execute()
  {
    if (Settings.Miscellaneous.RandomizeModuleGUIDs)
      RandomizeModuleGUIDs();
  }

  /// <summary>
  /// Replaces the Mvid, EncId and EncBaseId with new random GUIDs.
  /// </summary>
  private void RandomizeModuleGUIDs()
  {
    Module.Mvid = Guid.NewGuid();
    Module.EncId = Guid.NewGuid();
    Module.EncBaseId = Guid.NewGuid();

    Logger.Info("MiscObf", $"Mvid: {Module.Mvid}");
    Logger.Info("MiscObf", $"EncId: {Module.EncId}");
    Logger.Info("MiscObf", $"EncBaseId: {Module.EncBaseId}");
    Logger.Success("MiscObf", "Randomized module GUIDs.");
  }
}
