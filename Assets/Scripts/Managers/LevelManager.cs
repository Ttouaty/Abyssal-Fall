using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct LevelConfiguration
{
    public GameObject[] Players;
}

public class LevelManager : GenericSingleton<LevelManager>
{
    public List<SceneField>                 CurrentScenes = new List<SceneField>();

    /* Variables */
    [Header("Game Settings")]
    public ArenaConfiguration_SO            CurrentArenaConfig;
    public ModeConfiguration_SO             CurrentModeConfig;
    public MapConfiguration_SO              CurrentMapConfig;

    /* Defined Scenes */
    [Header("Scenes Settings")]
    public SceneField                       SceneLoading;
    public SceneField                       SceneMenu;
    public SceneField                       SceneCountdown;
    public SceneField                       ScenePause;
    public SceneField                       SceneEndStage;
    public SceneField                       SceneEndGame;

    /* Events */
    private float                           _loadingProgress;
    [HideInInspector]   public LoadEvent    OnLoadStart;
    [HideInInspector]   public LoadEvent    OnLoadProgress;
    [HideInInspector]   public LoadEvent    OnLoadEnd;

    /* States */
    private bool                            _bIsLoading = false;
    private bool                            _bIsOnMenu  = false;
    public bool                             IsLoading { get { return _bIsLoading;  } }
    public bool                             IsOnMenu { get { return _bIsOnMenu; } }

    void Awake()
    {
        Dictionary<string, SceneField> scenes = new Dictionary<string, SceneField> {
            { "SCENE_LOADING", SceneLoading },
            { "SCENE_MENU", SceneMenu },
            { "SCENE_COUNTDOWN", SceneCountdown },
            { "SCENE_PAUSE", ScenePause },
            { "SCENE_END_STAGE", SceneEndStage },
            { "SCENE_END_GAME", SceneEndGame },
        };

        foreach (KeyValuePair<string, SceneField> value in scenes)
        {
            if (value.Value.IsNull)
            {
                Debug.LogError(value.Key + " must be provided !");
                Debug.Break();
            }
        }
    }

    public void ToggleMenu()
    {
        if (_bIsOnMenu)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu ()
    {
        if(!_bIsOnMenu)
        {
            UnloadAllScenes();
            StartCoroutine(LoadScene(SceneMenu));

            if (CurrentArenaConfig != null)
            {
                UnloadScene(CurrentArenaConfig.BackgroundLevel);
                CurrentArenaConfig = null;
            }

            _bIsOnMenu = true;
        }
    }

    public void CloseMenu ()
    {
        if(_bIsOnMenu)
        {
            UnloadScene(SceneMenu);
            _bIsOnMenu = false;
        }
    }

    public void StartLevel(GameConfiguration config)
    {
        if (!_bIsLoading)
        {
            _bIsLoading = true;

            if (CurrentArenaConfig != null)
            {
                UnloadScene(CurrentArenaConfig.BackgroundLevel);
            }

            MainManager.Instance.DYNAMIC_CONFIG.GetConfig(config.ArenaConfiguration, out CurrentArenaConfig);
            MainManager.Instance.DYNAMIC_CONFIG.GetConfig(config.ModeConfiguration, out CurrentModeConfig);
            MainManager.Instance.DYNAMIC_CONFIG.GetConfig(config.MapConfiguration, out CurrentMapConfig);

            MainManager.Instance.GAME_OBJECT_POOL.DropAll();

            PoolConfiguration newPoolToLoad = new PoolConfiguration();

            newPoolToLoad.Prefab = CurrentArenaConfig.Ground;
            newPoolToLoad.Quantity = Mathf.CeilToInt(CurrentMapConfig.MapSize.x * CurrentMapConfig.MapSize.y);
            MainManager.Instance.GAME_OBJECT_POOL.AddPool(newPoolToLoad);

            newPoolToLoad.Prefab = CurrentArenaConfig.Obstacle;
            newPoolToLoad.Quantity = Mathf.CeilToInt(CurrentMapConfig.MapSize.x * CurrentMapConfig.MapSize.y);
            MainManager.Instance.GAME_OBJECT_POOL.AddPool(newPoolToLoad);

            for (int i = 0; i < GameManager.Instance.nbPlayers; ++i)
            {
                Player player = GameManager.Instance.RegisteredPlayers[i];
                if (player != null)
                {
                    PoolConfiguration[] assets = player.CharacterUsed._characterData.OtherAssetsToLoad;
                    for (int j = 0; j < assets.Length; ++j)
                    {
                        MainManager.Instance.GAME_OBJECT_POOL.AddPool(assets[j]);
                    }
                }
            }

            for (int i = 0; i < CurrentArenaConfig.OtherAssetsToLoad.Length; ++i)
            {
                MainManager.Instance.GAME_OBJECT_POOL.AddPool(CurrentArenaConfig.OtherAssetsToLoad[i]);
            }

            StartCoroutine(LoadLevel());
        }
    }

    private IEnumerator LoadScene(SceneField scene, bool bIsAsync = false, Action<AsyncOperation> callback = null)
    {
        if(bIsAsync)
        {
            yield return StartCoroutine(LoadAsyncScene(scene, callback));
        }
        else
        {
            Application.LoadLevelAdditive(SceneMenu);
        }
        CurrentScenes.Add(scene);
    }

    private IEnumerator LoadAsyncScene(SceneField scene, Action<AsyncOperation> callback = null)
    {
        AsyncOperation async = Application.LoadLevelAdditiveAsync(scene);
        while (!async.isDone)
        {
            if(callback != null)
            {
                callback.Invoke(async);
            }
            yield return null;
        }
    }

    private void UnloadScene(SceneField scene)
    {
        if(CurrentScenes.IndexOf(scene) >= 0)
        {
            Application.UnloadLevel(scene);
            CurrentScenes.Remove(scene);
        }
    }

    public void UnloadAllScenes()
    {
        foreach(SceneField scene in CurrentScenes)
        {
            UnloadScene(scene);
        }
    }

    private IEnumerator LoadLevel()
    {
        yield return StartCoroutine(LoadScene(SceneLoading, true));

        if (_bIsOnMenu)
        {
            CloseMenu();
        }

        MainManager.Instance.LOADING_MANAGER = LoadingScreen.Instance;

        // START LOADING
        MainManager.Instance.LOADING_MANAGER.SetStateText("start_loading");

        // Init loading events
        float originalLoadingBarSize = MainManager.Instance.LOADING_MANAGER.LoadBarProgress.rectTransform.rect.width;
        OnLoadProgress.AddListener((float progress) => {
            MainManager.Instance.LOADING_MANAGER.LoadBarProgress.fillAmount = progress;
        });

        // Show Loading menu
        _loadingProgress = 0.0f;
        OnLoadProgress.Invoke(_loadingProgress);

        // LOAD ASSETS
        MainManager.Instance.LOADING_MANAGER.SetStateText("loading_assets");

        // Add Events Listeners
        MainManager.Instance.GAME_OBJECT_POOL.LoadProgress.AddListener((float progress) => {
            _loadingProgress = progress * 0.75f;
            OnLoadProgress.Invoke(_loadingProgress);
        });

        // Load pool
        yield return StartCoroutine(MainManager.Instance.GAME_OBJECT_POOL.Start_Implementation());

        // Remove Listeners
        MainManager.Instance.GAME_OBJECT_POOL.LoadProgress.RemoveAllListeners();

        // BUILD LEVEL
        MainManager.Instance.LOADING_MANAGER.SetStateText("build_level");

        // Load background level
        yield return StartCoroutine(LoadScene(CurrentArenaConfig.BackgroundLevel, true, (AsyncOperation async) =>
        {
            _loadingProgress = 0.75f + async.progress * 0.05f;
            OnLoadProgress.Invoke(_loadingProgress);
        }));

        // Load Pause Menu
        yield return StartCoroutine(LoadScene(SceneCountdown, true, (AsyncOperation async) =>
        {
            _loadingProgress = 0.80f + async.progress * 0.05f;
            OnLoadProgress.Invoke(_loadingProgress);
        }));
        CountdownManager.Instance.Init();

        // Load Pause Menu
        yield return StartCoroutine(LoadScene(ScenePause, true, (AsyncOperation async) =>
        {
            _loadingProgress = 0.85f + async.progress * 0.05f;
            OnLoadProgress.Invoke(_loadingProgress);
        }));
        MenuPauseManager.Instance.Init();

        // Load End Stage Menu
        yield return StartCoroutine(LoadScene(SceneEndStage, true, (AsyncOperation async) =>
        {
            _loadingProgress = 0.90f + async.progress * 0.05f;
            OnLoadProgress.Invoke(_loadingProgress);
        }));
        EndStageManager.Instance.Init();

        // Load End Game Menu
        yield return StartCoroutine(LoadScene(SceneEndGame, true, (AsyncOperation async) =>
        {
            _loadingProgress = 0.95f + async.progress * 0.05f;
            OnLoadProgress.Invoke(_loadingProgress);
        }));
        EndGameManager.Instance.Init();

        _loadingProgress = 1.0f;
        OnLoadProgress.Invoke(_loadingProgress);

        // Disable loading screen
        UnloadScene(SceneLoading);

        // Initialize ArenaManager Instance
        MainManager.Instance.ARENA_MANAGER = ArenaManager.Instance;

        // Start Game
        MainManager.Instance.ARENA_MANAGER.Init();
        _bIsLoading = false;
    }

    public IEnumerator LoadLevelPreview (EArenaConfiguration arena, Action<AsyncOperation> callback = null)
    {
        if (_bIsOnMenu)
        {
            ArenaConfiguration_SO arenaConfig;
            MainManager.Instance.DYNAMIC_CONFIG.GetConfig(arena, out arenaConfig);
            yield return StartCoroutine(LoadScene(arenaConfig.BackgroundLevel, true, callback));
            if (CurrentArenaConfig != null)
            {
                UnloadScene(CurrentArenaConfig.BackgroundLevel);
            }
            CurrentArenaConfig = arenaConfig;
            Destroy(ArenaManager.Instance);
        }
    }
}
