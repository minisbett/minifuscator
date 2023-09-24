using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using System.Text;

namespace minifuscator.Shared.Utils;

/// <summary>
/// Provides utility methods for native methods.
/// </summary>
public static class NativeUtils
{
  /// <summary>
  /// Returns a base for native methods with the specified type signature as the return type.
  /// </summary>
  /// <param name="signature">The type signature of the return type.</param>
  /// <returns>A base for a native method.</returns>
  private static MethodDefinition GetNativeMethodBase(TypeSignature signature)
    => new MethodDefinition(Guid.NewGuid().ToString(), 0 /* Set further down */, MethodSignature.CreateStatic(signature))
    {
      Attributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PInvokeImpl,
      ImplAttributes = MethodImplAttributes.Native | MethodImplAttributes.Unmanaged | MethodImplAttributes.PreserveSig
    };

  /// <summary>
  /// Builds a native method returning the specified string.
  /// </summary>
  /// <param name="str">The raw string.</param>
  /// <param name="libTypeFactory">CorLibTypeFactory for getting a char* type signature.</param>
  /// <returns>The built native method.</returns>
  public static MethodDefinition GetNativeStringMethod(string str, CorLibTypeFactory libTypeFactory)
  {
    // Get a new method base with a char* as the return type.
    MethodDefinition method = GetNativeMethodBase(libTypeFactory.Char.MakePointerType());

    // Build the native method body using the assembly code for returning the string bytes.
    method.NativeMethodBody = new NativeMethodBody(method)
    {
      Code = new byte[]
      {
          0x48, 0x8D, 0x05, 0x01, 0x00, 0x00, 0x00, // lea rax, [rip + 0x1]
          0xC3                                      // ret
      }.Concat(Encoding.Unicode.GetBytes(str)).ToArray()
    };

    return method;
  }

  /// <summary>
  /// Builds a native method with the specified type signature returning the specified bytes.
  /// </summary>
  /// <param name="num">The number.</param>
  /// <param name="factory">CorLibTypeFactory for getting the correct type signature.</param>
  /// <returns>The built native method.</returns>
  public static MethodDefinition GetNativeNumberMethod(object num, CorLibTypeFactory factory)
  {
    // Create a new method base with the correct type signature.
    MethodDefinition method = GetNativeMethodBase(factory.FromName("System", num.GetType().Name)!.MakeGenericInstanceType());

    // Build the native method body using the assembly code for returning the number bytes.
    method.NativeMethodBody = new NativeMethodBody(method)
    {
      Code = (num switch
      {
        // Use the correct mov instruction based on whether it's an integer or floating point.
        int or long => new byte[] { 0x48, 0x8B, 0x05 }, // mov rax
        float or double => new byte[] { 0xF2, 0x0F, 0x10, 0x05 }, // movsd xmm0
        _ => throw new NotSupportedException($"Unsupported number type '{num.GetType().Name}'.")
      }).Concat(new byte[]
      {
        0x01, 0x00, 0x00, 0x00, // [rip + 0x1]
        0xC3                    // ret
      }).Concat(num switch
      {
        int => BitConverter.GetBytes((int)num),
        long => BitConverter.GetBytes((long)num),
        float => BitConverter.GetBytes((float)num),
        double => BitConverter.GetBytes((double)num),
        _ => throw new NotSupportedException($"Unsupported number type '{num.GetType().Name}'.")
      }).ToArray()
    };

    return method;
  }
}
