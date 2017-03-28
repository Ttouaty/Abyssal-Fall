using UnityEngine;
using System.Collections;

public class FaceCam : MonoBehaviour
{
	public Vector3 AxisMatrix = Vector3.one;
	public bool FrontFace = true;
	public Transform Up;
	void Start()
	{
		Update();
	}

	void Update()
	{
		Vector3 targetForward = (Camera.main.transform.position - transform.position);

		if (!FrontFace)
			targetForward *= -1;

		if (AxisMatrix.x == 0)
			targetForward.ZeroX();
		if (AxisMatrix.y == 0)
			targetForward.ZeroY();
		if (AxisMatrix.z == 0)
			targetForward.ZeroZ();

		if (targetForward.magnitude == 0)
			return;

		if(Up != null)
			transform.rotation = Quaternion.LookRotation(targetForward.normalized, Up.up);
		else
			transform.rotation = Quaternion.LookRotation(targetForward.normalized);
	}
}
