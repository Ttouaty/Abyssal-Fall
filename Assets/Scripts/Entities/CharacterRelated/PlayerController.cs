using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public struct Dash
{
	public float endingLag;
	public float rechargeTime;
	public Vector2[] Forces;
	public float Impact;
}

[Serializable]
public class PlayerSoundList
{
	public List<FmodSoundEvent> SoundList = new List<FmodSoundEvent>
	{
		new FmodSoundEvent("OnDashStart"),
		new FmodSoundEvent("OnDashEnd"),
		new FmodSoundEvent("OnDeath"),
		new FmodSoundEvent("OnHit"),
		new FmodSoundEvent("OnSpecialActivate"),
		new FmodSoundEvent("OnSpecialRestored")
	};
	private Dictionary<string, FmodSoundEvent> _soundDico = new Dictionary<string, FmodSoundEvent>();

	public FmodSoundEvent this[string Key]
	{
		get
		{
			if (!_soundDico.ContainsKey(Key))
			{
				Debug.LogWarning(Key + " was not found in PlayerAudioList");
				return new FmodSoundEvent(Key);
			}

			return _soundDico[Key];
		}
	}

	public void Generate()
	{
		FmodSoundEvent[] tempArray = SoundList.ToArray();
		//if (tempArray.Length < 7)
		//Debug.LogWarning("PlayerSoundList doesn't seem to have all base sounds.\n\"OnDashStart\",\"OnDashEnd\",\"OnDeath\",\"OnHit\",\"OnSpecialActivate\",\"OnSpecialRestored\",\"OnParry\".");
		for (int i = 0; i < tempArray.Length; i++)
		{
			_soundDico[tempArray[i].Key] = tempArray[i];
		}
	}
}

[Serializable]
public class FmodSoundEvent
{
	public string Key;
	[FMODUnity.EventRef]
	public string FmodEvent;

	public FmodSoundEvent(string defaultKey = "", string newFmodEvent = "")
	{
		Key = defaultKey;
		FmodEvent = newFmodEvent;
	}

	public void Play(GameObject position = null)
	{
		if (FmodEvent.Length == 0)
		{
			Debug.LogWarning("No FmodEvent linked for sound: " + Key);
			return;
		}

		if (position != null)
			FMODUnity.RuntimeManager.PlayOneShotAttached(FmodEvent, position);
		else
			FMODUnity.RuntimeManager.PlayOneShotAttached(FmodEvent, Camera.main.gameObject);
	}
}

[Serializable]
public class FmodOneShotSound
{
	[FMODUnity.EventRef]
	public string FmodEvent;

	public FmodOneShotSound(string newFmodEvent = "")
	{
		FmodEvent = newFmodEvent;
	}

	public void Play(GameObject position = null)
	{
		if (position != null)
			FMODUnity.RuntimeManager.PlayOneShotAttached(FmodEvent, position);
		else
			FMODUnity.RuntimeManager.PlayOneShot(FmodEvent, Camera.main.transform.position + Camera.main.transform.forward * 5);
	}
}

[Serializable]
public class Stats
{
	public static int maxValue = 10;
	[Range(0, 10)]
	public int strength = 5;
	[Range(0, 10)]
	public int speed = 5;
	[Range(0, 10)]
	public int resistance = 5;
}

public interface IDamageable
{
	void Damage(Vector3 direction, Vector3 impactPoint, DamageData Sender);
}

public interface IDamaging
{
	DamageDealer DmgDealerSelf { get; }
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour, IDamageable, IDamaging
{
	[HideInInspector]
	public Player _playerRef;
	[SyncVar]
	private bool _isInvulInternal = false;
	public bool _isInvul
	{
		get { return _isInvulInternal; }
		set
		{
			CmdSetIsInvul(value);
		}
	}
	[SyncVar(hook = "OnSetPlayerLayer")]
	protected int _playerLayer;

	[HideInInspector]
	public Spawn Spawn;
	[HideInInspector]
	public Animator _animator;
	[HideInInspector]
	public NetworkAnimator _networkAnimator;
	[HideInInspector]
	protected bool _isDead = false;

	public bool IsDead
	{
		get { return _isDead; }
		private set { _isDead = value; }
	}

	[HideInInspector]
	public bool IsGrounded = false;
	[HideInInspector]
	public bool _isInDebugMode = false;
	[HideInInspector]
	public CharacterProp _characterProp;

	#region references

	protected Transform _transf;
	protected Rigidbody _rigidB;
	protected FacialExpresionModule _FEMref;
	protected RaycastHit _hit;
	protected AnimationToolkit _animToolkit;
	protected GroundCheck _groundCheck;
	protected ParticleSystem _RespawnFlash;
	#endregion
	protected Vector3 _activeSpeed = Vector3.zero; // Activespeed est un vecteur qui est appliqué a chaque frame au rigibody.velocity => permet de modifier librement la vitesse du player.

	protected Vector3 _activeDirection = Vector3.forward;


	protected Vector2 _originalMaxSpeed;
	protected Vector2 _maxSpeed = new Vector2(7f, 20f);
	protected Vector2 _acceleration = new Vector2(0.1f, -2f); // X => time needed to reach max speed, Y => Gravity multiplier
	protected float _friction = 140; //friction applied to the player when it slides (pushed or end dash) (units/s)
	protected float _parryTime = 0.08f; //time at the beginning of a Dash when the player is in countering attacks
	protected float _relicInterval = 0.2f;
	//protected float _airborneDelay = 0.02f;

	protected float _fullDashActivationTime = 0.1f;
	private float _timeHeldDash;
	[HideInInspector]
	public bool WaitForDashRelease = false;
	private bool _dashMaxed = false;

	protected int _dashActivationSteps;
	protected int _dashStepActivated = 1;
	protected Dash _dashCopy;
	[SyncVar]
	protected bool _dashing = false;
	protected float _dashDamagingTime = 0.5f;

	protected bool _isDealingDamage
	{
		get { return _damageTimer.TimeLeft > 0; }
	}

	public bool _isDashing
	{
		get { return _dashing; }
		set
		{
			CmdSetIsDashing(value);
		}
	}
	[Space]
	public SO_Character _characterData;

	protected TimeCooldown _specialCooldown;
	protected TimeCooldown _stunTimer; //Seconds of stun on Hit
	protected TimeCooldown _invulTimer; //Seconds of invulnerability
	protected TimeCooldown _parryTimer; //Seconds of Parrying
	protected TimeCooldown _damageTimer; //Seconds of DealingDamage
	protected TimeCooldown _dashRechargeTimer; //Seconds of dash cooldown
											   //protected TimeCooldown _airborneTimeout; //Time before being considered airborne
	protected TimeCooldown _forcedAirborneTimeout;
	protected TimeCooldown _relicTimer;

	protected DamageDealer _dmgDealerSelf;
	public DamageDealer DmgDealerSelf { get { return _dmgDealerSelf; } }
	private DamageDealer _lastDamageDealer;
	public DamageDealer LastDamageDealer
	{
		get { return _lastDamageDealer; }
		set
		{
			if (_isInDebugMode)
				_lastDamageDealerTimeOut.Set(2);
			else
				_lastDamageDealerTimeOut.Set(GameManager.Instance.GameRules.TimeBeforeSuicide);
			_lastDamageDealer = value;
		}
	}
	private TimeCooldown _lastDamageDealerTimeOut;

	protected bool _isHoldingRelic = false;
	protected bool _isAffectedByFriction = true;
	protected bool _isFrozen = false;
	protected bool _internalAllowInput = true;
	protected bool _allowInput { get { return _internalAllowInput && !_isFrozen; } set { _internalAllowInput = value; } }
	public bool AllowInput { get { return _allowInput; } }
	protected bool _isStunned
	{
		get { return _isStunnedInternal; }
		set
		{
			CmdSetIsStunnedInternal(value);
		}
	}
	[SyncVar]
	protected bool _isStunnedInternal = false;
	public bool _isParrying { get { return _parryTimer.TimeLeft != 0; } }
	/// <summary>
	/// WARNING, use this instead of isLocalPlayer on PlayerControllers as they are not directly a player object
	/// </summary>
	public bool _isLocalPlayer { get { return _playerRef.isLocalPlayer || _isInDebugMode; } }
	[HideInInspector]
	public bool IsInitiated = false;
	[HideInInspector]
	public bool AllowDash = true;
	[HideInInspector]
	public bool AllowSpecial = true;

	protected bool _canSpecial
	{
		get
		{
			return AllowSpecial && !_dashing && !_isDead && _specialCooldown.TimeLeft <= 0 && !_isHoldingRelic;
		}
	}

	protected bool _canDash
	{
		get
		{
			return AllowDash && !_isDead && !_dashMaxed && (_dashing || _allowInput) && _dashRechargeTimer.TimeLeft == 0;
		}
	}

	public void Freeze()
	{
		_allowInput = false;
		_isFrozen = true;
		_rigidB.velocity = Vector3.zero;
		_rigidB.isKinematic = true;
		_activeSpeed = Vector3.zero;
		_groundCheck.Deactivate();
		//_animator.SetTrigger("Reset");
	}

	public void UnFreeze()
	{
		_isFrozen = false;
		_allowInput = true;
		_rigidB.isKinematic = false;
		_groundCheck.Activate();
	}

	[ClientRpc]
	public void RpcUnFreeze() { UnFreeze(); }
	[ClientRpc]
	public void RpcFreeze() { Freeze(); }

	#region Unity Functions
	protected void Awake()
	{
		_transf = transform;
		_rigidB = GetComponent<Rigidbody>();
	}

	public void Init(GameObject player)
	{
		_playerRef = player.GetComponent<Player>();
		_playerRef.Controller = this;
		_playerLayer = LayerMask.NameToLayer("PlayerDefault");

		if (_characterData == null)
		{
			Debug.Log("No SO_Character found for character " + gameObject.name + ". Seriously ?");
			return;
		}


		CameraManager.Instance.AddTargetToTrack(transform);
		CharacterModel playerMesh = _transf.GetComponentInChildren<CharacterModel>();
		_characterData.SoundList.Generate();

		playerMesh.Reskin(_playerRef.SkinNumber);
		playerMesh.SetOutlineColor(_playerRef.PlayerColor);

		if (ArenaManager.Instance != null)
		{
			if (ArenaManager.Instance.CurrentArenaConfig.AmbientRamp != null)
				playerMesh.SetAmbientRamp(ArenaManager.Instance.CurrentArenaConfig.AmbientRamp);
		}


		_animToolkit = GetComponentInChildren<AnimationToolkit>();

		_characterProp = transform.GetComponentInChildren<CharacterProp>();
		_characterProp.PropRespawnParticles = _animToolkit.GetParticleSystem("repop");

		_RespawnFlash = _animToolkit.GetParticleSystem("death flash");
		ParticleSystem[] tempParticles = _RespawnFlash.GetComponentsInChildren<ParticleSystem>();

		for (int i = 0; i < tempParticles.Length; i++)
		{
			tempParticles[i].startColor = _playerRef.PlayerColor;
		}

		_animator = playerMesh.GetComponentInChildren<Animator>();
		_networkAnimator = GetComponent<NetworkAnimator>();

		for (int i = 0; i < _animator.parameterCount; i++)
		{
			_networkAnimator.SetParameterAutoSend(i, true);
		}

		if (_characterProp == null)
			Debug.LogError("No player prop found in playermesh: " + gameObject.name + " !");

		_dashCopy = _characterData.Dash;
		_dashActivationSteps = _dashCopy.Forces.Length;

		_dashRechargeTimer = new TimeCooldown(this);


		_specialCooldown = new TimeCooldown(this);
		_specialCooldown.onFinish = OnSpecialReset;

		_stunTimer = new TimeCooldown(this);
		_FEMref = playerMesh.GetComponent<CharacterModel>().FEMref;
		CmdSetExpression(_FEMref.DefaultExpression);

		_stunTimer.onFinish = () => { _isStunned = false; _allowInput = true; /*CmdSetExpression(_FEMref.DefaultExpression);*/ };
		_stunTimer.onProgress = () =>
		{
			_allowInput = false;
			_isStunned = true;
		};

		_invulTimer = new TimeCooldown(this);
		_invulTimer.onFinish = () => { _isInvul = false; };
		_invulTimer.onProgress = () =>
		{
			if (!_isInvul)
				_isInvul = true;
		};

		_parryTimer = new TimeCooldown(this);
		_damageTimer = new TimeCooldown(this);

		_forcedAirborneTimeout = new TimeCooldown(this);
		_relicTimer = new TimeCooldown(this);
		_relicTimer.onFinish = () => {
			if (!NetworkServer.active)
				return;
			if(_isHoldingRelic)
			{
				if(GameManager.Instance.GameRules != null)
					((Relic_GameRules)GameManager.Instance.GameRules).UpdatePlayerScore(_playerRef.gameObject, _relicInterval);
				//Debug.Log("+ score");
				_relicTimer.Set(_relicInterval);
			}
		};
		_lastDamageDealerTimeOut = new TimeCooldown(this);
		_lastDamageDealerTimeOut.onFinish = OnLastDamageDealerTimeOut;
		GameManager.Instance.OnPlayerWin.AddListener(OnPlayerWin);

		_maxSpeed.x = _maxSpeed.x * 0.6f + _maxSpeed.x * _characterData.CharacterStats.speed.Percentage(0, Stats.maxValue, 0.8f);

		_originalMaxSpeed = _maxSpeed;
		_groundCheck = GetComponentInChildren<GroundCheck>(true);

		if (_groundCheck == null)
		{
			Debug.LogWarning("no GroundCheck found in player: " + gameObject.name + "\nCreating one.");
			GameObject tempGo = new GameObject();
			Instantiate(tempGo);
			tempGo.transform.parent = transform;
			_groundCheck = tempGo.AddComponent<GroundCheck>();
			_groundCheck.transform.position = _transf.position - Vector3.up;
		}

		_dmgDealerSelf = new DamageDealer();
		_dmgDealerSelf.PlayerRef = _playerRef;
		_dmgDealerSelf.InGameName = _characterData.IngameName;
		_dmgDealerSelf.Icon = _characterData.Icon;
		_dmgDealerSelf.ObjectRef = gameObject;

		TimeManager.Instance.OnPause.AddListener(OnPause);
		TimeManager.Instance.OnResume.AddListener(OnResume);
		TimeManager.Instance.OnTimeScaleChange.AddListener(OnTimeScaleChange);

		_activeDirection = transform.rotation * Vector3.forward;


		GetComponent<Rigidbody>().velocity = Vector3.zero;
		//DEFAULT FREEZE TO PREVENT LAG INPUT DETECTION
		Freeze();

		IsInitiated = true;
		CustomStart();
	}

	protected void Update()
	{
		if (_isDead || TimeManager.IsPaused || _playerRef == null)
			return;

		if (!_isLocalPlayer)
			return;

		//had to do that :p
		if (_forcedAirborneTimeout.TimeLeft > 0)
			IsGrounded = false;

		ProcessCoolDowns();

		ProcessActiveSpeed();
		//if (_allowInput)
		ProcessOrientation();

		ProcessInputs();
		ApplyCharacterFinalVelocity();

		_animator.SetBool("IsGrounded", IsGrounded);

		//if (IsGrounded)
		//	_rigidB.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
		//else
		//	_rigidB.constraints = RigidbodyConstraints.FreezeRotation;

		CustomUpdate();
	}

	protected virtual void CustomStart() { }
	protected virtual void CustomUpdate() { }
	#endregion
	#region EventCallbacks
	public void OnBeforeDestroy()
	{
		TimeManager.Instance.OnPause.RemoveListener(OnPause);
		TimeManager.Instance.OnResume.RemoveListener(OnResume);
		TimeManager.Instance.OnTimeScaleChange.RemoveListener(OnTimeScaleChange);
	}

	void OnPause(float timeScale)
	{
		_animator.speed = timeScale;
	}

	void OnResume(float timeScale)
	{
		_animator.speed = timeScale;
	}

	void OnTimeScaleChange(float timeScale)
	{
		_animator.speed = timeScale;
	}

	private void OnLastDamageDealerTimeOut()
	{
		LastDamageDealer = null;
	}

	protected virtual void OnSpecialReset()
	{
		_characterData.SoundList["OnSpecialRestored"].Play(gameObject);
		if (_characterProp.PropRenderer != null)
			_characterProp.PropRenderer.gameObject.SetActive(true);
		if (_characterProp.PropRespawnParticles != null)
			_characterProp.PropRespawnParticles.Play();
	}


	protected virtual void OnPlayerWin(Player winner)
	{
		_allowInput = false;
		if (winner == _playerRef)
		{
			Debug.LogWarning("Character " + _characterData.IngameName + " controller by player N°=> " + _playerRef.PlayerNumber + " has won !");
			//CmdSetExpression("Happy");
		}
		//enabled = false;
		Freeze();
		//_animator.SetTrigger("Reset");
	}

	[Command]
	public void CmdSetExpression(string expressionName) { _FEMref.SetExpressionPrecise(expressionName, _playerRef.SkinNumber); RpcSetExpression(expressionName); }
	[ClientRpc]
	public void RpcSetExpression(string expressionName)
	{
		_FEMref.SetExpressionPrecise(expressionName, _playerRef.SkinNumber);
	}
	#endregion

	#region Processes
	private void ProcessOrientation()
	{
		Vector3 oldDirection = _activeDirection;
		if (AllowInput)
		{
			if (!InputManager.StickIsNeutral(_playerRef.JoystickNumber) && !_isStunned)
			{
				_activeDirection.x = InputManager.GetAxis("x", _playerRef.JoystickNumber);
				_activeDirection.z = InputManager.GetAxis("y", _playerRef.JoystickNumber);
				_activeDirection = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeDirection;

				if (oldDirection.AnglePercent(_activeDirection) < -0.8f)
					OnFlip();
			}
			else
			{
				_activeDirection = transform.rotation * Vector3.forward;
				_activeDirection = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeDirection;
			}
		}

		transform.LookAt(transform.position + _activeDirection, Vector3.up);
	}

	protected void OnFlip()
	{
		//IMPORTANT, will need an flip animation
		// l'oseferie d'une vie !

		//_stunTime.Add(0.1f);
		//_activeSpeed = Vector3.zero;

		//Debug.Log("Character "+_characterData.IngameName+" Fliped.");
	}

	private void ProcessCoolDowns()
	{
		_animator.SetFloat("StunTime", _stunTimer.TimeLeft);
	}


	private void ProcessInputs()
	{
		if (InputManager.GetButtonHeld("Dash", _playerRef.JoystickNumber) && !WaitForDashRelease && _canDash)
		{
			if (_dashStepActivated <= (_dashActivationSteps - 1) * (_timeHeldDash / _fullDashActivationTime) + 1 && _dashCopy.Forces.Length > _dashStepActivated - 1)
			{
				Eject(Quaternion.FromToRotation(Vector3.right, _activeDirection) * _dashCopy.Forces[_dashStepActivated - 1]);

				if (_dashStepActivated == 1)
					StartCoroutine(ActivateDash());

				_dashStepActivated++;

				_dashMaxed = _dashStepActivated > _dashActivationSteps;
				WaitForDashRelease = _dashMaxed;
			}
			_timeHeldDash += Time.deltaTime;
		}
		else if (_dashing)
		{
			WaitForDashRelease = true;
		}

		if (InputManager.GetButtonUp("Dash", _playerRef.JoystickNumber) && IsGrounded)
			WaitForDashRelease = false;

		if (SpecialActivation() && _allowInput)
		{
			_specialCooldown.Set(_characterData.SpecialCoolDown);

			_networkAnimator.BroadCastTrigger("Special");
			//if (NetworkServer.active)
			//	_animator.ResetTrigger("Special");

			SpecialAction();
			_stunTimer.Add(_characterData.SpecialLag);
		}

		if (AllowInput)
		{
			if (InputManager.GetButtonDown("Taunt", _playerRef.JoystickNumber) && IsGrounded)
			{
				Taunt();
			}
		}

	}

	public void Taunt()
	{
		_allowInput = false;
		_networkAnimator.BroadCastTrigger("Taunt");
	}

	public void FinishTaunt()
	{
		_allowInput = true;
	}

	protected virtual bool SpecialActivation()
	{
		return InputManager.GetButtonDown("Special", _playerRef.JoystickNumber) && _canSpecial;
	}

	private void ProcessActiveSpeed()
	{
		if (IsGrounded)
		{
			bool secureAllowInput = _allowInput; // just security
			if (secureAllowInput)
				_activeSpeed = Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized)) * _activeSpeed;

			if (_isAffectedByFriction)
			{
				if (secureAllowInput)
				{
					if (InputManager.StickIsNeutral(_playerRef.JoystickNumber, 0.4f))
						ApplyFriction();
				}
				else
					ApplyFriction();
			}

			if (secureAllowInput)
			{
				Vector3 tempStickPosition = InputManager.GetStickDirection(_playerRef.JoystickNumber);
				tempStickPosition.z = tempStickPosition.y;
				tempStickPosition.y = 0;

				_activeSpeed = tempStickPosition * (_activeSpeed.magnitude + ((_maxSpeed.x / _acceleration.x) * TimeManager.DeltaTime));

				_activeSpeed = Vector3.ClampMagnitude(_activeSpeed.ZeroY(), _maxSpeed.x);

				_activeSpeed = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeSpeed;
			}
		}
		else
		{
			_activeSpeed.y += _acceleration.y * TimeManager.DeltaTime * Physics.gravity.magnitude;
			_activeSpeed.y = Mathf.Clamp(_activeSpeed.y, -_maxSpeed.y, _maxSpeed.y * 10f);

			AirControl();
		}
	}

	private void ApplyFriction()
	{
		_activeSpeed.x = _activeSpeed.x.Reduce(_friction * TimeManager.DeltaTime);
		_activeSpeed.z = _activeSpeed.z.Reduce(_friction * TimeManager.DeltaTime);
	}

	public void ContactGround()
	{
		IsGrounded = true;
		_activeSpeed.y = 0f;


		//Physics.Raycast(transform.position, Vector3.down, out _hit, 3, 1 << LayerMask.NameToLayer("Ground"));

		//if (_hit.point.magnitude > 1)
		//{
		//	Vector3 tempPosition = transform.position;
		//	tempPosition.y = _hit.point.y + (transform.position.y - (GetComponent<Collider>().bounds.center.y - GetComponent<Collider>().bounds.extents.y));
		//	transform.position = tempPosition;
		//}
		//else
		//	Debug.Log("point zero detected");
		//_airborneTimeout.Set(_airborneDelay);
		_dashMaxed = false;
	}

	private void ApplyCharacterFinalVelocity()
	{
		_rigidB.velocity = _activeSpeed;
		_animator.SetFloat("SpeedCoef", _activeSpeed.ZeroY().magnitude / ((Vector3)_maxSpeed).ZeroY().magnitude);
	}


	#endregion

	protected virtual void SpecialAction()
	{
		Debug.LogWarning("No default special Action defined in PlayerController, use a child class to code a special Action: " + gameObject.name);
	}

	[Command]
	public virtual void CmdLaunchProjectile(string poolName, Vector3 projPosition, Vector3 projDirection)
	{
		GameObject projectile = GameObjectPool.GetAvailableObject(poolName);
		DamageData tempDamageData = _characterData.SpecialDamageData.Copy();
		tempDamageData.Dealer = _dmgDealerSelf;

		projectile.GetComponent<ABaseProjectile>().Launch(projPosition, projDirection, tempDamageData, netId);
		NetworkServer.SpawnWithClientAuthority(projectile, _playerRef.gameObject);
		projectile.GetComponent<ABaseProjectile>().RpcSendOnLaunch(gameObject);
		//NetworkServer.Spawn(projectile);

		if (ArenaManager.Instance != null)
			projectile.transform.parent = ArenaManager.Instance.SpecialsRoot;
	}

	[Command]
	public virtual void CmdSetIsInvul(bool invulValue)
	{
		_isInvulInternal = invulValue;

		if (invulValue)
			_playerLayer = LayerMask.NameToLayer("PlayerInvul");
		else
			_playerLayer = LayerMask.NameToLayer("PlayerDefault");

	}

	private void OnSetPlayerLayer(int value)
	{
		_playerLayer = value;
		gameObject.layer = _playerLayer;
		//Debug.Log("Set playerLayer to => " + LayerMask.LayerToName(gameObject.layer));
	}

	[Command]
	public virtual void CmdSetIsDashing(bool value)
	{
		if (value)
			_damageTimer.Add(_dashDamagingTime);
		_dashing = value;
	}

	[Command]
	public virtual void CmdSetIsStunnedInternal(bool value)
	{
		_isStunnedInternal = value;
	}

	public void Kill()
	{
		Debug.Log("Player n°" + _playerRef.PlayerNumber + " with character " + _characterData.IngameName + " is DED!");

		//CmdSetExpression("Fear");
		_characterData.SoundList["OnDeath"].Play(gameObject);

		//_animator.SetTrigger("Death");
		if (_isLocalPlayer)
			_networkAnimator.BroadCastTrigger("Death");

		if (NetworkServer.active)
		{
			Debug.Log("Kill detected from server for character " + _characterData.IngameName + " / Player n°=> " + _playerRef.PlayerNumber);
			//_animator.ResetTrigger("Death");
		}

		_isDead = true;
		Player killer = LastDamageDealer != null ? LastDamageDealer.PlayerRef : null;
		GameManager.Instance.OnLocalPlayerDeath.Invoke(_playerRef, killer);
		CameraManager.Instance.RemoveTargetToTrack(transform);

		if (NetworkServer.active)
		{
			if (_isHoldingRelic)
				OnReleaseRelic();
		}
	}

	[ClientRpc]
	public void RpcRespawn(Vector3 newPos, int respawnTileIndex)
	{
		_isDead = false;
		CameraManager.Instance.AddTargetToTrack(transform);

		if(ArenaManager.Instance.Tiles[respawnTileIndex] != null)
		{
			ArenaManager.Instance.Tiles[respawnTileIndex].Restore();
			ArenaManager.Instance.Tiles[respawnTileIndex].SetTimeLeft(ArenaManager.Instance.Tiles[respawnTileIndex].TimeLeftSave * 2, false);
		}

		if (_isLocalPlayer)
			transform.position = newPos;

		TrailRenderer[] tempTrails = GetComponentsInChildren<TrailRenderer>(true);

		for (int i = 0; i < tempTrails.Length; i++)
		{
			tempTrails[i].Clear();
		}

		_stunTimer.Set(0.5f);
		_invulTimer.Set(1);

		_dashRechargeTimer.Set(0);
		_isDashing = false;
		_allowInput = true;

		_timeHeldDash = 0;
		_dashStepActivated = 1;
		_dashMaxed = false;
		WaitForDashRelease = false;

		Freeze();
		Invoke("UnFreeze", 0.5f);

		if(_isLocalPlayer)
		{
			_networkAnimator.BroadCastTrigger("WaitForEnter");
			_networkAnimator.BroadCastTrigger("Enter");
		}

		_RespawnFlash.Clear();
		_RespawnFlash.Play();
	}

	public void Eject(Vector3 direction)
	{
		if (direction.y > 0)
			ForceAirborne(direction.y.GravityTime(9.81f * _acceleration.y * -1));

		_activeSpeed = direction;
		_rigidB.velocity = _activeSpeed;
	}

	[ClientRpc]
	public void RpcDamage(Vector3 direction, float newStunTime)
	{
		if (_isLocalPlayer)
		{
			_forcedAirborneTimeout.Add(((direction.y / _acceleration.y) / 9.81f) * 0.7f);
			Eject(direction);

			_stunTimer.Set(newStunTime);
			_invulTimer.Set(0.3f);

			_activeDirection = -direction.ZeroY().normalized;
			transform.LookAt(transform.position + _activeDirection, Vector3.up);
			_networkAnimator.BroadCastTrigger("Hit");
		}

		if (direction.magnitude > 15)
			CameraManager.Shake(ShakeStrength.Medium);
		else
			CameraManager.Shake(ShakeStrength.Low);

		_characterData.SoundList["OnHit"].Play(gameObject);

		if(NetworkServer.active)
		{
			if(_isHoldingRelic)
				OnReleaseRelic();
		}
	}

	[ClientRpc]
	public void RpcDisplayAirshotPopup()
	{
		//Debug.LogError("Airshot");
		Instantiate(GameManager.Instance.Popups["AirShot"], transform.position + Vector3.up, Camera.main.transform.rotation);
	}

	public void Damage(Vector3 direction, Vector3 impactPoint, DamageData data)
	{
		if (_isInvul || _parryTimer.TimeLeft != 0)
			return;

		_animToolkit.ActivateParticle("hit");
		//CmdSetExpression("Pain");

		direction.x = direction.x * 0.5f + direction.x * ((1f - _characterData.CharacterStats.resistance.Percentage(0, Stats.maxValue)));
		direction.z = direction.z * 0.5f + direction.z * ((1f - _characterData.CharacterStats.resistance.Percentage(0, Stats.maxValue)));
		_isInvul = true;
		_allowInput = false;

		if (NetworkServer.active)
		{
			Debug.Log("Character \"" + _characterData.IngameName + "\" was damaged by: \"" + data.Dealer.InGameName + "\"");
			float stunInflicted = data.StunInflicted;
			if (!IsGrounded && (_isStunned || _isDashing))
			{
				direction *= 1.2f;
				stunInflicted *= 1.3f;

				RpcDisplayAirshotPopup();
			}
			LastDamageDealer = data.Dealer;
			RpcDamage(direction, stunInflicted);
		}
	}

	public void Parry(ABaseProjectile projectileParried)
	{
		if (!_isLocalPlayer)
			return;

		CameraManager.Shake(ShakeStrength.Medium);
		SoundManager.Instance.PlayOSAttached("OnParry", gameObject);
		//_characterData.SoundList["OnParry"].Play(gameObject);
		projectileParried.Parry(DmgDealerSelf);
		CmdParry();
	}

	[Command]
	public void CmdParry() { RpcParry(); _animToolkit.ActivateParticle("Parry"); }
	[ClientRpc]

	public void RpcParry()
	{
		Instantiate(GameManager.Instance.Popups["Parry"], transform.position + Vector3.up, Camera.main.transform.rotation);
		//Debug.LogError("Parry");
		_animToolkit.ActivateParticle("Parry");
	}

	public void AddStun(float stunTime) { _stunTimer.Add(stunTime); }
	public void SetStun(float stunTime) { _stunTimer.Set(stunTime); }


	protected virtual IEnumerator ActivateDash()
	{
		_isDashing = true;
		_isInvul = true;
		_invulTimer.Set(_dashDamagingTime);
		_allowInput = false;
		_parryTimer.Set(_parryTime);

		_networkAnimator.BroadCastTrigger("Dash_Start");
		_characterData.SoundList["OnDashStart"].Play(gameObject);

		ForceAirborne(0.4f);

		while (!IsGrounded && !_isStunned && !IsDead)
		{
			yield return null;
		} //wait for landing || hit

		if (!IsDead)
		{
			_networkAnimator.BroadCastTrigger("Dash_End");
			_characterData.SoundList["OnDashEnd"].Play(gameObject);

			if (!_isStunned)
			{
				_stunTimer.Add(_dashCopy.endingLag);
				yield return new WaitForSeconds(_dashCopy.endingLag);
			}
		}

		_dashRechargeTimer.Set(_dashCopy.rechargeTime);
		_isDashing = false;
		_allowInput = true;

		_timeHeldDash = 0;
		_dashStepActivated = 1;
		_dashMaxed = false;
		if (!InputManager.GetButtonHeld("Dash", _playerRef.JoystickNumber))
			WaitForDashRelease = false;
	}

	protected virtual void AirControl()
	{
		if (InputManager.StickIsNeutral(_playerRef.JoystickNumber, 0.3f))
			return;

		Vector3 directionHeld = InputManager.GetStickDirection(_playerRef.JoystickNumber);
		directionHeld.z = directionHeld.y;
		directionHeld = Quaternion.FromToRotation(Vector3.right, Camera.main.transform.right.ZeroY().normalized) * directionHeld.ZeroY().normalized;


		// angle = speed * 12 ( == 120° with 10 speed) * (Time.deltaTime * 2f) (per 0.5f second) * 1 - dot() (base on anle )
		float angleGiven = _characterData.CharacterStats.speed * 12 * (Time.deltaTime * 2f) * Vector3.Dot(directionHeld.normalized, Quaternion.AngleAxis(90, Vector3.up) * _activeSpeed.ZeroY().normalized);
		_activeDirection = Quaternion.AngleAxis(angleGiven, Vector3.up) * _activeDirection;
		_activeSpeed = Quaternion.AngleAxis(angleGiven, Vector3.up) * _activeSpeed;
		//_activeSpeed += (directionHeld * (0.10f + 0.005f * _characterData.CharacterStats.speed)) * (1 - Mathf.Abs(Vector3.Dot(_activeSpeed.normalized, directionHeld.normalized)));
	}

	public void ForceAirborne(float timeForced = 0)
	{
		//_airborneTimeout.Set(0);
		IsGrounded = false;
		if (timeForced == 0)
			_forcedAirborneTimeout.Set(100);
		else
			_forcedAirborneTimeout.Set(timeForced);
	}

	void OnGrabRelic(GameObject targetRelic)
	{
		_isHoldingRelic = true;
		targetRelic.GetComponent<Relic>().Grab(transform);
		_relicTimer.Set(_relicInterval);
	}

	void OnReleaseRelic()
	{
		_isHoldingRelic = false;

		if (!IsDead)
			GetComponentInChildren<Relic>().Eject(Quaternion.Euler(0,UnityEngine.Random.Range(0,360), 0) * Vector3.one * 3);

		_relicTimer.Set(_relicInterval);
	}


	protected virtual void OnCollisionEnter(Collision colli)
	{
		if (NetworkServer.active)
			PlayerCollisionHandler(colli);
	}

	protected virtual void OnCollisionStay(Collision colli)
	{
		if (NetworkServer.active)
			PlayerCollisionHandler(colli);
	}

	protected virtual void PlayerCollisionHandler(Collision colli)
	{
		if (colli.gameObject.GetComponent<IDamageable>() != null && _dashing && _isDealingDamage)
		{
			DamageData tempDamageData = _characterData.DashDamageData.Copy();
			tempDamageData.Dealer = _dmgDealerSelf;
			colli.gameObject.GetComponent<IDamageable>()
				.Damage(Quaternion.FromToRotation(Vector3.right,
				(colli.transform.position - transform.position).ZeroY().normalized + _rigidB.velocity.ZeroY().normalized) * (SO_Character.SpecialEjection.Multiply(Axis.x, _dashCopy.Impact)),
				colli.contacts[0].point,
				tempDamageData);
		}

		
	}

	protected void OnTriggerEnter(Collider colli)
	{
		if (colli.gameObject.tag == "Relic")
		{
			if (!_isHoldingRelic && _relicTimer.TimeLeft == 0 && !colli.GetComponent<Relic>().Grabbed)
				OnGrabRelic(colli.gameObject);
		}
	}
}
