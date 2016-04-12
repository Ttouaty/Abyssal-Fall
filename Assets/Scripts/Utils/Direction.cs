using UnityEngine;
using System.Collections;

public enum EDirection
{
	FORWARD,
	RIGHT,
	BACK,
	LEFT
}

public static class Direction
{
	public static EDirection GetRandomDirection ()
	{
		return (EDirection)Random.Range(0, 3);
	}
}
