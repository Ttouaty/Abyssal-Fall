using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public int PlayerNumber = 1;


	#region references

	private Transform _transf;
	private Rigidbody _rigidB;

	#endregion

	//Raw Inputs
	private float _inputRawX;
	private float _inputRawZ;

	private bool _isDead = false;
	private Vector3 _activeSpeed = Vector3.zero; // Activespeed est un vecteur qui est appliqué a chaque frame au rigibody.velocity => permet de modifier librement la vitesse du player.
	private Vector3 _activeDirection = Vector3.forward;


	private RaycastHit _hit;

	[Header("Collisions checks")]
	[SerializeField]
	private Transform _groundCheck;


	[Header("Movement")]
	[SerializeField]
	private Vector2 _maxSpeed = new Vector2(5f, 10f);

	//Vectors affecting the player
	[SerializeField]
	private Vector2 _acceleration = new Vector2(0.1f, 0.1f);
	[SerializeField]
	private Vector2 _friction = new Vector2(0.05f, 0);



	private float _invulTime = 0f; //Secondes of invulnerability
	private float _stunTime = 0f; //Secondes of stun on Hit


	private bool _allowInput = true;
	private bool _isGrounded = false;
	private bool _isStunned = false;
	private bool IsDead = false;

	void Start()
	{
		_transf = transform;
		_rigidB = GetComponent<Rigidbody>();
	}

	void Update()
	{
		if (IsDead)
			return;

		ProcessCoolDowns();

		_inputRawX = 0;
		_inputRawZ = 0;
		if (_allowInput)
			ProcessInputs();


		ProcessActiveSpeed();
		ProcessOrientation();
		ApplyCharacterFinalVelocity();
	}

	void FixedUpdate()
	{
		ProcessGroundedState();
	}



	#region Processes
	private void ProcessOrientation()
	{
		_activeDirection.x = Input.GetAxis("Horizontal");
		_activeDirection.z = Input.GetAxis("Vertical");
		transform.LookAt(transform.position + _activeDirection, Vector3.up);
	}


	void ProcessCoolDowns()
	{

		_invulTime -= _invulTime > 0 ? Time.deltaTime : 0;

		if (_stunTime > 0)
		{
			_allowInput = false;
			_stunTime -= Time.deltaTime;
		}
		else if (_isStunned)
		{
			_isStunned = false;
			_allowInput = true;
			_stunTime = 0;
		}

	}

	void ProcessInputs()
	{
		float deadZone = Input.GetJoystickNames().Length != 0 ? 0.5f : 0.1f;
		if (Mathf.Abs(Input.GetAxis("Horizontal")) > deadZone)
			_inputRawX = Input.GetAxisRaw("Horizontal");
		if (Mathf.Abs(Input.GetAxis("Vertical")) > deadZone)
			_inputRawZ = Input.GetAxisRaw("Vertical");
	}

	void ProcessActiveSpeed()
	{
		if (_isGrounded)
		{
			if (_inputRawX != 0f)
			{
				_activeSpeed.x += _acceleration.x * _inputRawX;
				_activeSpeed.x = Mathf.Clamp(_activeSpeed.x, -_maxSpeed.x, _maxSpeed.x);

			}
			//Grounded friction
			else if (_activeSpeed.x != 0f)
			{
				if (Mathf.Abs(_activeSpeed.x) < _friction.x * Time.deltaTime * 60)
					_activeSpeed.x = 0f;
				else
					_activeSpeed.x = _activeSpeed.x.Reduce(_friction.x * Time.deltaTime * 60);
			}

			if (_inputRawZ != 0f)
			{
				_activeSpeed.z += _acceleration.x * _inputRawZ;
				_activeSpeed.z = Mathf.Clamp(_activeSpeed.z, -_maxSpeed.x, _maxSpeed.x);
			}
			//Grounded friction
			else if (_activeSpeed.z != 0f)
			{
				if (Mathf.Abs(_activeSpeed.z) < _friction.x * Time.deltaTime * 60)
					_activeSpeed.z = 0f;
				else
					_activeSpeed.z = _activeSpeed.z.Reduce(_friction.x * Time.deltaTime * 60);
			}
		}
		else
		{
			this._activeSpeed.y += this._acceleration.y * Time.deltaTime * 60; // * 60 to match old prefabs (bug security)
			this._activeSpeed.y = Mathf.Clamp(this._activeSpeed.y, -this._maxSpeed.y, this._maxSpeed.y * 10f);
		}
	}

	void ProcessGroundedState()
	{
		if (_isGrounded)
		{
			_activeSpeed.y = 0;
			if (!Physics.Linecast(_transf.position, _groundCheck.position, out _hit, 1 << LayerMask.NameToLayer("Ground")))
				_isGrounded = false;
			else
			{
				//Contact Tile
				if (_hit.transform.gameObject.activeInHierarchy)
				{
					if (_hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
					{
						_hit.transform.GetComponent<Tile>().ReduceTime();
					}
				}
			}
		}
		else if (_activeSpeed.y <= 0)
		{
			if (Physics.Linecast(_transf.position, _groundCheck.position, out _hit, 1 << LayerMask.NameToLayer("Ground")))
				ContactGround(); // only activate if you make contact with the ground once
			else
				_isGrounded = false;
		}

	}

	void ContactGround()
	{
		_isGrounded = true;
		_activeSpeed.y = 0f;
	}

	void ApplyCharacterFinalVelocity()
	{
		_rigidB.velocity = new Vector3(_activeSpeed.x, _activeSpeed.y, _activeSpeed.z);
	}
	#endregion
}
