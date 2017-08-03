using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GUITimer : MonoBehaviour
{
	[System.Serializable]
	public class OnCompleteEvent : UnityEvent { }

	//private float                           _baseTimer;
	private float                           _currentTimer;
	private Coroutine                       _coroutine;
	//private bool                            _bIsWarning         = false;
	public Text								TimerDisplay;
	public GameObject						FinalTimer;

	public OnCompleteEvent                  OnCompleteCallback;
	public float                            LimitBeforeWarning;

	public void Stop ()
	{
		//_baseTimer = 0;
		_currentTimer = 0;
		if(_coroutine != null)
		{
			StopCoroutine(_coroutine);
		}
	}

	public void Run (float timer)
	{
		_coroutine = StartCoroutine(Run_Implementation(timer));
	}

	IEnumerator Run_Implementation (float timer)
	{
		bool warningActivated = false;
		FinalTimer.GetComponentInChildren<Animator>().SetTrigger("Reset");
		TimerDisplay.gameObject.SetActive(true);
		_currentTimer = timer;

		while (_currentTimer > 0)
		{
			_currentTimer -= TimeManager.DeltaTime;
			TimerDisplay.text = Mathf.Floor(_currentTimer / 60).ToString("00") + ":" + (_currentTimer % 60).ToString("00.00");

			if (_currentTimer <= LimitBeforeWarning && !warningActivated)
			{
				TimerDisplay.gameObject.SetActive(false);
				warningActivated = true;
				FinalTimer.GetComponentInChildren<Animator>().SetTrigger("Warn");
			}

			yield return null;
		}

		OnCompleteCallback.Invoke();
	}
}
