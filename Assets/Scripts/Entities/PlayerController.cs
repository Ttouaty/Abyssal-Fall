using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct Dash
{
	public float endingLag;
	public Vector2[] Forces;
	public Vector2 ImpactEjection;

	[HideInInspector]
	public bool inProgress;
}

[Serializable]
public struct PlayerAudioList
{
	public AudioClip OnDashStart;
	public AudioClip OnDashEnd;
	public AudioClip OnDeath;
	public AudioClip OnSpecialActivate;
	public AudioClip OnHit;
}

[Serializable]
public class Stats
{
	[Range(1, 5)]
	public int strength = 3;
	[Range(0, 30)]
	public float specialCooldown = 3;
	[Range(1, 5)]
	public int speed = 3;
	[Range(1, 5)]
	public int resistance = 3;
}

[RequireComponent(typeof(Rigidbody), typeof(AudioSource), typeof(DamageDealer))]
public class PlayerController : MonoBehaviour
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

	protected PlayerProp _playerProp;

	#region references

	protected Transform _transf;
	protected Rigidbody _rigidB;
	protected AudioSource _audioSource;
	protected RaycastHit _hit;
	protected Transform _groundCheck;

	#endregion

	protected Vector3 _activeSpeed = Vector3.zero; // Activespeed est un vecteur qui est appliqué a chaque frame au rigibody.velocity => permet de modifier librement la vitesse du player.
	protected Vector3 _activeDirection = Vector3.forward;
	protected Vector2 _maxSpeed = new Vector2(8f, 20f);
	protected Vector2 _acceleration = new Vector2(1.2f, -2f);
	protected float _friction = 60; //friction applied to the player when it slides (pushed or end dash) (units/s)
	protected float _airborneDelay = 0.01f;

	protected float _fullDashActivationTime = 0.05f;
	private float _timeHeldDash;
	private bool _waitForDashRelease = false;
	private bool _dashMaxed = false;

	protected int _dashActivationSteps;
	protected int _dashStepActivated = 1;
	[Space]
	public SO_Character _characterData;

	protected TimeCooldown _specialCooldown;
	protected TimeCooldown _stunTime; //Seconds of stun on Hit
	protected TimeCooldown _invulTime; //Seconds of invulnerability
	protected TimeCooldown _airborneTimeout; //Time before being considered airborne

	protected DamageDealer _dmgDealerSelf;
	public DamageDealer DamageDealerSelf { get { return _dmgDealerSelf; } }
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

	protected bool _allowInput = true;
	public bool AllowInput { get { return _allowInput; } }
	protected bool _isStunned = false;

	protected bool _canSpecial
	{
		get
		{
			return !_characterData.Dash.inProgress && !_isDead && _specialCooldown.TimeLeft <= 0;
		}
	}

	protected bool _canDash
	{
		get
		{
			return !_isDead && !_dashMaxed && (_characterData.Dash.inProgress || _allowInput);
		}
	}

	public void Freeze()
	{
		_allowInput = false;
		_rigidB.velocity = Vector3.zero;
		_rigidB.isKinematic = true;
		_activeSpeed = Vector3.zero;
		_animator.SetTrigger("Reset");
	}

	public void UnFreeze()
	{
		_allowInput = true;
		_rigidB.isKinematic = false;
	}

	#region Unity Functions
	protected void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		_transf = transform;
		_rigidB = GetComponent<Rigidbody>();
	}

	public void Init(Player player)
	{
		_playerRef = player;

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
		_playerProp = transform.GetComponentInChildren<PlayerProp>();

		if (_playerProp == null)
			Debug.LogError("No player prop found in playermesh: " + gameObject.name + " !");

		_dashActivationSteps = _characterData.Dash.Forces.Length;
		_characterData.Dash.inProgress = false; // security because sometimes dash is activated, lolwutomfgrektbbq

		_specialCooldown = new TimeCooldown(this);
		_specialCooldown.onFinish = OnSpecialReset;

		_stunTime = new TimeCooldown(this);
		_stunTime.onFinish = OnStunOver;
		_stunTime.onProgress = OnStunActive;

		_invulTime = new TimeCooldown(this);
		_invulTime.onFinish = OnInvulOver;
		_invulTime.onProgress = OnInvulActive;

		_airborneTimeout = new TimeCooldown(this);
		_airborneTimeout.onFinish = OnAirbornFinish;
		_airborneTimeout.onProgress = OnAirbornProgress;


		_lastDamageDealerTimeOut = new TimeCooldown(this);
		_lastDamageDealerTimeOut.onFinish = OnLastDamageDealerTimeOut;
		GameManager.Instance.OnPlayerWin.AddListener(OnPlayerWin);

		_maxSpeed.x = _maxSpeed.x + _maxSpeed.x * (20 * (_characterData.CharacterStats.speed - 3) / 100);

		if (GetComponentInChildren<GroundCheck>() == null)
		{
			Debug.LogWarning("no GroundCheck found in player: " + gameObject.name + "\nCreating one.");
			GameObject tempGo = new GameObject();
			Instantiate(tempGo);
			tempGo.transform.parent = transform;
			tempGo.AddComponent(typeof(GroundCheck));
			tempGo.transform.position = _transf.position - Vector3.up;
		}
		_dmgDealerSelf = GetComponent<DamageDealer>();
		if (_dmgDealerSelf == null)
			_dmgDealerSelf = gameObject.AddComponent<DamageDealer>();

		_dmgDealerSelf.PlayerRef = _playerRef;
		_dmgDealerSelf.InGameName = _characterData.IngameName;
		_dmgDealerSelf.Icon = _characterData.Icon;

		TimeManager.Instance.OnPause.AddListener(OnPause);
		TimeManager.Instance.OnResume.AddListener(OnResume);
		TimeManager.Instance.OnTimeScaleChange.AddListener(OnTimeScaleChange);

		CameraManager.Instance.AddTargetToTrack(transform);

		CustomStart();

	}

	protected void Update()
	{
		if (_isDead || TimeManager.IsPaused)
			return;

		ProcessCoolDowns();

		ProcessInputs();

		ProcessActiveSpeed();
		if (_allowInput)
			ProcessOrientation();
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
		if (_playerProp.PropRenderer != null)
			_playerProp.PropRenderer.enabled = true;
		if (_playerProp.PropRespawnParticles != null)
			_playerProp.PropRespawnParticles.Play();
	}

	protected virtual void OnStunOver()
	{
		_isStunned = false;
		_allowInput = true;
	}

	protected virtual void OnStunActive()
	{
		if (!IsGrounded)
		{
			_stunTime.Add(TimeManager.DeltaTime); //decrease Stun only if grounded
		}

		_allowInput = false;
		_isStunned = true;
	}

	protected virtual void OnInvulOver()
	{
		_isInvul = false;
	}

	protected virtual void OnInvulActive()
	{
		_isInvul = true;
	}

	protected virtual void OnPlayerWin()
	{
		_allowInput = false;
		//enabled = false;
		Freeze();
		_animator.SetTrigger("Reset");
	}

	private void OnAirbornFinish()
	{

	}

	private void OnAirbornProgress()
	{
		if (_characterData.Dash.inProgress)
			return;
		if(IsGrounded)
			_airborneTimeout.Add(Time.deltaTime);
		IsGrounded = true;
	}

	#endregion


	#region Processes
	private void ProcessOrientation()
	{
		if (!InputManager.StickIsNeutral(_playerRef.JoystickNumber) && !_isStunned)
		{
			//_activeDirection.x = Mathf.Lerp(_activeDirection.x, InputManager.GetAxis("x", _playerRef.JoystickNumber), 0.3f);
			//_activeDirection.z = Mathf.Lerp(_activeDirection.z, InputManager.GetAxis("y", _playerRef.JoystickNumber), 0.3f);
			_activeDirection.x = InputManager.GetAxis("x", _playerRef.JoystickNumber);
			_activeDirection.z = InputManager.GetAxis("y", _playerRef.JoystickNumber);
			_activeDirection = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeDirection;
			//_activeDirection.Normalize();
		}

		transform.LookAt(transform.position + _activeDirection, Vector3.up);
	}

	private void ProcessCoolDowns()
	{
		_animator.SetFloat("StunTime", _stunTime.TimeLeft);
	}

	private void ProcessInputs()
	{
		if (InputManager.GetButtonHeld("Dash", _playerRef.JoystickNumber) && _canDash && !_waitForDashRelease)
		{
			if (_dashStepActivated <= (_dashActivationSteps - 1) * (_timeHeldDash / _fullDashActivationTime) +1)
			{
				Eject(Quaternion.FromToRotation(Vector3.right, _activeDirection) * _characterData.Dash.Forces[_dashStepActivated - 1]);

				if(_dashStepActivated == 1)
					StartCoroutine(ActivateDash());

				_dashStepActivated++;

				_dashMaxed = _dashStepActivated > _dashActivationSteps;
				_waitForDashRelease = _dashMaxed;
			}

			_timeHeldDash += Time.deltaTime;
		}
		else if (InputManager.GetButtonUp("Dash", _playerRef.JoystickNumber))
		{
			_waitForDashRelease = false;
		}

		if (SpecialActivation() && _allowInput)
		{
			_specialCooldown.Set(_characterData.CharacterStats.specialCooldown);
			SpecialAction();
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
			if (_allowInput)
			{

#if UNITY_EDITOR
				_activeSpeed.x = _maxSpeed.x * InputManager.GetAxisRaw("x", _playerRef.JoystickNumber);
				_activeSpeed.z = _maxSpeed.x * InputManager.GetAxisRaw("y", _playerRef.JoystickNumber);
#else
				_activeSpeed.x = _maxSpeed.x * InputManager.GetAxis("x", _playerRef.JoystickNumber);
				_activeSpeed.z = _maxSpeed.x * InputManager.GetAxis("y", _playerRef.JoystickNumber);
#endif
				_activeSpeed = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeSpeed.normalized * _maxSpeed.x;
			}
		}
		else
		{
			_activeSpeed.y += _acceleration.y * TimeManager.DeltaTime * Physics.gravity.magnitude;
			_activeSpeed.y = Mathf.Clamp(_activeSpeed.y, -_maxSpeed.y, _maxSpeed.y * 10f);
		}
		ApplyFriction();
	}

	private void ApplyFriction()
	{
		if (IsGrounded)
		{
			_activeSpeed.x = _activeSpeed.x.Reduce(_friction * TimeManager.DeltaTime);
			_activeSpeed.z = _activeSpeed.z.Reduce(_friction * TimeManager.DeltaTime);
		}
	}

	public void ContactGround()
	{
		IsGrounded = true;
		_activeSpeed.y = 0f;
		_airborneTimeout.Set(_airborneDelay);
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
		_audioSource.PlayOneShot(_characterData.SoundList.OnDeath);
		_isDead = true;
		Player killer = _playerRef.Controller.LastDamageDealer != null ? _playerRef.Controller.LastDamageDealer.PlayerRef : null;
		GameManager.Instance.OnPlayerDeath.Invoke(_playerRef, killer);
	}

	public void Eject(Vector3 direction)
	{
		if (direction.y > 0)
			IsGrounded = false;

		_activeSpeed = direction;
		_rigidB.velocity = _activeSpeed;
	}

	public void Damage(Vector3 direction, float stunTime, DamageDealer Sender)
	{
		if (_isInvul)
			return;

		LastDamageDealer = Sender;

		Debug.Log("Character \"" + _characterData.IngameName + "\" was damaged by: \"" + Sender.InGameName + "\"");

		float oldMagnitude = direction.magnitude;
		_characterData.CharacterStats.resistance = _characterData.CharacterStats.resistance == 0 ? 1 : _characterData.CharacterStats.resistance;
		direction = direction.normalized * (oldMagnitude + oldMagnitude * (10 * (_characterData.CharacterStats.resistance - 3)) / 100 );

		Eject(direction);
		if (stunTime > 0)
			_stunTime.Set(stunTime);
		_invulTime.Add(stunTime * 1.2f);
		_activeDirection = -direction.ZeroY().normalized;
		_audioSource.PlayOneShot(_characterData.SoundList.OnHit);
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
		_audioSource.PlayOneShot(_characterData.SoundList.OnDashStart);
		gameObject.layer = LayerMask.NameToLayer("PlayerInvul");

		IsGrounded = false;

		yield return null; // wait a frame for internal calculation

		while (!IsGrounded)
		{
			yield return null;
		}

		gameObject.layer = LayerMask.NameToLayer("PlayerDefault");

		_isInvul = false;

		_animator.SetTrigger("Dash_End");
		_audioSource.PlayOneShot(_characterData.SoundList.OnDashEnd);

		yield return new WaitForSeconds(_characterData.Dash.endingLag);
		_characterData.Dash.inProgress = false;
		_allowInput = true;
		_timeHeldDash = 0;
		_dashStepActivated = 1;
		_dashMaxed = false;
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
		if (colli.gameObject.tag == "Player" && _characterData.Dash.inProgress)
		{
			colli.transform.GetComponent<PlayerController>()
				.Damage(Quaternion.FromToRotation(Vector3.right,
				(colli.transform.position - transform.position).ZeroY().normalized) * _characterData.Dash.ImpactEjection * (_characterData.CharacterStats.strength / 3),
				0.3f, _dmgDealerSelf);
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
