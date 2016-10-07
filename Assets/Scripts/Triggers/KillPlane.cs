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
		else if(col.gameObject.layer == LayerMask.NameToLayer("Ground")) 
		{
			Tile tileComp = col.transform.GetComponent<Tile>();
			if(tileComp != null && tileComp.CanFall)
			{
				GameManager.Instance.GameRules.RespawnFalledTiles(tileComp);
			}
		}
	}
}
