using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class Tile : MonoBehaviour, IPoolable
{
	private bool            _canFall		= true;
	private float           _timeLeft		= 1.5f;
	private float           _timeLeftSave;
	private bool            _isTouched		= false;
	private bool            _isFalling		= false;
	private Rigidbody       _rigidB;
	private MeshRenderer    _renderer;
	private ParticleSystem	_particles;
	private Color			_defaultColor;
	private Vector3			_initialPosition;
	private Quaternion		_initialRotation;
	[HideInInspector]
	public Obstacle			Obstacle;
	[HideInInspector]
	public Spawn SpawnComponent;
	public bool				CanFall			{ get { return _canFall; } }
	public bool				IsTouched		{ get { return _isTouched; } }
	public bool				IsFalling		{ get { return _isFalling; } }
	public bool             IsSpawn			{ get { return SpawnComponent == null; } }
	public Rigidbody		RigidBRef		{ get { return _rigidB; } }
	[HideInInspector]
	public int				TileIndex = 0;
	[HideInInspector]
	public Vector2			TileCoordinates;
	public int				MaterialChangeIndex = 0;

	public float TimeLeftSave { get { return _timeLeftSave; } }

	void Awake()
	{
		_rigidB         = GetComponent<Rigidbody>();
		_renderer       = GetComponentInChildren<MeshRenderer>();
		_defaultColor   = _renderer.materials[MaterialChangeIndex].color;
		_timeLeftSave   = _timeLeft;
		_particles		= GetComponentInChildren<ParticleSystem>();
		if (_initialPosition.magnitude == 0)
			Place(transform.localPosition);
	}

	void Update ()
	{
		if(_isFalling)
		{
			if(transform.position.y < -200)
				GameObjectPool.AddObjectIntoPool(gameObject);

			_rigidB.velocity += Vector3.up * -9.806f * Time.deltaTime *1.1f; // Double fall speed +10%
		}
	}

	public void OnGetFromPool()
	{
		gameObject.layer = LayerMask.NameToLayer("Ground");
		_rigidB.isKinematic = true;
		_renderer.materials[MaterialChangeIndex].color = _defaultColor; // Debug to see falling ground feedback
		_isTouched = false;
		_isFalling = false;
		StopAllCoroutines();
	}

	public void OnReturnToPool()
	{
		TimeManager.Instance.OnPause.RemoveListener(OnPause);
		TimeManager.Instance.OnResume.RemoveListener(OnResume);
		_rigidB.isKinematic = true;
		gameObject.layer = LayerMask.NameToLayer("Ground");
		StopAllCoroutines();
		gameObject.SetActive(false);
	}

	void OnPause(float value)
	{
		if (_isFalling)
			_rigidB.isKinematic = true;
	}

	void OnResume(float value)
	{
		if(_isFalling)
			_rigidB.isKinematic = false;
	}

	public void SetTimeLeft(float value, bool bSaveResult = true)
	{
		_timeLeft = value;
		if (_timeLeft != 0)
			_isTouched = false;
		if (bSaveResult)
		{
			_timeLeftSave = _timeLeft;
		}
	}

	public void PrepareRespawn ()
	{
		_canFall = false;
		_isFalling = false;
	}

	public void ActivateRespawn()
	{
		GetComponentInChildren<MeshRenderer>().materials[MaterialChangeIndex].color = _defaultColor;
		gameObject.SetActive(true);
		_rigidB.isKinematic = true;
		StartCoroutine(ActivateRespawn_Implementation());
	}

	IEnumerator ActivateRespawn_Implementation ()
	{
		float timer = 1.0f;
		Vector3 initialPosition = new Vector3(transform.localPosition.x, -50f, transform.localPosition.z);
		Quaternion initialRotation = transform.rotation;
		gameObject.layer = LayerMask.NameToLayer("NoColli");

		while (timer > 0.0f)
		{
			transform.localPosition = Vector3.Lerp(initialPosition, _initialPosition, 1.0f - timer);
			transform.rotation = Quaternion.Lerp(initialRotation, _initialRotation, 1.0f - timer);
			timer -= TimeManager.DeltaTime;

			if(timer < 0.1f && gameObject.layer != LayerMask.NameToLayer("Ground"))
				gameObject.layer = LayerMask.NameToLayer("Ground"); // Set To Ground a bit before comming back

			yield return null;
		}
		Restore();
	}

	public void Restore()
	{
		_canFall = true;
		_isTouched = false;
		_timeLeft = _timeLeftSave;
		transform.localPosition = _initialPosition;
		gameObject.layer = LayerMask.NameToLayer("Ground");
		StopAllCoroutines();

		_particles.Emit(10);

		if (ArenaManager.Instance != null)
			ArenaManager.Instance.ResetTile(this);
	}

	public void ActivateFall()
	{
		if (_isTouched)
			return;

		if(GameManager.Instance.GameRules != null)
		{
			if (GameManager.Instance.GameRules.AreTilesFrozen)
				return;
		}

		if (ArenaManager.Instance != null)
		{
			Player.LocalPlayer.CmdRemoveTile(TileIndex);
			TimeManager.Instance.OnPause.AddListener(OnPause);
			TimeManager.Instance.OnResume.AddListener(OnResume);
		}

		MakeFall();
	}

	public void Place(Vector3 newLocalPos)
	{
		gameObject.layer = LayerMask.NameToLayer("Ground");
		transform.localPosition = newLocalPos;
		_initialPosition = transform.localPosition;
		_initialRotation = transform.localRotation;
	}

	public void MakeFall()
	{
		_isTouched = true;
		StartCoroutine(ActivateFall_Implementation());
	}

	IEnumerator ActivateFall_Implementation()
	{
		MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();

		// Debug to see falling ground feedback
		Color color = meshRenderer.materials[MaterialChangeIndex].color;
		while(_timeLeft > 0)
		{
			// Add better feedback
			meshRenderer.materials[MaterialChangeIndex].color = Color.Lerp(Color.red, color, _timeLeft / _timeLeftSave);
			_timeLeft -= TimeManager.DeltaTime;
			yield return null;
		}

		Fall();
	}

	private void Fall()
	{
		_isFalling = true;
		_rigidB.isKinematic = false;
		gameObject.layer = LayerMask.NameToLayer("NoColli");

		if (_particles != null)
			_particles.Play();

		if (GameManager.Instance.GameRules != null)
			GameManager.Instance.GameRules.RespawnFallenTiles(this);

		// If the tile has an obstacle up, this obstacle will fall
		if (Obstacle != null)
		{
			Obstacle.ActivateFall();
		}
	}
}
