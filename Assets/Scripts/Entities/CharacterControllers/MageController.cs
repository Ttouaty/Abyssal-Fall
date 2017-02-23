using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MageController : PlayerController
{
	[Header("Explosion")]
	[SerializeField]
	private float _explosionDelay = 0.7f; // Time before explosion occurs after the special is activated
	[SerializeField]
	private float _explosionRadius = 4f;
	[SerializeField]
	private float _maxChargeTime = 0.7f;

	private float _chargingTime = 0;
	private bool _specialActivating = false;

	protected override bool SpecialActivation()
	{
		if (InputManager.GetButtonHeld("Special", _playerRef.JoystickNumber))
		{
			if(_canSpecial)
			{
				_specialActivating = true;
				if (_chargingTime <= _maxChargeTime)
					_chargingTime += Time.deltaTime;
				else
				{
					_chargingTime = _maxChargeTime;
					return true;
				}
			}
		}
		
		if(InputManager.GetButtonUp("Special", _playerRef.JoystickNumber))
		{
			if(_specialActivating && _canSpecial)
				return true;
		}

		return false;
	}

	protected override void SpecialAction()
	{
		_specialActivating = false;
		//cast fireball
		CmdLaunchFireProjectile(transform.position + transform.forward, transform.forward, _chargingTime);
		_chargingTime = 0;
	}

	[Command]
	public void CmdLaunchFireProjectile(Vector3 pos, Vector3 dir, float charge)
	{
		GameObject fireBallObj = GameObjectPool.GetAvailableObject("FireBall");
		DamageData tempDamageData = _characterData.DashDamageData.Copy();
		tempDamageData.Dealer = _dmgDealerSelf;
		fireBallObj.GetComponent<FireBall>().Launch(
			pos,
			dir,
			_explosionDelay,
			_explosionRadius,
			SO_Character.SpecialEjection.Multiply(Axis.x, _characterData.CharacterStats.strength),
			tempDamageData,
			gameObject.GetInstanceID()
		);

		NetworkServer.Spawn(fireBallObj);

		if (ArenaManager.Instance != null)
			fireBallObj.transform.parent = ArenaManager.Instance.SpecialsRoot;

		fireBallObj.GetComponent<FireBall>().Invoke("Activate", charge);
	}
}
