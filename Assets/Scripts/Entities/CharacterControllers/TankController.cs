using UnityEngine;
using System.Collections;

public class TankController : PlayerController {

	[Space()]
	[SerializeField]
	private float _specialStartSpeed = 50;
	[SerializeField]
	private float _specialTime = 0.3f;
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
		GetComponentInChildren<GroundCheck>().enabled = false;

		yield return new WaitForSeconds(_specialTime);

		GetComponentInChildren<GroundCheck>().enabled = true;

		yield return null;

		if(IsGrounded)
			_activeSpeed = Vector3.zero;

		_specialParticles.Stop();
		_charging = false;
		_isAffectedByFriction = true;

		yield return new WaitForSeconds(_characterData.SpecialCoolDown);

		_isInvul = false;
		_allowInput = true;
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
					(colli.transform.position - transform.position).ZeroY().normalized + _rigidB.velocity.normalized * 1.5f) * SO_Character.SpecialEjection.Multiply(Axis.x, _characterData.CharacterStats.strength),
					colli.contacts[0].point,
					_characterData.SpecialDamageData);
			}
		}
	}
}
