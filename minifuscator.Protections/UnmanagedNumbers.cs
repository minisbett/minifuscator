﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Cil;
using minifuscator.Shared;
using minifuscator.Shared.Utils;

namespace minifuscator.Protections;

/// <summary>
/// Applies number obfuscation to the assembly by replacing all Ldc instructions with calls to native methods.
/// </summary>
public class UnmanagedNumbers : Protection
{
  public override int Priority => 0;

  public override void Execute(ProtectionContext context)
  {
    if (!context.Settings.UnmanagedNumbers.Enabled)
      return;

    // Track the amount of obfuscated numbers to determine whether mixed-mode execution should be enabled and logging.
    int amount = 0;

    // Go through all methods and replace all Ldc instructions with a call to the native method.
    foreach (MethodDefinition method in context.Module.GetAllTypes().Where(x => !x.IsModuleType)
      .GetAllMethods().Where(x => x.CilMethodBody is not null))
    {
      CilInstructionCollection instructions = method.CilMethodBody!.Instructions!;

      // Replace all short inline branches with normal branches because we will be modifying the sizes,
      // of some operands, which moves offsets causing short inline branches to break if the offset is too large.
      instructions.ReplaceShortInlineBranches();

      foreach (CilInstruction instruction in instructions.Where(x => x.IsLdc()))
      {
        // Replace all Ldc_I4_M1 to Ldc_I4_8 and Ldc_I4_S instructions with Ldc_I4 instructions.
        // This way the instructions are more consistent and can be processed easier.
        if (instruction.OpCode.Code is >= CilCode.Ldc_I4_M1 and <= CilCode.Ldc_I4_8)
          instruction.ReplaceWith(CilOpCodes.Ldc_I4, (int)(instruction.OpCode.Code - CilCode.Ldc_I4_0));
        else if (instruction.OpCode.Code is CilCode.Ldc_I4_S)
          instruction.ReplaceWith(CilOpCodes.Ldc_I4, Convert.ToInt32((sbyte)instruction.Operand!));

        // Get the native method, add it to the module type and replace the instruction with a call to it.
        MethodDefinition native = NativeUtils.GetNativeNumberMethod(instruction.Operand!, context.Module.CorLibTypeFactory);
        context.Module.GetOrCreateModuleType().Methods.Add(native);
        instruction.ReplaceWith(CilOpCodes.Call, native);

        amount++;
      }
    }

    // If numbers were obfuscated, enable mixed-mode execution in the module.
    if (amount > 0)
      context.Module.Attributes &= ~DotNetDirectoryFlags.ILOnly;

    Logger.Success("NumObf", $"Obfuscated {amount} numbers.");
  }
}