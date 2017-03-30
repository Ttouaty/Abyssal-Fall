using UnityEngine;
using System.Collections;

public class LinkToGo : MonoBehaviour
{
	public GameObject targetGo;

	private Vector3 _distanceDiff;

	void Start()
	{
		_distanceDiff = transform.position - targetGo.transform.position;
		Link();
	}

	void Update()
	{
		Link();
	}

	void Link()
	{
		transform.position = targetGo.transform.position + _distanceDiff;

	}
}