using UnityEngine;
using System.Collections;

public class TankController : PlayerController
{

	[Space()]
	[SerializeField]
	private float _specialStartSpeed = 20;
	[SerializeField]
	private float _specialTime = 0.3f;
	[SerializeField]
	private ParticleSystem _specialParticles;
	[SerializeField]
	private SphereCollider ChargeHitbox;


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

		ChargeHitbox.enabled = true;

		_isAffectedByFriction = false;
		_isInvul = true;
		_allowInput = false;
		_activeSpeed = _activeDirection.normalized * _specialStartSpeed;
		_charging = true;
		GetComponentInChildren<GroundCheck>().Deactivate();

		while (eT < _specialTime)
		{
			_activeSpeed = _activeDirection.normalized * _specialStartSpeed;
			eT += Time.deltaTime;
			yield return null;
		}

		GetComponentInChildren<GroundCheck>().Activate();

		yield return null;

		if (IsGrounded)
			_activeSpeed = Vector3.zero;

		_specialParticles.Stop();
		_charging = false;
		_isAffectedByFriction = true;

		ChargeHitbox.enabled = false;
		if(Physics.CheckSphere(ChargeHitbox.transform.position, ChargeHitbox.radius + 0.2f, 1 << LayerMask.NameToLayer("Wall")))
		{

			Debug.Log("colli wall");
			_activeSpeed = Vector3.zero;
			_rigidB.velocity = _activeSpeed;
		}

		_isInvul = false;

		eT = 0;
		while (eT < _characterData.SpecialLag * 0.7f)
		{
			Debug.Log("waiting for end");
			_activeSpeed.y = 0;
			eT += Time.deltaTime;
			yield return null;
		}

	}

	protected override void PlayerCollisionHandler(Collision colli)
	{
		base.PlayerCollisionHandler(colli);
		if (_charging)
		{
			if (colli.transform.GetComponent<IDamageable>() != null)
			{
				DamageData tempDamageData = _characterData.SpecialDamageData.Copy();
				tempDamageData.Dealer = _dmgDealerSelf;

				colli.transform.GetComponent<IDamageable>()
					.Damage(Quaternion.FromToRotation(Vector3.right,
					(colli.transform.position - transform.position).ZeroY().normalized + _rigidB.velocity.normalized * 1.5f) * SO_Character.SpecialEjection.Multiply(Axis.x, _characterData.CharacterStats.strength),
					colli.contacts[0].point,
					tempDamageData);
			}
		}
	}
}
