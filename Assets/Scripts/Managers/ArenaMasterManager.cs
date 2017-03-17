using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ArenaMasterManager : NetworkBehaviour
{
	public static ArenaMasterManager Instance;

	//[SyncVar]
	//public bool GameInProgress = false;

	[HideInInspector]
	[SyncVar]
	public bool ForceIntroSkip = false;
	void Start()
	{
		if (ArenaManager.Instance != null)
			transform.SetParent(ArenaManager.Instance.transform); // parent to ArenaManager so it is destroyed when changing scenes (May cause bugs !)
	}

	void Update()
	{
		if (transform.parent == null)
			if (ArenaManager.Instance != null)
				transform.SetParent(ArenaManager.Instance.transform);
	}

	void Awake()
	{
		Instance = this;
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
	}

	[ClientRpc]
	public void RpcRemoveTile(int index)
	{
		ArenaManager.Instance.RemoveTile(index);
	}

	[ClientRpc]
	public void RpcRemoveTiles(int[] indexes)
	{
		for (int i = 0; i < indexes.Length; i++)
		{
			ArenaManager.Instance.RemoveTile(indexes[i]);
		}
	}

	[ClientRpc]
	public void RpcDisplayPlayerNumber(int playerNumber, Vector3 position, float destroyDelay)
	{
		GameObject newImage = Instantiate(GameManager.Instance.PlayerNumberImages[playerNumber - 1], transform, false) as GameObject;
		newImage.transform.position = position;
		Destroy(newImage, destroyDelay);
	}
}