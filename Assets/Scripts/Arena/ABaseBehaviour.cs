using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public struct BehaviourConfiguration
{
	public ABaseBehaviour Behaviour;
	public float Frequency;
}

public abstract class ABaseBehaviour : MonoBehaviour
{
	[Tooltip("Frequency of the behaviour, in seconds")]
	[HideInInspector] public float  Frequency       = 1.0f;
	[HideInInspector] public bool   Active          = false;

	private Coroutine               _coroutine;

	public void Run()
	{
		if(_coroutine != null)
		{
			Stop();
		}
		_coroutine = StartCoroutine(Run_Update());
		Active = true;
	}

	public void Stop ()
	{
		if (_coroutine != null)
		{
			StopCoroutine(_coroutine);
			_coroutine = null;
		}
		Active = false;
	}

	private IEnumerator Run_Update()
	{
		while(true)
		{
			yield return new WaitForSeconds(Frequency);
			StartCoroutine(Run_Implementation());
		}
	}

	protected abstract IEnumerator Run_Implementation();
}