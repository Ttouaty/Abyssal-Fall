using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public struct Dash
{
	public float endingLag;
	public Vector2[] Forces;
	public float Impact;

	[HideInInspector]
	public bool inProgress;
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
		new FmodSoundEvent("OnSpecialRestored"),
		new FmodSoundEvent("OnParry")
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
		if (tempArray.Length < 7)
			Debug.LogWarning("PlayerSoundList doesn't seem to have all base sounds.\n\"OnDashStart\",\"OnDashEnd\",\"OnDeath\",\"OnHit\",\"OnSpecialActivate\",\"OnSpecialRestored\",\"OnParry\".");
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
			FMODUnity.RuntimeManager.PlayOneShotAttached(FmodEvent, Camera.main.gameObject);

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
			
		}
	}

	[HideInInspector]
	public Spawn Spawn;
	[HideInInspector]
	public Animator _animator;
	private NetworkAnimator _networkAnimator;
	[HideInInspector]
	public bool _isDead = false;
	[HideInInspector]
	public bool IsGrounded = false;
	[HideInInspector]
	public bool _isInDebugMode = false;


	protected CharacterProp _characterProp;

	#region references

	protected Transform _transf;
	protected Rigidbody _rigidB;
	protected FacialExpresionModule _FEMref;
	protected RaycastHit _hit;

	#endregion
	protected Vector3 _activeSpeed = Vector3.zero; // Activespeed est un vecteur qui est appliqué a chaque frame au rigibody.velocity => permet de modifier librement la vitesse du player.
	protected Vector3 _activeDirection = Vector3.forward;
	protected Vector2 _originalMaxSpeed;
	protected Vector2 _maxSpeed = new Vector2(7f, 20f);
	protected Vector2 _acceleration = new Vector2(0.1f, -2f); // X => time needed to reach max speed, Y => Gravity multiplier
	protected float _friction = 80; //friction applied to the player when it slides (pushed or end dash) (units/s)
	protected float _parryTime = 0.08f; //time at the beginning of a Dash when the player is in countering attacks

	//protected float _airborneDelay = 0.02f;

	protected float _fullDashActivationTime = 0.08f;
	private float _timeHeldDash;
	[HideInInspector]
	public bool WaitForDashRelease = false;
	private bool _dashMaxed = false;

	protected int _dashActivationSteps;
	protected int _dashStepActivated = 1;
	[Space]
	public SO_Character _characterData;

	protected TimeCooldown _specialCooldown;
	protected TimeCooldown _stunTimer; //Seconds of stun on Hit
	protected TimeCooldown _invulTimer; //Seconds of invulnerability
	protected TimeCooldown _parryTimer; //Seconds of Parrying
										//protected TimeCooldown _airborneTimeout; //Time before being considered airborne
	protected TimeCooldown _forcedAirbornTimeout;

	protected DamageDealer _dmgDealerSelf;
	public DamageDealer DmgDealerSelf { get { return _dmgDealerSelf; } }
	private DamageDealer _lastDamageDealer;
	public DamageDealer LastDamageDealer
	{
		get { return _lastDamageDealer; }
		set
		{
			_lastDamageDealerTimeOut.Set(5);
			_lastDamageDealer = value;
		}
	}
	private TimeCooldown _lastDamageDealerTimeOut;

	protected bool _isAffectedByFriction = true;
	protected bool _isFrozen = false;
	protected bool _internalAllowInput = true;
	protected bool _allowInput { get { return _internalAllowInput && !_isFrozen; } set { _internalAllowInput = value; } }
	public bool AllowInput { get { return _allowInput; } }
	protected bool _isStunned = false;
	public bool _isParrying { get { return _parryTimer.TimeLeft != 0; } }
	/// <summary>
	/// WARNING, use this instead of isLocalPlayer on PlayerControllers as they are not directly a player object
	/// </summary>
	public bool _isLocalPlayer { get { return _playerRef.isLocalPlayer || _isInDebugMode; } }
	[HideInInspector]
	public bool AllowDash = true;
	[HideInInspector]
	public bool AllowSpecial = true;

	protected bool _canSpecial
	{
		get
		{
			return AllowSpecial && !_characterData.Dash.inProgress && !_isDead && _specialCooldown.TimeLeft <= 0;
		}
	}

	protected bool _canDash
	{
		get
		{
			return AllowDash && !_isDead && !_dashMaxed && (_characterData.Dash.inProgress || _allowInput);
		}
	}

	public void Freeze()
	{
		_allowInput = false;
		_isFrozen = true;
		_rigidB.velocity = Vector3.zero;
		_rigidB.isKinematic = true;
		_activeSpeed = Vector3.zero;
		//_animator.SetTrigger("Reset");
	}

	public void UnFreeze()
	{
		_isFrozen = false;
		_allowInput = true;
		_rigidB.isKinematic = false;
	}

	[ClientRpc]
	public void RpcUnFreeze()
	{
		UnFreeze();
	}

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

		if (_characterData == null)
		{
			Debug.Log("No SO_Character found for character " + gameObject.name + ". Seriously ?");
			return;
		}

		CameraManager.Instance.AddTargetToTrack(transform);
		CharacterModel playerMesh = _transf.GetComponentInChildren<CharacterModel>();
		_characterData.SoundList.Generate();

		playerMesh.Reskin(_playerRef.SkinNumber);

		_animator = playerMesh.GetComponentInChildren<Animator>();
		_characterProp = transform.GetComponentInChildren<CharacterProp>();
		_networkAnimator = GetComponent<NetworkAnimator>();

		for (int i = 0; i < _animator.parameterCount; i++)
		{
			_networkAnimator.SetParameterAutoSend(i, true);
		}

		if (_characterProp == null)
			Debug.LogError("No player prop found in playermesh: " + gameObject.name + " !");

		_dashActivationSteps = _characterData.Dash.Forces.Length;
		_characterData.Dash.inProgress = false; // security because sometimes dash is activated, lolwutomfgrektbbq

		_specialCooldown = new TimeCooldown(this);
		_specialCooldown.onFinish = OnSpecialReset;

		_stunTimer = new TimeCooldown(this);
		_FEMref = playerMesh.GetComponent<CharacterModel>().FEMref;
		_stunTimer.onFinish = () => { _isStunned = false; _allowInput = true; _FEMref.SetExpression(_FEMref.StartingExpression); };
		_stunTimer.onProgress = () =>
		{
			if (!IsGrounded) { _stunTimer.Add(TimeManager.DeltaTime); }
			_allowInput = false;
			_isStunned = true;
		};//decrease Stun only if grounded

		_invulTimer = new TimeCooldown(this);
		_invulTimer.onFinish = () => { _isInvul = false; };
		_invulTimer.onProgress = () => { _isInvul = true; if (!IsGrounded) _invulTimer.Add(Time.deltaTime); };

		_parryTimer = new TimeCooldown(this);

		//_airborneTimeout = new TimeCooldown(this);
		//_airborneTimeout.onFinish = () => {	if (IsGrounded)	_airborneTimeout.Set(_airborneDelay);};
		//_airborneTimeout.onProgress = () => {
		//	if (_characterData.Dash.inProgress)
		//		return;
		//	if (IsGrounded)
		//		_airborneTimeout.Set(_airborneDelay);
		//	IsGrounded = true;
		//};

		_forcedAirbornTimeout = new TimeCooldown(this);

		_lastDamageDealerTimeOut = new TimeCooldown(this);
		_lastDamageDealerTimeOut.onFinish = OnLastDamageDealerTimeOut;
		GameManager.Instance.OnPlayerWin.AddListener(OnPlayerWin);

		_maxSpeed.x = _maxSpeed.x + _maxSpeed.x * (5 * (_characterData.CharacterStats.speed - Stats.maxValue * 0.5f) / 100);

		_originalMaxSpeed = _maxSpeed;

		if (GetComponentInChildren<GroundCheck>() == null)
		{
			Debug.LogWarning("no GroundCheck found in player: " + gameObject.name + "\nCreating one.");
			GameObject tempGo = new GameObject();
			Instantiate(tempGo);
			tempGo.transform.parent = transform;
			tempGo.AddComponent(typeof(GroundCheck));
			tempGo.transform.position = _transf.position - Vector3.up;
		}

		_dmgDealerSelf = new DamageDealer();
		_dmgDealerSelf.PlayerRef = _playerRef;
		_dmgDealerSelf.InGameName = _characterData.IngameName;
		_dmgDealerSelf.Icon = _characterData.Icon;
		_dmgDealerSelf.ObjectRef = gameObject;

		_characterData.SpecialDamageData.Dealer = _characterData.DashDamageData.Dealer = _dmgDealerSelf;

		TimeManager.Instance.OnPause.AddListener(OnPause);
		TimeManager.Instance.OnResume.AddListener(OnResume);
		TimeManager.Instance.OnTimeScaleChange.AddListener(OnTimeScaleChange);

		_activeDirection = transform.rotation * Vector3.forward;

		GetComponent<Rigidbody>().velocity = Vector3.zero;
		CustomStart();
	}

	public void AddDifferentialAlpha(Material characterAlpha)
	{
		Debug.Log("Need to code that, faggot !");
	}

	protected void Update()
	{
		if (_isDead || TimeManager.IsPaused || _playerRef == null)
			return;

		if (!_isLocalPlayer)
			return;

		//had to do that :p
		if (_forcedAirbornTimeout.TimeLeft > 0)
			IsGrounded = false;

		ProcessCoolDowns();

		ProcessActiveSpeed();
		if (_allowInput)
			ProcessOrientation();

		ProcessInputs();
		ApplyCharacterFinalVelocity();

		_animator.SetBool("IsGrounded", IsGrounded);

		if (IsGrounded)
			_rigidB.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
		else
			_rigidB.constraints = RigidbodyConstraints.FreezeRotation;

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
		if (_characterProp.PropRenderer != null)
			_characterProp.PropRenderer.enabled = true;
		if (_characterProp.PropRespawnParticles != null)
			_characterProp.PropRespawnParticles.Play();
	}


	protected virtual void OnPlayerWin(Player winner)
	{
		_allowInput = false;
		if (winner == _playerRef)
		{
			Debug.LogWarning("Character " + _characterData.IngameName + " controller by player N°=> " + _playerRef.PlayerNumber + " has won !");
			_FEMref.SetExpression("Happy");
		}
		//enabled = false;
		Freeze();
		//_animator.SetTrigger("Reset");
	}

	#endregion


	#region Processes
	private void ProcessOrientation()
	{
		Vector3 oldDirection = transform.forward;
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

		transform.LookAt(transform.position + _activeDirection, Vector3.up);
	}

	protected void OnFlip()
	{
		//IMPORTANT, will need an flip animation

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
			if (_dashStepActivated <= (_dashActivationSteps - 1) * (_timeHeldDash / _fullDashActivationTime) + 1 && _characterData.Dash.Forces.Length > _dashStepActivated - 1)
			{
				Eject(Quaternion.FromToRotation(Vector3.right, _activeDirection) * _characterData.Dash.Forces[_dashStepActivated - 1]);

				if (_dashStepActivated == 1)
					StartCoroutine(ActivateDash());

				_dashStepActivated++;

				_dashMaxed = _dashStepActivated > _dashActivationSteps;
				WaitForDashRelease = _dashMaxed;
			}
			_timeHeldDash += Time.deltaTime;
		}
		else if (_characterData.Dash.inProgress)
		{
			WaitForDashRelease = true;
		}

		if (InputManager.GetButtonUp("Dash", _playerRef.JoystickNumber) && IsGrounded)
			WaitForDashRelease = false;

		if (SpecialActivation() && _allowInput)
		{
			_specialCooldown.Set(_characterData.SpecialCoolDown);

			_networkAnimator.SetTrigger("Special");
			if (NetworkServer.active)
				_animator.ResetTrigger("Special");

			SpecialAction();
			_stunTimer.Add(_characterData.SpecialLag);
		}
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
				ApplyFriction();

			if (secureAllowInput)
			{
				Vector3 tempStickPosition = InputManager.GetStickDirection(_playerRef.JoystickNumber);
				tempStickPosition.z = tempStickPosition.y;
				tempStickPosition.y = 0;

				_activeSpeed = tempStickPosition * (_activeSpeed.magnitude + ((_maxSpeed.x / _acceleration.x) * TimeManager.DeltaTime + _friction * TimeManager.DeltaTime));

				_activeSpeed = Vector3.ClampMagnitude(_activeSpeed.ZeroY(), _maxSpeed.x);

				_activeSpeed = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeSpeed;
			}
		}
		else
		{
			_activeSpeed.y += _acceleration.y * TimeManager.DeltaTime * Physics.gravity.magnitude;
			_activeSpeed.y = Mathf.Clamp(_activeSpeed.y, -_maxSpeed.y, _maxSpeed.y * 10f);
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
		projectile.GetComponent<ABaseProjectile>().Launch(projPosition, projDirection, _characterData.SpecialDamageData, gameObject.GetInstanceID());
		//NetworkServer.SpawnWithClientAuthority(projectile, connectionToClient);
		NetworkServer.Spawn(projectile);
		_characterData.SoundList["OnSpecialActivate"].Play(projectile);

		if (ArenaManager.Instance != null)
			projectile.transform.parent = ArenaManager.Instance.SpecialsRoot;
	}

	[Command]
	public virtual void CmdSetIsInvul(bool invulValue)
	{
		_isInvulInternal = invulValue;
	}

	public void Kill()
	{
		Debug.Log("Player n°" + _playerRef.PlayerNumber + " with character " + _characterData.IngameName + " is DED!");

		_FEMref.SetExpression("Fear");
		_characterData.SoundList["OnDeath"].Play(gameObject);

		//_animator.SetTrigger("Death");
		if (_isLocalPlayer)
			_networkAnimator.SetTrigger("Death");

		if (NetworkServer.active)
		{
			Debug.Log("Kill detected from server for character " + _characterData.IngameName + " / Player n°=> " + _playerRef.PlayerNumber);
			_animator.ResetTrigger("Death");
		}

		_isDead = true;
		Player killer = _playerRef.Controller.LastDamageDealer != null ? _playerRef.Controller.LastDamageDealer.PlayerRef : null;
		GameManager.Instance.OnLocalPlayerDeath.Invoke(_playerRef, killer);
		CameraManager.Instance.RemoveTargetToTrack(transform);
	}

	[ClientRpc]
	public void RpcRespawn(Vector3 newPos)
	{
		_isDead = false;
		CameraManager.Instance.AddTargetToTrack(transform);

		if (_isLocalPlayer)
		{
			transform.position = newPos;
			_networkAnimator.SetTrigger("Reset");
		}

		if (NetworkServer.active)
			_animator.ResetTrigger("Reset");
	}

	public void Eject(Vector3 direction)
	{
		if (direction.y > 0)
			ForceAirborne(0.1f);

		_activeSpeed = direction;
		_rigidB.velocity = _activeSpeed;
	}

	[ClientRpc]
	public void RpcDamage(Vector3 direction, float newStunTime)
	{
		if (_isLocalPlayer)
		{
			if (direction.magnitude > 15)
			{
				Debug.Log("strong push detected");
				CameraManager.Shake(ShakeStrength.High);
			}
			else
				CameraManager.Shake(ShakeStrength.Medium);

			Eject(direction);

			if (newStunTime > 0)
				_stunTimer.Set(newStunTime);
			_invulTimer.Set(newStunTime * 1.3f);

			_activeDirection = -direction.ZeroY().normalized;
			transform.LookAt(transform.position + _activeDirection, Vector3.up);

			_characterData.SoundList["OnHit"].Play(gameObject);
			//_animator.SetTrigger("Hit");

			_networkAnimator.SetTrigger("Hit");

			if (NetworkServer.active)
				_animator.ResetTrigger("Hit");
		}
	}

	public void Damage(Vector3 direction, Vector3 impactPoint, DamageData data)
	{
		if (_isInvul)
			return;

		_FEMref.SetExpression("Pain");

		direction.x += direction.x * ((0.5f - _characterData.CharacterStats.resistance.Percentage(0, Stats.maxValue)) * 0.5f);
		direction.z += direction.z * ((0.5f - _characterData.CharacterStats.resistance.Percentage(0, Stats.maxValue)) * 0.5f);

		if (NetworkServer.active)
		{
			Debug.Log("Character \"" + _characterData.IngameName + "\" was damaged by: \"" + data.Dealer.InGameName + "\"");
			LastDamageDealer = data.Dealer;
			RpcDamage(direction, data.StunInflicted);
		}

	}

	public void Parry(ABaseProjectile projectileParried)
	{
		if (!_isLocalPlayer)
			return;

		CameraManager.Shake(ShakeStrength.Medium);
		_characterData.SoundList["OnParry"].Play(gameObject);
		projectileParried.Parry(DmgDealerSelf);
	}

	public void AddStun(float stunTime) { _stunTimer.Add(stunTime); }
	public void SetStun(float stunTime) { _stunTimer.Set(stunTime); }


	protected virtual IEnumerator ActivateDash()
	{
		_characterData.Dash.inProgress = true;
		_isInvul = true;
		_allowInput = false;
		_parryTimer.Set(_parryTime);

		_stunTimer.Add(_characterData.Dash.endingLag);

		//_animator.SetTrigger("Dash_Start");
		_networkAnimator.SetTrigger("Dash_Start");

		if (NetworkServer.active)
			_animator.ResetTrigger("Dash_Start");
		_characterData.SoundList["OnDashStart"].Play(gameObject);
		gameObject.layer = LayerMask.NameToLayer("PlayerInvul");

		ForceAirborne(0.4f);

		yield return new WaitForSeconds(0.1f);
		while (!IsGrounded)
		{
			DashAirControl();
			yield return null;
		} //wait for landing

		gameObject.layer = LayerMask.NameToLayer("PlayerDefault");

		_isInvul = false;
		//_animator.SetTrigger("Dash_End");
		_networkAnimator.SetTrigger("Dash_End");

		if (NetworkServer.active)
			_animator.ResetTrigger("Dash_End");
		_characterData.SoundList["OnDashEnd"].Play(gameObject);

		yield return new WaitForSeconds(_characterData.Dash.endingLag);
		_characterData.Dash.inProgress = false;
		_allowInput = true;

		_timeHeldDash = 0;
		_dashStepActivated = 1;
		_dashMaxed = false;
		if (!InputManager.GetButtonHeld("Dash", _playerRef.JoystickNumber))
			WaitForDashRelease = false;
	}

	protected virtual void DashAirControl()
	{
		Vector3 directionHeld = InputManager.GetStickDirection(_playerRef.JoystickNumber);
		directionHeld.z = directionHeld.y;
		directionHeld = Quaternion.FromToRotation(Vector3.right, Camera.main.transform.right.ZeroY().normalized) * directionHeld.ZeroY();

		_activeSpeed += directionHeld * (0.035f + 0.005f * _characterData.CharacterStats.speed);
	}

	public void ForceAirborne(float timeForced = 0)
	{
		//_airborneTimeout.Set(0);
		IsGrounded = false;
		if (timeForced == 0)
			_forcedAirbornTimeout.Set(10000000);
		else
			_forcedAirbornTimeout.Set(timeForced);
	}


	protected void OnCollisionEnter(Collision colli)
	{
		if(_playerRef.isLocalPlayer)
			PlayerCollisionHandler(colli);
	}

	protected void OnCollisionStay(Collision colli)
	{
		if(_playerRef.isLocalPlayer)
			PlayerCollisionHandler(colli);
	}

	protected virtual void PlayerCollisionHandler(Collision colli)
	{
		if (colli.gameObject.GetComponent<IDamageable>() != null && _characterData.Dash.inProgress)
		{
			colli.gameObject.GetComponent<IDamageable>()
				.Damage(Quaternion.FromToRotation(Vector3.right,
				(colli.transform.position - transform.position).ZeroY().normalized) * (SO_Character.SpecialEjection.Multiply(Axis.x, _characterData.Dash.Impact)),
				colli.contacts[0].point,
				_characterData.DashDamageData);
		}
		//else if (colli.gameObject.layer == LayerMask.NameToLayer("Wall") && _characterData.Dash.inProgress)
		//{
		//	if (Vector3.Dot((colli.transform.position - transform.position), _activeSpeed) > 0) // if you are going towards a wall
		//	{
		//		_activeSpeed = _activeSpeed.ZeroX().ZeroZ(); // blocks you
		//		_rigidB.velocity = _activeSpeed;
		//	}
		//}
	}
}
