using UnityEngine;
using System.Collections;

public class KillPlane : MonoBehaviour
{
	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player")
		{
			col.GetComponent<PlayerController>().Kill();
		}
		else if(col.gameObject.layer == LayerMask.NameToLayer("Ground")) 
		{
			Tile tileComp = col.transform.GetComponent<Tile>();
			if(tileComp != null && tileComp.CanFall)
			{
				Debug.Log("respawn tile");
				GameManager.Instance.GameRules.RespawnFalledTiles(tileComp);
			}
		}
	}
}
