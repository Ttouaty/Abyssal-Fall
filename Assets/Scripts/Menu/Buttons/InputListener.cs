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

	//protected int[] JoysticksToListen = new int[12]; 
	
	public UnityEventInt Callback;
	
	protected float _timeHeld;
	private bool[] _waitForRelease = new bool[12];
	private bool _isCallbackActivatedThisFrame = false;
	private bool _isInputDown = false;

	protected virtual void Start()
	{
		//JoysticksToListen = new int[12];
		if (UseAxis && directionToListen.magnitude == 0)
			Debug.LogError("No axis set for inputListener: "+gameObject.name);
	}

	protected virtual void Update()
	{
		_isCallbackActivatedThisFrame = false;
		string[] tempStringArray = Input.GetJoystickNames();
		for (int i = -1; i < tempStringArray.Length; ++i)
		{
			if (i != -1)
			{
				if (tempStringArray[i] == "")
					continue;
			}
			TestInput(i + 1);
		}
	}

	void TestInput(int joystickNumber) 
	{ 
		if(_isCallbackActivatedThisFrame)
			return;

		if (UseAxis)
			TestAxis(joystickNumber);
		else
			TestButton(joystickNumber);
	}

	private Vector2 _tempStickPosition;
	void TestAxis(int joystickNumber)
	{
		_tempStickPosition = InputManager.GetStickDirection(joystickNumber);
		if (_waitForRelease[joystickNumber])
		{
			if (_tempStickPosition.magnitude < stickActivateThreshold - 0.2)
			{
				_waitForRelease[joystickNumber] = false;
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
				_waitForRelease[joystickNumber] = true;
				LaunchCallback(joystickNumber);
			}
		}
		else
		{
			_timeHeld = -1;
		}

	}

	void TestButton(int joystickNumber)
	{
		if (_waitForRelease[joystickNumber])
			_waitForRelease[joystickNumber] = !InputManager.GetButtonUp(InputToListen, joystickNumber);

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
			if (!_waitForRelease[joystickNumber])
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
			_waitForRelease[joy] = true;
		_timeHeld = 0;
		
		Callback.Invoke(new JoystickNumber(joy));
	}

	void OnEnable()
	{
		_isInputDown = false;
		_timeHeld = 0;
		for (int i = 0; i < _waitForRelease.Length; i++)
		{
			_waitForRelease[i] = false;
		}
	}
}

[Serializable]
public class UnityEventInt : UnityEvent<JoystickNumber> { }
public class JoystickNumber
{
	private int value;
	public JoystickNumber(int a)
	{
		value = a;
	}

	public static implicit operator int(JoystickNumber self)
	{
		return self.value;
	}
}