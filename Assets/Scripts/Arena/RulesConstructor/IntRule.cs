using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewIntRule", menuName = "Abyssal Fall/Rules/IntRule")]
public class IntRule : BaseRule 
{
	public static implicit operator int(IntRule self)
	{
		return Int32.Parse(self.Value);
	}

	public override string ToString()
	{
		if(StringFormat == "0:00")
		{
			int secs = int.Parse(Value) % 60;
			int min = int.Parse(Value) / 60;

			return min + ":" + secs.ToString("00");
		}

		return base.ToString();
	}
}
