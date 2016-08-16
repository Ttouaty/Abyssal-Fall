using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public struct LevelConfiguration
{
    public GameObject[] Players;
}

public class LevelManager : GenericSingleton<LevelManager>
{
    /* Variables */
    public ArenaConfiguration_SO    CurrentArenaConfig;
    public ModeConfiguration_SO     CurrentModeConfig;
    public MapConfiguration_SO      CurrentMapConfig;
    public float                    LoadingProgress         = 0.0f;
    
    /* Defined Scenes */
    public SceneField SCENE_LOADING;
    public SceneField SCENE_MENU;

    /* Events */
    public LoadEvent OnLoadStart;
    public LoadEvent OnLoadProgress;
    public LoadEvent OnLoadEnd;

    private bool _bIsLoading = false;
    private bool _bIsOnMenu = false;

    void Awake ()
    {
        if (SCENE_LOADING.SceneName == null || SCENE_LOADING.SceneName == "")
        {
            Debug.LogError("SCENE_LOADING must be provided !");
            Debug.Break();
        }

        if (SCENE_MENU.SceneName == null || SCENE_MENU.SceneName == "")
        {
            Debug.LogError("SCENE_MENU must be provided !");
            Debug.Break();
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

    IEnumerator LoadAsyncScene(string scene, Action<AsyncOperation> callback = null)
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

    public IEnumerator StartLevel(string arena, string mode, string map)
    {
        if (!_bIsLoading)
        {
            _bIsLoading = true;

            if (CurrentArenaConfig != null)
            {
                Application.UnloadLevel(CurrentArenaConfig.BackgroundLevel);
            }

            CurrentArenaConfig = MainManager.Instance.DYNAMIC_CONFIG.GetArenaConfig(arena);
            CurrentModeConfig = MainManager.Instance.DYNAMIC_CONFIG.GetModeConfig(mode);
            CurrentMapConfig = MainManager.Instance.DYNAMIC_CONFIG.GetMapConfig(map);

            MainManager.Instance.GAME_OBJECT_POOL.DropAll();

            PoolConfiguration newPoolToLoad = new PoolConfiguration();

            newPoolToLoad.Prefab = CurrentArenaConfig.Ground;
            newPoolToLoad.Quantity = Mathf.CeilToInt(CurrentMapConfig.MapSize.x * CurrentMapConfig.MapSize.y);
            MainManager.Instance.GAME_OBJECT_POOL.AddPool(newPoolToLoad);

            newPoolToLoad.Prefab = CurrentArenaConfig.Obstacle;
            newPoolToLoad.Quantity = Mathf.CeilToInt(CurrentMapConfig.MapSize.x * CurrentMapConfig.MapSize.y);
            MainManager.Instance.GAME_OBJECT_POOL.AddPool(newPoolToLoad);

            for (int i = 0; i < CurrentArenaConfig.OtherAssetsToLoad.Count; ++i)
            {
                MainManager.Instance.GAME_OBJECT_POOL.AddPool(CurrentArenaConfig.OtherAssetsToLoad[i]);
            }

            yield return StartCoroutine(LoadLevel());
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

    public IEnumerator ShowLevelPreview (string arena, Action<AsyncOperation> callback = null)
    {
        if (_bIsOnMenu)
        {
            ArenaConfiguration_SO arenaConfig = MainManager.Instance.DYNAMIC_CONFIG.GetArenaConfig(arena);
            yield return StartCoroutine(LoadAsyncScene(arenaConfig.BackgroundLevel, callback));
            if (CurrentArenaConfig != null)
            {
                Application.UnloadLevel(CurrentArenaConfig.BackgroundLevel);
            }
            CurrentArenaConfig = arenaConfig;
        }
    }
}
