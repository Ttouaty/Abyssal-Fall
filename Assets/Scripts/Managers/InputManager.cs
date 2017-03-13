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
	//  Buttons							0		1		  2		  3		4	  5		  6			7
	private string[] InputNames = { "Dash", "Cancel", "Special", null, null, null, "Select", "Start" };
	private KeyCode[][] KeyboardControls = { new KeyCode[] { KeyCode.R, KeyCode.Return }, new KeyCode[] { KeyCode.E, KeyCode.Backspace }, new KeyCode[] { KeyCode.Z, KeyCode.W }, new KeyCode[] { KeyCode.A, KeyCode.Q }, new KeyCode[] { KeyCode.None }, new KeyCode[] { KeyCode.None }, new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift }, new KeyCode[] { KeyCode.Space } };
	private static float _inputLockTime = 0;

	public static bool InputLocked { get { return _inputLockTime != 0; } }

	private static int GetButtonNumber(string buttonName)
	{
		return Array.IndexOf(Instance.InputNames, buttonName);
	}

	void LateUpdate()
	{
		_inputLockTime = _inputLockTime.Reduce(Time.deltaTime);
	}

	static string[] _tempJoyNames;
	/// <summary>
	/// It's a bit heavy since you have to check for every button of every controller + axis, don't abuse it.
	/// </summary>
	public static bool AnyDown(bool withAxis = true)
	{
		if (Input.anyKeyDown)
			return true;

		_tempJoyNames = GetJoystickNames();
		for (int i = 0; i < _tempJoyNames.Length; i++)
		{
			for (int j = 0; j < 7; j++)
			{
				if (GetButtonDown(j, i))
					return true;
			}

			if(withAxis)
			{
				if (Mathf.Abs(GetAxis("x", i)) > 0.5f || Mathf.Abs(GetAxis("y", i)) > 0.5f)
					return true;
			}
		}

		return false;
	}

	public static string[] GetJoystickNames()
	{
		string[] joystickNames = Input.GetJoystickNames();

		string[] returnArray = new string[joystickNames.Length + 1];
		for (int i = 1; i < joystickNames.Length + 1; i++)
		{
			returnArray[i] = joystickNames[i - 1];
		}
		returnArray[0] = "Keyboard";

		return returnArray;
	}

	public static string AnyKeyDown()
	{
		return Input.inputString;
	}

	/// <summary>
	/// Get any button pressed this frames on any gamepad
	/// </summary>
	/// <param name="gamepadNumber">True for gamepadNumber / false for button number</param>
	/// <returns> -1 if nothing is pressed</returns>
	public static int AnyButtonDown(bool gamepadNumber = true)
	{
		_tempJoyNames = GetJoystickNames();
		for (int i = 0; i < _tempJoyNames.Length; i++)
		{
			for (int j = 0; j < 7; j++)
			{
				if (GetButtonDown(j, i))
					return gamepadNumber? i : j;
			}
		}

		return -1;
	}

	#region GetButtonDown
	public static bool GetButtonDown(string inputName)
	{
		return Input.GetKeyDown(inputName);
	}

	public static bool GetButtonDown(InputEnum buttonName, int JoystickNumber = -1)
	{
		return GetButtonDown((int)buttonName, JoystickNumber);
	}

	public static bool GetButtonDown(string buttonName, int JoystickNumber)
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
		if (InputLocked)
			return false;
		if (JoystickNumber == -1)
			return Input.GetKeyDown("joystick button " + buttonNumber) || TestKeysDown(buttonNumber);
		else if (JoystickNumber == 0)
			return TestKeysDown(buttonNumber);
		else
			return Input.GetKeyDown("joystick " + JoystickNumber + " button " + buttonNumber);
	}

	public static bool TestKeysDown(int buttonNumber)
	{
		for (int i = 0; i < Instance.KeyboardControls[buttonNumber].Length; i++)
		{
			//if (buttonNumber == 0 && GameManager.InProgress)
			//{
			//	if (Input.GetKeyDown(KeyCode.Mouse0))
			//		return true;
			//}
			if (Input.GetKeyDown(Instance.KeyboardControls[buttonNumber][i]))
				return true;
		}
		return false;
	}
	#endregion

	#region GetButtonUp
	public static bool GetButtonUp(string inputName)
	{
		return Input.GetKeyUp(inputName);
	}

	public static bool GetButtonUp(InputEnum buttonName, int JoystickNumber = -1)
	{
		return GetButtonUp((int)buttonName, JoystickNumber);
	}

	public static bool GetButtonUp(string buttonName, int JoystickNumber)
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
		if (InputLocked)
			return false;
		if (JoystickNumber == -1)
			return Input.GetKeyUp("joystick button " + buttonNumber) || TestKeysUp(buttonNumber);
		else if (JoystickNumber == 0)
			return TestKeysUp(buttonNumber);
		else
			return Input.GetKeyUp("joystick " + JoystickNumber + " button " + buttonNumber);
	}

	public static bool TestKeysUp(int buttonNumber)
	{
		for (int i = 0; i < Instance.KeyboardControls[buttonNumber].Length; i++)
		{
			//if (buttonNumber == 0 && GameManager.InProgress)
			//{
			//	if (Input.GetKeyUp(KeyCode.Mouse0))
			//		return true;
			//}
			if (Input.GetKeyUp(Instance.KeyboardControls[buttonNumber][i]))
				return true;
		}
		return false;
	}
	#endregion

	#region GetButtonHeld
	public static bool GetButtonHeld(string inputName)
	{
		return Input.GetKey(inputName);
	}

	public static bool GetButtonHeld(InputEnum buttonName, int JoystickNumber = -1)
	{
		return GetButtonHeld((int)buttonName, JoystickNumber);
	}

	public static bool GetButtonHeld(string buttonName, int JoystickNumber)
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
		if (InputLocked)
			return false;

		if (JoystickNumber == -1)
			return Input.GetKey("joystick button " + buttonNumber) || TestKeysHeld(buttonNumber);
		else if (JoystickNumber == 0)
			return TestKeysHeld(buttonNumber);
		else
			return Input.GetKey("joystick " + JoystickNumber + " button " + buttonNumber);
	}

	public static bool TestKeysHeld(int buttonNumber)
	{
		for (int i = 0; i < Instance.KeyboardControls[buttonNumber].Length; i++)
		{
			//if(buttonNumber == 0 && GameManager.InProgress)
			//{
			//	if (Input.GetKey(KeyCode.Mouse0))
			//		return true;
			//}

			if (Input.GetKey(Instance.KeyboardControls[buttonNumber][i])) 
				return true;
		}
		return false;
	}
	#endregion

	#region GetAxis
	public static float GetAxis(string axisName = "x", int JoystickNumber = 0)
	{
		if (InputLocked)
			return 0;
		return Input.GetAxis(axisName + "_" + JoystickNumber);
	}

	public static float GetAxisRaw(string axisName = "x", int JoystickNumber = 0)
	{
		if(InputLocked)
			return 0;
		return Input.GetAxisRaw(axisName + "_" + JoystickNumber);
	}

	public static Vector2 GetAllStickDirection()
	{
		return GetAllStickDirection(true);
	}

	public static Vector2 GetAllStickDirection(bool normalized)
	{
		Vector2 returnVector = Vector2.zero;
		Vector2 tempVector;

		for (int i = 0; i < GetJoystickNames().Length; i++)
		{
			tempVector = GetStickDirection(i, normalized);
			if (tempVector.magnitude != 0)
				returnVector += tempVector;
		}

		return returnVector;
	}

	public static Vector2 GetStickDirection(int JoystickNumber)
	{
		return GetStickDirection(JoystickNumber, true);
	}

	public static Vector2 GetStickDirection(int JoystickNumber, bool normalized)
	{
		if (StickIsNeutral(JoystickNumber) || InputLocked)
			return Vector2.zero;
		if(normalized)
			return new Vector2(GetAxis("x", JoystickNumber), GetAxis("y", JoystickNumber)).normalized;
		return new Vector2(GetAxis("x", JoystickNumber), GetAxis("y", JoystickNumber));

	}

	public static bool StickIsNeutral(int JoystickNumber = 0, float deadZone = 0f)
	{
		if(deadZone == 0)
			return Input.GetAxisRaw("x_" + JoystickNumber) == 0 && Input.GetAxisRaw("y_" + JoystickNumber) == 0;
		else
			return Mathf.Abs(Input.GetAxis("x_" + JoystickNumber)) < deadZone && Mathf.Abs(Input.GetAxis("y_" + JoystickNumber)) < deadZone;
	}
	#endregion

	public static void AddInputLockTime(float additionnalTime)
	{
		_inputLockTime += additionnalTime;
	}

	public static void SetInputLockTime(float newTime)
	{
		_inputLockTime = newTime;
	}
	

}
