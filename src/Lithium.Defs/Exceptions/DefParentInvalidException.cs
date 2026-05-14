using System;

namespace Lithium.Defs.Exceptions;

/// <summary>
/// Exception indicating that a Def's parent as defined in XML is valid.
/// This can be due to self-reference or type mismatch.
/// </summary>
public class DefParentInvalidException(string defName, Type defType, string parentName, Type parentType) : Exception {
	private readonly string defName = defName;
	private readonly Type defType = defType;
	private readonly string parentName = parentName;
	private readonly Type parentType = parentType;

	/// <summary>
	/// Message describing the error.
	/// </summary>
	public override string Message {
		get {
			if (defName.Equals(parentName)) {
				return "A def cannot be its own parent.";
			}
			else if (!defType.Equals(parentType)) {
				return $"Def '{parentName}' ({parentType}) cannot be a parent of '{defName}' ({defType}).";
			}
			else {
				return "Parent invalid.";
			}
		}
	}
}
