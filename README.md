<div align="center">
<img width=1850 height=75 src="https://raw.githubusercontent.com/minisbett/minifuscator/master/.github/assets/logo.png" />

#
An open-source .NET obfuscation tool, fully documented and comprehensible.

**No 1:1 stolen code** (*unlike 99.9% of all .NET obfuscation repositories on GitHub*)</br>
For more info, refer to the [credits section](#credits) of this README.

[Features](#features) • [To-Do](#todo) • [How does it work?](#how-does-minifuscator-work)</br>
[Credits](#credits) • [Licenses](#licenses)
</div>

<div align="center">
<i>Made with ❤️ by minisbett</i>
</div>

# How does minifuscator work?

```cs
throw new NotImplementedException();
```

# How effective are .NET obfuscations tools?

The concept that such obfuscators are based on is called **security by obscurity**. It relies on the idea that attack vectors are secret or hidden. It works great as an extra security layer that works well with other security methods. It reduces the chances of a violant act against the application to be successful. Since .NET/MSIL/C# is managed by a runtime and contains lots of info even when compiled through the C# compiler, obfuscation of .NET assemblies is not necessarily always effective, especially not as a stand-alone solution for securing the application.

It definitely does it's job of, depending on the complexity of obfuscation, making it hard for people to understand what is going on. This can range from newbies or less tech-saavy people trying to tamper with your code or understand some functionality to complex obfuscations, like control flow obfuscation. Ultimately, the goal of a .NET obfuscation tool is to make the managed code as unreadable/reverse-engineerable and cryptic/obscure as possible, while looking to maintain a degree of stability and performance. Depending on the target application it is not always recommended or simply does not work to just throw all obfuscations/obfuscators you can find on it.

# Features

**Renamer** - Replaces all namespace, type, method, parameter, property, field and event names with random strings. Names are kept consistently random, meaning types share the same namespaces and overriden definitions have the same name as the original one. This is desired because 1. The application may rely on reflection (see [how to handle reflection](#how-to-handle-reflection)) and 2. The JIT compiler will fail to find an overriden definition.

**UnamangedStrings**/**UnmanagedNumbers** - Replaces all string/number constants (Ldstr, Ldc_I4/8, Ldc_R4/8) with calls to unmanaged functions. They are still *somewhere* inside the PE file, although no longer easily readable using a decompiler like dnSpy.

**CallToCalli** - Replaces all method calls with an indirect call instruction, getting the function pointer from the metadata token. Not implemented yet as there are problems with porting the code from [BitMono](https://github.com/sunnamed434/BitMono), which is targetting Mono, to CoreCLR.

**RandomizeModuleGuids** - Randomizes the Mvid (module version id), EncId (unique edit-and-continue generation id) and EncBaseId (base edit-and-continue generation id).

# ToDo

```
[ ] Fix CallToCalli (from BitMono), which is flawed and does not work well (potentially because BitMono targets Mono?)
[ ] Refactor handling of settings to be more modular
    [ ] Find a solution for preventing the Enabled checks at the start of each obfuscation
[ ] Implement anti-debugging mechanisms to run inside the application lifecycle
[ ] Implement some entry point obfuscation via TLS callbacks/DllMain/COR20 entry points
```

# How to handle reflection with renamer protection

A lot of applications utilize the `System.Reflection` namespace, for example to dynamically process classes as modules. In these scenarios, they rely on specifying a namespace, a method or any other definition for performing the lookup.
If the **Renamer** protection is applied on the assembly, it effectively breaks those lookups **in case you are specifying those definitions via a string**.

There are better ways to do it, providing more resilience and eliminating that problem. Here are some examples:
```cs
// Assuming "MyClass" is the class containing this code and "TestModule" is a module inside the modules namespace

// Bad
IEnumerable<Type> types = typeof(MyClass).Assembly.GetTypes().Where(x => x.Namespace is string ns && ns == "MyApp.Modules");
MethodInfo? method = typeof(MyOtherClass).GetMethod("Fetch");

// Good
IEnumerable<Type> types = typeof(Program).Assembly.GetTypes().Where(x => x.Namespace == typeof(TestModule).Namespace);
MethodInfo? method = typeof(MyOtherClass).GetMethod(nameof(MyOtherClass.Fetch));
```

# Credits

https://github.com/MrakDev/UnmanagedString - The idea for unmanaged constants (e.g. strings), which was adapted for more resilience and more data types here

https://github.com/sunnamed434/BitMono - The implementation of CallToCalli, although not fully working there and not fully implemented yet here, needs to be fixed first (see [todo](#todo))

# Licenses
