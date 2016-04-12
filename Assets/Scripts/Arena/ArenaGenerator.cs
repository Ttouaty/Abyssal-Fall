using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ArenaGenerator : MonoBehaviour
{
	private GameObject _root;
	private GameObject[] _tiles;

	private GameObject[] _walls;
	private Vector3[] _wallsPosition;

	private GameObject[] _spawns;

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
			_walls = new GameObject[4];
			_wallsPosition = new Vector3[4];
			_wallsPosition[0] = new Vector3(-Size * 0.5f, 1.5f, 0);
			_wallsPosition[1] = new Vector3(0, 1.5f, -Size * 0.5f);
			_wallsPosition[2] = new Vector3(Size * 0.5f, 1.5f, 0);
			_wallsPosition[3] = new Vector3(0, 1.5f, Size * 0.5f);

			for (var w = 0; w < _walls.Length; ++w)
			{
				GameObject wall = GameObjectPool.GetAvailableObject("Wall");
				wall.transform.position = _wallsPosition[w];
				Vector3 scale = wall.transform.localScale;
				wall.transform.localScale = new Vector3(Size, scale.y, scale.z);
				wall.transform.Rotate(Vector3.up, (90+ w * 90) % 360);
				wall.transform.parent =  _root.transform;
			}

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
