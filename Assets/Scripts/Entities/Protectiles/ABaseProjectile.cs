using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider), typeof(Poolable))]
public abstract class ABaseProjectile : NetworkBehaviour, IPoolable
{
	protected DamageDealer _shooter;
	protected Rigidbody _rigidB;
	protected Vector3 _ejectionForce;
	protected DamageData _ownDamageData;
	protected float _maxLifeSpan = 5;
	[SyncVar]
	protected NetworkInstanceId LauncherNetId;


	[SerializeField]
	protected int _speed = 20;

	protected virtual void Awake()
	{
		_rigidB = GetComponentInChildren<Rigidbody>();
		GetComponentInChildren<Collider>().isTrigger = true;
		if (GetComponent<Poolable>() == null)
			Debug.LogWarning("Projectile "+gameObject.name+" has no component \"Poolable\" this may break stuff!");
	}

	public virtual void Launch(Vector3 Position, Vector3 Direction, DamageData data, NetworkInstanceId newLauncherId)
	{
		gameObject.SetActive(true);
		_shooter = data.Dealer;
		 
		if (_shooter.PlayerRef != null) 
		{
			_ejectionForce = SO_Character.SpecialEjection.Multiply(Axis.x, _shooter.PlayerRef.Controller._characterData.CharacterStats.strength);
		}
		else // Cheat if environnement
		{
			_ejectionForce = SO_Character.SpecialEjection.Multiply(Axis.x, 5);
		}

		_ownDamageData = data.SetProjectile(this).Copy();
		LauncherNetId = newLauncherId; 

		//Only activate collision is launcher is local
		//GetComponentInChildren<Collider>().enabled = _ownDamageData.Dealer.PlayerRef.isLocalPlayer;
		//Deactivated (removed collisions entierly)

		transform.position = Position;
		transform.rotation = Quaternion.LookRotation(Direction, Vector3.up);

		_rigidB.velocity = Direction.normalized * _speed;

		StartCoroutine(DelayStop());
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if(NetworkServer.active)
			RpcSendOnLaunch(_shooter.ObjectRef);
	}

	[ClientRpc]
	protected void RpcSendOnLaunch(GameObject Launcher)
	{
		OnLaunch(_shooter.ObjectRef);
	}

	protected virtual void OnLaunch(GameObject Launcher) { }

	private IEnumerator DelayStop()
	{
		yield return new WaitForSeconds(_maxLifeSpan);
		Stop();
	}

	protected virtual void Stop()
	{
		_rigidB.velocity = Vector3.zero;
		//_ownDamageData = null;
		StopAllCoroutines();

		if (NetworkServer.active)
			NetworkServer.UnSpawn(gameObject);
		GameObjectPool.AddObjectIntoPool(gameObject);
	}

	public virtual void OnGetFromPool()
	{
	}

	public virtual void OnReturnToPool()
	{
		//gameObject.SetActive(false);
	}

	private void OnTriggerEnter(Collider colli)
	{
		if (colli.GetComponent<IDamageable>() != null)
		{
			if(colli.gameObject.GetComponentInParent<NetworkIdentity>().netId != LauncherNetId)
			{
				OnHitPlayer(colli.GetComponent<IDamageable>());
			}
			else
			{
				Debug.Log("Same netid detected / own => "+LauncherNetId+" / colli => "+ colli.gameObject.GetComponentInParent<NetworkIdentity>().netId); 
			}
		}
		else if (colli.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			//_audioSource.PlayOneShot(OnHitObstacle);
			OnHitEnvironnement();
		}
	}

	public virtual void OnHitPlayer(IDamageable damagedEntity)
	{
		DamageEntity(damagedEntity);
	}

	public virtual void DamageEntity(IDamageable damagedEntity)
	{
		damagedEntity.Damage(Quaternion.FromToRotation(Vector3.right, _rigidB.velocity.ZeroY()) * _ejectionForce,
				transform.position,
				_ownDamageData);
	}

	public virtual void OnHitEnvironnement()
	{
		Stop();
	}

	public virtual void Parry(DamageDealer characterParrying)
	{
		StopAllCoroutines();
		_ownDamageData.Dealer = characterParrying;
		
		Launch(transform.position, (_shooter.ObjectRef.transform.position - transform.position).ZeroY(), _ownDamageData, characterParrying.ObjectRef.GetComponent<NetworkIdentity>().netId);
	}
}
