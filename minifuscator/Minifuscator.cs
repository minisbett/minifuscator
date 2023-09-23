using AsmResolver;
using AsmResolver.DotNet.Bundles;
using minifuscator.Models;
using minifuscator.Obfuscations;
using minifuscator.Utils;

namespace minifuscator;

/// <summary>
/// Main class for the minifuscator. Handles preparing the obfuscation job, executing the obfuscation engine
/// and writing the output assembly. Also handles potentially necessary parsing of AppHost bundles.
/// </summary>
public static class Minifuscator
{
  /// <summary>
  /// Bool whether logging should be disabled.
  /// </summary>
  public static bool DisableLogging { get; set; } = false;

  /// <summary>
  /// Runs the specified obfuscation job.
  /// </summary>
  public static void Obfuscate(ObfuscationJob job)
  {
    if (!DisableLogging)
    {
      Version version = SRAssembly.GetExecutingAssembly().GetName().Version!;
      Console.WriteLine($"Minifuscator v{version.Major}.{version.Minor}.{version.Build}");
    }

    ArgumentNullException.ThrowIfNull(job);
    ArgumentNullException.ThrowIfNull(job.Settings);
    ArgumentException.ThrowIfNullOrEmpty(job.Target.Input, nameof(job.Target.Input));
    ArgumentException.ThrowIfNullOrEmpty(job.Target.Output, nameof(job.Target.Output));

    Logger.Info("Main", $"Starting obfuscation job...");
    Logger.Info("Main", $"Input: {job.Target.Input}");
    Logger.Info("Main", $"Output: {job.Target.Output}");
    Logger.Info("Main", $"IsAppHostBundle: {job.Target.IsAppHostBundle}");

    // Get the assembly from the input file.
    Assembly assembly = GetAssembly(job.Target);
    Logger.Info("Main", $"Loaded assembly '{assembly.FullName}'.");

    // Create a new instance of all obfuscations, sorted by priority.
    ObfuscationBase[] obfuscations = typeof(ObfuscationBase).Assembly.GetTypes()
      .Where(x => typeof(ObfuscationBase).IsAssignableFrom(x) && !x.IsAbstract)
      .Select(x => (ObfuscationBase)Activator.CreateInstance(x)!)
      .OrderBy(x => x.Priority)
      .ToArray();
    Logger.Info("Main", $"Loaded obfuscations: {string.Join(", ", obfuscations.Select(x => x.GetType().Name))}");

    // Iterate through all obfuscation, inject the assembly and settings and execute them.
    foreach (ObfuscationBase obfuscation in obfuscations)
    {
      Logger.Info("Main", $"Executing obfuscation '{obfuscation.GetType().Name}'...");

      obfuscation.Assembly = assembly;
      obfuscation.Settings = job.Settings;
      obfuscation.Execute();
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
  private static Assembly GetAssembly(ObfuscationTarget target)
  {
    // Parse the assembly depending on whether the input is an AppHost bundle or not.
    if (target.IsAppHostBundle)
    {
      // Parse the AppHost bundle from the specified input file.
      Bundle bundle = Bundle.FromFile(target.Input);
      Logger.Info("Parser", $"Loaded AppHost bundle '{bundle.BundleID}'.");

      // Identify the main application binary from the bundle and parse the main module of the assembly.
      BundleFile mainAppBinary = bundle.GetFile($"{Path.GetFileNameWithoutExtension(target.Input)}.dll")!;
      Logger.Info("Parser", $"Found main app binary '{mainAppBinary.RelativePath}'.");
      return Assembly.FromBytes(mainAppBinary.GetData());
    }
    else
    {
      // Parse the assembly from the specified input file. The file is read first to prevent file locking.
      return Assembly.FromBytes(File.ReadAllBytes(target.Input));
    }
  }

  /// <summary>
  /// Writes the specified assembly into the output.
  /// </summary>
  private static void Write(ObfuscationTarget target, Assembly assembly)
  {
    // Write the assembly to either the output file or a temporary file if the target is an AppHost bundle.
    string file = target.IsAppHostBundle ? Path.GetTempFileName() : target.Output;
    assembly.Write(file);
    Logger.Info("Writer", $"Assembly written to file '{file}'.");

    // If the target is an AppHost bundle, write the bundle to the output file.
    if (target.IsAppHostBundle)
    {
      // Parse the AppHost bundle from the specified input file.
      Bundle bundle = Bundle.FromFile(target.Input);
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