using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArenaManager : GenericSingleton<ArenaManager>
{
    public Transform ArenaRoot;

    private ArenaConfiguration_SO _currentArenaConfig;
    private ModeConfiguration_SO _currentModeConfig;
    private List<GameObject> _tiles;
    private List<GameObject> _obstacles;
    private int _tilesDropped;

    void Awake ()
    {
        if(ArenaRoot == null)
        {
            Debug.Log("ArenaRoot is required !");
            Debug.Break();
        }
    }

    void Start ()
    {
        _tiles = new List<GameObject>();
        _obstacles = new List<GameObject>();
    }

    public void Init ()
    {
        _currentArenaConfig = MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig;
        _currentModeConfig = MainManager.Instance.LEVEL_MANAGER.CurrentModeConfig;

        Vector3 position = new Vector3(0, Camera.main.transform.position.y, 0);

        int tileCount = _currentModeConfig.ArenaSize * _currentModeConfig.ArenaSize;
        for (int z = 0; z < _currentModeConfig.ArenaSize; ++z)
        {
            position.z = Mathf.Floor(z % _currentModeConfig.ArenaSize) - _currentModeConfig.ArenaSize * 0.5f;

            for (int x = 0; x < _currentModeConfig.ArenaSize; ++x)
            {
                GameObject tile = GameObjectPool.GetAvailableObject(_currentArenaConfig.Ground.name);
                tile.transform.SetParent(ArenaRoot);

                position.x = (x % _currentModeConfig.ArenaSize) - _currentModeConfig.ArenaSize * 0.5f;

                tile.transform.position = position;
                _tiles.Add(tile);
            }
        }

        StartCoroutine(DropArena());
    }


    private IEnumerator DropArena ()
    {
        int ligne = 0;
        for (int t = 0; t < _tiles.Count; t += _currentModeConfig.ArenaSize)
        {
            float delay = 0.15f + 0.05f * ligne;
            for (int i = 0; i < _currentModeConfig.ArenaSize; ++i)
            {
                StartCoroutine(DropGround(_tiles[t + i], delay, i, _tiles[t + i].transform.position.y, i % _currentModeConfig.ArenaSize * 2 == 0));
            }
            ++ligne;
        }
        while (_tilesDropped < _tiles.Count)
        {
            yield return null;
        }
    }

    private IEnumerator DropGround(GameObject element, float delay, float pos, float initY, bool sound = false)
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
}
