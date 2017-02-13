using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewIntRule", menuName = "Abyssal Fall/Rules/IntRule")]
public class IntRule : BaseRule 
{
	public static implicit operator int(IntRule self)
	{
		return Int32.Parse(self.Value);
	}
}
