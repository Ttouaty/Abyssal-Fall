using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

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

	[SerializeField]
	private ParticleSystem[] HitParticles;

	[SyncVar]
	private bool _charging = false;
	protected override void SpecialAction()
	{
		StartCoroutine(SpecialCoroutine());
	}

	IEnumerator SpecialCoroutine()
	{
		float eT = 0;

		CmdActiveCharge(true);

		_isAffectedByFriction = false;
		_isInvul = true;
		_allowInput = false;
		_activeSpeed = _activeDirection.normalized * _specialStartSpeed;
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

		CmdActiveCharge(false);
		_isAffectedByFriction = true;

		if(Physics.CheckSphere(ChargeHitbox.transform.position, ChargeHitbox.radius * ChargeHitbox.transform.lossyScale.x, 1 << LayerMask.NameToLayer("Wall")))
		{
			_activeSpeed = Vector3.zero;
			_rigidB.velocity = _activeSpeed;
		}

		_isInvul = false;

		eT = 0;
		while (eT < _characterData.SpecialLag * 0.7f)
		{
			_activeSpeed.y = 0;
			eT += Time.deltaTime;
			yield return null;
		}

	}

	[Command]
	public void CmdActiveCharge(bool active) { RpcActiveCharge(active); }

	[ClientRpc]
	public void RpcActiveCharge(bool active)
	{

		_charging = active;
		ChargeHitbox.enabled = active;

		if (active)
		{
			_specialParticles.Clear();
			_specialParticles.Play();
		}
		else
		{
			_specialParticles.Stop();
		}
	}

	protected override void PlayerCollisionHandler(Collision colli)
	{
		base.PlayerCollisionHandler(colli);

		if (_charging)
		{
			if (colli.transform.GetComponent<IDamageable>() != null && !colli.transform.GetComponent<PlayerController>()._isInvul)
			{
				Destroy(Instantiate(HitParticles[_playerRef.SkinNumber].gameObject, (colli.transform.position + transform.position) * 0.5f, Quaternion.identity) as GameObject, HitParticles[0].duration + HitParticles[0].startLifetime);

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
