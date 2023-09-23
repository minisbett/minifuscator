using AsmResolver;
using AsmResolver.DotNet;
using minifuscator.Utils;

namespace minifuscator.Obfuscations;

/// <summary>
/// Applies obfuscation of the name identifiers of namespaces, types, methods, fields, properties, etc. to the assembly.
/// </summary>
internal class Names : ObfuscationBase
{
  public override int Priority => 1;

  /// <summary>
  /// A dictionary of all obfuscated names and their original names. This is used to maintain a consistency between
  /// the names and obfuscated names inside the lifespan of the instance of this obfuscation.
  /// 
  /// The JIT compiler depends on definitions like inherited methods or properties to have exactly the same name in
  /// both the inheriting type and the inherited type. (Example: the Execute method of the ObfuscationModule class)
  /// </summary>
  private readonly Dictionary<string, string> _obfuscatedNames = new Dictionary<string, string>();

  public override void Execute()
  {
    ArgumentNullException.ThrowIfNull(Settings.NameObfuscation, nameof(Settings.NameObfuscation));
    ArgumentNullException.ThrowIfNull(Settings.NameObfuscation.Length, nameof(Settings.NameObfuscation.Length));
    ArgumentNullException.ThrowIfNull(Settings.NameObfuscation.CharSet, nameof(Settings.NameObfuscation.CharSet));

    // Ignore if name obfuscation is disabled.
    if (!Settings.NameObfuscation.Enabled)
      return;

    // Create a list of all targetted name providers.
    List<INameProvider> providers = new List<INameProvider>();
    if (Settings.NameObfuscation.Types)
      providers.AddRange(Module.GetAllTypes());
    if (Settings.NameObfuscation.Methods)
      providers.AddRange(Module.GetAllTypes().GetAllMethods());
    if (Settings.NameObfuscation.Parameters)
      providers.AddRange(Module.GetAllTypes().GetAllMethods().GetAllParameters());
    if (Settings.NameObfuscation.Properties)
      providers.AddRange(Module.GetAllTypes().GetAllProperties());
    if (Settings.NameObfuscation.Fields)
      providers.AddRange(Module.GetAllTypes().GetAllFields());
    if (Settings.NameObfuscation.Events)
      providers.AddRange(Module.GetAllTypes().GetAllEvents());

    // Filter all name providers with null names or that are not eligible for obfuscation.
    providers = providers.Where(x => x.Name is not null).Where(IsEligibleForObfuscation).ToList();
    Logger.Info("NameObf", $"Prepared {providers.Count} name providers for obfuscation.");

    // Apply obfuscation to all name providers.
    foreach (INameProvider provider in providers)
      provider.GetType().GetProperty("Name")!.SetValue(provider,
            new Utf8String(GetObfuscatedName(provider.Name!)));

    // Apply namespace obfuscation to all types if enabled.
    if (Settings.NameObfuscation.Namespaces)
      foreach (Type type in Module.GetAllTypes().Where(x => x.Namespace is not null))
        type.Namespace = GetObfuscatedName(type.Namespace!);

    // Apply module name obfuscation if enabled.
    if (Settings.NameObfuscation.Module)
      Module.Name = GetObfuscatedName(Module.Name!);

    Logger.Success("NameObf", "Finished.");
  }

  /// <summary>
  /// Returns the obfuscated name of the specified name. Inside the lifespan of the instance of this obfuscation,
  /// every name will always be obfuscated to the same name due to reasons mentioned in the summary of the dictionary.
  /// </summary>
  /// <returns>The random string.</returns>
  private string GetObfuscatedName(string name)
  {
    // Check whether the name has already been obfuscated. If not, generate a new random name and add it.
    if (!_obfuscatedNames.ContainsKey(name))
      _obfuscatedNames.Add(name, StringUtils.GetRandomString(Settings.NameObfuscation.Length, Settings.NameObfuscation.CharSet));

    // Return the obfuscated name.
    return _obfuscatedNames[name];
  }

  /// <summary>
  /// Returns whether the specified INameProvider is eligible for it's name to be obfuscated.
  /// </summary>
  /// <param name="provider">The INameProvider instance.</param>
  /// <returns>Bool whether the specified INameProvider is eligible for it's name to be obfuscated.</returns>
  private bool IsEligibleForObfuscation(INameProvider provider)
  {
    // Exclude constructors, getters and setters.
    if (provider is Method method)
      return !method.IsConstructor && !method.IsGetMethod && !method.IsSetMethod;

    // Otherwise, return true.
    return true;
  }
}
