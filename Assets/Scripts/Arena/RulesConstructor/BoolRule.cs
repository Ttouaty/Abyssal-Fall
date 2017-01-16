using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "NewBoolRule", menuName = "Abyssal Fall/Rules/BoolRule")]
public class BoolRule : BaseRule
{
	public static implicit operator bool(BoolRule self)
	{
		return Convert.ToBoolean(self.Value);
	}
}
