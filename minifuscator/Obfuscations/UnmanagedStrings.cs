using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Cil;
using minifuscator.Utils;

namespace minifuscator.Obfuscations;

/// <summary>
/// Applies obfuscation of Ldstr instructions to the assembly.
/// </summary>
internal class UnmanagedStrings : ObfuscationBase
{
  public override int Priority => 0;

  public override void Execute()
  {
    ArgumentNullException.ThrowIfNull(Settings.StringObfuscation, nameof(Settings.StringObfuscation));

    // Ignore if string obfuscation is disabled.
    if (!Settings.StringObfuscation.Enabled)
      return;

    // Track the amount of obfuscated strings to determine whether mixed-mode execution should be enabled and logging.
    int amount = 0;

    // Import the string::.ctor(char*) constructor required for the obfuscation.
    IMethodDescriptor charPtrCtor = Module.Import(typeof(string).GetConstructor(new[] { typeof(char*) })!);

    // Go through all methods and replace all Ldstr instructions with a call to the deobfuscation method.
    foreach (Method method in Module.GetAllTypes().Where(x => !x.IsModuleType).GetAllMethods().Where(x => x.CilMethodBody is not null))
    {
      CilInstructionCollection instructions = method.CilMethodBody!.Instructions!;

      // Replace all short inline branches with normal branches because we will be inserting instructions,
      // which moves offsets causing short inline branches to break if the offset is too large.
      instructions.ReplaceShortInlineBranches();

      // Go through all instructions and replace the Ldstr instructions with a call to the deobfuscation method.
      for (int i = 0; i < instructions.Count; i++)
      {
        CilInstruction instruction = instructions![i];
        if (instruction.OpCode.Code != CilCode.Ldstr)
          continue;

        // Get the native method for the string, add it to the module and replace the instruction with a call to it.
        Method native = NativeUtils.GetNativeStringMethod($"{instruction.Operand}\0", Module.CorLibTypeFactory);
        Module.GetOrCreateModuleType().Methods.Add(native);
        instruction.ReplaceWith(CilOpCodes.Call, native);

        // Call the string::.ctor(char*) constructor to turn the char* loaded onto the stack into a string.
        instructions.Insert(++i, new CilInstruction(CilOpCodes.Newobj, charPtrCtor));

        amount++;
      }
    }

    // If strings were obfuscated, enable mixed-mode execution in the module.
    if (amount > 0)
      Module.Attributes &= ~DotNetDirectoryFlags.ILOnly;

    Logger.Success("StrObf", $"Obfuscated {amount} strings.");
  }
}
