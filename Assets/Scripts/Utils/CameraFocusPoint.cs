using UnityEngine;
using System.Collections;

public class CameraFocusPoint : MonoBehaviour {

	public float timeTaken = 3;
	public float distance = 30;
	public bool applyRotation = false;
	public bool autoFocus = true;

	void Start ()
	{
		if (autoFocus)
			Invoke("Focus", 0.1f); //mon dieu pardonnez moi, car j'ai eu la flemme de faire un truc propre (doigt dans ton cul)
	}

	public void Focus()
	{
		CameraManager.Instance.SetCenterPoint(transform, timeTaken, distance, applyRotation);
	}

}
