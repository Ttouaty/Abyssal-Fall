using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ArenaGenerator : MonoBehaviour
{
	private GameObject[] _tiles;
	private GameObject _root;

	public uint Size = 20;
	public string PoolName;

	// Use this for initialization
	public void Init () {
		if (!GameObjectPool.PoolExists(PoolName))
		{
			Debug.LogError("The pool " + PoolName + " don't exists. Please verify if it exists or create it");
			Debug.Break();
		}
		else
		{
			_root = new GameObject("Root");
			_root.transform.parent = transform;

			_tiles = new GameObject[Size * Size];
			for (var z = 0; z < Size; ++z)
			{
				for (var x = 0; x < Size; ++x)
				{
					GameObject tile = GameObjectPool.GetAvailableObject("Tile.Default");
					tile.transform.position = new Vector3(-Size * 0.5f + x, 0, -Size * 0.5f + z);
					tile.transform.parent = _root.transform;
				}
			}
			CreateSpawns();
		}
	}

	private void CreateSpawns ()
	{
		Debug.Log("Generate spawns, players and inputs");
	}
}
