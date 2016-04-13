using UnityEngine;
using System.Collections;

public class Island : MonoBehaviour
{
	[Header("Floating Options")]
	[SerializeField]
	[Range(1, 10)]
	private float _amplitude = 5;
	[SerializeField]
	[Range(0.1f,2)]
	private float _speed = 2;

	private float _delay;
	private float _initialY;

	void Start ()
	{
		_initialY = transform.position.y;
		_delay = Random.Range(-20, 20);
	}

	// Update is called once per frame
	void Update ()
	{
		transform.position = new Vector3(transform.position.x, _initialY + _amplitude * Mathf.Sin(_speed * (Time.time - _delay)), transform.position.z);
	}
}
