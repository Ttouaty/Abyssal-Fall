using UnityEngine;
using System.Collections;

public class FaceCam : MonoBehaviour
{

	void Start()
	{
		Update();
	}

	void Update()
	{
		transform.LookAt(Camera.main.transform, Camera.main.transform.up);
	}
}
