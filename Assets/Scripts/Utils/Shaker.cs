using UnityEngine;
using System.Collections;

public class Shaker : MonoBehaviour
{
	public static Shaker instance;

	void OnEnable()
	{
		instance = this;
	}

	public void Shake(GameObject target, float duration, float shakeAmount = 0.7f, float decreaseFactor = 1.0f, bool bEraseAllShakes = true)
	{
		StopAllCoroutines();
		StartCoroutine(OnShake(target, duration, shakeAmount, decreaseFactor));
	}

	IEnumerator OnShake(GameObject target, float duration, float shakeAmount, float decreaseFactor)
	{
		Vector3 originalPosition = target.transform.localPosition;
		while(duration > 0)
		{
			transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
			duration -= TimeManager.DeltaTime * decreaseFactor;
			yield return null;
		}
		transform.localPosition = originalPosition;
	}
}
