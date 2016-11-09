﻿using UnityEngine;
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
public class PlayerAudioList
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
	private bool _isGenerated = false;
	
	public FmodSoundEvent this[string Key]
	{
		get
		{
			if (!_isGenerated)
				Generate();

			if (!_soundDico.ContainsKey(Key))
			{
				Debug.LogWarning(Key+ " was not found in PlayerAudioList, generating an empty one !");
				_soundDico[Key] = new FmodSoundEvent(Key);
				SoundList.Add(_soundDico[Key]); // Just to see it inInspector
			}

			return _soundDico[Key];
		}
	}

	public void Generate()
	{
		FmodSoundEvent[] tempArray = SoundList.ToArray();
		if(tempArray.Length < 6)
			Debug.LogWarning("PlayerSoundList doesn't seem to have all base sounds.\n\"OnDashStart\",\"OnDashEnd\",\"OnDeath\",\"OnHit\",\"OnSpecialActivate\",\"OnSpecialRestored\",");
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
	void Damage(Vector3 direction, Vector3 impactPoint,  DamageData Sender);
}

public interface IDamaging
{
	DamageDealer DmgDealerSelf { get; }
}

[RequireComponent(typeof(Rigidbody), typeof(DamageDealer))]
public class PlayerController : MonoBehaviour, IDamageable, IDamaging
{
	[HideInInspector]
	public Player _playerRef;
	[HideInInspector]
	public bool _isInvul = false;
	[HideInInspector]
	public Spawn Spawn;
	[HideInInspector]
	public Animator _animator;
	[HideInInspector]
	public bool _isDead = false;
	[HideInInspector]
	public bool IsGrounded = false;

	protected CharacterProp _characterProp;

	#region references

	protected Transform _transf;
	protected Rigidbody _rigidB;
	protected RaycastHit _hit;

	#endregion

	protected Vector3 _activeSpeed = Vector3.zero; // Activespeed est un vecteur qui est appliqué a chaque frame au rigibody.velocity => permet de modifier librement la vitesse du player.
	protected Vector3 _activeDirection = Vector3.forward;
	protected Vector2 _originalMaxSpeed;
	protected Vector2 _maxSpeed = new Vector2(7f, 20f);
	protected Vector2 _acceleration = new Vector2(0.1f, -2f); // X => time needed to reach max speed, Y => Gravity multiplier
	protected float _friction = 80; //friction applied to the player when it slides (pushed or end dash) (units/s)
	//protected float _airborneDelay = 0.02f;

	protected float _fullDashActivationTime = 0.05f;
	private float _timeHeldDash;
	[HideInInspector]
	public bool WaitForDashRelease = false;
	private bool _dashMaxed = false;

	protected int _dashActivationSteps;
	protected int _dashStepActivated = 1;
	[Space]
	public SO_Character _characterData;

	protected TimeCooldown _specialCooldown;
	protected TimeCooldown _stunTime; //Seconds of stun on Hit
	protected TimeCooldown _invulTime; //Seconds of invulnerability
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
		_animator.SetTrigger("Reset");
	}

	public void UnFreeze()
	{
		_isFrozen = false;
		_allowInput = true;
		_rigidB.isKinematic = false;
	}

	#region Unity Functions
	protected void Awake()
	{
		_transf = transform;
		_rigidB = GetComponent<Rigidbody>();
	}

	public void Init(Player player)
	{
		_playerRef = player;
		_playerRef.Controller = this;
		if (_characterData == null)
		{
			Debug.Log("No SO_Character found for character " + gameObject.name + ". Seriously ?");
			return;
		}

		//Instantiates player mesh and retrieves its props and particles
		GameObject playerMesh = Instantiate(_characterData.CharacterModel.gameObject) as GameObject;
		playerMesh.transform.SetParent(_transf.FindChild("CharacterModel"));
		playerMesh.transform.localPosition = Vector3.zero;
		playerMesh.transform.localRotation = Quaternion.identity;

		_transf.GetComponentInChildren<CharacterModel>().Reskin(_characterData.CharacterMaterials[_playerRef.SkinNumber]);

		_animator = _transf.GetComponentInChildren<Animator>();
		_characterProp = transform.GetComponentInChildren<CharacterProp>();

		if (_characterProp == null)
			Debug.LogError("No player prop found in playermesh: " + gameObject.name + " !");

		_dashActivationSteps = _characterData.Dash.Forces.Length;
		_characterData.Dash.inProgress = false; // security because sometimes dash is activated, lolwutomfgrektbbq

		_specialCooldown = new TimeCooldown(this);
		_specialCooldown.onFinish = OnSpecialReset;

		_stunTime = new TimeCooldown(this);
		_stunTime.onFinish = () => { _isStunned = false; _allowInput = true; };
		_stunTime.onProgress = () =>
		{
			if (!IsGrounded) { _stunTime.Add(TimeManager.DeltaTime); }
			_allowInput = false;
			_isStunned = true;
		};//decrease Stun only if grounded

		_invulTime = new TimeCooldown(this);
		_invulTime.onFinish = () => { _isInvul = false; };
		_invulTime.onProgress = () => { _isInvul = true; if (!IsGrounded) _invulTime.Add(Time.deltaTime); };

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

		_maxSpeed.x = _maxSpeed.x + _maxSpeed.x * (10 * (_characterData.CharacterStats.speed - Stats.maxValue * 0.5f) / 100);

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

		_characterData.SpecialDamageData.Dealer = _characterData.DashDamageData.Dealer = _dmgDealerSelf;

		TimeManager.Instance.OnPause.AddListener(OnPause);
		TimeManager.Instance.OnResume.AddListener(OnResume);
		TimeManager.Instance.OnTimeScaleChange.AddListener(OnTimeScaleChange);

		CameraManager.Instance.AddTargetToTrack(transform);
		_activeDirection = transform.rotation * Vector3.forward;

		CustomStart();

	}

	protected void Update()
	{
		if (_isDead || TimeManager.IsPaused)
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


	protected virtual void OnPlayerWin()
	{
		_allowInput = false;
		//enabled = false;
		Freeze();
		_animator.SetTrigger("Reset");
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
		_animator.SetFloat("StunTime", _stunTime.TimeLeft);
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
		else if(_characterData.Dash.inProgress)
		{
			WaitForDashRelease = true;
		}

		if (SpecialActivation() && _allowInput)
		{
			_specialCooldown.Set(_characterData.SpecialCoolDown);
			SpecialAction();
			_stunTime.Add(_characterData.SpecialLag);
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
			{
				_activeSpeed = Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized)) * _activeSpeed;
				//#if UNITY_EDITOR 
				//				_activeSpeed.x = _maxSpeed.x * InputManager.GetAxisRaw("x", _playerRef.JoystickNumber);
				//				_activeSpeed.z = _maxSpeed.x * InputManager.GetAxisRaw("y", _playerRef.JoystickNumber);
				//#else
				//				_activeSpeed.x = _maxSpeed.x * InputManager.GetAxis("x", _playerRef.JoystickNumber);
				//				_activeSpeed.z = _maxSpeed.x * InputManager.GetAxis("y", _playerRef.JoystickNumber);
				//#endif
			}
			if(_isAffectedByFriction)
				ApplyFriction();

			if (secureAllowInput)
			{
				_activeSpeed.x += ((_maxSpeed.x / _acceleration.x) * Time.deltaTime + _friction * TimeManager.DeltaTime) * InputManager.GetAxis("x", _playerRef.JoystickNumber);
				_activeSpeed.z += ((_maxSpeed.x / _acceleration.x) * Time.deltaTime + _friction * TimeManager.DeltaTime) * InputManager.GetAxis("y", _playerRef.JoystickNumber);
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
		//_airborneTimeout.Set(_airborneDelay);
		_dashMaxed = false;
	}

	private void ApplyCharacterFinalVelocity()
	{
		_rigidB.velocity = _activeSpeed;
		_animator.SetFloat("Speed", Mathf.Abs(_activeSpeed.x) + Mathf.Abs(_activeSpeed.z));
	}


	#endregion

	protected virtual void SpecialAction()
	{
		Debug.LogWarning("No default special Action defined in PlayerController, use a child class to code a special Action: " + gameObject.name);
	}

	public void Kill()
	{
		Debug.Log("Player is DED!");
		_animator.SetTrigger("Death");
		_characterData.SoundList["OnDeath"].Play(gameObject);
		_isDead = true;
		Player killer = _playerRef.Controller.LastDamageDealer != null ? _playerRef.Controller.LastDamageDealer.PlayerRef : null;
		GameManager.Instance.OnPlayerDeath.Invoke(_playerRef, killer);
	}

	public void Eject(Vector3 direction)
	{
		if (direction.y > 0)
			ForceAirborne(0.1f);

		_activeSpeed = direction;
		_rigidB.velocity = _activeSpeed;
	}

	public void Damage(Vector3 direction,Vector3 impactPoint, DamageData data)
	{
		if (_isInvul)
			return;

		LastDamageDealer = data.Dealer;

		Debug.Log("Character \"" + _characterData.IngameName + "\" was damaged by: \"" + data.Dealer.InGameName + "\"");

		//direction.x += direction.x * (10 * (_characterData.CharacterStats.resistance - (Stats.maxValue * 0.5f))) / 100;

		direction.x += direction.x * (0.5f - _characterData.CharacterStats.resistance.Percentage(0, Stats.maxValue));
		direction.z += direction.z * (0.5f - _characterData.CharacterStats.resistance.Percentage(0, Stats.maxValue));

		Eject(direction);
		if (data.StunInflicted > 0)
			_stunTime.Set(data.StunInflicted);
		_invulTime.Set(data.StunInflicted * 1.2f);
		_activeDirection = -direction.ZeroY().normalized;
		transform.LookAt(transform.position + _activeDirection, Vector3.up);

		_characterData.SoundList["OnHit"].Play(gameObject);
		_animator.SetTrigger("Stun_Start");
	}

	public void AddStun(float stunTime) { _stunTime.Add(stunTime); }
	public void SetStun(float stunTime) { _stunTime.Set(stunTime); }


	protected virtual IEnumerator ActivateDash()
	{
		_characterData.Dash.inProgress = true;
		_isInvul = true;
		_allowInput = false;

		_stunTime.Add(_characterData.Dash.endingLag);

		_animator.SetTrigger("Dash_Start");
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
		_animator.SetTrigger("Dash_End");
		_characterData.SoundList["OnDashEnd"].Play(gameObject);

		yield return new WaitForSeconds(_characterData.Dash.endingLag);
		_characterData.Dash.inProgress = false;
		_allowInput = true;

		_timeHeldDash = 0;
		_dashStepActivated = 1;
		_dashMaxed = false;
		WaitForDashRelease = false;
	}

	protected virtual void DashAirControl()
	{
		Vector3 directionHeld = InputManager.GetStickDirection(_playerRef.JoystickNumber);
		directionHeld.z = directionHeld.y;
		directionHeld = Quaternion.FromToRotation(Vector3.right, Camera.main.transform.right.ZeroY().normalized) * directionHeld.ZeroY();

		_activeSpeed += directionHeld * (0.03f + 0.002f * _characterData.CharacterStats.speed);
	}

	public void ForceAirborne(float timeForced = 0)
	{
		//_airborneTimeout.Set(0);
		IsGrounded = false;
		if(timeForced == 0)
			_forcedAirbornTimeout.Set(10000000);
		else
			_forcedAirbornTimeout.Set(timeForced);
	}


	protected void OnCollisionEnter(Collision colli)
	{
		PlayerCollisionHandler(colli);
	}

	protected void OnCollisionStay(Collision colli)
	{
		PlayerCollisionHandler(colli);
	}

	protected virtual void PlayerCollisionHandler(Collision colli)
	{
		if (colli.gameObject.GetComponent<IDamageable>() != null && _characterData.Dash.inProgress)
		{
			colli.gameObject.GetComponent<IDamageable>()
				.Damage(Quaternion.FromToRotation(Vector3.right,
				(colli.transform.position - transform.position).ZeroY().normalized) * (_characterData.SpecialEjection.Multiply(Axis.x, _characterData.Dash.Impact)),
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
