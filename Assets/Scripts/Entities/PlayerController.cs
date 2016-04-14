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

	[HideInInspector]
	public bool inProgress;
}

public class PlayerController : MonoBehaviour
{
	public int PlayerNumber = 1;

	[SerializeField]
	private Dash dash;


	[SerializeField]
	public Animator _animator;
	#region references

	private Transform _transf;
	private Rigidbody _rigidB;

	#endregion


	[HideInInspector]
	public bool _isDead = false;
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

	[SerializeField]
	private Vector2 _acceleration = new Vector2(1.5f, -1f);

	[Header("Hammer")]
	[SerializeField]
	private float _hammerRechargeRate = 3;
	private float _hammerCooldown = 0;



	private float _stunTime = 0f; //Secondes of stun on Hit
	[Header("Models")]
	[SerializeField]
	private GameObject _playerModel;
	[SerializeField]
	public GameObject _hammerPropModel;


	private bool _allowInput = true;
	private bool _isGrounded = false;
	private bool _isStunned = false;

	[HideInInspector]
	public bool _isInvul = false;

	private bool _canFire
	{
		get
		{
			return !dash.inProgress && !_isDead && _hammerCooldown <= 0;
		}
	}

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

		GameManager.instance.OnPlayerWin.AddListener(OnPlayerWin);
	}

	void Update()
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
	}

	void FixedUpdate()
	{
		ProcessGroundedState();
	}

	void OnPlayerWin(GameObject player)
	{
		_allowInput = false;
		_activeSpeed = Vector3.zero;
	}


	#region Processes
	private void ProcessOrientation()
	{
		_activeDirection.x = Mathf.Lerp(_activeDirection.x, Input.GetAxis("Horizontal_P" + PlayerNumber), 15 * Time.deltaTime);
		_activeDirection.z = Mathf.Lerp(_activeDirection.z, Input.GetAxis("Vertical_P" + PlayerNumber), 15 * Time.deltaTime);
		transform.LookAt(transform.position + (Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeDirection), Vector3.up);
	}


	void ProcessCoolDowns()
	{
		if (_hammerCooldown > 0)
			_hammerCooldown -= Time.deltaTime;
		else
		{
			_hammerPropModel.GetComponent<Renderer>().enabled = true;
			_hammerCooldown = 0;
		}


		_animator.SetFloat("StunTime", _stunTime);
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
		if (Input.GetButtonDown("Dash_P" + PlayerNumber) && _canDash)
		{
			StartCoroutine(ActivateDash());
		}

		if (Input.GetButtonDown("Throw_P" + PlayerNumber) && _canFire)
		{
			ThrowHammer();
		}
	}

	void ProcessActiveSpeed()
	{
		if (_isGrounded)
		{
			if (!_allowInput)
			{
				return;
			}

			if (dash.inProgress)
			{
				_activeSpeed.x = _activeSpeed.x.Reduce(_maxSpeed.x * Time.deltaTime * 2);
				_activeSpeed.z = _activeSpeed.z.Reduce(_maxSpeed.x * Time.deltaTime * 2);
				return;
			}
			_activeSpeed.x = _maxSpeed.x * Input.GetAxis("Horizontal_P" + PlayerNumber);
			_activeSpeed.z = _maxSpeed.x * Input.GetAxis("Vertical_P" + PlayerNumber);
			_activeSpeed = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * _activeSpeed.normalized * _maxSpeed.x;
		}
		else
		{
			_activeSpeed.y += _acceleration.y * Time.deltaTime * Physics.gravity.magnitude;
			_activeSpeed.y = Mathf.Clamp(_activeSpeed.y, -_maxSpeed.y, _maxSpeed.y * 10f);
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
		if (_hit.rigidbody.isKinematic == true)
			transform.position = transform.position - (_snapGround.transform.position - _hit.point);
	}

	void ApplyCharacterFinalVelocity()
	{
		_rigidB.velocity = new Vector3(_activeSpeed.x, _activeSpeed.y, _activeSpeed.z);
		_animator.SetFloat("Speed", Mathf.Abs(_activeSpeed.x) + Mathf.Abs(_activeSpeed.z));
	}


	#endregion

	private void ThrowHammer()
	{
		_animator.SetTrigger("Throw");
		_hammerCooldown = _hammerRechargeRate;
		_hammerPropModel.GetComponent<Renderer>().enabled = false;
		GameObjectPool.GetAvailableObject("Hammer").GetComponent<Hammer>().Launch(transform.position + transform.forward, transform.forward, PlayerNumber);
	}

	public void Kill()
	{
		Debug.Log("Player is DED!");
		_animator.SetTrigger("Death");
		_isDead = true;
		GameManager.instance.OnPlayerDeath.Invoke(gameObject);
	}

	public void Eject(Vector3 direction, float stunTime)
	{
		if (direction.y > 0)
			_isGrounded = false;

		_activeSpeed = direction;
		_stunTime = stunTime;
	}

	public void Damage(Vector3 direction, float stunTime)
	{
		if (_isInvul)
			return;
		Eject(direction, stunTime);
		_animator.SetTrigger("Stun_Start");
	}


	IEnumerator ActivateDash()
	{
		dash.inProgress = true;
		_isInvul = true;
		_allowInput = false;

		Eject(transform.forward * dash.range / dash.length + Vector3.up * Physics.gravity.magnitude * -_acceleration.y * dash.length * 0.5f, dash.length + dash.endingLag);
		_animator.SetTrigger("Dash_Start");

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
		_animator.SetTrigger("Dash_End");
		yield return new WaitForSeconds(dash.endingLag);

		dash.inProgress = false;
		_allowInput = true;
	}

	void OnTriggerEnter(Collider colli)
	{
		if (colli.tag == "Player" && GetComponent<Collider>().isTrigger)
		{
			_rigidB.velocity = new Vector3(0, _rigidB.velocity.y, 0);
			colli.GetComponent<PlayerController>().Damage((colli.transform.position - transform.position) * 5 + Vector3.up * Physics.gravity.magnitude * 0.5f, 0.5f);
		}
	}
}
