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
	[HideInInspector]
	public ETileType[,]                             Map;
	[HideInInspector]
	public Vector3                                  Position;

	[HideInInspector] public Transform              ArenaRoot;
	[HideInInspector] public Transform              TilesRoot;
	[HideInInspector] public Transform              ObstaclesRoot;
	[HideInInspector] public Transform              PlayersRoot;
	[HideInInspector] public Transform              SpecialsRoot;
	[HideInInspector] public Transform              BehavioursRoot;

	public ArenaMasterManager MasterManagerPrefab;
	public Transform VictoryPlateformParent;

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

	void Update()
	{
		if(NetworkServer.active)
		{
			ArenaMasterManager.Instance.ForceIntroSkip = false;

			// Skip Intro with L + R + A
			if(InputManager.GetButtonHeld(InputEnum.LB))
			{
				if (InputManager.GetButtonHeld(InputEnum.RB))
				{
					ArenaMasterManager.Instance.ForceIntroSkip = InputManager.GetButtonHeld(InputEnum.A);
				}
			}
		}
	}

	public void ResetMap(bool animate = true)
	{
		gameObject.SetActive(true);

		UnloadArena();
		//ArenaMasterManager.Instance.GameInProgress = false;
		StartCoroutine(Initialization(animate));
	}

	public void UnloadArena()
	{
		CameraManager.Instance.ClearTrackedTargets();

		Transform[] roots = new Transform[] { TilesRoot, ObstaclesRoot, SpecialsRoot, BehavioursRoot };
		int i;
		Transform child;

		for (i = 0; i < roots.Length; ++i)
		{
			if (roots[i].childCount > 0)
			{
				for (int j = roots[i].childCount - 1; j >= 0; --j)
				{
					child = roots[i].GetChild(j);
					Poolable poolable = child.GetComponent<Poolable>();
					if (poolable != null)
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

		for (i = 0; i < PlayersRoot.childCount; ++i)
		{
			child = PlayersRoot.GetChild(i);
			child.GetComponent<PlayerController>().OnBeforeDestroy();
			Destroy(child.gameObject);
		}

		_tilesDropped = 0;
		_obstaclesDropped = 0;

		_tiles = new Tile[(int)(_currentMapConfig.MapSize.x * _currentMapConfig.MapSize.y)];
		_obstacles = new List<Obstacle>();
		_spawns = new List<Spawn>();
		_behaviours = new List<ABaseBehaviour>();

		if (NetworkServer.active)
		{
			for (i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
			{
				if (ServerManager.Instance.RegisteredPlayers[i].Controller != null)
				{
					Debug.Log("Destroying character " + ServerManager.Instance.RegisteredPlayers[i].Controller._characterData.IngameName);
					Destroy(ServerManager.Instance.RegisteredPlayers[i].Controller.gameObject);
				}
			}
		}
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
			//Debug.Log("Tile n°=> "+index+" was null in Arenamanager");
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

		for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
		{
			ServerManager.Instance.RegisteredPlayers[i].Controller.RpcUnFreeze();
		}
		//ArenaMasterManager.Instance.GameInProgress = true;
	}

	public void PlaceCharacters()
	{
		_players = new GameObject[ServerManager.Instance.RegisteredPlayers.Count];

		//Clean up remaining characters
		//for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
		//{
		//	if(ServerManager.Instance.RegisteredPlayers[i].Controller != null)
		//	{
		//		Debug.Log("Destroying character "+ ServerManager.Instance.RegisteredPlayers[i].Controller._characterData.IngameName);
		//		Destroy(ServerManager.Instance.RegisteredPlayers[i].Controller.gameObject);
		//	}
		//}

		List<int> unUsedSpawnIndexes = new List<int>();
		for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
		{
			unUsedSpawnIndexes.Add(i);
		}

		for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; ++i)
		{
			Player player = ServerManager.Instance.RegisteredPlayers[i];
			if (player != null)
			{
				_players[i] = Instantiate(player.CharacterUsed.gameObject) as GameObject;
				PlayerController playerController = _players[i].GetComponent<PlayerController>();
				Spawn selectedSpawn = _spawns[unUsedSpawnIndexes.ShiftRandomElement()];

				selectedSpawn.SpawnPlayer(playerController);
				ArenaMasterManager.Instance.RpcDisplayPlayerNumber(player.PlayerNumber, selectedSpawn.transform.position + Vector3.up * 6, 3);
				selectedSpawn.Colorize(GameManager.Instance.PlayerColors[i]);

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
					tileComp.TileCoordinates = new Vector2(x,y);
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

		yield return new WaitUntil(() => !AutoFade.Fading);

		for (i = 0; i < _tiles.Length; ++i)
		{
			if(_tiles[i] != null)
			{
				if(animate)
				{
					StartCoroutine(DropGround(_tiles[i].gameObject, index));
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
					StartCoroutine(DropObstacle(_obstacles[i].gameObject, 0.02f * index, index % 5 == 0));
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

	private IEnumerator DropGround (GameObject element, float pos)
	{
		float timeForMapToSpawn = 1.5f;

		float timer = -timeForMapToSpawn * pos / Map.Length;
		float initialY = element.transform.localPosition.y;

		while (timer < 1 && !ArenaMasterManager.Instance.ForceIntroSkip)
		{
			timer += TimeManager.DeltaTime;
			float y = Mathf.Lerp(initialY, 0, timer);
			element.transform.localPosition = new Vector3(element.transform.localPosition.x, y, element.transform.localPosition.z);
			yield return null;
		}
		element.transform.localPosition = new Vector3(element.transform.localPosition.x, 0, element.transform.localPosition.z);
		++_tilesDropped;

		yield return null;
	}

	private IEnumerator DropObstacle (GameObject element, float delay, bool bIsSounded)
	{
		yield return new WaitForSeconds(delay);

		float timer = 0;
		float initialY = element.transform.localPosition.y;

		while (timer < 1 && !ArenaMasterManager.Instance.ForceIntroSkip)
		{
			timer += TimeManager.DeltaTime;
			float y = Mathf.Lerp(initialY, TileScale, timer);
			element.transform.localPosition = new Vector3(element.transform.localPosition.x, y, element.transform.localPosition.z);
			yield return null;
		}
		element.transform.localPosition = new Vector3(element.transform.localPosition.x, TileScale, element.transform.localPosition.z);

		if (bIsSounded)
		{
			GameManager.Instance.AudioSource.PlayOneShot(GameManager.Instance.OnObstacleDrop);
		}

		element.GetComponent<Obstacle>().OnDropped();
		CameraManager.Shake(ShakeStrength.Medium);
		++_obstaclesDropped;

		yield return null;
	}

	public int[] GetOutsideTiles(int distanceToBorder)
	{
		//Determine most distant tile row
		int farthestRow = Mathf.CeilToInt(Mathf.Max(CurrentMapConfig.MapSize.x * 0.5f, CurrentMapConfig.MapSize.y * 0.5f)) - distanceToBorder;
		Vector2 mapCenter = CurrentMapConfig.MapSize * 0.5f;
		
		//fix truelle
		mapCenter.x -= TileScale * 0.5f;
		mapCenter.y -= TileScale * 0.5f;


		List<int> selectedTiles = new List<int>();

		for (int i = 0; i < Tiles.Length; i++)
		{
			if(Tiles[i] != null)
			{
				if ((int)Mathf.Max(Mathf.Abs(Tiles[i].TileCoordinates.x - mapCenter.x), Mathf.Abs(Tiles[i].TileCoordinates.y - mapCenter.y)) >= farthestRow)
					selectedTiles.Add(i);
			}
		}

		return selectedTiles.ToArray();
	}

	public void DisplayWinner(GameObject winnerGo)
	{
		Debug.Log("Start coroutine DisplayWinnerCoroutine with gameObject => " + winnerGo.name);
		StartCoroutine(DisplayWinnerCoroutine(winnerGo));
	}

	IEnumerator DisplayWinnerCoroutine(GameObject winnerGo)
	{
		AutoFade.StartFade(0.5f, 0.5f, 0.5f, Color.white);
		yield return new WaitForSeconds(0.5f);

		UnloadArena();

		VictoryPlatform victoryPlatform = Instantiate(_currentArenaConfig.VictoryPlatformGo, VictoryPlateformParent, false) as VictoryPlatform;
		victoryPlatform.transform.localPosition = Vector3.zero;

		GameObject characterGo = Instantiate(winnerGo.GetComponent<Player>().CharacterUsed._characterData.CharacterWinModel.gameObject, victoryPlatform.CharacterPos.transform, false) as GameObject;

		characterGo.GetComponentInChildren<CharacterModel>().Reskin(winnerGo.GetComponent<Player>().SkinNumber);
		characterGo.GetComponentInChildren<CharacterModel>().SetOutlineColor(winnerGo.GetComponent<Player>().PlayerColor);
		characterGo.transform.localPosition = Vector3.zero;
		characterGo.transform.localRotation = Quaternion.identity;

		yield return new WaitForSeconds(0.2f);
		characterGo.GetComponentInChildren<Animator>().SetTrigger("Win");
		yield return new WaitForSeconds(4);

		EndGameManager.Instance.Open();
	}
}
