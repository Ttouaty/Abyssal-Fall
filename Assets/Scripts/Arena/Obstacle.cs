using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour
{
	public GameObject Next;

	[SerializeField]
	private bool _isTouched;
	private Rigidbody _rigidB;

	public void ActivateFall()
	{
		_rigidB = GetComponent<Rigidbody>();
		if (_isTouched)
			return;

		_isTouched = true;
		_rigidB.isKinematic = false;
	}
}
