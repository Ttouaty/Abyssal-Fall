using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class Vector3Extensions
{
	public static Vector3 ZeroX(this Vector3 vect)
	{
		vect.x = 0;
		return vect;
	}

	public static Vector3 ZeroY(this Vector3 vect)
	{
		vect.y = 0;
		return vect;
	}

	public static Vector3 ZeroZ(this Vector3 vect)
	{
		vect.z = 0;
		return vect;
	}

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

public static class ListExtensions
{
	private static System.Random rng = new System.Random();

	public static void Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	public static T First<T> (this IList<T> list)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		else
		{
			return list[0];
		}
	}

	public static T Last<T> (this IList<T> list)
	{
		if(list.Count == 0)
		{
			return default(T);
		}
		else 
		{
			return list[list.Count - 1];
		}
	}

	public static T RandomElement<T>(this IList<T> list)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		else
		{
			int r = rng.Next(0, list.Count);
			return list[r];
		}
	}

	public static T ShiftRandomElement<T>(this IList<T> list)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		else
		{
			int r = rng.Next(0, list.Count);
			T el = list[r];
			list.RemoveAt(r);
			return el;
		}
	}

	public static void Add<T> (this IList<T> list, List<T> elements)
	{
		if(elements != null)
		{
			for (int i = 0; i < elements.Count; ++i)
			{
				list.Add(elements[i]);
			}
		}
	}

	public static void Add<T> (this IList<T> list, params T[] elements)
	{
		if(elements != null)
		{
			for (int i = 0; i < elements.Length; ++i)
			{
				list.Add(elements[i]);
			}
		}
	}
}
