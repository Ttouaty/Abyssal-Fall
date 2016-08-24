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

public class InputCallback : MonoBehaviour
{
	public InputButton InputToListen;
	[Space()]
	public UnityEvent Callback;


	public InputMethod InputType;
	public float TimeToHold = 0.5f;
	public bool CanLoop = false;

	public bool ListenToAllJoysticks = true;
	public List<int> JoysticksToListen = new List<int>(); 
	
	private float _timeHeld;
	private bool _waitForRelease;
	private bool _isCallbackActivatedThisFrame = false;
	private bool _isInputDown = false;

	void Start()
	{
		if (InputToListen == null)
			Debug.LogWarning("InputCallback \""+gameObject.name+"\" doesn't listen any [Input].\nThe callback will never launch.");
		if(!ListenToAllJoysticks && JoysticksToListen.Count == 0)
			Debug.LogWarning("InputCallback \"" + gameObject.name + "\" doesn't listen any [Controller].\nThe callback will never launch.");

	}

	void Update()
	{
		_isCallbackActivatedThisFrame = false;
		if (ListenToAllJoysticks)
			TestInput();
		else
		{ 
			for(int i = 0; i < JoysticksToListen.Count; ++i)
			{
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
		{
			_isInputDown = InputManager.GetButtonHeld(InputToListen, joystickNumber);
		}

		if (_isInputDown && !_waitForRelease)
			_timeHeld += Time.deltaTime;
		else
			_timeHeld.Reduce(Time.deltaTime);

		if (_timeHeld > TimeToHold)
			LaunchCallback();
		
	}

	void LaunchCallback()
	{
		_isCallbackActivatedThisFrame = true;
		if (!CanLoop)
			_waitForRelease = true;
		_timeHeld = 0;
		Debug.Log("Callback");
		Callback.Invoke();
	}
}
