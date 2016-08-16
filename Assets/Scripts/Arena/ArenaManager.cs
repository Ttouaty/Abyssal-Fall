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

    public Transform TilesRoot;
    private List<GameObject> _tiles;
    private int _tilesDropped;

    public Transform ObstaclesRoot;
    private List<GameObject> _obstacles;
    private int _obstaclesDropped;

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
        _tiles = new List<GameObject>();
        _obstacles = new List<GameObject>();

        _tilesDropped = 0;
        _obstaclesDropped = 0;

        _currentArenaConfig = MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig;
        _currentModeConfig = MainManager.Instance.LEVEL_MANAGER.CurrentModeConfig;
        _mapConfig = MainManager.Instance.LEVEL_MANAGER.CurrentMapConfig;

        StartCoroutine(LoadArena());
    }

    IEnumerator LoadArena()
    {
        // Génération par fichier de config !!!
        Vector3 position            = new Vector3(0, Camera.main.transform.position.y + 5.0f, 0);
        List<List<string>> list     = CSVReader.Read(_mapConfig.MapFile);
        Map                         = new int[(int)_mapConfig.MapSize.y, (int)_mapConfig.MapSize.x];

        for (int y = 0; y < list.Count; ++y)
        {
            for(int x = 0; x < list[y].Count; ++x)
            {
                Map[y, x] = int.Parse(list[y][x]);
            }
        }

        CreateGrounds   (position);
        CreateObstacles (position);
        yield return StartCoroutine(DropArena(position));
    }

    void CreateGrounds(Vector3 position)
    {
        // Via fichier de config
        for(int y = 0; y < _mapConfig.MapSize.y; ++y)
        {
            for(int x = 0; x < _mapConfig.MapSize.x; ++x)
            {
                ETileType type = (ETileType)Map[y, x];
                if (type != ETileType.HOLE)
                {

                }
            }
        }
    }

    void CreateObstacles(Vector3 position)
    {
        // Via fichier de config
    }

    IEnumerator DropArena (Vector3 position)
    {
        yield return null;
    }

    IEnumerator DropGround(GameObject element, float delay, float pos, float initY, bool sound = false)
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

        ++_tilesDropped;

        yield return null;
    }

    IEnumerator DropObstacle(GameObject element, float delay, bool sound = false)
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

        if (sound)
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
