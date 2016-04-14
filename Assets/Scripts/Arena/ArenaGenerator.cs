using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ArenaGenerator : MonoBehaviour
{
	#region properties
	private GameObject _tilesRoot;
	private GameObject _obstaclesRoot;
	private GameObject _playersRoot;
	private List<GameObject> _tiles;
	private List<GameObject> _obstacles;
	private int _amoutGroupsToDrop;
	private List<List<GameObject>> _groundsToDrop;

	private int _groundsDropped;
	private int _obstaclessDropped;

	private Vector2[] _spawnPositions;

	[Header("ArenaGenerator References")]
	[Tooltip("References to players meshes")]
	public GameObject[] PlayerRef = new GameObject[4];
	[Tooltip("References to players Materials")]
	public Material[] PlayerMaterialRef = new Material[4];
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
	[Tooltip("Time before the exterior tiles drop automaticaly")]
	public int SecondsBeforeNextDrop = 5;

	[Header("ArenaGenerator Obstacles")]
	[Tooltip("Obstacles quantity")]
	public int ObstaclesQuantity = 10;
	[Tooltip("Obstacles probability decrease")]
	public int ObstaclesDecrease = 10;

	[HideInInspector]
	public List<Spawn> Spawns;
	[HideInInspector]
	public List<GameObject> Players;
	#endregion

	void Awake ()
	{
		int dist = SpawnDistanceFromBorder;

		_spawnPositions = new Vector2[4];
		_spawnPositions[0] = new Vector2(dist, dist);
		_spawnPositions[1] = new Vector2(dist, Size - dist - 1);
		_spawnPositions[2] = new Vector2(Size - dist - 1, dist);
		_spawnPositions[3] = new Vector2(Size - dist - 1, Size - dist - 1);

		_groundsToDrop = new List<List<GameObject>>();
	}

	void Start ()
	{
		GameManager.instance.OnPlayerDeath.AddListener(OnPlayerDeath);
		Init();
	}

	private void Init ()
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

		_groundsDropped = 0;
		_obstaclessDropped = 0;

		_groundsToDrop.Clear();
		_amoutGroupsToDrop = Mathf.FloorToInt(Size * 0.5f - Size / 10);
	}

	void OnPlayerDeath (GameObject player)
	{
		// Debug to play alone
		Players.Remove(player);
		player.GetComponent<PlayerController>().Spawn.Destroy();
		if (Players.Count == 1)
		{
			int playerId = Players[0].GetComponent<PlayerController>().PlayerNumber;
			GameManager.instance.PlayersScores[playerId-1]++;
			GameManager.instance.OnPlayerWin.Invoke(Players[0]);
			StopAllCoroutines();
			for (int t = 0; t < _tiles.Count; ++t)
			{
				_tiles[t].GetComponent<Tile>().StopAllCoroutines();
			}

			for (int o = 0; o < _obstacles.Count; ++o)
			{
				_obstacles[o].GetComponent<Obstacle>().StopAllCoroutines();
			}
			for (int s = 0; s < Spawns.Count; ++s)
			{
				Spawns[s].DestroyId();
				Spawns.Remove(Spawns[s]);
			}
		}
	}

	public void EndStage ()
	{
		for (int s = 0; s < Spawns.Count; ++s)
		{
			Spawn spawn = Spawns[s].GetComponent<Spawn>();
			spawn.StopAllCoroutines();
			spawn.Destroy();
			Destroy(spawn);
		}
		for (int t = 0; t < _tiles.Count; ++t)
		{
			_tiles[t].GetComponent<Poolable>().StopAllCoroutines();
			_tiles[t].GetComponent<Rigidbody>().isKinematic = true;
			GameObjectPool.AddObjectIntoPool(_tiles[t]);
		}
		for(int o = 0; o < _obstacles.Count; ++o)
		{
			_obstacles[o].GetComponent<Poolable>().StopAllCoroutines();
			_obstacles[o].GetComponent<Rigidbody>().isKinematic = true;
			GameObjectPool.AddObjectIntoPool(_obstacles[o]);
		}
	}

	public void ResetStage ()
	{
		StopAllCoroutines();
		EndStage();

		if(_tilesRoot != null)
			Destroy(_tilesRoot);
		if (_obstaclesRoot != null)
			Destroy(_obstaclesRoot);
		if (_playersRoot != null)
			Destroy(_playersRoot);

		Init();
	}

	public void StartGame ()
	{
		for (int s = 0; s < Spawns.Count; ++s)
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
				tile.transform.localScale = new Vector3(1,1,1);
				tile.transform.position = new Vector3(-Size * 0.5f * TileScale + x * TileScale, Camera.main.transform.position.y + (index++ % Size), -Size * 0.5f * TileScale + z * TileScale);
				tile.transform.parent = _tilesRoot.transform;
				Ground ground = tile.GetComponent<Ground>();
				ground.GridPosition = new Vector2(x, z);
				tile.GetComponent<Poolable>().SetReturnToPool(false);
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

		List<GameObject> tiles = new List<GameObject>();
		for(var t = 0; t < _tiles.Count; ++t)
		{
			tiles.Add(_tiles[t]);
		}

		for(var i = 0; i < _amoutGroupsToDrop; ++i)
		{
			List<GameObject> list = new List<GameObject>();
			for(var t = 0; t < tiles.Count; ++t)
			{
				GameObject tile = _tiles[t];
				Ground ground = tile.GetComponent<Ground>();
				
				if(ground.GridPosition.x == i || ground.GridPosition.x == Size - 1 - i ||
					ground.GridPosition.y == i || ground.GridPosition.y == Size - 1 - i)
				{
					list.Add(tile);
				}
			}
			_groundsToDrop.Add(list);
		}
	}

	public void CreateSpawns()
	{
		Spawns = new List<Spawn>();
		for (var s = 0; s < GameManager.instance.RegisteredPlayers.Length; ++s)
		{
			if(GameManager.instance.RegisteredPlayers[s] > 0 || (s == 0 || s == 1))
			{
				int target = Mathf.FloorToInt(_spawnPositions[s].x + _spawnPositions[s].y * Size);
				GameObject tile = _tiles[target];
				Spawns.Add(tile.AddComponent<Spawn>());
			}
			if((s == 0 || s == 1) && GameManager.instance.RegisteredPlayers[s] == 0)
			{
				GameManager.instance.RegisteredPlayers[s] = 1;
			}
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

		int quantity = Mathf.Min(ObstaclesQuantity, tiles.Count);

		for (int o = 0; o < quantity; ++o)
		{
			int index = Random.Range(0, tiles.Count - 1);
			GameObject tile = tiles[index];
			Ground ground = tile.GetComponent<Ground>();
			EDirection direction = Direction.GetRandomDirection();
			GameObject obstacle = GameObjectPool.GetAvailableObject("Obstacle");

			if(ground != null)
			{
				ground.Obstacle = obstacle;
				obstacle.transform.parent = _obstaclesRoot.transform;
				obstacle.transform.position = new Vector3(tile.transform.position.x, Camera.main.transform.position.y, tile.transform.position.z);
				obstacle.transform.Rotate(new Vector3(-90, 0, 0));
				obstacle.GetComponent<Poolable>().SetReturnToPool(false);

				_obstacles.Add(obstacle);

				tiles.RemoveAt(index);

				CreateObstacleTrail(tiles, direction, tile, 80);
			}
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

			if (nextGround.Obstacle == null && nextTile.GetComponent<Spawn>() == null)
			{
				GameObject obstacle = GameObjectPool.GetAvailableObject("Obstacle");
				nextGround.Obstacle = obstacle;
				obstacle.transform.parent = _obstaclesRoot.transform;
				obstacle.transform.position = new Vector3(nextTile.transform.position.x, Camera.main.transform.position.y, nextTile.transform.position.z);
				obstacle.transform.Rotate(new Vector3(-90, 0, 0));
				obstacle.GetComponent<Poolable>().SetReturnToPool(false);

				_obstacles.Add(obstacle);

				tiles.Remove(nextTile);
			}

			CreateObstacleTrail(tiles, direction, nextTile, probabilite - 20);
		}
	}

	public void CreatePlayers ()
	{
		Players = new List<GameObject>();
		for (int s = 0; s < Spawns.Count; ++s)
		{
			if (Spawns[s] != null && PlayerRef[s] != null)
			{
				GameObject player = Spawns[s].SpawnPlayer(s, PlayerRef[s], PlayerMaterialRef[s], SpritesIDMaterialRef[s]);
				player.transform.parent = _playersRoot.transform;
				player.GetComponent<PlayerController>().Spawn = Spawns[s];
				Players.Add(player);
			}
		}
	}
	public IEnumerator DropArena ()
	{
		yield return StartCoroutine(DropGrounds());
		CreateObstacles();
		yield return StartCoroutine(DropObstacles());
	}

	public IEnumerator DropGrounds ()
	{
		int ligne = 0;
		for (int t = 0; t < _tiles.Count; t += Size)
		{
			float delay = 0.15f + 0.05f * ligne;
			Debug.Log(delay);
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
			StartCoroutine(DropObstacle(_obstacles[t], 0.05f * t));
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
		element.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

		while (timer < 1)
		{
			timer += Time.deltaTime;
			float y = Mathf.Lerp(initialY, TileScale * 0.5f, timer);
			element.transform.position = new Vector3(element.transform.position.x, y, element.transform.position.z);
			yield return null;
		}

		element.GetComponent<Obstacle>().OnDropped();
		CameraShake.instance.Shake(0.2f);
		++_obstaclessDropped;

		yield return null;
	}

	public IEnumerator DropArenaOverTime ()
	{
		while(_groundsToDrop.Count > 0)
		{
			yield return new WaitForSeconds(SecondsBeforeNextDrop);
			for (var i = 0; i < _groundsToDrop[0].Count; ++i)
			{
				GameObject tile = _groundsToDrop[0][i];
				tile.GetComponent<Tile>().ActivateFall();
				tile.GetComponent<Poolable>().SetReturnToPool(true);

				Ground ground = tile.GetComponent<Ground>();
				if (ground != null && ground.Obstacle != null)
				{
					ground.Obstacle.GetComponent<Obstacle>().ActivateFall();
					ground.Obstacle.GetComponent<Poolable>().SetReturnToPool(true);
				}
			}
			_groundsToDrop.RemoveAt(0);
			GameManager.instance.OnZoom.Invoke();
		}
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
		Gizmos.DrawWireSphere(transform.position - new Vector3(TileScale * 0.5f, 0, TileScale * 0.5f), Size * 0.5f * TileScale);
	}
}
