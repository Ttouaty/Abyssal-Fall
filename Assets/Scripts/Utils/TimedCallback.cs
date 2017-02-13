using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TimedCallback : MonoBehaviour
{
	public float Interval = 1;
	public bool AutoStart = false;
	public bool CallOnStart = true;
	public bool Repeat = true;
	public UnityEvent Callback;

	private float eT = 0;
	private bool _isRunning = false;

	void Start()
	{
		_isRunning = AutoStart;
		if(CallOnStart)
			Callback.Invoke();
	}

	void Update()
	{
		if (!_isRunning)
			return;

		eT += Time.deltaTime;
		if(eT >= Interval)
		{
			Callback.Invoke();
			eT = 0;
			if (!Repeat)
				_isRunning = false;
		}
	}

	public void Activate()
	{
		if (CallOnStart && !_isRunning)
			Callback.Invoke();
		_isRunning = true;
	}

	public void Stop()
	{
		_isRunning = false;
		eT = 0;
	}
}
