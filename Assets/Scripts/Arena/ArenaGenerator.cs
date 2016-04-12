using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ArenaGenerator : MonoBehaviour
{
	private GameObject _root;
	private GameObject[] _tiles;

	public uint Size = 20;
	public string PoolName;

	[HideInInspector]
	public Spawn[] Spawns;

	// Use this for initialization
	public void CreateArena () {
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

			int index = 0;
			for (var z = 0; z < Size; ++z)
			{
				for (var x = 0; x < Size; ++x)
				{
					GameObject tile = GameObjectPool.GetAvailableObject("Tile.Default");
					Vector3 scale = tile.transform.localScale;
					tile.transform.position = new Vector3(-Size * 0.5f * scale.x + x * scale.x, 0, -Size * 0.5f * scale.z + z * scale.z);
					tile.transform.parent = _root.transform;
					_tiles[index++] = tile;
				}
			}
		}
	}

	public void CreateSpawns ()
	{
		int rand = Random.Range(0, _tiles.Length - 1);
		GameObject randomSpawn = _tiles[rand];
		randomSpawn.AddComponent<Spawn>();
		Spawns = new Spawn[1];
		Spawns[0] = randomSpawn.GetComponent<Spawn>();
	}
}
