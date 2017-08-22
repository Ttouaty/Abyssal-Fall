using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;

public enum InputMethod
{
	Down,
	Up,
	Held
}

// most fields a public because of custom editor, I need to check how to fix this :p
public class InputListener : MonoBehaviour
{
	private float stickActivateThreshold = 0.85f;

	public bool UseAxis = false;
	public Vector2 directionToListen = new Vector2(1, 0);
	public float AxisPrecision = 0.5f;

	public InputEnum InputToListen;
	public string InputStringToListen;

	public InputMethod InputMethodUsed;
	public float TimeToHold = 0.5f;
	public bool CanLoop = false;
	public bool NeedConfirmation = false;

	//protected int[] JoysticksToListen = new int[12]; 

	public UnityEventInt Callback;
	public bool UseKeyboard = false;
	protected float _timeHeld;
	private bool[] _waitForRelease = new bool[12];
	private bool[] _firstHeldFrame = new bool[12] { true, true, true, true, true, true, true, true, true, true, true, true }; // init dégueux => doigt
	private bool _isInputDown = false;

	protected int _JoystickRequestCallback = -1;
	protected int _JoystickHeld = -1;
	private Vector2 _tempStickPosition;

	protected virtual void Start()
	{
		//JoysticksToListen = new int[12];
		if (UseAxis && directionToListen.magnitude == 0)
			Debug.LogError("No axis set for inputListener: " + gameObject.name);
	}

	protected virtual void Update()
	{
		if (!MenuPanelNew.InputIsActive)
			return;

		string[] tempStringArray = InputManager.GetJoystickNames();
		for (int i = 0; i < tempStringArray.Length; ++i)
		{
			if (tempStringArray[i] == "")
				continue;

			TestInput(i);
		}

		if ((InputMethodUsed == InputMethod.Held && !UseAxis) || (UseAxis && CanLoop))
		{
			if (_JoystickHeld != -1)
				_timeHeld += Time.deltaTime;
			else
				_timeHeld = _timeHeld.Reduce(Time.deltaTime);

			if (_timeHeld > TimeToHold)
			{
				LaunchCallback(_JoystickHeld);
			}
		}

		if (_JoystickRequestCallback != -1)
		{
			LaunchCallback(_JoystickRequestCallback);
		}

		_JoystickHeld = -1;
		_JoystickRequestCallback = -1;

	}

	void TestInput(int joystickNumber)
	{
		if (UseAxis)
			TestAxis(joystickNumber);
		else
			TestButton(joystickNumber);
	}

	void TestAxis(int joystickNumber)
	{
		_tempStickPosition = InputManager.GetStickDirection(joystickNumber, false);

		if (!_firstHeldFrame[joystickNumber])
		{
			if (_tempStickPosition.magnitude < stickActivateThreshold - 0.2)
				_firstHeldFrame[joystickNumber] = true;
		}

		if (_waitForRelease[joystickNumber])
		{
			if (_tempStickPosition.magnitude < stickActivateThreshold - 0.2)
			{
				_waitForRelease[joystickNumber] = false;
			}
		}
		else if (directionToListen.AnglePercent(_tempStickPosition) > AxisPrecision && _tempStickPosition.magnitude > stickActivateThreshold)
		{
			if (CanLoop)
			{
				_JoystickHeld = joystickNumber;

				if (_firstHeldFrame[joystickNumber])
				{
					_firstHeldFrame[joystickNumber] = false;
					_JoystickRequestCallback = joystickNumber;
				}
			}
			else
				_JoystickRequestCallback = joystickNumber;
		}
	}
	void TestButton(int joystickNumber)
	{
		
		if (_waitForRelease[joystickNumber])
		{
			if (UseKeyboard)
				_waitForRelease[joystickNumber] = !InputManager.GetButtonUp(InputStringToListen);
			else
				_waitForRelease[joystickNumber] = !InputManager.GetButtonUp(InputToListen, joystickNumber);
		}

		if (InputMethodUsed == InputMethod.Down)
		{
			if (UseKeyboard)
			{
				if (InputManager.GetButtonDown(InputStringToListen))
					_JoystickRequestCallback = joystickNumber;
			}
			else if (InputManager.GetButtonDown(InputToListen, joystickNumber))
					_JoystickRequestCallback = joystickNumber;
		}
		else if (InputMethodUsed == InputMethod.Up)
		{
			if (UseKeyboard)
			{
				if (InputManager.GetButtonUp(InputStringToListen))
					_JoystickRequestCallback = joystickNumber;
			}
			else if(InputManager.GetButtonUp(InputToListen, joystickNumber))
				_JoystickRequestCallback = joystickNumber;
		}
		else
		{
			if (UseKeyboard)
				_isInputDown = InputManager.GetButtonHeld(InputStringToListen);
			else
				_isInputDown = InputManager.GetButtonHeld(InputToListen, joystickNumber);
		}
		
	

		if (_isInputDown)
		{
			if (!_waitForRelease[joystickNumber])
			{
				_JoystickHeld = joystickNumber;
			}
		}
	}

	public virtual void LaunchCallback(int joy)
	{
		if (!CanLoop)
			_waitForRelease[joy] = true;
		_timeHeld = 0;
		_JoystickRequestCallback = -1;

		if(NeedConfirmation)
		{
			StartCoroutine(DeactivateAllSiblings(FindObjectsOfType<InputListener>()));
			Confirm.Instance.Open(WaitForConfirm(joy, FindObjectsOfType<InputListener>()), ReactivateAllSiblings(FindObjectsOfType<InputListener>()));
		}
		else
			Callback.Invoke(joy);
	}

	IEnumerator WaitForConfirm(int joy, InputListener[] siblingsButtons)
	{
		StartCoroutine(ReactivateAllSiblings(siblingsButtons));
		Callback.Invoke(joy);
		yield return null;
	}

	IEnumerator ReactivateAllSiblings(InputListener[] siblingsButtons)
	{
		for (int i = 0; i < siblingsButtons.Length; i++)
		{
			siblingsButtons[i].enabled = true;
		}
		yield return null;
	}

	IEnumerator DeactivateAllSiblings(InputListener[] siblingsButtons)
	{
		for (int i = 0; i < siblingsButtons.Length; i++)
		{
			siblingsButtons[i].enabled = false;
		}
		yield return null;
	}



	void OnEnable()
	{
		_JoystickHeld = -1;
		_JoystickRequestCallback = -1;
		_isInputDown = false;
		_timeHeld = 0;
		for (int i = 0; i < _waitForRelease.Length; i++)
		{
			_waitForRelease[i] = false;
		}
	}


	public void ClickSelfButton()
	{
		if(GetComponent<Button>() != null)
		{
			GetComponent<Button>().onClick.Invoke();
		}
	}
}

[Serializable]
public class UnityEventInt : UnityEvent<int> { }