using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewIntRule", menuName = "Abyssal Fall/Rules/IntRule")]
public class IntRule : BaseRule 
{
	public static implicit operator int(IntRule self)
	{
		return (int)self.Value;
	}
}
