﻿using UnityEngine;
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

public class ArenaManager : GenericSingleton<ArenaManager>
{
	#region private
	private bool                                    _initialInit = false;

	private ArenaConfiguration_SO                   _currentArenaConfig;
	private AGameRules								_currentModeConfig;
	private MapConfiguration_SO                     _currentMapConfig;

	private int                                     _tilesDropped;
	private int                                     _obstaclesDropped;
	private GameObject[]                            _players;
	private List<Tile>								_tiles;
	private List<Obstacle>							_obstacles;
	private List<ABaseBehaviour>                    _behaviours;
	private List<Spawn>                             _spawns;
	#endregion
	#region public
	public float                                    TileScale = 1.51f;
	public ETileType[,]                             Map;
	public Vector3                                  Position;

	public Transform                                ArenaRoot;
	public Transform                                TilesRoot;
	public Transform                                ObstaclesRoot;
	public Transform                                PlayersRoot;
	public Transform                                SpecialsRoot;
	public Transform                                BehavioursRoot;
	#endregion
	#region getter/setter
	public List<Tile>								Tiles				{ get { return _tiles;				} }
	public List<Obstacle>							Obstacles			{ get { return _obstacles;			} }
	public GameObject[]                             Players				{ get { return _players;			} }
	public List<Spawn>                              Spawns				{ get { return _spawns;				} }
	public List<ABaseBehaviour>                     Behaviours			{ get { return _behaviours;			} }
	public ArenaConfiguration_SO					CurrentArenaConfig	{ get { return _currentArenaConfig;	} }
	public AGameRules								CurrentModeConfig	{ get { return _currentModeConfig;	} }
	public MapConfiguration_SO						CurrentMapConfig	{ get { return _currentMapConfig;	} }
	#endregion

	public override void Init ()
	{
		if(!_initialInit)
		{
			_currentArenaConfig		= MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig;
			_currentModeConfig		= MainManager.Instance.LEVEL_MANAGER.CurrentModeConfig;
			_currentMapConfig		= MainManager.Instance.LEVEL_MANAGER.CurrentMapConfig;

			GameObject killPlane			= new GameObject("KillPlane", typeof(KillPlane));
			BoxCollider collider			= killPlane.AddComponent<BoxCollider>();
			collider.isTrigger				= true;
			collider.size					= new Vector3(100, 5, 100);
			killPlane.transform.position	= new Vector3(0, -15.0f, 0);
			killPlane.layer					= LayerMask.NameToLayer("KillPlane");
			killPlane.transform.parent		= transform;

			ArenaRoot				= new GameObject("ArenaRoot").transform;
			ArenaRoot.SetParent(transform);
			TilesRoot				= new GameObject("TilesRoot").transform;
			TilesRoot.SetParent(ArenaRoot);
			ObstaclesRoot			= new GameObject("ObstaclesRoot").transform;
			ObstaclesRoot.SetParent(ArenaRoot);
			PlayersRoot				= new GameObject("PlayersRoot").transform;
			PlayersRoot.SetParent(ArenaRoot);
			SpecialsRoot			= new GameObject("SpecialsRoot").transform;
			SpecialsRoot.SetParent(ArenaRoot);
			BehavioursRoot			= new GameObject("BehavioursRoot").transform;
			BehavioursRoot.SetParent(ArenaRoot);

			ResetMap(true);
		}
		else
		{
			Debug.LogError("[ArenaManager] Trying to reinitialize the instance");
			Debug.Break();
		}
	}

	public void ResetMap(bool animate = true)
	{
		Transform[] roots = new Transform[] { TilesRoot, ObstaclesRoot, PlayersRoot, SpecialsRoot, BehavioursRoot };

		for (int i = 0; i < roots.Length; ++i)
		{
			if (roots[i].childCount > 0)
			{
				for(int j = roots[i].childCount - 1; j >= 0; --j)
				{
					Transform child = roots[i].GetChild(j);
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

		_tilesDropped       = 0;
		_obstaclesDropped   = 0;

		_tiles              = new List<Tile>();
		_obstacles          = new List<Obstacle>();
		_spawns             = new List<Spawn>();
		_behaviours         = new List<ABaseBehaviour>();

		StartCoroutine(Initialization(animate));
	}

	public void RemoveTile(Tile tile)
	{
		_tiles.Remove(tile);
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

		// Spanw Players
		_players = new GameObject[GameManager.Instance.nbPlayers];
		for (int i = 0; i < GameManager.Instance.nbPlayers; ++i)
		{
			Player player = GameManager.Instance.RegisteredPlayers[i];
			if (player != null)
			{
				_players[i] = Instantiate(player.CharacterUsed.gameObject);
				_players[i].transform.SetParent(PlayersRoot);
				PlayerController playerController = _players[i].GetComponent<PlayerController>();
				if (playerController != null)
				{
					playerController.Init(player);
					player.Controller = playerController;
				}
				else
				{
					Debug.LogError("No player controller");
					Debug.Break();
				}
				_spawns[i].SpawnPlayer(playerController);
			}
		}

		yield return StartCoroutine(CountdownManager.Instance.Countdown());
		GameManager.Instance.GameRules.InitGameRules();
		EnableBehaviours();
	}

	private IEnumerator LoadArena (bool animate = true)
	{
		Map = ParseMapFile();

		Position = new Vector3(
			-_currentMapConfig.MapSize.x * 0.5f * TileScale + 0.5f * TileScale, 
			Camera.main.transform.position.y + 5.0f, 
			-_currentMapConfig.MapSize.y * 0.5f * TileScale + 0.5f * TileScale
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
					tile.transform.localScale       = new Vector3(TileScale, TileScale, TileScale);
					tile.transform.position         = new Vector3(x * TileScale, 0, y * TileScale) + position;
					tile.transform.parent           = TilesRoot;

					// Remove Spawn Comp
					Destroy(tile.GetComponent<Spawn>());

					// Add Tile Comp
					Tile tileComp = tile.GetComponent<Tile>();
					if(tileComp == null)
					{
						tileComp = tile.AddComponent<Tile>();
					}
					tileComp.SetTimeLeft(0.8f); // TODO -> Regler dans l'inspecteur
					_tiles.Add(tileComp);


					if (type == ETileType.SPAWN)
					{
						// Add Spawn Comp
						tileComp.SpawnComponent = tile.AddComponent<Spawn>();
						_spawns.Add(tileComp.SpawnComponent);
						tileComp.SetTimeLeft(1.5f);
					}
					else if (type == ETileType.OBSTACLE)
					{
						// Create Obstacle
						GameObject obstacle             = GameObjectPool.GetAvailableObject(_currentArenaConfig.Obstacle.name);
						obstacle.transform.localScale   = new Vector3(TileScale, TileScale, TileScale);
						obstacle.transform.position     = new Vector3(x * TileScale, 1, y * TileScale) + position;
						obstacle.transform.parent       = ObstaclesRoot;

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
		for (i = 0; i < _tiles.Count; ++i)
		{
			if(_tiles[i] != null)
			{
				if(animate)
				{
					float delay = 0.05f * Mathf.Floor(i / _currentMapConfig.MapSize.y);
					StartCoroutine(DropGround(_tiles[i].gameObject, delay, index, _tiles[i].transform.position.y));
					++index;
				}
				else
				{
					Vector3 targetPosition = _tiles[i].transform.position;
					targetPosition.y = 0;
					_tiles[i].transform.position = targetPosition;
					++_tilesDropped;
				}
			}
			else
			{
				++_tilesDropped;
			}
		}
		while (_tilesDropped < _tiles.Count)
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
					Vector3 targetPosition = _obstacles[i].transform.position;
					targetPosition.y = 1.0f * TileScale;
					_obstacles[i].transform.position = targetPosition;
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
		float initialY = element.transform.position.y;

		while (timer < 1)
		{
			timer += TimeManager.DeltaTime * 2;
			float y = Mathf.Lerp(initialY, 0, timer);
			element.transform.position = new Vector3(element.transform.position.x, y, element.transform.position.z);
			yield return null;
		}

		++_tilesDropped;

		yield return null;
	}

	private IEnumerator DropObstacle (GameObject element, float delay, bool bIsSounded)
	{
		yield return new WaitForSeconds(delay);

		float timer = 0;
		float initialY = element.transform.position.y;
		element.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

		while (timer < 1)
		{
			timer += TimeManager.DeltaTime;
			float y = Mathf.Lerp(initialY, 1.0f * TileScale, timer);
			element.transform.position = new Vector3(element.transform.position.x, y, element.transform.position.z);
			yield return null;
		}

		if(bIsSounded)
		{
			GameManager.Instance.AudioSource.PlayOneShot(GameManager.Instance.OnObstacleDrop);
		}

		element.GetComponent<Obstacle>().OnDropped();
		CameraShake.instance.Shake(0.2f);
		++_obstaclesDropped;

		yield return null;
	}
}
