﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using minifuscator.Shared;
using minifuscator.Shared.Utils;
using System.Reflection;

namespace minifuscator.Protections;

/// <summary>
/// Applies obfuscation of all kinds of method calls to the assembly.
/// </summary>
public class IndirectCalls : Protection
{
  public override int Priority => 2;

  public override void Execute(ProtectionContext context)
  {
    return;
    // Import the RuntimeMethodHandle type to create local variables of thatr type.
    TypeSignature runtimeMethodHandle = context.Module.Import(typeof(RuntimeMethodHandle)).ToTypeSignature(isValueType: true);

    // Import Type.GetTypeFromHandle(RuntimeTypeHandle) to get the <Module> type at runtime.
    // Calling this method on the <Module> handle is the equivalent of typeof(<Module>).
    IMethodDescriptor getTypeFromHandle = context.Module.Import(
      typeof(Type).GetMethod(nameof(System.Type.GetTypeFromHandle),
      new Type[] { typeof(RuntimeTypeHandle) })!);

    // Import the getter of Type::Module to get the module of a type.
    IMethodDescriptor getModule = context.Module.Import(
      typeof(Type).GetProperty(nameof(Type.Module))!.GetMethod!);

    // Import Module::ResolveMethod(int) to get a method inside the module by it's metadata token.
    IMethodDescriptor resolveMethod = context.Module.Import(
      typeof(Module).GetMethod(nameof(Module.ResolveMethod),
      new Type[] { typeof(int) })!);

    // Import MethodBase::MethodHandle to get the handle of a method.
    IMethodDescriptor getMethodHandle = context.Module.Import(
      typeof(MethodBase).GetProperty(nameof(MethodBase.MethodHandle))!.GetMethod!);

    // Import RuntimeMethodHandle::GetFunctionPointer to get the function pointer of a method.
    IMethodDescriptor getFunctionPointer = context.Module.Import(
      typeof(RuntimeMethodHandle).GetMethod(nameof(RuntimeMethodHandle.GetFunctionPointer))!);

    // Go through all methods and replace all Call instructions with indirect calls. (Calli)
    foreach (MethodDefinition method in context.Module.GetAllTypes().Where(x => !x.IsModuleType).GetAllMethods()
      .Where(x => x.IsEntryPoint() && x.HasMethodBody))
    {
      CilInstructionCollection instructions = method.CilMethodBody!.Instructions!;

      // Replace all short inline branches with normal branches because we will be inserting instructions,
      // which moves offsets causing short inline branches to break if the offset is too large.
      instructions.ReplaceShortInlineBranches();

      // Go through all instructions and replace the Call instructions with indirect calls.
      for (int i = 0; i < instructions.Count; i++)
      {
        CilInstruction instruction = instructions[i];
        if (instruction.OpCode.Code != CilCode.Call || instruction.Operand is not IMethodDescriptor methodDescriptor)
          continue;

        // Resolve the method descriptor and check whether it was resolved properly.
        MethodDefinition? target = methodDescriptor.Resolve();
        if (target is null || target.Signature is null)
        {
          Logger.Error("CallObf", $"Failed to resolve method '{methodDescriptor.FullName}'.");
          continue;
        }

        // If the target method has a non-value return type, skip it.
        // TODO: Find a way to support all return types consistently.
        if (target.Signature.ReturnsValue && !target.Signature.ReturnType.IsValueType)
        {
          Logger.Warn("CallObf", $"Ignoring '{target.FullName}' (non-value return types are not supported yet)");
        }

        // Lookup the target method in the module by its metadata token and check whether it was found.
        if (!context.Module.TryLookupMember(target.MetadataToken, out IMetadataMember? targetMetadata) || targetMetadata is null)
          continue;

        // Add a local variable of type RuntimeMethodHandle to store the method handle in.
        CilLocalVariable runtimeMethodHandleLocal = new CilLocalVariable(runtimeMethodHandle);
        method.CilMethodBody!.LocalVariables.Add(runtimeMethodHandleLocal);

        // Replace the Call instruction with a bunch of instructions representing a Calli call.
        // The instructions further down roughly translate to the following code:
        //
        // calli(Signature, Parameters..., Function Pointer)
        //
        // The Function Pointer is retrieved by calling the following methods:
        //
        // typeof(<Module>).Module.ResolveMethod(MetadataToken).MethodHandle.GetFunctionPointer()
        //
        instruction.ReplaceWith(CilOpCodes.Ldtoken, context.Module.GetOrCreateModuleType());
        method.CilMethodBody!.Instructions.InsertRange(i + 1, new CilInstruction[]
        {
            new(CilOpCodes.Call, getTypeFromHandle),
            new(CilOpCodes.Callvirt, getModule),
            new(CilOpCodes.Ldc_I4, targetMetadata!.MetadataToken.ToInt32()),
            new(CilOpCodes.Call, resolveMethod),
            new(CilOpCodes.Callvirt, getMethodHandle),
            new(CilOpCodes.Stloc, runtimeMethodHandleLocal),
            new(CilOpCodes.Ldloca, runtimeMethodHandleLocal),
            new(CilOpCodes.Call, getFunctionPointer),
            new(CilOpCodes.Calli, target.Signature.MakeStandAloneSignature())
        });

        // Move the cursor to skip the inserted instructions.
        i += 9;
      }
    }
  }
}