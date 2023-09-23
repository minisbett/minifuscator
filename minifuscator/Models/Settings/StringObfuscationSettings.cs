﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minifuscator.Models.Settings;

/// <summary>
/// Represents the settings for the string obfuscation.
/// </summary>
public class StringObfuscationSettings
{
    /// <summary>
    /// Bool whether string obfuscation is being executed.
    /// </summary>
    public bool Enabled { get; set; } = false;
}