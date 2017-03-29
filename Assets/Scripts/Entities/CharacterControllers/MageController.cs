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
	//[SerializeField]
	//private float _maxChargeTime = 0.7f;

	private GameObject _fireBallObject;
	private bool _fireballIsActive = false;


	protected override void CustomStart()
	{
		base.CustomStart();
		_fireballIsActive = false;
	}

protected override bool SpecialActivation()
	{
		if (InputManager.GetButtonDown("Special", _playerRef.JoystickNumber))
		{
			if(_canSpecial && !_fireballIsActive)
			{
				return true;
			}
		}
		
		if(InputManager.GetButtonUp("Special", _playerRef.JoystickNumber))
		{
			if(_fireballIsActive)
			{
				_fireballIsActive = false;
				CmdActivateFireBall();
				_animator.SetBool("SpecialIsActive", false);
			}
		}

		return false;
	}

	protected override void SpecialAction()
	{
		//cast fireball
		_fireballIsActive = true;
		CmdLaunchFireProjectile(transform.position + transform.forward, transform.forward);
		_animator.SetBool("SpecialIsActive", true);
	}

	[Command]
	protected void CmdActivateFireBall()
	{
		_fireBallObject.GetComponent<FireBall>().Activate();
		_fireBallObject = null;
	}

	[Command]
	public void CmdLaunchFireProjectile(Vector3 pos, Vector3 dir)
	{
		_fireBallObject = GameObjectPool.GetAvailableObject("FireBall");
		DamageData tempDamageData = _characterData.SpecialDamageData.Copy();
		tempDamageData.Dealer = _dmgDealerSelf;

		_fireBallObject.GetComponent<FireBall>().Launch(
			pos,
			dir,
			_explosionDelay,
			_explosionRadius,
			SO_Character.SpecialEjection.Multiply(Axis.x, _characterData.CharacterStats.strength),
			tempDamageData,
			netId
		);

		NetworkServer.SpawnWithClientAuthority(_fireBallObject, _playerRef.gameObject);
		_fireBallObject.GetComponent<ABaseProjectile>().RpcSendOnLaunch(gameObject);

		if (ArenaManager.Instance != null)
			_fireBallObject.transform.parent = ArenaManager.Instance.SpecialsRoot;
	}
}
