using UnityEngine;
using System.Collections;

public class CameraFocusPoint : MonoBehaviour {

	public float timeTaken = 3;
	void Start ()
	{
		CameraManager.Instance.SetCenterPoint(transform, timeTaken);
	}
	
}
