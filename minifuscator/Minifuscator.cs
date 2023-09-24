using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Bundles;
using minifuscator.Protections;
using minifuscator.Shared;
using minifuscator.Shared.Utils;
using System.Reflection;

namespace minifuscator;

/// <summary>
/// Main class for the minifuscator. Handles preparing the obfuscation job, executing the obfuscation engine
/// and writing the output assembly. Also handles potentially necessary parsing of AppHost bundles.
/// </summary>
public static class Minifuscator
{
  /// <summary>
  /// Runs the specified obfuscation job.
  /// </summary>
  public static void Obfuscate(ObfuscationJob job)
  {
    Version version = Assembly.GetExecutingAssembly().GetName().Version!;
    Console.WriteLine($"Minifuscator v{version.Major}.{version.Minor}.{version.Build}");

    ArgumentNullException.ThrowIfNull(job);
    ArgumentNullException.ThrowIfNull(job.Settings);
    ArgumentException.ThrowIfNullOrEmpty(job.Target.Input, nameof(job.Target.Input));
    ArgumentException.ThrowIfNullOrEmpty(job.Target.Output, nameof(job.Target.Output));

    Logger.Info("Main", $"Starting obfuscation job...");
    Logger.Info("Main", $"Input: {job.Target.Input}");
    Logger.Info("Main", $"Output: {job.Target.Output}");
    Logger.Info("Main", $"IsAppHostBundle: {job.Target.IsAppHostBundle}");

    // Get the assembly from the input file.
    AssemblyDefinition assembly = GetAssembly(job.Target);
    Logger.Info("Main", $"Loaded assembly '{assembly.FullName}'.");

    // Create a new instance of all protections, sorted by priority.
    Protection[] protections = typeof(Protection).Assembly.GetTypes()
      .Where(x => typeof(Protection).IsAssignableFrom(x) && !x.IsAbstract)
      .Select(x => (Protection)Activator.CreateInstance(x)!)
      .OrderBy(x => x.Priority)
      .ToArray();
    Logger.Info("Main", $"Loaded protections: {string.Join(", ", protections.Select(x => x.GetType().Name))}");

    // Iterate through all protections, build the context and execute them.
    foreach (Protection protection in protections)
    {
      Logger.Info("Main", $"Executing obfuscation '{protection.GetType().Name}'...");

      protection.Execute(new ProtectionContext(assembly.ManifestModule!, job.Settings));
    }

    Logger.Success("Main", "Obfuscation process finished.");

    // Write the assembly to the target output file, handling AppHost bundles if necessary.
    try
    {
      Write(job.Target, assembly);
    }
    catch (Exception ex)
    {
      Logger.Error("Writer", $"Failed to write output file.");
      Logger.Error("Writer", ex.Message);
      return;
    }

    Logger.Success("Main", "Obfuscation finished.");
  }

  /// <summary>
  /// Parses the assembly from the input file of the job, potentially parsing an AppHost bundle.
  /// </summary>
  /// <returns>The parsed assembly.</returns>
  private static AssemblyDefinition GetAssembly(ObfuscationTarget target)
  {
    // Parse the assembly depending on whether the input is an AppHost bundle or not.
    if (target.IsAppHostBundle)
    {
      // Parse the AppHost bundle from the specified input file.
      BundleManifest bundle = BundleManifest.FromFile(target.Input);
      Logger.Info("Parser", $"Loaded AppHost bundle '{bundle.BundleID}'.");

      // Identify the main application binary from the bundle and parse the main module of the assembly.
      BundleFile mainAppBinary = bundle.GetFile($"{Path.GetFileNameWithoutExtension(target.Input)}.dll")!;
      Logger.Info("Parser", $"Found main app binary '{mainAppBinary.RelativePath}'.");
      return AssemblyDefinition.FromBytes(mainAppBinary.GetData());
    }
    else
    {
      // Parse the assembly from the specified input file. The file is read first to prevent file locking.
      return AssemblyDefinition.FromBytes(File.ReadAllBytes(target.Input));
    }
  }

  /// <summary>
  /// Writes the specified assembly into the output.
  /// </summary>
  private static void Write(ObfuscationTarget target, AssemblyDefinition assembly)
  {
    // Write the assembly to either the output file or a temporary file if the target is an AppHost bundle.
    string file = target.IsAppHostBundle ? Path.GetTempFileName() : target.Output;
    assembly.Write(file);
    Logger.Info("Writer", $"Assembly written to file '{file}'.");

    // If the target is an AppHost bundle, write the bundle to the output file.
    if (target.IsAppHostBundle)
    {
      // Parse the AppHost bundle from the specified input file.
      BundleManifest bundle = BundleManifest.FromFile(target.Input);
      Logger.Info("Writer", $"Loaded AppHost bundle '{bundle.BundleID}'.");

      // Identify the main application binary from the bundle and write the assembly back into it.
      BundleFile mainAppBinary = bundle.GetFile($"{Path.GetFileNameWithoutExtension(target.Input)}.dll")!;
      Logger.Info("Writer", $"Found main app binary '{mainAppBinary.RelativePath}'");
      mainAppBinary.Contents = new DataSegment(File.ReadAllBytes(file));
      Logger.Info("Writer", "Assembly written into the main app binary.");

      // Rebuild the AppHost bundle using the original bundle as the template.
      BundlerParameters parameters = BundlerParameters.FromExistingBundle(target.Input, mainAppBinary.RelativePath);
      bundle.WriteUsingTemplate(target.Output, parameters);
      Logger.Info("Writer", $"Bundle written to '{target.Output}'.");

      // Clean up the temporary file.
      File.Delete(file);
    }
  }
}