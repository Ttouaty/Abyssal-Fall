using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ArenaGenerator : MonoBehaviour
{

	private GameObject _tilesRoot;
	private GameObject _obstaclesRoot;
	private GameObject _playersRoot;
	private List<GameObject> _tiles;
	private List<GameObject> _obstacles;

	private int _groundsDropped;
	private int _obstaclessDropped;

	private Vector2[] _spawnPositions;

	[Header("ArenaGenerator References")]
	[Tooltip("References to players meshes")]
	public GameObject[] PlayerRef = new GameObject[4];
	[Tooltip("References to players meshes")]
	public Material[] SpritesIDMaterialRef = new Material[4];
	[Tooltip("Names of the pools required to build the level")]
	public List<string> RequiredPools = new List<string>();

	[Header("ArenaGenerator Options")]
	[Tooltip("Size of the level")]
	public int Size = 20;
	[Tooltip("Distance from border when the spawns appears")]
	public int SpawnDistanceFromBorder = 3;
	[Tooltip("Scale of tiles")]
	public float TileScale = 1.5f;

	[Header("ArenaGenerator Obstacles")]
	[Tooltip("Obstacles quantity")]
	public int ObstaclesQuantity = 10;
	[Tooltip("Obstacles probability decrease")]
	public int ObstaclesDecrease = 10;

	[HideInInspector]
	public Spawn[] Spawns;

	void Awake ()
	{
		_groundsDropped = 0;
		_obstaclessDropped = 0;

		int dist = SpawnDistanceFromBorder;
		_spawnPositions = new Vector2[4];
		_spawnPositions[0] = new Vector2(dist, dist);
		_spawnPositions[1] = new Vector2(dist, Size - dist - 1);
		_spawnPositions[2] = new Vector2(Size - dist - 1, dist);
		_spawnPositions[3] = new Vector2(Size - dist - 1, Size - dist - 1);
	}

	void Start ()
	{
		_tiles = new List<GameObject>();
		_tilesRoot = new GameObject("Tiles");
		_tilesRoot.transform.parent = transform;

		_obstacles = new List<GameObject>();
		_obstaclesRoot = new GameObject("Obstacles");
		_obstaclesRoot.transform.parent = transform;
		_obstaclesRoot.transform.position = new Vector3(0, TileScale * 1, 0);

		_playersRoot = new GameObject("Players");
		_playersRoot.transform.parent = transform;
	}

	public void StartGame()
	{
		for (int s = 0; s < Spawns.Length; ++s)
		{
			if (Spawns[s] != null)
			{
				Spawns[s].ActivatePlayer();
			}
		}
	}

	public void CreateArena () {
		for (var r = 0; r < RequiredPools.Count; ++r)
		{
			if (!GameObjectPool.PoolExists(RequiredPools[r]))
			{
				Debug.LogError("The pool " + RequiredPools[r] + " don't exists and is required by the ArenaGenerator. Please verify if it exists or create it");
				Debug.Break();
			}
		}

		int index = 0;
		for (var z = 0; z < Size; ++z)
		{
			for (var x = 0; x < Size; ++x)
			{
				GameObject tile = GameObjectPool.GetAvailableObject("Ground");
				tile.transform.localScale = new Vector3(TileScale, TileScale, TileScale);
				tile.transform.position = new Vector3(-Size * 0.5f * TileScale + x * TileScale, Camera.main.transform.position.y + (index++ % Size), -Size * 0.5f * TileScale + z * TileScale);
				tile.transform.parent = _tilesRoot.transform;
				_tiles.Add(tile);
			}
		}

		for(int t = 0; t < _tiles.Count; ++t)
		{
			Ground ground = _tiles[t].GetComponent<Ground>();

			int x = t % Size;
			int z = Mathf.FloorToInt(t / Size);

			if(x > 0)
			{
				ground.TileLeft = _tiles[x - 1 + z * Size];
			}
			if(x < Size - 1)
			{
				ground.TileRight = _tiles[x + 1 + z * Size];
			}
			if (z > 0)
			{
				ground.TileBack = _tiles[x + (z - 1) * Size];
			}
			if (z < Size - 1)
			{
				ground.TileForward = _tiles[x + (z + 1) * Size];
			}
		}
	}

	public void CreateSpawns()
	{

		Spawns = new Spawn[4];
		for (var s = 0; s < Spawns.Length; ++s)
		{
			int target = Mathf.FloorToInt(_spawnPositions[s].x + _spawnPositions[s].y * Size);
			GameObject tile = _tiles[target];
			Spawns[s] = tile.AddComponent<Spawn>();
		}
	}

	public void CreateObstacles ()
	{
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, Size * 0.5f * TileScale);
		List<GameObject> tiles = new List<GameObject>();
		for (var c = 0; c < hitColliders.Length; ++c)
		{
			tiles.Add(hitColliders[c].gameObject);
		}


		for (int o = 0; o < ObstaclesQuantity; ++o)
		{
			int index = Random.Range(0, tiles.Count - 1);
			GameObject tile = tiles[index];
			Ground ground = tile.GetComponent<Ground>();
			EDirection direction = Direction.GetRandomDirection();
			GameObject obstacle = GameObjectPool.GetAvailableObject("Obstacle");

			tiles.RemoveAt(index);

			ground.Obstacle = obstacle;
			obstacle.transform.parent = _obstaclesRoot.transform;
			obstacle.transform.position = new Vector3(tile.transform.position.x, Camera.main.transform.position.y + 1, tile.transform.position.z);

			_obstacles.Add(obstacle);

			CreateObstacleTrail(tiles, direction, tile, 80);
		}
	}

	private void CreateObstacleTrail (List<GameObject> tiles, EDirection direction, GameObject tile, int probabilite)
	{
		if(Random.Range(0,100) <= probabilite)
		{
			int intDirection = (int)direction;
			Ground ground = tile.GetComponent<Ground>();

			GameObject nextTile;
			switch(intDirection)
			{
				case 0: // FORWARD
					if(ground.TileForward == null)
						return;
					nextTile = ground.TileForward;
					break;
				case 1: // RIGHT
					if (ground.TileRight == null)
						return;
					nextTile = ground.TileRight;
					break;
				case 2: // BACK
					if (ground.TileBack == null)
						return;
					nextTile = ground.TileBack;
					break;
				default: // LEFT
					if (ground.TileLeft == null)
						return;
					nextTile = ground.TileLeft;
					break;
			}

			Ground nextGround = nextTile.GetComponent<Ground>();

			if (nextGround.Obstacle == null)
			{
				GameObject obstacle = GameObjectPool.GetAvailableObject("Obstacle");
				ground.Obstacle = obstacle;
				obstacle.transform.parent = _obstaclesRoot.transform;
				obstacle.transform.position = new Vector3(tile.transform.position.x, Camera.main.transform.position.y + 1, tile.transform.position.z);

				_obstacles.Add(obstacle);

				tiles.Remove(nextTile);
			}

			CreateObstacleTrail(tiles, direction, nextTile, probabilite - 20);
		}
	}

	public void CreatePlayers ()
	{
		for (int s = 0; s < Spawns.Length; ++s)
		{
			if (Spawns[s] != null && PlayerRef[s] != null)
			{
				GameObject player = Spawns[s].SpawnPlayer(s, PlayerRef[s], SpritesIDMaterialRef[s]);
				player.transform.parent = _playersRoot.transform;
			}
		}
	}
	public IEnumerator DropArena ()
	{
		yield return DropGrounds();
		CreateObstacles();
		yield return DropObstacles();
	}

	public IEnumerator DropGrounds ()
	{
		int ligne = 0;
		for (int t = 0; t < _tiles.Count; t += Size)
		{
			float delay = 0.05f * ligne;
			for(int i = 0; i < Size; ++i)
			{
				StartCoroutine(DropGround(_tiles[t+i], delay, i, _tiles[t+i].transform.position.y));
			}
			++ligne;
		}
		while (_groundsDropped < _tiles.Count)
		{
			yield return null;
		}
	}

	private IEnumerator DropGround(GameObject element, float delay, float pos, float initY)
	{
		yield return new WaitForSeconds(delay);

		float timer = -pos * 0.05f;
		float initialY = initY;

		while (timer < 1)
		{
			timer += Time.deltaTime * 0.75f;
			float y = Mathf.Lerp(initialY, 0, timer);
			element.transform.position = new Vector3(element.transform.position.x, y, element.transform.position.z);
			yield return null;
		}

		++_groundsDropped;

		yield return null;
	}

	public IEnumerator DropObstacles ()
	{
		for (int t = 0; t < _obstacles.Count; ++t)
		{
			StartCoroutine(DropObstacle(_obstacles[t], 0));
		}
		while (_obstaclessDropped < _obstacles.Count)
		{
			yield return null;
		}

		yield return null;
	}

	private IEnumerator DropObstacle(GameObject element, float delay)
	{
		yield return new WaitForSeconds(delay);

		float timer = 0;
		float initialY = 100;

		while (timer < 1)
		{
			timer += Time.deltaTime;
			float y = Mathf.Lerp(initialY, TileScale, timer);
			element.transform.position = new Vector3(element.transform.position.x, y, element.transform.position.z);
			yield return null;
		}

		element.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		++_obstaclessDropped;

		yield return null;
	}

	void OnDrawGizmos ()
	{
		int dist = SpawnDistanceFromBorder;

		Vector2[] spawnPositions = new Vector2[4];
		spawnPositions[0] = new Vector2(dist, dist);
		spawnPositions[1] = new Vector2(dist, Size - dist - 1);
		spawnPositions[2] = new Vector2(Size - dist - 1, dist);
		spawnPositions[3] = new Vector2(Size - dist - 1, Size - dist - 1);

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position + new Vector3(-0.5f * TileScale, 0, -0.5f * TileScale), new Vector3(Size * TileScale + 0.1f, TileScale + 0.2f, Size * TileScale + 0.1f));

		for (var i = 0; i < Size * Size; ++i)
		{
			int x = i % Size;
			int z = Mathf.FloorToInt(i / Size);

			bool spawn = false;
			for(var s = 0; s < spawnPositions.Length; ++s)
			{
				if(x == spawnPositions[s].x && z == spawnPositions[s].y)
				{
					spawn = true;
					break;
				}
			}
			if(spawn)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawCube(new Vector3(-Size * 0.5f * TileScale + x * TileScale, 0, -Size * 0.5f * TileScale + z * TileScale), new Vector3(TileScale, TileScale, TileScale));
			}
			else
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(new Vector3(-Size * 0.5f * TileScale + x * TileScale, 0, -Size * 0.5f * TileScale + z * TileScale), new Vector3(TileScale, TileScale, TileScale));
			}
		}

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, Size * 0.5f * TileScale);
	}
}
