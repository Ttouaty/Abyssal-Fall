using UnityEngine;
using System.Collections;

public class TankController : PlayerController {

	[Space()]
	[SerializeField]
	private float _specialStartSpeed = 30;
	[SerializeField]
	private float _specialTime = 0.2f;
	[SerializeField]
	private float _specialSpeedDecreaseFactor = 0.2f;
	[SerializeField]
	private ParticleSystem _specialParticles;
	
	private bool _charging = false;
	protected override void SpecialAction()
	{
		StartCoroutine(SpecialCoroutine());
	}

	IEnumerator SpecialCoroutine()
	{
		float eT = 0;
		_specialParticles.Clear();
		_specialParticles.Play();

		_isAffectedByFriction = false;
		_isInvul = true;
		_allowInput = false;
		_activeSpeed = _activeDirection.normalized * _specialStartSpeed;
		_charging = true;

		yield return null;
		while (eT < _specialTime)
		{
			eT += Time.deltaTime;
			_activeSpeed = Vector3.Lerp(_activeSpeed, Vector3.zero, _specialSpeedDecreaseFactor).ZeroY();
			yield return null;
		}

		_specialParticles.Stop();
		_isInvul = false;
		_allowInput = true;
		_charging = false;
		_isAffectedByFriction = true;

	}

	protected override void PlayerCollisionHandler(Collision colli)
	{
		base.PlayerCollisionHandler(colli);
		if (_charging)
		{
			if (colli.transform.GetComponent<IDamageable>() != null)
			{
				colli.transform.GetComponent<IDamageable>()
					.Damage(Quaternion.FromToRotation(Vector3.right,
					(colli.transform.position - transform.position).ZeroY().normalized + _rigidB.velocity.normalized * 1.5f) * _characterData.SpecialEjection.Multiply(Axis.x, _characterData.CharacterStats.strength),
					colli.contacts[0].point,
					_characterData.SpecialDamageData);
			}
		}
	}
}
