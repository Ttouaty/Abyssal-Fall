using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Tile : MonoBehaviour {

	[SerializeField]
	private float _timeLeft = 0.5f;

	private Rigidbody _rigidB;

	void Start () 
	{
		_rigidB = GetComponent<Rigidbody>();
		_rigidB.isKinematic = true;
	}
	
	void Update () 
	{
	
	}

	public void ReduceTime()
	{
		_timeLeft -= Time.deltaTime;
		if (_timeLeft <= 0)
		{
			Fall();
		}
	}

	private void Fall()
	{
		Debug.Log("ass");
		_rigidB.isKinematic = false;
	}
}
