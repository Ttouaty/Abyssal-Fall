﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputManager : MonoBehaviour 
{
	//  Buttons						0		1		 2			3		4		5		6		7
	private string[] InputNames = { "Dash", null, "Special", null, null, null, "Select", "Start" };
	private bool[] _inputCatched = new bool[20]; // capé a 20 buttons. C'est un peu dangereux, si tu catch une input, pendant 1 frame, tous les autres appels a ce bouton renverons false.
	private static InputManager instance;

	void Awake()
	{ 
		instance = this;
	}

	void Update()
	{
		for (int i = 0; i < _inputCatched.Length; ++i) { _inputCatched[i] = false; }
	}

	private static int GetButtonNumber(string buttonName) 
	{
		return Array.IndexOf(instance.InputNames, buttonName);
	}

	public static bool GetButtonDown(string buttonName, int JoystickNumber = -1, bool catchInput = false)
	{
		int buttonNumber = GetButtonNumber(buttonName);
		if(buttonNumber == -1)
		{
			Debug.LogError("ButtonName '"+buttonName+"' was not found in InputNames.");
			return false;
		}
		return GetButtonDown(buttonNumber, JoystickNumber, catchInput);
	}

	public static bool GetButtonDown(int buttonNumber, int JoystickNumber = -1, bool catchInput = false)
	{
		if (instance._inputCatched[buttonNumber] == true)
			return false;
		instance._inputCatched[buttonNumber] = true;

		if (JoystickNumber == -1)
			return Input.GetKeyDown("joystick button " + buttonNumber);
		else
			return Input.GetKeyDown("joystick " + JoystickNumber + " button " + buttonNumber);
	}

	public static bool GetButtonUp(string buttonName, int JoystickNumber = -1, bool catchInput = false)
	{
		int buttonNumber = GetButtonNumber(buttonName);
		if (buttonNumber == -1)
		{
			Debug.LogError("ButtonName '" + buttonName + "' was not found in InputNames.");
			return false;
		}
		return GetButtonUp(buttonNumber, JoystickNumber, catchInput);
	}

	public static bool GetButtonUp(int buttonNumber, int JoystickNumber = -1, bool catchInput = false)
	{
		if (instance._inputCatched[buttonNumber] == true)
			return false;
		instance._inputCatched[buttonNumber] = true;
		if (JoystickNumber == -1)
			return Input.GetKeyUp("joystick button " + buttonNumber);
		else
			return Input.GetKeyUp("joystick " + JoystickNumber + " button " + buttonNumber);
	}

	public static bool GetButtonHeld(string buttonName, int JoystickNumber = -1, bool catchInput = false)
	{
		int buttonNumber = GetButtonNumber(buttonName);
		if (buttonNumber == -1)
		{
			Debug.LogError("ButtonName '" + buttonName + "' was not found in InputNames.");
			return false;
		}
		return GetButtonHeld(buttonNumber, JoystickNumber, catchInput);
	}

	public static bool GetButtonHeld(int buttonNumber, int JoystickNumber = -1, bool catchInput = false)
	{
		if (instance._inputCatched[buttonNumber] == true)
			return false;
		instance._inputCatched[buttonNumber] = true;
		if (JoystickNumber == -1)
			return Input.GetKey("joystick button " + buttonNumber);
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
