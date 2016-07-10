using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
	public static CameraShake instance;

	public float shakeDuration = 0f;
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;


	private Vector3 _oldShakeOffset = Vector3.zero;
	private Vector3 _shakeOffset = Vector3.zero;
	private bool _isShaking;

	void OnEnable()
	{
		instance = this;
		_isShaking = false;
	}

	public void Shake (float duration)
	{
		_isShaking = true;
		shakeDuration = duration;
	}

	void Update()
	{

		//It may seem like trash code, but it is efficient.
		transform.position = transform.position - _oldShakeOffset;
		_oldShakeOffset = _shakeOffset;
		transform.position = transform.position + _shakeOffset;
		_shakeOffset = Vector3.zero;

		if (shakeDuration > 0 && _isShaking)
		{
			_shakeOffset = Random.insideUnitSphere * shakeAmount;
			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
		else if(_isShaking)
		{
			_isShaking = false;
			shakeDuration = 0f;
		}
	}
}
