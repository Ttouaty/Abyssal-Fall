using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum ETileType : int
{
	GROUND      = -1,
	OBSTACLE    =  0,
	SPAWN       =  1,
	HOLE        =  2
}

public class ArenaManager : MonoBehaviour
{
	public static ArenaManager Instance;
	#region private
	private bool                                    _initialInit = false;

	private ArenaConfiguration_SO                   _currentArenaConfig;
	private AGameRules								_currentModeConfig;
	private MapConfiguration_SO                     _currentMapConfig;

	private int                                     _tilesDropped;
	private int                                     _obstaclesDropped;
	private GameObject[]                            _players;
	private Tile[]									 _tiles;
	private List<Obstacle>							_obstacles;
	private List<ABaseBehaviour>                    _behaviours;
	private List<Spawn>                             _spawns;
	#endregion
	#region public
	public float                                    TileScale = 1.50f;
	public ETileType[,]                             Map;
	public Vector3                                  Position;

	[HideInInspector] public Transform              ArenaRoot;
	[HideInInspector] public Transform              TilesRoot;
	[HideInInspector] public Transform              ObstaclesRoot;
	[HideInInspector] public Transform              PlayersRoot;
	[HideInInspector] public Transform              SpecialsRoot;
	[HideInInspector] public Transform              BehavioursRoot;

	public ArenaMasterManager MasterManagerPrefab;

	#endregion
	#region getter/setter
	public Tile[]									Tiles				{ get { return _tiles;				} }
	public List<Obstacle>							Obstacles			{ get { return _obstacles;			} }
	public GameObject[]                             Players				{ get { return _players;			} }
	public List<Spawn>                              Spawns				{ get { return _spawns;				} }
	public List<ABaseBehaviour>                     Behaviours			{ get { return _behaviours;			} }
	public ArenaConfiguration_SO					CurrentArenaConfig	{ get { return _currentArenaConfig;	} }
	public AGameRules								CurrentModeConfig	{ get { return _currentModeConfig;	} }
	public MapConfiguration_SO						CurrentMapConfig	{ get { return _currentMapConfig;	} }
	#endregion

	void Awake()
	{
		Instance = this;

		if(NetworkServer.active)
		{
			GameObject ArenaMasterManagerGo = Instantiate(MasterManagerPrefab.gameObject);
			NetworkServer.SpawnWithClientAuthority(ArenaMasterManagerGo, ServerManager.Instance.HostingClient.gameObject);
		}

	}

	public void Init ()
	{
		if(!_initialInit)
		{
			
			_currentArenaConfig		= MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig;
			_currentModeConfig		= MainManager.Instance.LEVEL_MANAGER.CurrentModeConfig;
			_currentMapConfig		= MainManager.Instance.LEVEL_MANAGER.CurrentMapConfig;

			ArenaRoot				= new GameObject("ArenaRoot").transform;
			ArenaRoot.SetParent(transform, false);
			TilesRoot				= new GameObject("TilesRoot").transform;
			TilesRoot.SetParent(ArenaRoot, false);
			ObstaclesRoot = new GameObject("ObstaclesRoot").transform;
			ObstaclesRoot.SetParent(ArenaRoot, false);
			PlayersRoot				= new GameObject("PlayersRoot").transform;
			PlayersRoot.SetParent(ArenaRoot, false);
			SpecialsRoot			= new GameObject("SpecialsRoot").transform;
			SpecialsRoot.SetParent(ArenaRoot, false);
			BehavioursRoot			= new GameObject("BehavioursRoot").transform;
			BehavioursRoot.SetParent(ArenaRoot, false);

			GameObject killPlane = new GameObject("KillPlane", typeof(KillPlane));
			BoxCollider collider = killPlane.AddComponent<BoxCollider>();
			killPlane.transform.SetParent(ArenaRoot, false);
			collider.isTrigger = true;
			collider.size = new Vector3(100, 30, 100);
			killPlane.transform.localScale = new Vector3(1, 1, 1);
			killPlane.transform.localPosition = new Vector3(0, 10, 0);
			killPlane.layer = LayerMask.NameToLayer("KillPlane");


			Player.LocalPlayer.CmdReadyToSpawnMap();
		}
		else
		{
			Debug.LogError("[ArenaManager] Trying to reinitialize the instance");
			Debug.Break();
		}
	}

	public void ResetMap(bool animate = true)
	{
		CameraManager.Instance.ClearTrackedTargets();
		gameObject.SetActive(true);

		Transform[] roots = new Transform[] { TilesRoot, ObstaclesRoot, SpecialsRoot, BehavioursRoot };
		int i;
		Transform child;

		for (i = 0; i < roots.Length; ++i)
		{
			if (roots[i].childCount > 0)
			{
				for(int j = roots[i].childCount - 1; j >= 0; --j)
				{
					child = roots[i].GetChild(j);
					Poolable poolable = child.GetComponent<Poolable>();
					if(poolable != null)
					{
						poolable.AddToPool();
					}
					else
					{
						Destroy(child.gameObject);
					}
				}
			}
		}

		for(i = 0; i < PlayersRoot.childCount; ++i)
		{
			child = PlayersRoot.GetChild(i);
			child.GetComponent<PlayerController>().OnBeforeDestroy();
			Destroy(child.gameObject);
		}

		_tilesDropped       = 0;
		_obstaclesDropped   = 0;

		_tiles              = new Tile[(int)(_currentMapConfig.MapSize.x * _currentMapConfig.MapSize.y)];
		_obstacles          = new List<Obstacle>();
		_spawns             = new List<Spawn>();
		_behaviours         = new List<ABaseBehaviour>();

		StartCoroutine(Initialization(animate));
	}

	//Don't use this one
	public void RemoveTile(Tile tile)
	{
		Player.LocalPlayer.CmdRemoveTile(Array.IndexOf(_tiles, tile));
	}

	public void RemoveTile(int index)
	{
		if(_tiles[index] != null)
		{
			if (!_tiles[index].IsFalling)
				_tiles[index].MakeFall();
			_tiles[index] = null;
		}
		else
		{
			Debug.Log("Tile n°=> "+index+" was null in Arenamanager");
		}
	}

	public void RemoveObstacle(Obstacle obstacle)
	{
		_obstacles.Remove(obstacle);
	}

	public void EnableBehaviours()
	{
		foreach(ABaseBehaviour behaviour in _behaviours)
		{
			behaviour.Run();
		}
	}

	public void DisableBehaviours()
	{
		foreach (ABaseBehaviour behaviour in _behaviours)
		{
			behaviour.Stop();
		}
	}

	private IEnumerator Initialization (bool animate = true)
	{
		MenuPauseManager.Instance.CanPause = false;

		List<BehaviourConfiguration> list = new List<BehaviourConfiguration>();
		list.Add(_currentArenaConfig.Behaviours);
		list.Add(_currentModeConfig.Behaviours);

		for(int i = 0; i < list.Count; ++i)
		{
			Debug.LogWarning("behaviors needs to be managed by server !");
			ABaseBehaviour behaviour    = Instantiate(list[i].Behaviour);
			behaviour.transform.parent  = BehavioursRoot;
			_behaviours.Add(behaviour);
		}

		yield return StartCoroutine(LoadArena(animate));

		if(NetworkServer.active)
			PlaceCharacters();

		yield return StartCoroutine(CountdownManager.Instance.Countdown());
		GameManager.Instance.GameRules.InitGameRules();
		EnableBehaviours();
	}

	public void PlaceCharacters()
	{
		_players = new GameObject[ServerManager.Instance.RegisteredPlayers.Count];

		//Clean up remaining characters
		for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
		{
			if(ServerManager.Instance.RegisteredPlayers[i].Controller != null)
			{
				Debug.Log("Destroying character "+ ServerManager.Instance.RegisteredPlayers[i].Controller._characterData.IngameName);
				Destroy(ServerManager.Instance.RegisteredPlayers[i].Controller.gameObject);
			}
		}

		for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; ++i)
		{
			Player player = ServerManager.Instance.RegisteredPlayers[i];
			if (player != null)
			{
				_players[i] = Instantiate(player.CharacterUsed.gameObject) as GameObject;
				PlayerController playerController = _players[i].GetComponent<PlayerController>();
				_spawns[i].SpawnPlayer(playerController);
				playerController.UnFreeze();

				NetworkServer.SpawnWithClientAuthority(_players[i], player.gameObject);
				player.RpcInitController(_players[i]);
			}
		}
	}

	private IEnumerator LoadArena (bool animate = true)
	{
		Map = ParseMapFile();

		Position = new Vector3(
			transform.position.x - _currentMapConfig.MapSize.x * 0.5f * TileScale + 0.5f * TileScale,
			transform.position.y + 50,
			transform.position.z - _currentMapConfig.MapSize.y * 0.5f * TileScale + 0.5f * TileScale
		);

		CreateMap(Position);
		yield return StartCoroutine(DropArena(Position, animate));
	}

	private ETileType[,] ParseMapFile ()
	{
		List<List<string>> rawMap = CSVReader.Read(_currentMapConfig.MapFile);
		ETileType[,] map = new ETileType[(int)_currentMapConfig.MapSize.y, (int)_currentMapConfig.MapSize.x];
		for (int y = 0; y < rawMap.Count; ++y)
		{
			for (int x = 0; x < rawMap[y].Count; ++x)
			{
				if(rawMap[y][x] != "")
				{
					map[y, x] = (ETileType)int.Parse(rawMap[y][x]);
				}
			}
		}
		return map;
	}

	private void CreateMap (Vector3 position)
	{
		for (int y = 0; y < _currentMapConfig.MapSize.y; ++y)
		{
			for(int x = 0; x < _currentMapConfig.MapSize.x; ++x)
			{
				ETileType type = Map[y, x];
				if (type != ETileType.HOLE)
				{
					// Create Tile
					GameObject tile                 = GameObjectPool.GetAvailableObject(_currentArenaConfig.Ground.name);
					tile.transform.SetParent(TilesRoot, false);
					tile.transform.localScale       = new Vector3(TileScale, TileScale, TileScale);
					tile.transform.position			= new Vector3(x * TileScale, 0, y * TileScale) + position;

					// Remove Spawn Comp
					Destroy(tile.GetComponent<Spawn>());

					// Add Tile Comp
					Tile tileComp = tile.GetComponent<Tile>();
					if(tileComp == null)
					{
						tileComp = tile.AddComponent<Tile>();
					}
					
					tileComp.SetTimeLeft(tileComp.TimeLeftSave); // TODO -> Regler dans l'inspecteur
					_tiles[(int)(y * _currentMapConfig.MapSize.x) + x] = tileComp;
					tileComp.TileIndex = (int)(y * _currentMapConfig.MapSize.x) + x;
					tileComp.SpawnComponent = null;
					tileComp.enabled = true;
					

					if (type == ETileType.SPAWN)
					{
						// Add Spawn Comp
						tileComp.SpawnComponent = tile.AddComponent<Spawn>();
						_spawns.Add(tileComp.SpawnComponent);
						tileComp.SetTimeLeft(tileComp.TimeLeftSave * 2, false);
					}
					else if (type == ETileType.OBSTACLE)
					{
						// Create Obstacle
						GameObject obstacle             = GameObjectPool.GetAvailableObject(_currentArenaConfig.Obstacle.name);
						obstacle.transform.SetParent(ObstaclesRoot, false);
						obstacle.transform.localScale   = new Vector3(TileScale, TileScale, TileScale);
						obstacle.transform.position     = new Vector3(x * TileScale, 1, y * TileScale) + position;

						// Add Obstacle Comp
						Obstacle obstacleComp = obstacle.GetComponent<Obstacle>();
						if(obstacleComp == null)
						{
							obstacleComp = obstacle.AddComponent<Obstacle>();
						}
						tileComp.Obstacle = obstacleComp;
						_obstacles.Add(obstacleComp);
					}
				}
			}
		}
	}

	private IEnumerator DropArena (Vector3 position, bool animate = true)
	{
		int i = 0;
		int index = 0;
		for (i = 0; i < _tiles.Length; ++i)
		{
			if(_tiles[i] != null)
			{
				if(animate)
				{
					float delay = 0.01f * Mathf.Floor(i / _currentMapConfig.MapSize.y);
					StartCoroutine(DropGround(_tiles[i].gameObject, delay, index, _tiles[i].transform.localPosition.y));
					++index;
				}
				else
				{
					Vector3 targetPosition = _tiles[i].transform.localPosition;
					targetPosition.y = 0;
					_tiles[i].transform.localPosition = targetPosition;
					++_tilesDropped;
				}
			}
			else
			{
				++_tilesDropped;
			}
		}
		while (_tilesDropped < _tiles.Length)
		{
			yield return null;
		}

		index = 0;
		for (i = 0; i < _obstacles.Count; ++i)
		{
			if(_obstacles[i] != null)
			{
				if(animate)
				{
					StartCoroutine(DropObstacle(_obstacles[i].gameObject, 0.05f * index, index % 5 == 0));
					++index;
				}
				else
				{
					Vector3 targetPosition = _obstacles[i].transform.localPosition;
					targetPosition.y = 1.0f * TileScale;
					_obstacles[i].transform.localPosition = targetPosition;
					++_obstaclesDropped;
				}
			}
			else
			{
				++_obstaclesDropped;
			}
		}
		while (_obstaclesDropped < _obstacles.Count)
		{
			yield return null;
		}
	}

	private IEnumerator DropGround (GameObject element, float delay, float pos, float initY)
	{
		yield return new WaitForSeconds(delay);

		float timer = -pos * 0.05f;
		float initialY = element.transform.localPosition.y;

		while (timer < 1)
		{
			timer += TimeManager.DeltaTime * 5;
			float y = Mathf.Lerp(initialY, 0, timer);
			element.transform.localPosition = new Vector3(element.transform.localPosition.x, y, element.transform.localPosition.z);
			yield return null;
		}

		++_tilesDropped;

		yield return null;
	}

	private IEnumerator DropObstacle (GameObject element, float delay, bool bIsSounded)
	{
		yield return new WaitForSeconds(delay);

		float timer = 0;
		float initialY = element.transform.localPosition.y;
		element.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

		while (timer < 1)
		{
			timer += TimeManager.DeltaTime;
			float y = Mathf.Lerp(initialY, 1.0f * TileScale, timer);
			element.transform.localPosition = new Vector3(element.transform.localPosition.x, y, element.transform.localPosition.z);
			yield return null;
		}

		if(bIsSounded)
		{
			GameManager.Instance.AudioSource.PlayOneShot(GameManager.Instance.OnObstacleDrop);
		}

		element.GetComponent<Obstacle>().OnDropped();
		CameraManager.Shake(ShakeStrength.Medium);
		++_obstaclesDropped;

		yield return null;
	}
}
