using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider), typeof(AudioSource))]
public abstract class ABaseProjectile : MonoBehaviour, IPoolable
{
	protected DamageDealer _shooter;
	protected Rigidbody _rigidB;
	protected AudioSource _audioSource;

	[SerializeField]
	protected int _speed = 20;

	protected virtual void Awake()
	{
		_audioSource = GetComponentInChildren<AudioSource>();
		_rigidB = GetComponentInChildren<Rigidbody>();
		GetComponentInChildren<Collider>().isTrigger = true;
	}

	public virtual void Launch(Vector3 Position, Vector3 Direction, DamageDealer Shooter)
	{
		gameObject.SetActive(true);
		_shooter = Shooter;

		transform.position = Position;
		transform.rotation = Quaternion.LookRotation(Direction, Vector3.up);

		_rigidB.velocity = Direction.normalized * _speed;
	}

	protected virtual void Stop()
	{
		_rigidB.velocity = Vector3.zero;
		GameObjectPool.AddObjectIntoPool(gameObject);
	}

	public virtual void OnGetFromPool()
	{
	}

	public virtual void OnReturnToPool()
	{
		gameObject.SetActive(false);
	}

	private void OnTriggerEnter(Collider colli)
	{
		if (colli.GetComponent<IDamageable>() != null)
		{
			if (colli.gameObject.GetInstanceID() == _shooter.PlayerRef.Controller.gameObject.GetInstanceID())
				return;
			OnHitPlayer(colli.GetComponent<IDamageable>());
		}
		else if (colli.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			//_audioSource.PlayOneShot(OnHitObstacle);
			OnHitEnvironnement();
		}
	}

	public virtual void OnHitPlayer(IDamageable damagedEntity)
	{
		damagedEntity.Damage(Quaternion.FromToRotation(Vector3.right, _rigidB.velocity.ZeroY()) * _shooter.PlayerRef.Controller._characterData.SpecialEjection.Multiply(Axis.x, _shooter.PlayerRef.Controller._characterData.CharacterStats.strength),
				transform.position,
				_shooter.PlayerRef.Controller._characterData.SpecialDamageData);
	}

	public virtual void OnHitEnvironnement()
	{
		Stop();
	}
}
