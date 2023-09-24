using AsmResolver;
using AsmResolver.DotNet;
using minifuscator.Shared;
using minifuscator.Shared.Utils;

namespace minifuscator.Protections;

/// <summary>
/// Applies obfuscation of the name identifiers to the assembly by replacing the names of namespaces, types, methods, fields, etc.
/// </summary>
public class Renamer : Protection
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

  public override void Execute(ProtectionContext context)
  {
    if (!context.Settings.Renamer.Enabled)
      return;

    // Create a list of all targetted name providers.
    List<INameProvider> providers = new List<INameProvider>();
    if (context.Settings.Renamer.Types)
      providers.AddRange(context.Module.GetAllTypes());
    if (context.Settings.Renamer.Methods)
      providers.AddRange(context.Module.GetAllTypes().GetAllMethods());
    if (context.Settings.Renamer.Parameters)
      providers.AddRange(context.Module.GetAllTypes().GetAllMethods().GetAllParameters());
    if (context.Settings.Renamer.Properties)
      providers.AddRange(context.Module.GetAllTypes().GetAllProperties());
    if (context.Settings.Renamer.Fields)
      providers.AddRange(context.Module.GetAllTypes().GetAllFields());
    if (context.Settings.Renamer.Events)
      providers.AddRange(context.Module.GetAllTypes().GetAllEvents());

    // Filter all name providers with null names or that are not eligible for obfuscation.
    providers = providers.Where(x => x.Name is not null).Where(IsEligibleForObfuscation).ToList();
    Logger.Info("NameObf", $"Prepared {providers.Count} name providers for obfuscation.");

    // Apply obfuscation to all name providers.
    foreach (INameProvider provider in providers)
      provider.GetType().GetProperty("Name")!.SetValue(provider,
            new Utf8String(GetObfuscatedName(provider.Name!, context.Settings.Renamer.Length, context.Settings.Renamer.CharSet)));

    // Apply namespace obfuscation to all types if enabled.
    if (context.Settings.Renamer.Namespaces)
      foreach (TypeDefinition type in context.Module.GetAllTypes().Where(x => x.Namespace is not null))
        type.Namespace = GetObfuscatedName(type.Namespace!, context.Settings.Renamer.Length, context.Settings.Renamer.CharSet);

    // Apply module name obfuscation if enabled.
    if (context.Settings.Renamer.Module)
      context.Module.Name = GetObfuscatedName(context.Module.Name!, context.Settings.Renamer.Length, context.Settings.Renamer.CharSet);

    Logger.Success("NameObf", "Finished.");
  }

  /// <summary>
  /// Returns the obfuscated name of the specified name. Inside the lifespan of the instance of this obfuscation,
  /// every name will always be obfuscated to the same name due to reasons mentioned in the summary of the dictionary.
  /// </summary>
  /// <param name="name" >The name to obfuscate.</param>
  /// <param name="length" >The length of the obfuscated name.</param>
  /// <param name="charSet" >The character set for the randomized, obfuscated name.</param>
  /// <returns>The random string.</returns>
  private string GetObfuscatedName(string name, int length, char[] charSet)
  {
    // Check whether the name has already been obfuscated. If not, generate a new random name and add it.
    if (!_obfuscatedNames.ContainsKey(name))
      _obfuscatedNames.Add(name, StringUtils.GetRandomString(length, charSet));

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
    if (provider is MethodDefinition method)
      return !method.IsConstructor && !method.IsGetMethod && !method.IsSetMethod;

    // Otherwise, return true.
    return true;
  }
}
