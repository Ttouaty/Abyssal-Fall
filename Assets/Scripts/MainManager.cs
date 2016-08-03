using UnityEngine;
using System.Collections;

public class MainManager : GenericSingleton<MainManager>
{
    public GameObjectPool        GAME_OBJECT_POOL;
    public DynamicConfig         DYNAMIC_CONFIG;
    public LevelManager          LEVEL_MANAGER;
    public SoundManager          SOUND_MANAGER;

    public TestLoadingScreen     LOADING_MANAGER;
    public GameManager           GAME_MANAGER;
    public ArenaManager          ARENA_MANAGER;

    public InputManager          INPUT_MANAGER;
    public CoolDownManager       COOLDOWN_MANAGER;

    void Awake ()
    {
        GAME_OBJECT_POOL    = GameObjectPool.Instance;
        DYNAMIC_CONFIG      = DynamicConfig.Instance;
        LEVEL_MANAGER       = LevelManager.Instance;
        INPUT_MANAGER       = InputManager.Instance;
        COOLDOWN_MANAGER    = CoolDownManager.Instance;
        GAME_MANAGER        = GameManager.Instance;

        if (GAME_OBJECT_POOL == null)
        {
            Debug.LogError("[public GameObjectPool GAME_OBJECT_POOL] is Required to launch the game");
            Debug.Break();
        }
        if (DYNAMIC_CONFIG == null)
        {
            Debug.LogError("[public DynamiConfig DYNAMIC_CONFIG] is Required to launch the game");
            Debug.Break();
        }
        if (LEVEL_MANAGER == null)
        {
            Debug.LogError("[public LevelManager LEVEL_MANAGER] is Required to launch the game");
            Debug.Break();
        }
    }

    void Start ()
    {
        LEVEL_MANAGER.OpenMenu();
    }
}
