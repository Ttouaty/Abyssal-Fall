using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputManager : GenericSingleton<InputManager> 
{
	//  Buttons						0		1		 2			3		4		5		6		7
	private string[] InputNames = { "Dash", null, "Special", null, null, null, "Select", "Start" };
	private KeyCode[] KeyboardControls = { KeyCode.J, KeyCode.L, KeyCode.K, KeyCode.UpArrow, KeyCode.None, KeyCode.None, KeyCode.Return, KeyCode.Space };

	private static int GetButtonNumber(string buttonName) 
	{
		return Array.IndexOf(Instance.InputNames, buttonName);
	}

	public static bool GetButtonDown(string buttonName, int JoystickNumber = -1)
	{
		int buttonNumber = GetButtonNumber(buttonName);
		if(buttonNumber == -1)
		{
			Debug.LogError("ButtonName '"+buttonName+"' was not found in InputNames.");
			return false;
		}
		return GetButtonDown(buttonNumber, JoystickNumber);
	}

	public static bool GetButtonDown(int buttonNumber, int JoystickNumber = -1)
	{
        if (JoystickNumber == -1)
			return Input.GetKeyDown("joystick button " + buttonNumber);
		else if (JoystickNumber == 0)
			return Input.GetKeyDown(Instance.KeyboardControls[buttonNumber]);
		else
			return Input.GetKeyDown("joystick " + JoystickNumber + " button " + buttonNumber);
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
		if (JoystickNumber == -1)
			return Input.GetKeyUp("joystick button " + buttonNumber);
		else if (JoystickNumber == 0)
			return Input.GetKeyUp(Instance.KeyboardControls[buttonNumber]);
		else
			return Input.GetKeyUp("joystick " + JoystickNumber + " button " + buttonNumber);
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
		if (JoystickNumber == -1)
			return Input.GetKey("joystick button " + buttonNumber);
		else if (JoystickNumber == 0)
			return Input.GetKey(Instance.KeyboardControls[buttonNumber]);
		else
			return Input.GetKey("joystick " + JoystickNumber + " button " + buttonNumber);
	}

	public static float GetAxis(string axisName = "x", int JoystickNumber = 0)
	{
		return Input.GetAxis(axisName + "_" + JoystickNumber); ;
	}

	public static float GetAxisRaw(string axisName = "x", int JoystickNumber = 0)
	{
		return Input.GetAxisRaw(axisName + "_" + JoystickNumber);
	}

}
