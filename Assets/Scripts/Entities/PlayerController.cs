using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct Dash 
{
	[SerializeField]
	public float length;
	[SerializeField]
	public float endingLag;
	[SerializeField]
	public int range;

	public bool inProgress;
}

public class PlayerController : MonoBehaviour
{
	public int PlayerNumber = 1;

	[SerializeField]
	private Dash dash;

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
	[SerializeField]
	private Transform _snapGround;


	[Header("Movement")]
	[SerializeField]
	private Vector2 _maxSpeed = new Vector2(5f, 10f);

	//Vectors affecting the player
	[SerializeField]
	private Vector2 _acceleration = new Vector2(0.1f, 0.1f);
	[SerializeField]
	private Vector2 _friction = new Vector2(0.05f, 0);



	private float _stunTime = 0f; //Secondes of stun on Hit
	[SerializeField]
	private GameObject _playerModel;

	private bool _allowInput = true;
	private bool _isGrounded = false;
	private bool _isStunned = false;
	private bool _isInvul = false;


	private bool _canDash
	{
		get
		{
			return !dash.inProgress && _isGrounded && !_isDead;
		}
	}

	void Start()
	{
		_transf = transform;
		_rigidB = GetComponent<Rigidbody>();
	}

	void Update()
	{
		if (_isDead)
			return;

		ProcessCoolDowns();

		_inputRawX = 0;
		_inputRawZ = 0;
		if (_allowInput)
			ProcessInputs();


		ProcessActiveSpeed();
		if(_allowInput)
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
		_activeDirection.x = Input.GetAxis("Horizontal_P" + PlayerNumber);
		_activeDirection.z = Input.GetAxis("Vertical_P" + PlayerNumber);
		transform.LookAt(transform.position + _activeDirection, Vector3.up);
	}


	void ProcessCoolDowns()
	{
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
		if (Mathf.Abs(Input.GetAxis("Horizontal_P" + PlayerNumber)) > deadZone)
			_inputRawX = Input.GetAxisRaw("Horizontal_P" + PlayerNumber);
		if (Mathf.Abs(Input.GetAxis("Vertical_P" + PlayerNumber)) > deadZone)
			_inputRawZ = Input.GetAxisRaw("Vertical_P" + PlayerNumber);

		if (Input.GetButtonDown("Dash_P" + PlayerNumber) && _canDash)
		{
			StartCoroutine(ActivateDash());
		}
	}

	void ProcessActiveSpeed()
	{
		if (_isGrounded)
		{
			if (dash.inProgress)
			{
				_activeSpeed.x = _activeSpeed.x.Reduce(_maxSpeed.x * Time.deltaTime * 5);
				_activeSpeed.z = _activeSpeed.z.Reduce(_maxSpeed.x * Time.deltaTime * 5);
				return;
			}
			_activeSpeed.x = _maxSpeed.x * Input.GetAxis("Horizontal_P" + PlayerNumber);
			_activeSpeed.z = _maxSpeed.x * Input.GetAxis("Vertical_P" + PlayerNumber);
		}
		else
		{
			this._activeSpeed.y += this._acceleration.y * Time.deltaTime * Physics.gravity.magnitude; // * 60 to match old prefabs (bug security)
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
						_hit.transform.GetComponent<Tile>().ActivateFall();
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
		if(_hit.rigidbody.isKinematic == true)
			this.transform.position = this.transform.position - (this._snapGround.transform.position - this._hit.point);
	}

	void ApplyCharacterFinalVelocity()
	{
		_rigidB.velocity = new Vector3(_activeSpeed.x, _activeSpeed.y, _activeSpeed.z);
	}


	#endregion


	public void Kill()
	{
		Debug.Log("Player is DED!");
		_isDead = true;
	}

	public void Eject(Vector3 direction, float stunTime)
	{
		if (direction.y > 0)
			this._isGrounded = false;

		this._activeSpeed = direction;
		this._stunTime = stunTime;
	}


	IEnumerator ActivateDash() 
	{
		dash.inProgress = true;
		_isInvul = true;
		_allowInput = false;

		Eject(this.transform.forward * dash.range / dash.length + Vector3.up * Physics.gravity.magnitude * -_acceleration.y * dash.length * 0.5f, dash.length + dash.endingLag);

		GetComponent<Collider>().isTrigger = true;

		yield return new WaitForSeconds(dash.length);
		//float elapsedTime = 0;

		//Quaternion originalRotation = _playerModel.transform.rotation;
		//while (elapsedTime < dash.length)
		//{
		//	elapsedTime += Time.deltaTime;
		//	_playerModel.transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * Quaternion.Euler(-180, 0, 0), elapsedTime / dash.length) * Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(-180, 0, 0), elapsedTime / dash.length);
		//	yield return null;
		//}
		//_playerModel.transform.rotation = transform.rotation;
		_isInvul = false;
		GetComponent<Collider>().isTrigger = false;
		while (!_isGrounded)
		{
			yield return null;
		}

		yield return new WaitForSeconds(dash.endingLag);

		dash.inProgress = false;
		_allowInput = true;
	}

	void OnTriggerEnter(Collider colli)
	{
		if (colli.tag == "Player")
		{
			colli.GetComponent<PlayerController>().Eject((colli.transform.position - transform.position) * 5 + Vector3.up * Physics.gravity.magnitude * 0.5f, 0.5f);
		}
	}

}
