using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Reflection;

public enum Axis { x, y, z }

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

	public static Vector3 Multiply(this Vector3 vect, Axis designatedAxis, float value)
	{
		if (designatedAxis == Axis.x)
			vect.x *= value;
		else if (designatedAxis == Axis.y)
			vect.y *= value;
		else
			vect.z *= value;
		return vect;
	}

	public static Vector3 Add(this Vector3 vect, Axis designatedAxis, float value)
	{
		if (designatedAxis == Axis.x)
			vect.x += value;
		else if (designatedAxis == Axis.y)
			vect.y += value;
		else
			vect.z += value;
		return vect;
	}
	/// <summary>
	/// Reduces a float's absolute value by ammount & cap at 0 ( -10.Reduce(3); == -7 )
	/// </summary>
	public static float Reduce(this float number, float amount)
	{
		if (Mathf.Abs(number) < Mathf.Abs(amount))
			return 0;
		return Mathf.Sign(number) * (Mathf.Abs(number) - amount);
	}

	public static float HighestAxis(this Vector3 vect)
	{
		return Mathf.Max(Mathf.Abs(vect.x), Mathf.Abs(vect.y), Mathf.Abs(vect.z));
	}

	public static float GravityTime(this float targetSpeed, float gravityModifier = 9.81f)
	{
		return targetSpeed / gravityModifier;
	}

	public static float AnglePercent(this Vector3 vect, Vector3 target)
	{
		float angle = Vector3.Angle(vect, target);

		if (vect.magnitude == 0 || target.magnitude == 0)
			return 0;

		if (angle > 90)
			return Mathf.InverseLerp(90, 180, angle) * -1;
		else
			return Mathf.InverseLerp(90, 0, angle);
	}

	public static float AnglePercent(this Vector2 vect, Vector2 target)
	{
		float angle = Vector2.Angle(vect, target);

		if (vect.magnitude == 0 || target.magnitude == 0)
			return 0;

		if (angle > 90)
			return Mathf.InverseLerp(90, 180, angle) * -1;
		else
			return Mathf.InverseLerp(90, 0, angle);
	}

	public static Vector3 SetAxis(this Vector3 vect, Axis designatedAxis, float value)
	{
		if (designatedAxis == Axis.x)
			vect.x = value;
		else if (designatedAxis == Axis.y)
			vect.y = value;
		else
			vect.z = value;
		return vect;
	}


}

public static class IntExtensions
{
	/// <summary>
	/// Reduces a int's absolute value by ammount & cap at 0 ( -10.Reduce(3); == -7 )
	/// </summary>
	public static int Reduce(this int number, int amount)
	{
		if (Mathf.Abs(number) < Mathf.Abs(amount))
		{
			number = 0;
			return 0;
		}
		number = ((int)Mathf.Sign(number)) * (Mathf.Abs(number) - amount);
		return number;
	}

	public static int LoopAround(this int number, int min, int max)
	{
		if (number > max)
			number = number - max + min - 1;
		else if (number < min)
			number = number - min + max + 1;
		return number;
	}

	public static float Percentage(this int number, float min, float max, float maxPercent = 1f)
	{
		return Mathf.InverseLerp(min, max, number) * maxPercent;
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

	public static T First<T>(this IList<T> list)
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

	public static T Last<T>(this IList<T> list)
	{
		if (list.Count == 0)
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

	public static void Add<T>(this IList<T> list, List<T> elements)
	{
		if (elements != null)
		{
			for (int i = 0; i < elements.Count; ++i)
			{
				list.Add(elements[i]);
			}
		}
	}

	public static void Add<T>(this IList<T> list, params T[] elements)
	{
		if (elements != null)
		{
			for (int i = 0; i < elements.Length; ++i)
			{
				list.Add(elements[i]);
			}
		}
	}
}

public static class Curves
{
	public static AnimationCurve EaseInOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	public static AnimationCurve LinearCurve = AnimationCurve.Linear(0, 0, 1, 1);
}

public static class MonoBehaviourExtensions
{
	private static AnimationCurve _easeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	public static Transform DestroyAllChildren(this Transform transform)
	{
		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		return transform;
	}

	public static Transform ResetPosNRot(this Transform transform, bool local = true)
	{
		if(local)
		{
			transform.localRotation = Quaternion.identity;
			transform.localPosition = Vector3.zero;
		}
		else
		{
			transform.rotation = Quaternion.identity;
			transform.position = Vector3.zero;
		}

		return transform;
	}

	public static void MoveTo(this MonoBehaviour objectMoved, Vector3 target, float time, bool local = false)
	{
		objectMoved.StartCoroutine(MoveCoroutine(objectMoved, target, time, local));
	}

	static IEnumerator MoveCoroutine(MonoBehaviour target, Vector3 end, float time, bool local = false)
	{
		float eT = 0;

		Vector3 start;
		if (local)
			start = target.transform.localPosition;
		else
			start = target.transform.position;
		while (eT < time)
		{
			eT += Time.deltaTime;
			if (local)
				target.transform.localPosition = Vector3.Lerp(start, end, _easeOutCurve.Evaluate(eT / time));
			else
				target.transform.position = Vector3.Lerp(start, end, _easeOutCurve.Evaluate(eT / time));

			yield return null;
		}

		if (local)
			target.transform.localPosition = end;
		else
			target.transform.position = end;
	}

	public static void FadeDestroy(this MonoBehaviour target, float time)
	{
		target.StartCoroutine(FadeNDestroy(target, time));
	}

	static IEnumerator FadeNDestroy(MonoBehaviour target, float time)
	{
		Material objMat = target.GetComponentInChildren<Renderer>().material;
		if (!objMat.IsKeywordEnabled("_ALPHATEST_ON") && !objMat.IsKeywordEnabled("_ALPHABLEND_ON") && !objMat.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON"))
		{
			Debug.LogWarning("Object must have a transparency using rendering mode for FadeDestroy() to work!  \n(cutout, fade, transparent would work). Canceling!");
			yield break;
		}
		float eT = 0;
		Color startColor = objMat.color;
		Color targetColor = objMat.color;
		targetColor.a = 0;

		while (eT < time)
		{
			eT += Time.deltaTime;

			objMat.color = Color.Lerp(startColor, targetColor, eT / time);
			yield return null;
		}

		MonoBehaviour.Destroy(target.gameObject);
	}

	public static T GetCopyOf<T>(this Component comp, T other) where T : Component
	{
		Type type = comp.GetType();
		if (type != other.GetType()) return null; // type mis-match
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
		PropertyInfo[] pinfos = type.GetProperties(flags);
		foreach (var pinfo in pinfos)
		{
			if (pinfo.CanWrite)
			{
				try
				{
					pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
				}
				catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
			}
		}
		FieldInfo[] finfos = type.GetFields(flags);
		foreach (var finfo in finfos)
		{
			finfo.SetValue(comp, finfo.GetValue(other));
		}
		return comp as T;
	}

	public static T CopyComponentFrom<T>(this GameObject go, T toAdd) where T : Component
	{
		return go.AddComponent<T>().GetCopyOf(toAdd) as T;
	}
}

public static class NetworkBehaviourExtensions
{
	[ClientRpc]
	public static void RpcSetNetworkParent(this NetworkBehaviour child, GameObject networkedTargetGo)
	{
		child.transform.SetParent(networkedTargetGo.transform);
	}
}

public static class ButtonExtensions
{
	public static void SelfClick(this Button target)
	{
		target.onClick.Invoke();
	}
}


public static class CanvasGroupExtension
{
	public static void CrossFadeAlpha(this CanvasGroup target, float alpha, float time)
	{
		if (target.GetComponent<MonoBehaviour>() != null)
			target.GetComponent<MonoBehaviour>().StartCoroutine(CrossFadeAlphaCoroutine(target, alpha, time));
		else
			MainManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(CrossFadeAlphaCoroutine(target, alpha, time));

	}

	static IEnumerator CrossFadeAlphaCoroutine(CanvasGroup target, float alpha, float time)
	{
		float eT = 0;

		float startAlpha = target.alpha;
		while (eT < time)
		{
			eT += Time.deltaTime;
			target.alpha = Mathf.Lerp(startAlpha, alpha, eT / time);
			yield return null;
		}
		target.alpha = alpha;
	}
}

public static class AnimatorExtensions
{
	public static void SetTriggerAfterInit(this Animator target, string triggerName)
	{
		MainManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(SetTriggerAfterInitCoroutine(target, triggerName));
	}

	static IEnumerator SetTriggerAfterInitCoroutine(Animator target, string triggerName)
	{
		while (target != null)
		{
			if (target.isInitialized && target.gameObject.activeInHierarchy)
				break;
			yield return null;
		}

		if (target != null)
			target.SetTrigger(triggerName);
	}

	public static void SetBoolAfterInit(this Animator target, string boolName, bool active)
	{
		MainManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(SetBoolAfterInitCoroutine(target, boolName, active));
	}

	static IEnumerator SetBoolAfterInitCoroutine(Animator target, string boolName, bool active)
	{
		while (target != null)
		{
			if (target.isInitialized && target.gameObject.activeInHierarchy)
				break;
			yield return null;
		}

		if (target != null)
			target.SetBool(boolName, active);
	}

	public static void SetIntAfterInit(this Animator target, string intName, int value)
	{
		MainManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(SetIntAfterInitCoroutine(target, intName, value));
	}

	static IEnumerator SetIntAfterInitCoroutine(Animator target, string intName, int value)
	{
		while (target != null)
		{
			if (target.isInitialized && target.gameObject.activeInHierarchy)
				break;
			yield return null;
		}

		if (target != null)
			target.SetInteger(intName, value);
	}

	public static void SetFloatAfterInit(this Animator target, string floatName, float value)
	{
		MainManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(SetFloatAfterInitCoroutine(target, floatName, value));
	}

	static IEnumerator SetFloatAfterInitCoroutine(Animator target, string floatName, float value)
	{
		while (target != null)
		{
			if (target.isInitialized && target.gameObject.activeInHierarchy)
				break;
			yield return null;
		}

		if (target != null)
			target.SetFloat(floatName, value);
	}

	public static void BroadCastTrigger(this NetworkAnimator targetAnimator, string triggerName)
	{
		targetAnimator.SetTrigger(triggerName);
		if (NetworkServer.active)
			targetAnimator.animator.ResetTrigger(triggerName);
	}
}

public static class ColorExtensions
{
	public static Color SetAlpha(this Color target, float a)
	{
		target.a = a;
		return target;
	}
}
