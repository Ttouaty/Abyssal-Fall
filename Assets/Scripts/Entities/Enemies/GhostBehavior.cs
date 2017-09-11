using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;

public class GhostBehavior : NetworkBehaviour, IDamageable, IDamaging, IPoolable
{
	public static Player GhostPlayer;
	[HideInInspector]
	public PlayerController[] AvailableTargets;

	public float MovemementSpeed = 2;
	public float ImpactForce = 2;
	public Sprite GhostIcon;

	public DamageData AttackStats;

	private PlayerController ActiveTarget;
	private Rigidbody _rigidB;
	private NetworkAnimator _animator;
	private Projector _shadow;

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

	void Start()
	{
		_shadow = GetComponentInChildren<Projector>(true);
	}

	public void Init(PlayerController[] targets)
	{
		if (GhostPlayer == null)
		{
			GhostPlayer = ServerManager.Instance.ForceAddPlayer();
			GhostPlayer.PlayerNumber = 0;
		}

		AvailableTargets = targets;

		_dmgDealerSelf = new DamageDealer();
		_dmgDealerSelf.PlayerRef = GhostPlayer;
		_dmgDealerSelf.InGameName = "Ghost";
		_dmgDealerSelf.Icon = GhostIcon;
		_dmgDealerSelf.ObjectRef = gameObject;

		AttackStats.Dealer = _dmgDealerSelf;

		_rigidB = GetComponent<Rigidbody>();
		_animator = GetComponent<NetworkAnimator>();
		_shadow = GetComponentInChildren<Projector>(true);

		gameObject.SetActive(true);
		NetworkServer.Spawn(gameObject);

		if(GhostPlayer.Icon == null) //Wait for ghost to be spawned
			RpcSetGhostPlayerIcon(GhostPlayer.gameObject);

		_animator.BroadCastTrigger("Spawn");

		_isAlive = true;
		_isInit = true;
	}

	[ClientRpc]
	public void RpcSetGhostPlayerIcon(GameObject playerRef) { playerRef.GetComponent<Player>().Icon = GhostIcon; }

	public void OnFinishSpawn()
	{
		if (NetworkServer.active)
			HasFinishedSpawning = true;
	}

	public void ReturnToPool()
	{
		if (NetworkServer.active)
		{
			HasFinishedSpawning = false;
			ActiveTarget = null;

			GameObjectPool.AddObjectIntoPool(gameObject);
			gameObject.SetActive(false);
			_rigidB.velocity = Vector3.zero;
			if (isServer)//if isspawned
				NetworkServer.UnSpawn(gameObject);
		}
	}

	void Update()
	{
		_shadow.transform.rotation = Quaternion.Euler(90, 0, 0);

		if (!NetworkServer.active || !IsActive || TimeManager.TimeScale < 1)
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
		if (!IsActive || Sender.Dealer == null)
			return;

		Kill(direction);

		GameManager.Instance.OnLocalPlayerDeath.Invoke(GhostPlayer, Sender.Dealer.PlayerRef);
	}

	public void Kill(Vector3 direction)
	{
		_isAlive = false;
		_rigidB.velocity = Vector3.zero;

		transform.LookAt(transform.position - direction.ZeroY().normalized);
		_animator.BroadCastTrigger("Kill");
	}

	public void OnHurtBoxCollide(Collider colli)
	{
		if (!IsActive || !NetworkServer.active)
			return;

		if(colli.GetComponentInParent<IDamaging>().DoContactDamage)
			Damage(transform.position - colli.transform.position, Vector3.zero, colli.GetComponentInParent<IDamaging>().DmgDealerSelf.DamageData);
	}

	public void OnHitBoxCollide(Collider colli)
	{
		if (!IsActive || !NetworkServer.active)
			return;

		if (colli.GetComponentInParent<IDamageable>() != null)
		{
			colli.GetComponentInParent<IDamageable>().Damage(
				Quaternion.FromToRotation(Vector3.right, (colli.transform.position - transform.position).ZeroY().normalized) 
				* SO_Character.SpecialEjection.Multiply(Axis.x, ImpactForce),  
				transform.position, AttackStats);
			Player.LocalPlayer.PlaySoundForAll(SoundManager.Instance.GetFmodKeyOfOS("Hammer Hit Character"));
		}
	}

	public void OnGetFromPool() { }
	public void OnReturnToPool() { }
}
