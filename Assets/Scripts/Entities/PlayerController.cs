using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct Dash
{
	public float endingLag;
	public Vector2 force;
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
	[Range(1,5)]
	public int strength = 3;
	[Range(0, 30)]
	public float specialCooldown = 3;
	[Range(1, 5)]
	public int speed = 3;
	[Range(1, 5)]
	public int resistance = 3;
}

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
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
	//change to protected
	[HideInInspector]
	public PlayerProp _playerProp;

	#region references

	protected Transform _transf;
	protected Rigidbody _rigidB;
	protected AudioSource _audioSource;
	protected RaycastHit _hit;
	private Transform _groundCheck;

	#endregion

	protected Vector3 _activeSpeed = Vector3.zero; // Activespeed est un vecteur qui est appliqué a chaque frame au rigibody.velocity => permet de modifier librement la vitesse du player.
	protected Vector3 _activeDirection = Vector3.forward;
	protected Vector2 _maxSpeed = new Vector2(7f, 20f);
	protected Vector2 _acceleration = new Vector2(1.2f, -2f);
	protected float _friction = 50; //friction applied to the player when it slides (pushed or end dash) (units/s)

	[Space]
	public SO_Character _characterData;

	protected TimeCooldown _specialCooldown;
	protected TimeCooldown _stunTime; //Secondes of stun on Hit
	

	protected bool _allowInput = true;
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
			return !_characterData.Dash.inProgress && IsGrounded && !_isDead;
		}
	}

    protected void Awake ()
    {
        _audioSource = GetComponent<AudioSource>();
        _transf = transform;
        _rigidB = GetComponent<Rigidbody>();
    }

	public void Init(Player player)
	{
        //Debug.Log("remove that part from player code !");
        ////CHEESE
        //_playerRef = new Player();
        //_playerRef.SkinNumber = 0;
        //_playerRef.JoystickNumber = 1;
        ////CHEESE END

        _playerRef = player;

        //Instantiates player mesh and retrieves its props and particles
        GameObject playerMesh = Instantiate(_characterData.CharacterModel.gameObject, _transf.position, _characterData.CharacterModel.transform.rotation) as GameObject;
		playerMesh.transform.parent = _transf.FindChild("CharacterModel");

        _transf.GetComponentInChildren<CharacterModel>().Reskin(_characterData.CharacterMaterials[_playerRef.SkinNumber]);

		_animator = _transf.GetComponentInChildren<Animator>();
		_playerProp = transform.GetComponentInChildren<PlayerProp>();

		if (_playerProp == null)
			Debug.LogError("No player prop found in playermesh: "+gameObject.name+" !");

		_characterData.Dash.inProgress = false; // security because sometimes dash is activated, lolwutomfgrektbbq
		_specialCooldown = new TimeCooldown(this);
		_specialCooldown.onFinish = OnSpecialReset;

		_stunTime = new TimeCooldown(this);
		_stunTime.onFinish = OnStunOver;
		_stunTime.onProgress = OnStunActive;

		GameManager.Instance.OnPlayerWin.AddListener(OnPlayerWin);

		_maxSpeed.x = _maxSpeed.x * _characterData.CharacterStats.speed / 3;

		if (GetComponentInChildren<GroundCheck>() == null)
		{
			Debug.LogWarning("no GroundCheck found in player: "+gameObject.name+"\nCreating one.");
			GameObject tempGo = new GameObject();
			Instantiate(tempGo);
			tempGo.transform.parent = transform;
			tempGo.AddComponent(typeof(GroundCheck));
			tempGo.transform.position = _transf.position - Vector3.up;
		}

		CustomStart();
	}

	protected void Update()
	{
		if (_isDead)
			return;

		ProcessCoolDowns();

		if (_allowInput)
			ProcessInputs();

        ProcessActiveSpeed();
		if (_allowInput)
			ProcessOrientation();
		ApplyCharacterFinalVelocity();

		CustomUpdate();
	}

	protected virtual void CustomStart() { }
	protected virtual void CustomUpdate() { }

	protected virtual void OnPlayerWin(GameObject player)
	{
		_allowInput = false;
		_activeSpeed = Vector3.zero;
		enabled = false;
		_animator.SetTrigger("Reset");
	}


	#region Processes
	private void ProcessOrientation()
	{
		_activeDirection.x = Mathf.Lerp(_activeDirection.x, InputManager.GetAxis("x", _playerRef.JoystickNumber), 15 * Time.deltaTime);
		_activeDirection.z = Mathf.Lerp(_activeDirection.z, InputManager.GetAxis("y", _playerRef.JoystickNumber), 15 * Time.deltaTime);
		transform.LookAt(transform.position + (Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeDirection), Vector3.up);
	}

	protected virtual void OnSpecialReset()
	{
		_playerProp.PropRenderer.enabled = true;
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
			_stunTime.Add(Time.deltaTime); //decrease Stun only if grounded
		}
		
		_allowInput = false;
		_isStunned = true;
	}

	private void ProcessCoolDowns()
	{
		_animator.SetFloat("StunTime", _stunTime.TimeLeft);
	}

	private void ProcessInputs()
	{
		if (InputManager.GetButtonDown("Dash", _playerRef.JoystickNumber) && _canDash)
		{
			StartCoroutine(ActivateDash());
		}

		if (InputManager.GetButtonDown("Special", _playerRef.JoystickNumber) && _canSpecial)
		{
			SpecialAction();
		}
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
				_activeSpeed.x = _maxSpeed.x * InputManager.GetAxis("x", _playerRef.JoystickNumber) ;
				_activeSpeed.z = _maxSpeed.x * InputManager.GetAxis("y", _playerRef.JoystickNumber) ;
#endif
				_activeSpeed = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeSpeed.normalized * _maxSpeed.x;
			}
		}
		else
		{
			_activeSpeed.y += _acceleration.y * Time.deltaTime * Physics.gravity.magnitude;
			_activeSpeed.y = Mathf.Clamp(_activeSpeed.y, -_maxSpeed.y, _maxSpeed.y * 10f);
		}
		ApplyFriction();
	}

	private void ApplyFriction()
	{
		if (IsGrounded)
		{
			_activeSpeed.x = _activeSpeed.x.Reduce(_friction * Time.deltaTime);
			_activeSpeed.z = _activeSpeed.z.Reduce(_friction * Time.deltaTime);
		}
	}

	public void ContactGround()
	{
		IsGrounded = true;
		_activeSpeed.y = 0f;
		//Breaks physics
		//if (_hit.rigidbody.isKinematic == true)
		//	transform.position = transform.position - (_snapGround.transform.position - _hit.point);
	}

	private void ApplyCharacterFinalVelocity()
	{
		_rigidB.velocity = _activeSpeed;
		_animator.SetFloat("Speed", Mathf.Abs(_activeSpeed.x) + Mathf.Abs(_activeSpeed.z));
	}


	#endregion

	protected virtual void SpecialAction()
	{
		Debug.LogWarning("No default special Action defined in PlayerController, use a child class to code a special Action: "+gameObject.name);
	}

    public void Kill()
	{
		Debug.Log("Player is DED!");
		_animator.SetTrigger("Death");
		_audioSource.PlayOneShot(_characterData.SoundList.OnDeath);
		_isDead = true;
		GameManager.Instance.OnPlayerDeath.Invoke(gameObject);
	}

	public void Eject(Vector3 direction, float stunTime)
	{
		if (direction.y > 0)
			IsGrounded= false;

		float oldMagnitude = direction.magnitude;
		_characterData.CharacterStats.resistance = _characterData.CharacterStats.resistance == 0 ? 1 : _characterData.CharacterStats.resistance;
		direction = direction.normalized * (oldMagnitude * 3 / _characterData.CharacterStats.resistance);
		
		_activeSpeed = direction;
		
		_stunTime.Set(stunTime);
	}

	public void Damage(Vector3 direction, float stunTime)
	{
		if (_isInvul)
			return;
		Eject(direction, stunTime);
		_activeDirection = -direction.ZeroY();
		_audioSource.PlayOneShot(_characterData.SoundList.OnHit);
		_animator.SetTrigger("Stun_Start");
	}


	protected virtual IEnumerator ActivateDash()
	{
		_characterData.Dash.inProgress = true;
		_isInvul = true;
		_allowInput = false;

		Eject(Quaternion.FromToRotation(Vector3.right,transform.forward) * _characterData.Dash.force, _characterData.Dash.endingLag);

		_animator.SetTrigger("Dash_Start");
		_audioSource.PlayOneShot(_characterData.SoundList.OnDashStart);
		gameObject.layer = LayerMask.NameToLayer("PlayerInvul");


		while (!IsGrounded)
		{
			yield return null;
		}

		gameObject.layer = LayerMask.NameToLayer("Default");

		_isInvul = false;

		_animator.SetTrigger("Dash_End");
		_audioSource.PlayOneShot(_characterData.SoundList.OnDashEnd);

		yield return new WaitForSeconds(_characterData.Dash.endingLag);
		_characterData.Dash.inProgress = false;
		_allowInput = true;
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
				0.3f);
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
