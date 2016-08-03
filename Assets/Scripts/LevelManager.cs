using UnityEngine;
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
    public float LoadingProgress = 0.0f;
    
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
            Application.LoadLevelAdditive(SCENE_MENU.SceneName);

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

    public void StartLevel(string config, string mode)
    {
        if (!_bIsLoading)
        {
            _bIsLoading = true;

            if (CurrentArenaConfig != null)
            {
                Application.UnloadLevel(CurrentArenaConfig.BackgroundLevel);
            }

            CurrentArenaConfig = MainManager.Instance.DYNAMIC_CONFIG.GetArenaConfig(config);
            CurrentModeConfig = MainManager.Instance.DYNAMIC_CONFIG.GetModeConfig(mode);

            MainManager.Instance.GAME_OBJECT_POOL.DropAll();

            PoolConfiguration groundToLoad = new PoolConfiguration();
            groundToLoad.Prefab = CurrentArenaConfig.Ground;
            groundToLoad.Quantity = CurrentModeConfig.ArenaSize * CurrentModeConfig.ArenaSize;

            MainManager.Instance.GAME_OBJECT_POOL.AddPool(groundToLoad);
            for (int i = 0; i < CurrentArenaConfig.OtherAssetsToLoad.Count; ++i)
            {
                MainManager.Instance.GAME_OBJECT_POOL.AddPool(CurrentArenaConfig.OtherAssetsToLoad[i]);
            }

            StartCoroutine(LoadLevel());
        }
    }

    private IEnumerator LoadLevel()
    {
        AsyncOperation async = Application.LoadLevelAdditiveAsync(SCENE_LOADING.SceneName);
        while(!async.isDone)
        {
            yield return true;
        }

        if (_bIsOnMenu)
        {
            CloseMenu();
        }

        MainManager.Instance.LOADING_MANAGER = TestLoadingScreen.Instance;

        string currentState = "start_loading";
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
            currentState = "loading_assets";
            LoadingProgress = progress * 0.75f;
            OnLoadProgress.Invoke(LoadingProgress);
        });

        // Load pool
        yield return StartCoroutine(MainManager.Instance.GAME_OBJECT_POOL.Init());

        // Remove Listeners
        MainManager.Instance.GAME_OBJECT_POOL.LoadProgress.RemoveAllListeners();

        // Load background level
        async = Application.LoadLevelAdditiveAsync(CurrentArenaConfig.BackgroundLevel);
        while (async.progress < 1.0f)
        {
            currentState = "building_level";
            LoadingProgress = 0.75f + async.progress * 0.25f;
            OnLoadProgress.Invoke(LoadingProgress);
            yield return null;
        }
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

#if UNITY_EDITOR
    void Update()
    {
        if (!_bIsLoading)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && !_bIsOnMenu)
            {
                OpenMenu();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartLevel("Default", "FreeForAll");
            }
        }
    }
#endif
}
