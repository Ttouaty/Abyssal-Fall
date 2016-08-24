using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainManager : GenericSingleton<MainManager>
{
    [HideInInspector] public GameObjectPool       GAME_OBJECT_POOL;
	[HideInInspector] public DynamicConfig DYNAMIC_CONFIG;
	[HideInInspector] public LevelManager LEVEL_MANAGER;
	[HideInInspector] public TimeManager TIME_MANAGER;
	[HideInInspector] public SoundManager SOUND_MANAGER;
	[HideInInspector] public LoadingScreen LOADING_MANAGER;
	[HideInInspector] public GameManager GAME_MANAGER;
	[HideInInspector] public ArenaManager ARENA_MANAGER;
	[HideInInspector] public InputManager INPUT_MANAGER;
	[HideInInspector] public CoolDownManager COOLDOWN_MANAGER;

    void Awake ()
    {
        GAME_OBJECT_POOL    = GameObjectPool.Instance;
        DYNAMIC_CONFIG      = DynamicConfig.Instance;
        LEVEL_MANAGER       = LevelManager.Instance;
        INPUT_MANAGER       = InputManager.Instance;
        COOLDOWN_MANAGER    = CoolDownManager.Instance;
        GAME_MANAGER        = GameManager.Instance;
        TIME_MANAGER        = TimeManager.Instance;

        if (GAME_OBJECT_POOL == null)
        {
            Debug.LogError("[public GameObjectPool GAME_OBJECT_POOL] is Required to launch the game");
            Debug.Break();
        }
        if (DYNAMIC_CONFIG == null)
        {
            Debug.LogError("[public DynamicConfig DYNAMIC_CONFIG] is Required to launch the game");
            Debug.Break();
        }
        if (LEVEL_MANAGER == null)
        {
            Debug.LogError("[public LevelManager LEVEL_MANAGER] is Required to launch the game");
            Debug.Break();
        }
        if (INPUT_MANAGER == null)
        {
            Debug.LogError("[public InputManager INPUT_MANAGER] is Required to launch the game");
            Debug.Break();
        }
        if (COOLDOWN_MANAGER == null)
        {
            Debug.LogError("[public CoolDownManager COOLDOWN_MANAGER] is Required to launch the game");
            Debug.Break();
        }
        if (GAME_MANAGER == null)
        {
            Debug.LogError("[public GameManager GAME_MANAGER] is Required to launch the game");
            Debug.Break();
        }
        if (TIME_MANAGER == null)
        {
            Debug.LogError("[public TimeManager TIME_MANAGER] is Required to launch the game");
            Debug.Break();
        }
    }

    void Start ()
    {
        LEVEL_MANAGER.OpenMenu();
    }
}
