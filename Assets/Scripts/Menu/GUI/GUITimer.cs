using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;


public class GUITimer : MonoBehaviour
{
	[System.Serializable]
	public class OnCompleteEvent : UnityEvent { }

	//private float                           _baseTimer;
	private float                           _currentTimer;
	private Coroutine                       _coroutine;
	//private bool                            _bIsWarning         = false;
	private Localizator.LocalizedText       _timerDisplay;

	public OnCompleteEvent                  OnCompleteCallback;
	public float                            LimitBeforeWarning;

	void Awake ()
	{
		_timerDisplay = GetComponent<Localizator.LocalizedText>();
	}

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
		IEnumerator warning = Warning_Implementation();
		//_baseTimer = timer;
		_currentTimer = timer;

		while (_currentTimer > 0)
		{
			_currentTimer -= TimeManager.DeltaTime;
			int currentTimeInt = Mathf.RoundToInt(_currentTimer);
			_timerDisplay.SetText(new KeyValuePair<string, string>("%NUMBER%", currentTimeInt.ToString()));

			if (_currentTimer <= LimitBeforeWarning)
			{
				warning.MoveNext();
			}

			yield return null;
		}

		OnCompleteCallback.Invoke();
	}

	IEnumerator Warning_Implementation ()
	{
		Color baseColor = _timerDisplay.Text.color;
		while (true)
		{
			_timerDisplay.Text.color = Color.Lerp(baseColor, Color.red, 1 - (_currentTimer % 1));
			_timerDisplay.transform.localScale = new Vector3(1 - (_currentTimer % 1), 1 - (_currentTimer % 1), 1 - (_currentTimer % 1));
			yield return null;
		}
	}
}
