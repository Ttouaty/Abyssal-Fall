using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public enum InputEnum
{
	A = 0,
	B = 1,
	X = 2,
	Y = 3,
	LB = 4,
	RB = 5,
	Select = 6,
	Start = 7
}

public class InputManager : GenericSingleton<InputManager>
{
	//  Buttons						0		1		 2			3		4		5		6		7
	private string[] InputNames = { "Dash", "Cancel", "Special", null, null, null, "Select", "Start" };
	private KeyCode[] KeyboardControls = { KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.None, KeyCode.None, KeyCode.Return, KeyCode.Space };
	private static float _inputLockTime = 0;
	private static bool _inputLocked = false;
	private static GameObject _eventSystemGO;

	private static int GetButtonNumber(string buttonName)
	{
		return Array.IndexOf(Instance.InputNames, buttonName);
	}

	void Update()
	{
		_inputLocked = _inputLockTime != 0;

		_inputLockTime = _inputLockTime.Reduce(Time.deltaTime);

		if (_eventSystemGO == null)
		{
			if (MenuManager.Instance != null)
			{
				_eventSystemGO = MenuManager.Instance.GetComponentInChildren<EventSystem>().gameObject;
			}
		}
		else if (_eventSystemGO.activeInHierarchy != !_inputLocked)
			_eventSystemGO.SetActive(!_inputLocked);
	}

	#region GetButtonDown
	public static bool GetButtonDown(InputEnum buttonName, int JoystickNumber = -1)
	{
		return GetButtonDown((int)buttonName, JoystickNumber);
	}
	public static bool GetButtonDown(string buttonName, int JoystickNumber = -1)
	{
		int buttonNumber = GetButtonNumber(buttonName);
		if (buttonNumber == -1)
		{
			Debug.LogError("ButtonName '" + buttonName + "' was not found in InputNames.");
			return false;
		}
		return GetButtonDown(buttonNumber, JoystickNumber);
	}

	public static bool GetButtonDown(int buttonNumber, int JoystickNumber = -1)
	{
		if (_inputLocked)
			return false;
		if (JoystickNumber == -1)
			return Input.GetKeyDown("joystick button " + buttonNumber) || Input.GetKeyDown(Instance.KeyboardControls[buttonNumber]);
		else if (JoystickNumber == 0)
			return Input.GetKeyDown(Instance.KeyboardControls[buttonNumber]);
		else
			return Input.GetKeyDown("joystick " + JoystickNumber + " button " + buttonNumber);
	}
	#endregion

	#region GetButtonUp
	public static bool GetButtonUp(InputEnum buttonName, int JoystickNumber = -1)
	{
		return GetButtonUp((int)buttonName, JoystickNumber);
	}

	public static bool GetButtonUp(string buttonName, int JoystickNumber = -1)
	{
		int buttonNumber = GetButtonNumber(buttonName);
		if (buttonNumber == -1)
		{
			Debug.LogError("ButtonName '" + buttonName + "' was not found in InputNames.");
			return false;
		}
		return GetButtonUp(buttonNumber, JoystickNumber);
	}

	public static bool GetButtonUp(int buttonNumber, int JoystickNumber = -1)
	{
		if (_inputLocked)
			return false;
		if (JoystickNumber == -1)
			return Input.GetKeyUp("joystick button " + buttonNumber) || Input.GetKeyUp(Instance.KeyboardControls[buttonNumber]);
		else if (JoystickNumber == 0)
			return Input.GetKeyUp(Instance.KeyboardControls[buttonNumber]);
		else
			return Input.GetKeyUp("joystick " + JoystickNumber + " button " + buttonNumber);
	}
	#endregion

	#region GetButtonHeld
	public static bool GetButtonHeld(InputEnum buttonName, int JoystickNumber = -1)
	{
		return GetButtonHeld((int)buttonName, JoystickNumber);
	}

	public static bool GetButtonHeld(string buttonName, int JoystickNumber = -1)
	{
		int buttonNumber = GetButtonNumber(buttonName);
		if (buttonNumber == -1)
		{
			Debug.LogError("ButtonName '" + buttonName + "' was not found in InputNames.");
			return false;
		}
		return GetButtonHeld(buttonNumber, JoystickNumber);
	}

	public static bool GetButtonHeld(int buttonNumber, int JoystickNumber = -1)
	{
		if (_inputLocked)
			return false;

		if (JoystickNumber == -1)
		{
			return Input.GetKey("joystick button " + buttonNumber) || Input.GetKey(Instance.KeyboardControls[buttonNumber]);
		}
		else if (JoystickNumber == 0)
			return Input.GetKey(Instance.KeyboardControls[buttonNumber]);
		else
			return Input.GetKey("joystick " + JoystickNumber + " button " + buttonNumber);
	}
	#endregion

	#region GetAxis
	public static float GetAxis(string axisName = "x", int JoystickNumber = 0)
	{
		if (_inputLocked)
			return 0;
		return Input.GetAxis(axisName + "_" + JoystickNumber);
	}

	public static float GetAxisRaw(string axisName = "x", int JoystickNumber = 0)
	{
		if(_inputLocked)
			return 0;
		return Input.GetAxisRaw(axisName + "_" + JoystickNumber);
	}

	public static Vector2 GetStickDirection(int JoystickNumber = 0)
	{
		if (StickIsNeutral(JoystickNumber) || _inputLocked)
			return Vector2.zero;
		return new Vector2(GetAxis("x", JoystickNumber), GetAxis("y", JoystickNumber)).normalized;
	}

	public static bool StickIsNeutral(int JoystickNumber = 0)
	{
		return Input.GetAxisRaw("x_" + JoystickNumber) == 0 && Input.GetAxisRaw("y_" + JoystickNumber) == 0;
	}
	#endregion

	public static void AddInputLockTime(float additionnalTime)
	{
		_inputLockTime += additionnalTime;
	}

	public static void SetInputLockTime(float newTime)
	{
		_inputLockTime = newTime;
		_inputLocked = newTime != 0;
	}
	

}
