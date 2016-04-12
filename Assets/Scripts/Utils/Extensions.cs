using UnityEngine;
using System.Collections;

public static class Extensions
{
	public static float Reduce(this float number, float amount)
	{
		if (Mathf.Sign((Mathf.Abs(number) - amount)) != Mathf.Sign(number))
			return 0;
		return Mathf.Sign(number) * (Mathf.Abs(number) - amount);
	}

	public static int Reduce(this int number, int amount)
	{
		if (Mathf.Sign((Mathf.Abs(number) - amount)) != Mathf.Sign(number))
			return 0;
		return ((int)Mathf.Sign(number)) * (Mathf.Abs(number) - amount);
	}
}
