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

// most fields a public because of custom editor, I need to check how to fix this :p
public class InputListener : MonoBehaviour
{
	private float stickActivateThreshold = 0.7f;

	public bool UseAxis = false;
	public Vector2 directionToListen = new Vector2(1, 0);
	public float AxisPrecision = 0.5f;

	public InputEnum InputToListen;

	public InputMethod InputMethodUsed;
	public float TimeToHold = 0.5f;
	public bool CanLoop = false;

	public bool ListenToAllJoysticks = true;
	public int[] JoysticksToListen = new int[12]; 
	
	public UnityEvent Callback;
	
	protected float _timeHeld;
	private bool[] _waitForRelease = new bool[13];
	private bool _isCallbackActivatedThisFrame = false;
	private bool _isInputDown = false;

	protected virtual void Start()
	{
		if (!ListenToAllJoysticks && JoysticksToListen.Length == 0) 
			JoysticksToListen = new int[12];
		if (UseAxis && directionToListen.magnitude == 0)
			Debug.LogError("No axis set for inputListener: "+gameObject.name);
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
				if (!JoysticksToListen[i].Equals(null))
					TestInput(JoysticksToListen[i]);
			}
		}
	}

	void TestInput(int joystickNumber = -1) 
	{ 
		if(_isCallbackActivatedThisFrame)
			return;

		if (UseAxis)
			TestAxis(joystickNumber);
		else
			TestButton(joystickNumber);
	}

	private Vector2 _tempStickPosition;
	void TestAxis(int joystickNumber = -1)
	{
		_tempStickPosition = InputManager.GetStickDirection(joystickNumber);
		if (_waitForRelease[1 +joystickNumber])
		{
			if (_tempStickPosition.magnitude < stickActivateThreshold - 0.2)
			{
				_waitForRelease[1 +joystickNumber] = false;
				_timeHeld = 0;
			} 
		}
		else if (directionToListen.AnglePercent(_tempStickPosition) > AxisPrecision && _tempStickPosition.magnitude > stickActivateThreshold)
		{
			if (CanLoop)
			{
				_timeHeld -= Time.deltaTime;
				if (_timeHeld < 0)
				{ 
					LaunchCallback(joystickNumber);
					_timeHeld = TimeToHold;
				}

			}
			else {
				_waitForRelease[1 +joystickNumber] = true;
				LaunchCallback(joystickNumber);
			}
		}

	}

	void TestButton(int joystickNumber = -1)
	{
		if (_waitForRelease[1 +joystickNumber])
			_waitForRelease[1 +joystickNumber] = !InputManager.GetButtonUp(InputToListen, joystickNumber);

		if (InputMethodUsed == InputMethod.Down)
		{
			if (InputManager.GetButtonDown(InputToListen, joystickNumber))
				LaunchCallback(joystickNumber);
		}
		else if (InputMethodUsed == InputMethod.Up)
		{
			if (InputManager.GetButtonUp(InputToListen, joystickNumber))
				LaunchCallback(joystickNumber);
		}
		else
			_isInputDown = InputManager.GetButtonHeld(InputToListen, joystickNumber);

		if (_isInputDown)
		{
			if (!_waitForRelease[1 +joystickNumber])
				_timeHeld += Time.deltaTime;
		}
		else
			_timeHeld = _timeHeld.Reduce(Time.deltaTime);

		if (_timeHeld > TimeToHold)
			LaunchCallback(joystickNumber);
	}

	protected virtual void LaunchCallback(int joy)
	{
		_isCallbackActivatedThisFrame = true;
		if (!CanLoop)
			_waitForRelease[1 +joy] = true;
		_timeHeld = 0;
		
		Callback.Invoke();
	}

	void OnEnable()
	{
		_isInputDown = false;
		_timeHeld = 0;
		for (int i = 0; i < 12; i++)
		{
			_waitForRelease[1 +i] = false;
		}
	}
}
