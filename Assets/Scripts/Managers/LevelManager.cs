using UnityEngine;
using UnityEngine.SceneManagement;
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
	#region private
	private bool                            _bIsLoading                 = false;
	private bool                            _bIsOnMenu                  = false;
	private float                           _loadingProgress            = 0.0f;
	ArenaConfiguration_SO                   _oldArenaConfig				= null;
	#endregion
	#region public
	public List<SceneField>                 CurrentScenes               = new List<SceneField>();

	/* Variables */
	[Header("Game Settings")]
	public ArenaConfiguration_SO            CurrentArenaConfig;
	public AGameRules			            CurrentModeConfig;
	public MapConfiguration_SO              CurrentMapConfig;

	/* Defined Scenes */
	[Header("Scenes Settings")]
	public SceneField                       SceneLoading;
	public SceneField                       SceneMenu;
	public SceneField                       SceneCountdown;
	public SceneField                       ScenePause;
	public SceneField                       SceneEndStage;
	public SceneField                       SceneEndGame;
	public SceneField                       SceneGUI;
	public SceneField 						SceneTutorial;

	/* Events */
	[HideInInspector]   public LoadEvent    OnLoadStart;
	[HideInInspector]   public LoadEvent    OnLoadProgress;
	[HideInInspector]   public LoadEvent    OnLoadEnd;

	/* States */
	public bool                             IsLoading                   { get { return _bIsLoading;  } }
	public bool                             IsOnMenu                    { get { return _bIsOnMenu; } }
	#endregion

	protected override void Awake()
	{
		base.Awake();

		Dictionary<string, SceneField> scenes = new Dictionary<string, SceneField> {
			{ "SCENE_LOADING", SceneLoading },
			{ "SCENE_MENU", SceneMenu },
			{ "SCENE_COUNTDOWN", SceneCountdown },
			{ "SCENE_PAUSE", ScenePause },
			{ "SCENE_END_STAGE", SceneEndStage },
			{ "SCENE_END_GAME", SceneEndGame },
			{ "SCENE_GUI", SceneGUI },
			{ "SCENE_TUTORIAL", SceneTutorial }
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

	public Coroutine OpenMenu (bool showSplashScreens = false)
	{
		return StartCoroutine(OpenMenu_Implementation(showSplashScreens));
	}

	private IEnumerator OpenMenu_Implementation (bool showSplashScreens)
	{
		if (!_bIsOnMenu)
		{
			UnloadAllScenes();
			yield return StartCoroutine(LoadScene(SceneMenu));

			_bIsOnMenu = true;

			MenuManager.Instance.FadeSplashscreens(showSplashScreens);
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

	public IEnumerator StartLevel(GameConfiguration config)
	{
		if (!_bIsLoading)
		{
			_bIsLoading = true;

			_oldArenaConfig = CurrentArenaConfig;

			MainManager.Instance.DYNAMIC_CONFIG.GetConfig(config.ArenaConfiguration, out CurrentArenaConfig);
			MainManager.Instance.DYNAMIC_CONFIG.GetConfig(config.ModeConfiguration, out CurrentModeConfig);
			MainManager.Instance.DYNAMIC_CONFIG.GetConfig(config.MapConfiguration, out CurrentMapConfig);

			AGameRules rules				= Instantiate(CurrentModeConfig);
			rules.transform.parent			= GameManager.Instance.transform;
			rules.name						= "Game Rules";
			GameManager.Instance.GameRules	= rules;

			MainManager.Instance.GAME_OBJECT_POOL.DropAll();

			PoolConfiguration newPoolToLoad = new PoolConfiguration();

			newPoolToLoad.Prefab = CurrentArenaConfig.Ground;
			newPoolToLoad.Quantity = Mathf.CeilToInt(CurrentMapConfig.MapSize.x * CurrentMapConfig.MapSize.y);
			MainManager.Instance.GAME_OBJECT_POOL.AddPool(newPoolToLoad);

			newPoolToLoad.Prefab = CurrentArenaConfig.Obstacle;
			newPoolToLoad.Quantity = Mathf.CeilToInt(CurrentMapConfig.MapSize.x * CurrentMapConfig.MapSize.y);
			MainManager.Instance.GAME_OBJECT_POOL.AddPool(newPoolToLoad);

			int i;
			for (i = 0; i < GameManager.Instance.nbPlayers; ++i)
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

			for (i = 0; i < CurrentArenaConfig.AdditionalPoolsToLoad.Length; ++i)
			{
				MainManager.Instance.GAME_OBJECT_POOL.AddPool(CurrentArenaConfig.AdditionalPoolsToLoad[i]);
			}

			for (i = 0; i < CurrentModeConfig.AdditionalPoolsToLoad.Count; ++i)
			{
				MainManager.Instance.GAME_OBJECT_POOL.AddPool(CurrentModeConfig.AdditionalPoolsToLoad[i]);
			}

			return LoadLevel();
		}
		return null;
	}

	private IEnumerator LoadScene(SceneField scene, bool bIsAsync = false, Action<AsyncOperation> callback = null)
	{
		if(bIsAsync)
		{
			yield return StartCoroutine(LoadAsyncScene(scene, callback));
		}
		else
		{
			SceneManager.LoadScene(scene, LoadSceneMode.Additive);
		}
		CurrentScenes.Add(scene);
	}

	private IEnumerator LoadAsyncScene(SceneField scene, Action<AsyncOperation> callback = null)
	{
		AsyncOperation async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		while (!async.isDone)
		{
			if(callback != null)
			{
				callback.Invoke(async);
			}
			yield return null;
		}
	}

	public void UnloadScene(SceneField scene)
	{
		if(CurrentScenes.IndexOf(scene) >= 0)
		{
			CurrentScenes.Remove(scene);
			SceneManager.UnloadScene(scene);
		}
	}

	public void UnloadAllScenes()
	{
		while(CurrentScenes.Count > 0)
		{
			UnloadScene(CurrentScenes[0]);
		}
		_bIsOnMenu = false;
	}

	public IEnumerator LoadSceneAlone (SceneField scene)
	{
		UnloadAllScenes();
		yield return StartCoroutine(LoadScene(scene));
	}

	private Dictionary<SceneField, Action> _defaultScenesToLoad = new Dictionary<SceneField, Action>();

	private IEnumerator LoadLevel()
	{
		yield return StartCoroutine(LoadScene(SceneLoading, true));

		if (_bIsOnMenu)
		{
			CloseMenu();
		}

		if (_oldArenaConfig != null)
		{
			UnloadScene(_oldArenaConfig.BackgroundLevel);
			_oldArenaConfig = null;
		}

		MainManager.Instance.LOADING_MANAGER = LoadingScreen.Instance;

		// START LOADING
		MainManager.Instance.LOADING_MANAGER.SetStateText("start_loading");

		// Init loading events
		//float originalLoadingBarSize = MainManager.Instance.LOADING_MANAGER.LoadBarProgress.rectTransform.rect.width;
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
		yield return StartCoroutine(MainManager.Instance.GAME_OBJECT_POOL.Load_Implementation());

		// Remove Listeners
		MainManager.Instance.GAME_OBJECT_POOL.LoadProgress.RemoveAllListeners();

		// BUILD LEVEL
		MainManager.Instance.LOADING_MANAGER.SetStateText("build_level");

		_defaultScenesToLoad.Clear();
		_defaultScenesToLoad.Add(CurrentArenaConfig.BackgroundLevel, null);
		_defaultScenesToLoad.Add(SceneCountdown, () => { CountdownManager.Instance.Init(); });
		_defaultScenesToLoad.Add(ScenePause, () => { MenuPauseManager.Instance.Init(); });
		_defaultScenesToLoad.Add(SceneEndStage, () => { EndStageManager.Instance.Init(); });
		_defaultScenesToLoad.Add(SceneEndGame, () => { EndGameManager.Instance.Init(); });
		_defaultScenesToLoad.Add(SceneGUI, () => { GUIManager.Instance.Init(); });

		_loadingProgress = 0.7f;
		float stepPerScene = (1.0f - 0.7f) / _defaultScenesToLoad.Count;
		foreach(KeyValuePair<SceneField, Action> value in _defaultScenesToLoad)
		{
			yield return StartCoroutine(LoadScene(value.Key, true, (AsyncOperation async) =>
			{
				_loadingProgress += async.progress * stepPerScene;
				OnLoadProgress.Invoke(_loadingProgress);
			}));

			if(value.Value != null) 
			{
				value.Value();
			}
		}

		_loadingProgress = 1.0f;
		OnLoadProgress.Invoke(_loadingProgress);

		// Disable loading screen
		UnloadScene(SceneLoading);

		// Initialize ArenaManager Instance
		MainManager.Instance.ARENA_MANAGER = ArenaManager.Instance;

		// Start Game
		AutoFade.StartFade(0,0.5f,1);
		MainManager.Instance.ARENA_MANAGER.Init();	
		_bIsLoading = false;
	}

	public IEnumerator LoadLevelPreview (EArenaConfiguration arena, Action<AsyncOperation> callback = null)
	{
		ArenaConfiguration_SO arenaConfig;
		MainManager.Instance.DYNAMIC_CONFIG.GetConfig(arena, out arenaConfig);
		yield return StartCoroutine(LoadScene(arenaConfig.BackgroundLevel, true, callback));
		//if (CurrentArenaConfig != null)
		//{
		//	UnloadScene(CurrentArenaConfig.BackgroundLevel);
		//}
		CurrentArenaConfig = arenaConfig;
		Destroy(ArenaManager.Instance);
	}
}
