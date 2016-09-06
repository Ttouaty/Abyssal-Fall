using UnityEngine;
using System.Collections;

public class MageController : PlayerController 
{
	private float _specialActiveRange = 5;
	
	[Space()]
	[SerializeField]
	private float _specialMaxChargeTime = 1;
	private float _timeHeld = 0;
	[SerializeField]
	private float _specialChargeSpeed = 5; // Range charge in Units/Sec
	private float _specialChargeSpeedOriginal;
	[SerializeField]
	private float _specialChargeSpeedIncrease = 1.2f; // How much the speed of the range increase must grow in %/s

	[Space()]
	[SerializeField]
	private int _specialMinRange = 3;

	protected override void CustomStart()
	{
		_specialChargeSpeedOriginal = _specialChargeSpeed;
	}

	protected override bool SpecialActivation()
	{
		if (_canSpecial)
		{
			return InputManager.GetButtonUp("Special", _playerRef.JoystickNumber) || SpecialCharge();
		}
		else
		{
			_specialChargeSpeed = _specialChargeSpeedOriginal;
			_specialActiveRange = _specialMinRange;
			_timeHeld = 0;
		}
		return false;
	}

	private bool SpecialCharge()
	{ 
		if(InputManager.GetButtonHeld("Special", _playerRef.JoystickNumber))
		{
			_timeHeld += Time.deltaTime;
			if (_timeHeld > _specialMaxChargeTime)
			{
				return true; // force special activation after x secs;
			}
			_specialChargeSpeed += _specialChargeSpeedOriginal * _specialChargeSpeedIncrease * Time.deltaTime;
			_specialActiveRange += _specialChargeSpeed * Time.deltaTime;
			Debug.DrawRay(transform.position, _activeDirection.normalized * _specialActiveRange, Color.red, 0.1f);
		}

		return false;
	}

	protected override void SpecialAction()
	{
		Debug.DrawRay(transform.position+ Vector3.up * 0.2f, _activeDirection.normalized * _specialActiveRange, Color.green, 2f);

		/*
		 TODO: MAKE BOOM after 0.5f secs
		 
		 */
		Debug.Log("special Activated");
	}
}
