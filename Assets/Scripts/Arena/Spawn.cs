using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	void Start ()
	{
		Tile tileComponent = GetComponent<Tile>();
		if(tileComponent != null)
		{
			tileComponent.enabled = false; //commenté pour débug
		}
	}

	public void SpawnPlayer (PlayerController player)
	{
		player.transform.position = transform.position + Vector3.up * transform.localScale.y * 0.5f + Vector3.up * player.GetComponent<CapsuleCollider>().height * 0.5f * player.transform.localScale.y;
		player.transform.LookAt(transform.parent.position.SetAxis(Axis.y, player.transform.position.y));
	}
}
