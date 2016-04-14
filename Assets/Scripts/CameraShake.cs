using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
	public static CameraShake instance;

	public float shakeDuration = 0f;
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;

	private Vector3 _originalPos;
	private bool _isShaking;

	void OnEnable()
	{
		instance = this;
		_isShaking = false;
		_originalPos = transform.localPosition;
	}

	public void Shake (float duration)
	{
		_isShaking = true;
		shakeDuration = duration;
	}

	void Update()
	{
		if (shakeDuration > 0 && _isShaking)
		{
			transform.localPosition = _originalPos + Random.insideUnitSphere * shakeAmount;
			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
		else if(_isShaking)
		{
			_isShaking = false;
			shakeDuration = 0f;
			transform.localPosition = _originalPos;
		}
	}
}
