using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Tile : MonoBehaviour {

	[SerializeField]
	private float _timeLeft = 0.8f;

	private Rigidbody _rigidB;
	private bool _isTouched = false;

	void Start () 
	{
		_rigidB = GetComponent<Rigidbody>();
		_rigidB.isKinematic = true;
	}
	
	void Update () 
	{
	
	}


	public void ActivateFall()
	{
		if (_isTouched)
			return;

		_isTouched = true;
		StartCoroutine(FallCoroutine());
	}

	IEnumerator FallCoroutine()
	{
		while (_timeLeft > 0)
		{
			_timeLeft -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		Fall();
	}

	//public void ReduceTime()
	//{
	//	_timeLeft -= Time.deltaTime;
	//	if (_timeLeft <= 0)
	//	{
	//		Fall();
	//	}
	//}

	private void Fall()
	{
		_rigidB.isKinematic = false;
	}
}
