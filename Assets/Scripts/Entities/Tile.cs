﻿using UnityEngine;
using System.Collections;

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
	private Color			_defaultColor;

	public Obstacle			Obstacle;
	public Spawn            SpawnComponent;
	public bool				CanFall			{ get { return _canFall; } }
	public bool				IsFalling		{ get { return _isFalling; } }
	public bool             IsSpawn			{ get { return SpawnComponent == null; } }
	

	public float TimeLeftSave { get { return _timeLeftSave; } }

	void Awake()
	{
		_rigidB         = GetComponent<Rigidbody>();
		_renderer       = GetComponentInChildren<MeshRenderer>();
		_defaultColor = _renderer.material.color;
		_timeLeftSave   = _timeLeft;
	}

	void Update ()
	{
		if(_isFalling && transform.position.y < -200)
		{
			GameObjectPool.AddObjectIntoPool(gameObject);
		}
	}

	public void OnGetFromPool()
	{
		_rigidB.isKinematic = true;
		_renderer.material.color = _defaultColor; // Debug to see falling ground feedback
		_isTouched = false;
		_isFalling = false;
		StopAllCoroutines();
	}

	public void OnReturnToPool()
	{
		TimeManager.Instance.OnPause.RemoveListener(OnPause);
		TimeManager.Instance.OnResume.RemoveListener(OnResume);
		_rigidB.isKinematic = true;
		StopAllCoroutines();
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
		if(bSaveResult)
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
		GetComponentInChildren<MeshRenderer>().material.color = _defaultColor;
		gameObject.SetActive(true);
		_rigidB.isKinematic = true;
		StartCoroutine(ActivateRespawn_Implementation());
	}

	IEnumerator ActivateRespawn_Implementation ()
	{
		float timer = 1.0f;
		Vector3 initialPosition = new Vector3(transform.localPosition.x, 15f, transform.localPosition.z);
		Vector3 targetPosition = new Vector3(initialPosition.x, 0, initialPosition.z);
		while(timer > 0.0f)
		{
			transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, 1.0f - timer);
			timer -= TimeManager.DeltaTime;
			yield return null;
		}
		transform.localPosition = targetPosition;
		_canFall = true;
		_isTouched = false;
		_timeLeft = _timeLeftSave;
	}

	public void ActivateFall()
	{
		if (_isTouched)
			return;

		ArenaManager.Instance.RemoveTile(this);

		TimeManager.Instance.OnPause.AddListener(OnPause);
		TimeManager.Instance.OnResume.AddListener(OnResume);

		_isTouched = true;
		StartCoroutine(ActivateFall_Implementation());
	}

	IEnumerator ActivateFall_Implementation()
	{
		MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();

		// Debug to see falling ground feedback
		Color color = meshRenderer.material.color;
		while(_timeLeft > 0)
		{
			// Add better feedback
			meshRenderer.material.color = Color.Lerp(Color.red, color, _timeLeft / _timeLeftSave);
			_timeLeft -= TimeManager.DeltaTime;
			yield return null;
		}

		Fall();
	}

	private void Fall()
	{
		_isFalling = true;
		_rigidB.isKinematic = false;

		// If the tile has an obstacle up, this obstacle will fall
		if (Obstacle != null)
		{
			Obstacle.ActivateFall();
		}
	}
}
