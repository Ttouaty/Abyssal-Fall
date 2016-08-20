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
    /* Variables */
    public ArenaConfiguration_SO            CurrentArenaConfig;
    public ModeConfiguration_SO             CurrentModeConfig;
    public MapConfiguration_SO              CurrentMapConfig;
    public float                            LoadingProgress;

    /* Defined Scenes */
    public SceneField SCENE_LOADING;
    public SceneField SCENE_MENU;
    public SceneField SCENE_COUNTDOWN;
    public SceneField SCENE_END_STAGE;
    public SceneField SCENE_END_GAME;

    /* Events */
    public LoadEvent OnLoadStart;
    public LoadEvent OnLoadProgress;
    public LoadEvent OnLoadEnd;

    /* States */
    private bool _bIsLoading = false;
    public bool IsLoading { get { return _bIsLoading;  } }
    private bool _bIsOnMenu = false;
    public bool IsOnMenu { get { return _bIsOnMenu; } }

    void Awake ()
    {
        CheckScenes();
    }

    void CheckScenes ()
    {
        SceneField[] scenes = new SceneField[] { SCENE_LOADING, SCENE_MENU, SCENE_COUNTDOWN, SCENE_END_STAGE, SCENE_END_GAME };
        for(int i = 0; i < scenes.Length; ++i)
        {
            if(scenes[i].IsNull)
            {
                Debug.LogError(scenes[i] + " must be provided !");
            }
        }
    }

    public void OpenMenu ()
    {
        if(!_bIsOnMenu)
        {
            Application.LoadLevelAdditive(SCENE_MENU);

            if (CurrentArenaConfig != null)
            {
                Application.UnloadLevel(CurrentArenaConfig.BackgroundLevel);
                CurrentArenaConfig = null;
            }

            _bIsOnMenu = true;
        }
    }

    public void CloseMenu ()
    {
        if(_bIsOnMenu)
        {
            Application.UnloadLevel(SCENE_MENU.SceneName);
            _bIsOnMenu = false;
        }
    }

    public void ToggleMenu ()
    {
        if(_bIsOnMenu)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void StartLevel(GameConfiguration config)
    {
        if (!_bIsLoading)
        {
            _bIsLoading = true;

            if (CurrentArenaConfig != null)
            {
                Application.UnloadLevel(CurrentArenaConfig.BackgroundLevel);
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

    private IEnumerator LoadAsyncScene(string scene, Action<AsyncOperation> callback = null)
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

    private IEnumerator LoadLevel()
    {
        yield return StartCoroutine(LoadAsyncScene(SCENE_LOADING));

        if (_bIsOnMenu)
        {
            CloseMenu();
        }

        MainManager.Instance.LOADING_MANAGER = TestLoadingScreen.Instance;

        Localizator.EFragmentsEnum currentState = Localizator.EFragmentsEnum.start_loading;
        float originalLoadingBarSize = MainManager.Instance.LOADING_MANAGER.LoadBarProgress.rectTransform.rect.width;
        OnLoadProgress.AddListener((float progress) => {
            MainManager.Instance.LOADING_MANAGER.LoadBarProgress.rectTransform.sizeDelta = new Vector2(progress * originalLoadingBarSize, MainManager.Instance.LOADING_MANAGER.LoadBarProgress.rectTransform.rect.height);
            MainManager.Instance.LOADING_MANAGER.SetStateText(currentState);
        });

        // Show Loading menu
        LoadingProgress = 0.0f;
        OnLoadProgress.Invoke(LoadingProgress);

        // Add Events Listeners
        MainManager.Instance.GAME_OBJECT_POOL.LoadProgress.AddListener((float progress) => {
            currentState = Localizator.EFragmentsEnum.loading_assets;
            LoadingProgress = progress * 0.75f;
            OnLoadProgress.Invoke(LoadingProgress);
        });

        // Load pool
        yield return StartCoroutine(MainManager.Instance.GAME_OBJECT_POOL.Init());

        // Remove Listeners
        MainManager.Instance.GAME_OBJECT_POOL.LoadProgress.RemoveAllListeners();

        // Load background level
        yield return StartCoroutine(LoadAsyncScene(CurrentArenaConfig.BackgroundLevel, (AsyncOperation async) =>
        {
            currentState = Localizator.EFragmentsEnum.build_level;
            LoadingProgress = 0.75f + async.progress * 0.25f;
            OnLoadProgress.Invoke(LoadingProgress);
        }));

        LoadingProgress = 1.0f;
        OnLoadProgress.Invoke(LoadingProgress);

        // Disable loading screen
        Application.UnloadLevel(SCENE_LOADING.SceneName);

        // Initialize ArenaManager Instance
        MainManager.Instance.ARENA_MANAGER = ArenaManager.Instance;

        // Start Game
        MainManager.Instance.ARENA_MANAGER.Init();
        _bIsLoading = false;
    }

    public IEnumerator ShowLevelPreview (EArenaConfiguration arena, Action<AsyncOperation> callback = null)
    {
        if (_bIsOnMenu)
        {
            ArenaConfiguration_SO arenaConfig;
            MainManager.Instance.DYNAMIC_CONFIG.GetConfig(arena, out arenaConfig);
            yield return StartCoroutine(LoadAsyncScene(arenaConfig.BackgroundLevel, callback));
            if (CurrentArenaConfig != null)
            {
                Application.UnloadLevel(CurrentArenaConfig.BackgroundLevel);
            }
            CurrentArenaConfig = arenaConfig;
        }
    }
}
