using UnityEngine;
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

	private FireBall _lastFireBallCasted;
	public FireBall ActiveFireBall
	{
		get { return _lastFireBallCasted; }
		set { _lastFireBallCasted = value; }
	}

	protected override bool SpecialActivation()
	{
		if (InputManager.GetButtonDown("Special", _playerRef.JoystickNumber))
		{
			if (_lastFireBallCasted != null)
			{
				_lastFireBallCasted.Activate();
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
		GameObject fireBallObj = GameObjectPool.GetAvailableObject("FireBall");
		_lastFireBallCasted = fireBallObj.GetComponent<FireBall>();

		_lastFireBallCasted.Launch(
			transform.position + transform.forward,
			transform.forward,
			_explosionDelay,
			_explosionRadius,
			SO_Character.SpecialEjection.Multiply(Axis.x, _characterData.CharacterStats.strength),
			_characterData.SpecialDamageData,
			gameObject.GetInstanceID()
		);

		if (ArenaManager.Instance != null)
			fireBallObj.transform.parent = ArenaManager.Instance.SpecialsRoot;
	}
}
