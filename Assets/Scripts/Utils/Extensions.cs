using UnityEngine;
using System.Collections;

public static class Extensions
{
	public static float Reduce(this float number, float amount)
	{
		if (Mathf.Abs(number) < Mathf.Abs(amount))
			return 0;
		return Mathf.Sign(number) * (Mathf.Abs(number) - amount);
	}

	public static int Reduce(this int number, int amount)
	{
		if (Mathf.Abs(number) < Mathf.Abs(amount))
			return 0;
		return ((int)Mathf.Sign(number)) * (Mathf.Abs(number) - amount);
	}
}
