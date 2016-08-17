using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum ETileType : int
{
    GROUND      = 0,
    OBSTACLE    = 1,
    SPAWN       = 2,
    HOLE        = 3
}

public class ArenaManager : GenericSingleton<ArenaManager>
{
    [SerializeField] private ArenaConfiguration_SO  _currentArenaConfig;
    [SerializeField] private ModeConfiguration_SO   _currentModeConfig;
    [SerializeField] private MapConfiguration_SO    _mapConfig;

    public Transform ArenaRoot;
    public int[,] Map;
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
        _mapConfig              = MainManager.Instance.LEVEL_MANAGER.CurrentMapConfig;

        _tilesDropped           = 0;
        _obstaclesDropped       = 0;
        _mapSize                = (int)(_mapConfig.MapSize.y * _mapConfig.MapSize.x);

        _tiles                  = new GameObject[_mapSize];
        _obstacles              = new GameObject[_mapSize];
        _spawns                 = new List<Spawn>();

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
            if(player != null)
            {
                _players[i] = player.GetPlayerController().InstantiatePlayer();
                _spawns[i].SpawnPlayer(_players[i]);
            }
        }
    }

    IEnumerator LoadArena()
    {
        position    = new Vector3(
            -_mapConfig.MapSize.x * 0.5f + 0.5f, 
            Camera.main.transform.position.y + 5.0f, 
            -_mapConfig.MapSize.y * 0.5f + 0.5f
        );
        Map         = ParseMapFile();

        CreateMap(position);
        yield return StartCoroutine(DropArena(position));
    }

    int[,] ParseMapFile ()
    {
        List<List<string>> rawMap = CSVReader.Read(_mapConfig.MapFile);
        int[,] map = new int[(int)_mapConfig.MapSize.y, (int)_mapConfig.MapSize.x];
        for (int y = 0; y < rawMap.Count; ++y)
        {
            for (int x = 0; x < rawMap[y].Count; ++x)
            {
                map[y, x] = int.Parse(rawMap[y][x]);
            }
        }
        return map;
    }

    void CreateMap(Vector3 position)
    {
        // Via fichier de config
        for (int y = 0; y < _mapConfig.MapSize.y; ++y)
        {
            for(int x = 0; x < _mapConfig.MapSize.x; ++x)
            {
                ETileType type = (ETileType)Map[y, x];
                if (type != ETileType.HOLE)
                {
                    GameObject tile = GameObjectPool.GetAvailableObject(_currentArenaConfig.Ground.name);
                    if(type == ETileType.SPAWN)
                    {
                        Spawn spawn = tile.AddComponent<Spawn>();
                        _spawns.Add(spawn);
                    }
                    tile.transform.position = new Vector3(x, 0, y) + position;
                    tile.transform.parent = TilesRoot;
                    _tiles[y * (int)_mapConfig.MapSize.y + x] = tile;

                    if (type == ETileType.OBSTACLE)
                    {
                        GameObject obstacle = GameObjectPool.GetAvailableObject(_currentArenaConfig.Obstacle.name);
                        obstacle.transform.position = new Vector3(x, 1, y) + position;
                        obstacle.transform.parent = ObstaclesRoot;
                        _obstacles[y * (int)_mapConfig.MapSize.y + x] = obstacle;
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
                float delay = 0.05f * Mathf.Floor(i / _mapConfig.MapSize.y);
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
        float initialY = initY;

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
        float initialY = 150;
        element.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        while (timer < 1)
        {
            timer += Time.deltaTime;
            float y = Mathf.Lerp(initialY, 0.5f, timer);
            element.transform.position = new Vector3(element.transform.position.x, y, element.transform.position.z);
            yield return null;
        }

        if(bIsSounded)
        {
            GameManager.Instance.AudioSource.PlayOneShot(GameManager.Instance.OnObstacleDrop);
        }

        element.GetComponent<Obstacle>().OnDropped();
        CameraShake.instance.shakeAmount = 0.7f;
        CameraShake.instance.Shake(0.2f);
        ++_obstaclesDropped;

        yield return null;
    }


    void OnDrawGizmos()
    {
        if(_currentModeConfig != null)
        {
            for (var i = 0; i < _currentModeConfig.ArenaSizeSquared; ++i)
            {
                int x = i % _currentModeConfig.ArenaSize;
                int z = Mathf.FloorToInt(i / _currentModeConfig.ArenaSize);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(
                    ArenaRoot.position + new Vector3(-_currentModeConfig.ArenaSize * 0.5f + x + 0.5f, 0, -_currentModeConfig.ArenaSize * 0.5f + z + 0.5f), 
                    new Vector3(1,1,1)
                );
            }
        }
    }
}
