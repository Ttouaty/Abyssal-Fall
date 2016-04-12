using UnityEngine;
using System.Collections;

public class KillPlane : MonoBehaviour
{
	void OnTriggerEnter(Collider colli)
	{
		if (colli.tag == "Player")
		{
			colli.GetComponent<PlayerController>().Kill();
		}
	}
	
}
