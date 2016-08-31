#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisplayMap : MonoBehaviour 
{
	public TextAsset			MapFile;
	public GameObject			PrefabGround;
	public GameObject			PrefabObstacle;
	public float				GlobalScale = 1;

	[Space(10)]
	[Header("Debug, do not touch !")]
	private Vector2             _mapSize;
	private GameObject          _arenaRoot;
	private GameObject          _groundsRoot;
	private List<GameObject>    _grounds;
	private GameObject          _obstaclesRoot;
	private List<GameObject>    _obstacles;
	private ETileType[,]		_map;

	void Start ()
	{
		Transform arenaRoot = transform.FindChild("ArenaRoot");
		if(arenaRoot != null)
		{
			Destroy(arenaRoot.gameObject);
		}
		GenerateMap();
	}

	public void GenerateMap()
	{
		if (MapFile != null && PrefabGround != null && PrefabObstacle != null)
		{
			_map = ParseMapFile();

			Vector3 Position = new Vector3(
				-_mapSize.x * 0.5f * GlobalScale + 0.5f * GlobalScale,
				0.0f,
				-_mapSize.y * 0.5f * GlobalScale + 0.5f * GlobalScale
			);

			if (_arenaRoot != null)
			{
				DestroyImmediate(_arenaRoot, false);
			}

			CreateMap(Position);
		}
		else 
		{
			Debug.LogError("Invalid parameters");
		}
	}

	private ETileType[,] ParseMapFile ()
	{
		List<List<string>> rawMap = CSVReader.Read(MapFile);
		_mapSize = new Vector2(rawMap.Count - (rawMap[rawMap.Count-1][0] == "" ? 1 : 0), rawMap[0].Count);
		ETileType[,] map = new ETileType[rawMap.Count, rawMap[0].Count];
		for (int y = 0; y < rawMap.Count; ++y)
		{
			for (int x = 0; x < rawMap[y].Count; ++x)
			{
				if (rawMap[y][x] != "")
				{
					map[y, x] = (ETileType)int.Parse(rawMap[y][x]);
				}
			}
		}
		return map;
	}

	private void CreateMap(Vector3 position)
	{
		_arenaRoot		= new GameObject("ArenaRoot");
		_groundsRoot	= new GameObject("GroundRoot");
		_obstaclesRoot	= new GameObject("ObstaclesRoot");

		_arenaRoot.transform.parent		= transform;
		_groundsRoot.transform.parent	= _arenaRoot.transform;
		_obstaclesRoot.transform.parent = _arenaRoot.transform;

		_grounds = new List<GameObject>();
		_obstacles = new List<GameObject>();

		for (int y = 0; y < _mapSize.y; ++y)
		{
			for (int x = 0; x < _mapSize.x; ++x)
			{
				ETileType type = _map[y, x];
				if (type != ETileType.HOLE)
				{
					GameObject tile = Instantiate(PrefabGround);
					tile.transform.localScale = new Vector3(GlobalScale, GlobalScale, GlobalScale);
					tile.transform.position = new Vector3(x * GlobalScale, 0, y * GlobalScale) + position;
					tile.transform.parent = _groundsRoot.transform;

					_grounds.Add(tile);

					if (type == ETileType.OBSTACLE)
					{
						GameObject obstacle = Instantiate(PrefabObstacle);
						obstacle.transform.localScale = new Vector3(GlobalScale, GlobalScale, GlobalScale);
						obstacle.transform.position = new Vector3(x * GlobalScale, GlobalScale, y * GlobalScale) + position;
						obstacle.transform.parent = _obstaclesRoot.transform;

						_obstacles.Add(obstacle);
					}
				}
			}
		}
	}
}
#endif
