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

	private ArenaConfiguration_SO   _configuration;
	private Coroutine               _coroutine;

	protected virtual void Start()
	{
		_configuration = MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig;
	}

	public void Run()
	{
		if(_coroutine != null)
		{
			Stop();
		}
		_coroutine = StartCoroutine(Run_Update());
		Active = true;
	}

	public void Stop()
	{
		if(_coroutine != null)
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