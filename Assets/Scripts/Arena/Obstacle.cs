using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour
{
	public GameObject Next;

	[SerializeField]
	private bool _isTouched;
	[SerializeField]
	private GameObject _particleSystem;
	private Rigidbody _rigidB;

	public void OnDropped ()
	{
		_particleSystem.SetActive(true);
	}

	public void ActivateFall()
	{
		if (_isTouched)
			return;

		_rigidB = GetComponent<Rigidbody>();
		_isTouched = true;
		_rigidB.isKinematic = false;
	}
}
