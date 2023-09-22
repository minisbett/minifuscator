using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minifuscator.Models.Settings;

/// <summary>
/// Represents the settings for the number obfuscation.
/// </summary>
public class NumberObfuscationSettings
{
    /// <summary>
    /// Bool whether number obfuscation is being executed.
    /// </summary>
    public bool Enabled { get; set; } = false;
}
