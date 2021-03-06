﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DebugPoolTotos : MonoBehaviour
{

	IEnumerator Start()
	{
		yield return new WaitUntil(()=> NetworkServer.active);
		PlayerController[] tempPlayersFound = FindObjectsOfType<PlayerController>();
		for (int i = 0; i < tempPlayersFound.Length; i++)
		{

			PoolConfiguration[] assets = tempPlayersFound[i]._characterData.OtherAssetsToLoad;
			for (int j = 0; j < assets.Length; ++j)
			{
				GetComponent<GameObjectPool>().AddPool(assets[j]);
			}
		}
		GetComponent<GameObjectPool>().Load();
	}

}
