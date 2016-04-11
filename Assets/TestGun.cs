using UnityEngine;
using System.Collections;

public class TestGun : MonoBehaviour {


	[SerializeField]
	private GameObject bullet;

	private Vector3 pouet
	{
		get 
		{
			return transform.position + transform.forward;
		}
	}

	void Start () {

	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
		{
			//Instantiate(bullet, transform.position + Vector3.forward * 3, Quaternion.AngleAxis(90, Vector3.right) * transform.rotation);
			RaycastHit _hit;
			//Physics.Linecast();
			if ( (bool) Physics.Raycast(transform.position, transform.forward, out _hit, 1 << LayerMask.NameToLayer("Flingue")))
			{
				Debug.Log(_hit.transform.gameObject.name);
			}
		}
	}
}
