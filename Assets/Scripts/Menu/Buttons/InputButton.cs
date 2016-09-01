using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public enum InputMethod
{ 
	Down,
	Up,
	Held
}

public class InputButton : MonoBehaviour
{
	public InputEnum InputToListen;
	[Space()]
	public UnityEvent Callback;

	public InputMethod InputType;
	public float TimeToHold = 0.5f;
	public bool CanLoop = false;

	public bool ListenToAllJoysticks = true;
	public int[] JoysticksToListen = new int[12]; 
	
	protected float _timeHeld;
	private bool _waitForRelease;
	private bool _isCallbackActivatedThisFrame = false;
	private bool _isInputDown = false;

	protected virtual void Start()
	{
		if (InputToListen == null)
			Debug.LogWarning("InputButton \""+gameObject.name+"\" doesn't listen to any [Input].\nThe callback will never launch.");
		if (!ListenToAllJoysticks && JoysticksToListen.Length == 0) { 
			//Debug.LogWarning("InputButton \"" + gameObject.name + "\" doesn't listen to any [Controller].");
			JoysticksToListen = new int[12];
		}

	}

	protected virtual void Update()
	{
		_isCallbackActivatedThisFrame = false;
		if (ListenToAllJoysticks)
			TestInput();
		else
		{ 
			for(int i = 0; i < JoysticksToListen.Length; ++i)
			{
				if (JoysticksToListen[i] != null)
					TestInput(JoysticksToListen[i]);
			}
		}
	}

	void TestInput(int joystickNumber = -1) 
	{ 
		if(_isCallbackActivatedThisFrame)
			return;

		if (_waitForRelease)
			_waitForRelease = !InputManager.GetButtonUp(InputToListen, joystickNumber);

		if (InputType == InputMethod.Down)
		{
			if (InputManager.GetButtonDown(InputToListen, joystickNumber))
				LaunchCallback();
		}
		else if (InputType == InputMethod.Up)
		{
			if (InputManager.GetButtonUp(InputToListen, joystickNumber))
				LaunchCallback();
		}
		else
			_isInputDown = InputManager.GetButtonHeld(InputToListen, joystickNumber);

		if (_isInputDown)
		{ 
			if(!_waitForRelease)
				_timeHeld += Time.deltaTime;
		}
		else
			_timeHeld = _timeHeld.Reduce(Time.deltaTime);

		if (_timeHeld > TimeToHold)
			LaunchCallback();
		
	}

	protected virtual void LaunchCallback()
	{
		_isCallbackActivatedThisFrame = true;
		if (!CanLoop)
			_waitForRelease = true;
		_timeHeld = 0;
		
		Callback.Invoke();
	}

	void OnEnable()
	{
		_isInputDown = false;
		_timeHeld = 0;
		_waitForRelease = false;
	}
}
