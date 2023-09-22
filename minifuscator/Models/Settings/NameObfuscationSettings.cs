using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace minifuscator.Models.Settings;

/// <summary>
/// Represents the settings for the name obfuscation.
/// </summary>
public class NameObfuscationSettings
{
  /// <summary>
  /// Bool whether name obfuscation is being executed.
  /// </summary>
  public bool Enabled { get; set; } = false;

  /// <summary>
  /// The length of the randomly generated names.
  /// </summary>
  public int Length { get; set; } = 32;

  /// <summary>
  /// The character set to use for the randomly generated names.
  /// </summary>
  public char[] CharSet { get; set; } = Enumerable.Range(1, ushort.MaxValue).Select(x => (char)x).ToArray();

  /// <summary>
  /// Bool whether module names should be obfuscated.
  /// </summary>
  public bool Module { get; set; } = false;

  /// <summary>
  /// Bool whether namespace names should be obfuscated.
  /// Note: Consistency between types in a namespaces is ensured.
  /// </summary>
  public bool Namespaces { get; set; } = false;

  /// <summary>
  /// Bool whether type names should be obfuscated.
  /// </summary>
  public bool Types { get; set; } = false;

  /// <summary>
  /// Bool whether method names should be obfuscated.
  /// </summary>
  public bool Methods { get; set; } = false;

  /// <summary>
  /// Bool whether parameter names should be obfuscated.
  /// </summary>
  public bool Parameters { get; set; } = false;

  /// <summary>
  /// Bool whether property names should be obfuscated.
  /// </summary>
  public bool Properties { get; set; } = false;

  /// <summary>
  /// Bool whether field names should be obfuscated.
  /// </summary>
  public bool Fields { get; set; } = false;

  /// <summary>
  /// Bool whether event names should be obfuscated.
  /// </summary>
  public bool Events { get; set; } = false;
}
