using AsmResolver.DotNet;
using AsmResolver.DotNet.Bundles;
using AsmResolver.PE.DotNet.Cil;
using System.Reflection;

namespace minifuscator.Utils;

/// <summary>
/// Provides utility methods for the AsmResolver library.
/// </summary>
internal static class AsmResolverUtils
{
  /// <summary>
  /// Returns all methods of all types.
  /// </summary>
  /// <param name="types">The enumerable of types.</param>
  /// <returns>An enumerable with all methods of the targetted types.</returns>
  public static IEnumerable<Method> GetAllMethods(this IEnumerable<Type> types)
  {
    return types.SelectMany(x => x.Methods);
  }

  /// <summary>
  /// Returns all parameters of all methods.
  /// </summary>
  /// <param name="methods">The enumerable of methods.</param>
  /// <returns>An enumerable with all parameters of the targetted methods.</returns>
  public static IEnumerable<Parameter> GetAllParameters(this IEnumerable<Method> methods)
  {
    return methods.SelectMany(x => x.ParameterDefinitions);
  }

  /// <summary>
  /// Returns all properties of all types.
  /// </summary>
  /// <param name="types">The enumerable of types.</param>
  /// <returns>An enumerable with all properties of the targetted types.</returns>
  public static IEnumerable<Property> GetAllProperties(this IEnumerable<Type> types)
  {
    return types.SelectMany(x => x.Properties).ToArray();
  }

  /// <summary>
  /// Returns all fields of all types.
  /// </summary>
  /// <param name="types">The enumerable of types.</param>
  /// <returns>An enumerable with all fields of the targetted types.</returns>
  public static IEnumerable<Field> GetAllFields(this IEnumerable<Type> types)
  {
    return types.SelectMany(x => x.Fields).ToArray();
  }

  /// <summary>
  /// Returns all events of all types.
  /// </summary>
  /// <param name="types">The enumerable of types.</param>
  /// <returns>An enumerable with all events of the targetted types.</returns>
  public static IEnumerable<Event> GetAllEvents(this IEnumerable<Type> types)
  {
    return types.SelectMany(x => x.Events).ToArray();
  }

  /// <summary>
  /// Returns the bundle file with the specified relative path or null if it was not found.
  /// </summary>
  /// <param name="bundle">The bundle.</param>
  /// <param name="relativePath">The relative path.</param>
  /// <returns>The bundle file or null if it was not found.</returns>
  public static BundleFile? GetFile(this BundleManifest bundle, string relativePath)
  {
    return bundle.Files.FirstOrDefault(x => x.RelativePath == relativePath);
  }

  /// <summary>
  /// Replace all short inline branch instructions with normal branch instructions.
  /// </summary>
  /// <param name="instructions">The instructions to modify.</param>
  public static void ReplaceShortInlineBranches(this IEnumerable<CilInstruction> instructions)
  {
    foreach (CilInstruction instruction in instructions)
      instruction.OpCode = instruction.OpCode.Code switch
      {
        CilCode.Br_S => CilOpCodes.Br,
        CilCode.Brfalse_S => CilOpCodes.Brfalse,
        CilCode.Brtrue_S => CilOpCodes.Brtrue,
        CilCode.Leave_S => CilOpCodes.Leave,
        CilCode.Beq_S => CilOpCodes.Beq,
        CilCode.Bge_S => CilOpCodes.Bge,
        CilCode.Bgt_S => CilOpCodes.Bgt,
        CilCode.Ble_S => CilOpCodes.Ble,
        CilCode.Blt_S => CilOpCodes.Blt,
        CilCode.Bne_Un_S => CilOpCodes.Bne_Un,
        CilCode.Bge_Un_S => CilOpCodes.Bge_Un,
        CilCode.Bgt_Un_S => CilOpCodes.Bgt_Un,
        CilCode.Ble_Un_S => CilOpCodes.Ble_Un,
        CilCode.Blt_Un_S => CilOpCodes.Blt_Un,
        _ => instruction.OpCode
      };
  }

  /// <summary>
  /// Gets the Ldc constant of the specified instruction.
  /// </summary>
  /// <param name="instruction">The instruction to get the constant of.</param>
  /// <returns>The Ldc constant.</returns>
  public static object GetLdcConstant(this CilInstruction instruction)
  {
    // Check if the instruction is an Ldc instruction.
    if (!instruction.IsLdc())
      throw new ArgumentException("Instruction is not an Ldc instruction.");

    // Return the constant in its casted form.
    return instruction.Operand switch
    {
      sbyte sb => sb,
      byte b => b,
      short s => s,
      ushort us => us,
      int i => i,
      uint ui => ui,
      long l => l,
      ulong ul => ul,
      float f => f,
      double d => d,
      _ => throw new InvalidCastException("Invalid Operand type.")
    };
  }

  /// <summary>
  /// Returns whether the specified instruction is an LdcI instruction. (Ldc_I4 or Ldc_I8)
  /// </summary>
  /// <param name="instruction">The instruction to check.</param>
  /// <returns>Bool whether the specified instruction is an LdcI instruction.</returns>
  public static bool IsLdc(this CilInstruction instruction)
    => instruction.OpCode.Code is >= CilCode.Ldc_I4_M1 and <= CilCode.Ldc_R8;

  /// <summary>
  /// Imports the specified type into the module and returns it.
  /// </summary>
  /// <param name="module">The module to import the type into.</param>
  /// <param name="type">The type to be imported into the module.</param>
  /// <returns>The imported type.</returns>
  public static ITypeDefOrRef Import(this Module module, System.Type type)
  {
    // Create a new reference importer, import the type and return it.
    ReferenceImporter importer = new ReferenceImporter(module);
    return importer.ImportType(type);
  }

  /// <summary>
  /// Imports the specified method into the module and returns it.
  /// </summary>
  /// <param name="module">The module to import the method into.</param>
  /// <param name="method">The method to be imported into the module.</param>
  /// <returns>The imported method.</returns>
  public static IMethodDescriptor Import(this Module module, MethodBase method)
  {
    // Create a new reference importer, import the method and return it.
    ReferenceImporter importer = new ReferenceImporter(module);
    return importer.ImportMethod(method);
  }
}