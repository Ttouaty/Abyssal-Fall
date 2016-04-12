using UnityEngine;
using System.Collections;

public class SpriteID : MonoBehaviour
{
	private Vector3 _cameraPos;

	[HideInInspector]
	public Transform Target;

	void Start ()
	{
		_cameraPos = new Vector3(Camera.main.transform.position.x, 5, Camera.main.transform.position.z);
		transform.localScale = new Vector3(3, 3, 3);
	}

	// Update is called once per frame
	void Update ()
	{
		if(Target != null)
		{
			transform.LookAt(2 * Target.position - _cameraPos);
			transform.position = Target.position + Vector3.up * 3;
		}
	}
}
