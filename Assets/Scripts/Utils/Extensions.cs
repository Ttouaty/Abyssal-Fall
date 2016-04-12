using UnityEngine;
using System.Collections;

public static class Extensions
{
	public static float Reduce(this float number, float amount)
	{
		return Mathf.Sign(number) * (Mathf.Abs(number) - amount);
	}

	public static int Reduce(this int number, int amount)
	{
		return ((int)Mathf.Sign(number)) * (Mathf.Abs(number) - amount);
	}
}
