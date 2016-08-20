using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum ETileType : int
{
    GROUND      = -1,
    OBSTACLE    = 0,
    SPAWN       = 1,
    HOLE        = 2
}

public class ArenaManager : GenericSingleton<ArenaManager>
{
    [SerializeField] private ArenaConfiguration_SO  _currentArenaConfig;
    [SerializeField] private ModeConfiguration_SO   _currentModeConfig;
    [SerializeField] private MapConfiguration_SO    _currentMapConfig;

    public Transform ArenaRoot;
    public ETileType[,] Map;
    public Vector3 position;
    private int _mapSize;

    public Transform TilesRoot;
    private GameObject[] _tiles;
    private int _tilesDropped;

    public Transform ObstaclesRoot;
    private GameObject[] _obstacles;
    private int _obstaclesDropped;

    public Transform PlayersRoot;
    private GameObject[] _players;

    private List<Spawn> _spawns;

    void Awake ()
    {
        if (ArenaRoot == null)
        {
            Debug.Log("ArenaRoot is required !");
            Debug.Break();
        }

        if (TilesRoot == null)
        {
            Debug.Log("TilesRoot is required !");
            Debug.Break();
        }

        if (ObstaclesRoot == null)
        {
            Debug.Log("ObstaclesRoot is required !");
            Debug.Break();
        }
    }

    public void Init ()
    {
        _currentArenaConfig     = MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig;
        _currentModeConfig      = MainManager.Instance.LEVEL_MANAGER.CurrentModeConfig;
        _currentMapConfig       = MainManager.Instance.LEVEL_MANAGER.CurrentMapConfig;

        _tilesDropped           = 0;
        _obstaclesDropped       = 0;
        _mapSize                = (int)(_currentMapConfig.MapSize.y * _currentMapConfig.MapSize.x);

        _tiles                  = new GameObject[_mapSize];
        _obstacles              = new GameObject[_mapSize];
        _spawns                 = new List<Spawn>();

        GameObject go = new GameObject("KillPlane", typeof(KillPlane), typeof(BoxCollider));
        go.GetComponent<BoxCollider>().isTrigger = true;
        go.transform.parent = transform;
        go.transform.position.Set(0, 0, -10.0f);
        go.GetComponent<BoxCollider>().size.Set(100, 1, 100);

        StartCoroutine(Initialization());
    }

    IEnumerator Initialization ()
    {
        yield return StartCoroutine(LoadArena());

        // Spanw Players
        _players = new GameObject[GameManager.Instance.nbPlayers];
        for (int i = 0; i < GameManager.Instance.nbPlayers; ++i)
        {
            Player player = GameManager.Instance.RegisteredPlayers[i];
            if (player != null)
            {
                _players[i] = Instantiate(player.CharacterUsed.gameObject);
                PlayerController playerController = _players[i].GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.Init(player);
                }
                else
                {
                    Debug.LogError("No player controller");
                    Debug.Break();
                }
                _spawns[i].SpawnPlayer(_players[i]);
            }
        }
    }

    IEnumerator LoadArena()
    {
        position    = new Vector3(-_currentMapConfig.MapSize.x * 0.5f + 0.5f, Camera.main.transform.position.y + 5.0f, -_currentMapConfig.MapSize.y * 0.5f + 0.5f);
        Map = ParseMapFile();

        CreateMap(position);
        yield return StartCoroutine(DropArena(position));
    }

    ETileType[,] ParseMapFile ()
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

    void CreateMap(Vector3 position)
    {
        for (int y = 0; y < _currentMapConfig.MapSize.y; ++y)
        {
            for(int x = 0; x < _currentMapConfig.MapSize.x; ++x)
            {
                ETileType type = Map[y, x];
                if (type != ETileType.HOLE)
                {
                    GameObject tile = GameObjectPool.GetAvailableObject(_currentArenaConfig.Ground.name);
                    tile.transform.position = new Vector3(x, 0, y) + position;
                    tile.transform.parent = TilesRoot;
                    _tiles[y * (int)_currentMapConfig.MapSize.y + x] = tile;

                    if (type == ETileType.SPAWN)
                    {
                        Spawn spawn = tile.AddComponent<Spawn>();
                        _spawns.Add(spawn);
                    }
                    else if (type == ETileType.OBSTACLE)
                    {
                        GameObject obstacle = GameObjectPool.GetAvailableObject(_currentArenaConfig.Obstacle.name);
                        obstacle.transform.parent = ObstaclesRoot;
                        obstacle.transform.position = new Vector3(x, 1, y) + position;
                        _obstacles[y * (int)_currentMapConfig.MapSize.y + x] = obstacle;

                        Ground groundComponent = tile.GetComponent<Ground>();
                        if(groundComponent != null)
                        {
                            groundComponent.Obstacle = obstacle.GetComponent<Obstacle>();
                        }
                    }
                }
            }
        }
    }

    IEnumerator DropArena (Vector3 position)
    {
        int i = 0;
        int index = 0;
        for (i = 0; i < _tiles.Length; ++i)
        {
            if(_tiles[i] != null)
            {
                float delay = 0.05f * Mathf.Floor(i / _currentMapConfig.MapSize.y);
                StartCoroutine(DropGround(_tiles[i], delay, index, _tiles[i].transform.position.y));
                ++index;
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
        for (i = 0; i < _obstacles.Length; ++i)
        {
            if(_obstacles[i] != null)
            {
                StartCoroutine(DropObstacle(_obstacles[i], 0.05f * index, index % 5 == 0));
                ++index;
            }
            else
            {
                ++_obstaclesDropped;
            }
        }
        while (_obstaclesDropped < _obstacles.Length)
        {
            yield return null;
        }
    }

    IEnumerator DropGround(GameObject element, float delay, float pos, float initY)
    {
        yield return new WaitForSeconds(delay);

        float timer = -pos * 0.05f;
        float initialY = element.transform.position.y;

        while (timer < 1)
        {
            timer += Time.deltaTime * 2;
            float y = Mathf.Lerp(initialY, 0, timer);
            element.transform.position = new Vector3(element.transform.position.x, y, element.transform.position.z);
            yield return null;
        }

        ++_tilesDropped;

        yield return null;
    }

    IEnumerator DropObstacle(GameObject element, float delay, bool bIsSounded)
    {
        yield return new WaitForSeconds(delay);

        float timer = 0;
        float initialY = element.transform.position.y;
        element.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        while (timer < 1)
        {
            timer += Time.deltaTime;
            float y = Mathf.Lerp(initialY, 1.0f, timer);
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

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(_currentMapConfig != null)
        {
            int mapSize = (int)(_currentMapConfig.MapSize.x * _currentMapConfig.MapSize.y);
            for (var i = 0; i < mapSize; ++i)
            {
                int x = i % (int)_currentMapConfig.MapSize.x;
                int z = Mathf.FloorToInt(i / _currentMapConfig.MapSize.y);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(
                    ArenaRoot.position + new Vector3(-_currentMapConfig.MapSize.x * 0.5f + x + 0.5f, 0, -_currentMapConfig.MapSize.y * 0.5f + z + 0.5f), 
                    new Vector3(1,1,1)
                );
            }
        }
    }
    #endif
}
