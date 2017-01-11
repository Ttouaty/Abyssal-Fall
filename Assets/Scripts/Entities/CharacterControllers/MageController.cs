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
	//private Vector2 _explosionEjection = new Vector2(5, 5);

	private GameObject _lastFireBallCasted;
	
	protected override bool SpecialActivation()
	{
		if (InputManager.GetButtonDown("Special", _playerRef.JoystickNumber))
		{
			if (_lastFireBallCasted != null)
			{
				_lastFireBallCasted.GetComponent<FireBall>().Activate();
				_lastFireBallCasted = null;
				return false;
			}
			else if(_canSpecial)
			{
				return _lastFireBallCasted == null;
			}
		}

		return false;
	}

	protected override void SpecialAction()
	{
		//cast fireball
		CmdLaunchFireProjectile(transform.position + transform.forward, transform.forward, gameObject);
	}

	[Command]
	public void CmdLaunchFireProjectile(Vector3 pos, Vector3 dir, GameObject ownerMage)
	{
		GameObject fireBallObj = GameObjectPool.GetAvailableObject("FireBall");

		fireBallObj.GetComponent<FireBall>().Launch(
			pos,
			dir,
			_explosionDelay,
			_explosionRadius,
			SO_Character.SpecialEjection.Multiply(Axis.x, _characterData.CharacterStats.strength),
			_characterData.SpecialDamageData,
			gameObject.GetInstanceID()
		);

		NetworkServer.SpawnWithClientAuthority(fireBallObj, ownerMage);

		TargetLinkFireBall(connectionToClient, fireBallObj);

		if (ArenaManager.Instance != null)
			fireBallObj.transform.parent = ArenaManager.Instance.SpecialsRoot;
	}

	[TargetRpc]
	public void TargetLinkFireBall(NetworkConnection target, GameObject targetFireball)
	{
		_lastFireBallCasted = targetFireball;
	}

}
