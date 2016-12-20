using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ArenaMasterManager : NetworkBehaviour
{
	public static ArenaMasterManager Instance;

	/*
		Doit gérer:
		Les tiles
		Les behaviors
		Les CallBacks de morts
		
	*/

	void Awake()
	{
		Instance = this;
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		transform.SetParent(ArenaManager.Instance.transform); // parent to ArenaManager so it is destroyed when changing scenes (May cause bugs !)



	}

	[ClientRpc]
	public void RpcRemoveTileAtIndex(int index)
	{
		ArenaManager.Instance.RemoveTile(index);
	}

}
