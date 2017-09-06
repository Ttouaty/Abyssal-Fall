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
	private float _invulTime = 0.2f;
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
		StartCoroutine("SpecialCoroutine");
	}

	protected override void CustomUpdate()
	{
		base.CustomUpdate();
		if (_charging)
			_activeSpeed = _activeSpeed.SetAxis(Axis.y, -1);
		_rigidB.velocity = _activeSpeed;
	}

	IEnumerator SpecialCoroutine()
	{
		float eT = 0;

		CmdActiveCharge(true);

		_isAffectedByFriction = false;
		_invulTimer.Set(_invulTime);
		_damageTimer.Set(_specialTime);

		_allowInput = false;
		_activeSpeed = _activeDirection.normalized * _specialStartSpeed;
		_groundCheck.Deactivate();
		_rigidB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

		while (eT < _specialTime)
		{
			yield return new WaitForFixedUpdate();
			_activeSpeed = _activeSpeed.Apply(_activeDirection.normalized * _specialStartSpeed, _specialStartSpeed);
			_rigidB.velocity = _activeSpeed;
			eT += Time.fixedDeltaTime;
		}

		StopCharge();
		_rigidB.constraints = RigidbodyConstraints.FreezeRotation;

		yield return new WaitForFixedUpdate();

		//if (IsGrounded)
		//{
		//	_activeSpeed = Vector3.zero;
		//	_rigidB.velocity = _activeSpeed;
		//}

		//if (Physics.CheckSphere(ChargeHitbox.transform.position, ChargeHitbox.radius * ChargeHitbox.transform.lossyScale.x, 1 << LayerMask.NameToLayer("Wall")))
		//{ //impact wall

		//	_activeSpeed = Vector3.zero;
		//	_rigidB.velocity = _activeSpeed;
		//}
		eT = 0;
		while (eT < 0.2f)
		{
			_activeSpeed.y *= 0.8f;
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
			_characterData.SoundList["OnSpecialActivate"].Play(gameObject);
			_specialParticles.Clear();
			_specialParticles.Play();
		}
		else
		{
			_characterData.SoundList["OnSpecialEnd"].Play(gameObject);
			_specialParticles.Stop();
		}
	}

	void StopCharge()
	{
		CmdActiveCharge(false);
		_isAffectedByFriction = true;
		_groundCheck.Activate();
		_charging = false;
	}

	protected override void PlayerCollisionHandler(Collision colli)
	{
		base.PlayerCollisionHandler(colli);

		if (_charging)
		{
		
			if(colli.transform.GetComponent<TankController>() != null)
			{
				if (colli.transform.GetComponent<TankController>()._charging)
				{
					Debug.Log("DOUBLE HIT!");
					StopCharge();

					_activeSpeed = Vector3.zero;
					_rigidB.velocity = _activeSpeed;

					colli.transform.GetComponent<TankController>().StopCharge();

					colli.transform.GetComponent<TankController>()._activeSpeed = Vector3.zero;
					colli.transform.GetComponent<TankController>()._rigidB.velocity = Vector3.zero;

					colli.transform.GetComponent<TankController>().ForceCollisions(GetComponent<Collider>(), Vector3.up + transform.forward * 2);
					ForceCollisions(colli.collider, Vector3.up + colli.transform.forward * 2);
				}
				else
					ForceCollisions(colli.collider);

			}
			else if (colli.transform.GetComponent<IDamageable>() != null)
			{
				ForceCollisions(colli.collider);
			}
		}
	}

	public void ForceCollisions(Collider colli, Vector3 additionnalDirection = default(Vector3))
	{
		if (colli.transform.GetComponent<PlayerController>() != null)
		{
			if (colli.transform.GetComponent<PlayerController>()._isInvul)
				return;
		}
		else
			return;
		Debug.Log("HIT");

		_characterData.SoundList["OnSpecialHit"].Play(gameObject);
		Destroy(Instantiate(HitParticles[_playerRef.SkinNumber].gameObject, (colli.transform.position + transform.position) * 0.5f, Quaternion.identity) as GameObject, HitParticles[0].duration + HitParticles[0].startLifetime);

		DamageData tempDamageData = _characterData.SpecialDamageData.Copy();
		tempDamageData.Dealer = _dmgDealerSelf;

		colli.transform.GetComponent<IDamageable>()
			.Damage(Quaternion.FromToRotation(Vector3.right,
			(colli.transform.position - transform.position).ZeroY().normalized + _rigidB.velocity.normalized) * SO_Character.SpecialEjection.Multiply(Axis.x, _characterData.CharacterStats.strength) + additionnalDirection,
			colli.ClosestPointOnBounds(transform.position),
			tempDamageData);
	}
}
