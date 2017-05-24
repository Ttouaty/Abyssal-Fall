using UnityEngine;
using System.Collections;

public class KillPlane : MonoBehaviour
{
	void OnTriggerExit(Collider col)
	{
		if (col.tag == "Player")
		{
			col.GetComponent<PlayerController>().Kill();
		}
		if (col.tag == "Relic")
		{
			col.GetComponent<Relic>().Respawn();
		}
	}
}
