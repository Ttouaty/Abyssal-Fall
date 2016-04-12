using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ArenaGenerator : MonoBehaviour
{
	private GameObject _root;
	private GameObject[] _tiles;

	[Header("ArenaGenerator Options")]
	[Tooltip("Size of the level")]
	public int Size = 20;
	[Tooltip("Distance from border when the spawns appears")]
	public int SpawnDistanceFromBorder = 3;
	[Tooltip("Names of the pools required to build the level")]
	public string[] RequiredPools;

	[HideInInspector]
	public Spawn[] Spawns;

	// Use this for initialization
	public void CreateArena () {
		for (var r = 0; r < RequiredPools.Length; ++r)
		{
			if (!GameObjectPool.PoolExists(RequiredPools[r]))
			{
				Debug.LogError("The pool " + RequiredPools[r] + " don't exists and is required by the ArenaGenerator. Please verify if it exists or create it");
				Debug.Break();
			}
		}

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

	public void CreateSpawns()
	{
		int dist = SpawnDistanceFromBorder;

		Vector2[] spawnPositions = new Vector2[4];
		spawnPositions[0] = new Vector2(dist, dist);
		spawnPositions[1] = new Vector2(dist, Size - dist);
		spawnPositions[2] = new Vector2(Size - dist, dist);
		spawnPositions[3] = new Vector2(Size - dist, Size - dist);

		Spawns = new Spawn[4];
		for (var s = 0; s < Spawns.Length; ++s)
		{
			int target = Mathf.FloorToInt(spawnPositions[s].x + spawnPositions[s].y * Size);
			GameObject tile = _tiles[target];
			Spawns[s] = tile.AddComponent<Spawn>();
		}
	}
}
