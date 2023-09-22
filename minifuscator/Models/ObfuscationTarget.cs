using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minifuscator.Models;

/// <summary>
/// Represents the input and output assembly of the obfuscation, as well as information about them.
/// </summary>
public class ObfuscationTarget
{
    /// <summary>
    /// The file path to the input assembly to be obfuscated.
    /// </summary>
    public required string Input { get; init; }

    /// <summary>
    /// The file path to the output assembly to be written. This can be the same as the input assembly and will overwrite it.
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// Bool whether the input assembly is an AppHost bundle.
    /// </summary>
    public bool IsAppHostBundle { get; init; } = false;
}
