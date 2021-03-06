﻿using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewEnumRule", menuName = "Abyssal Fall/Rules/EnumRule")]
public class EnumRule : BaseRule
{ 
	public static implicit operator int(EnumRule self)
	{
		return self._valueIndex;
	}

	public override string ToString()
	{
		return (string)Value;
	}
}
