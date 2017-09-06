using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;

public class GhostBehavior : NetworkBehaviour, IDamageable, IDamaging
{
	public static Player EnvironnementPlayer;
	[HideInInspector]
	public PlayerController[] AvailableTargets;

	public float MovemementSpeed = 2;
	public float ImpactForce = 2;
	public Sprite GhostIcon;

	public DamageData AttackStats;

	private PlayerController ActiveTarget;
	private Rigidbody _rigidB;
	private Animator _animator;

	private DamageDealer _dmgDealerSelf;
	public DamageDealer DmgDealerSelf
	{
		get
		{
			return _dmgDealerSelf;
		}
	}

	private bool _isInit = false;
	private bool _isAlive = true;
	[HideInInspector]
	public bool HasFinishedSpawning = false;

	public bool IsActive
	{
		get
		{
			return _isInit && HasFinishedSpawning && _isAlive;
		}
	}

	public bool DoContactDamage { get { return true; } }

	public void Init(PlayerController[] targets)
	{
		if (EnvironnementPlayer == null)
			EnvironnementPlayer = ServerManager.Instance.ForceAddPlayer();

		AvailableTargets = targets;

		_dmgDealerSelf = new DamageDealer();
		_dmgDealerSelf.PlayerRef = EnvironnementPlayer;
		_dmgDealerSelf.InGameName = "Ghost";
		_dmgDealerSelf.Icon = GhostIcon;
		_dmgDealerSelf.ObjectRef = gameObject;

		AttackStats.Dealer = _dmgDealerSelf;

		_rigidB = GetComponent<Rigidbody>();
		_animator = GetComponent<Animator>();

		_animator.SetTrigger("Spawn");

		_isInit = true;
	}

	public void OnFinishSpawn()
	{
		HasFinishedSpawning = true;
	}

	void Update()
	{
		if (!NetworkServer.active || !IsActive)
			return;
		//Server only

		SearchForBestTarget();
		SeekTarget();
	}

	public void SearchForBestTarget()
	{
		
		for (int i = 0; i < AvailableTargets.Length; i++)
		{
			if (AvailableTargets[i].IsDead)
				continue;

			if (ActiveTarget == null)
			{
				ActiveTarget = AvailableTargets[i];
				continue;
			}

			if(Vector3.Distance(AvailableTargets[i].transform.position, transform.position) < Vector3.Distance(ActiveTarget.transform.position, transform.position))
			{
				ActiveTarget = AvailableTargets[i];
				Debug.Log("Switch target to => "+ ActiveTarget.name);
			}
		}
	}

	public void SeekTarget()
	{
		if (ActiveTarget == null)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((CameraManager.Instance.transform.position - transform.position).normalized, Vector3.up), 5 * Time.deltaTime);
			_rigidB.velocity = Vector3.zero;
			return;
		}

		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((ActiveTarget.transform.position - transform.position).normalized, Vector3.up), 5 * Time.deltaTime);
		_rigidB.velocity = (ActiveTarget.transform.position - transform.position).ZeroY().normalized * MovemementSpeed;

		if (ActiveTarget.IsDead)
			ActiveTarget = null;
	}

	public int GetTeamIndex()
	{
		return -1;
	}

	public void Damage(Vector3 direction, Vector3 impactPoint, DamageData Sender)
	{
		if (!IsActive)
			return;

		_isAlive = false;
		_rigidB.velocity = Vector3.zero;

		transform.LookAt(transform.position - direction.ZeroY().normalized);
		_animator.SetTrigger("Kill");

		// Send server +1 point
		Destroy(gameObject,1);
	}

	public void OnHurtBoxCollide(Collider colli)
	{
		if (!IsActive)
			return;

		if(colli.GetComponentInParent<IDamaging>().DoContactDamage)
			Damage(transform.position - colli.transform.position, Vector3.zero, colli.GetComponentInParent<IDamaging>().DmgDealerSelf.DamageData);
	}

	public void OnHitBoxCollide(Collider colli)
	{
		if (!IsActive)
			return;

		if (colli.GetComponentInParent<IDamageable>() != null)
			colli.GetComponentInParent<IDamageable>().Damage(
				Quaternion.FromToRotation(Vector3.right, (colli.transform.position - transform.position).ZeroY().normalized) 
				* SO_Character.SpecialEjection.Multiply(Axis.x, ImpactForce),  
				transform.position, AttackStats);
	}

}
