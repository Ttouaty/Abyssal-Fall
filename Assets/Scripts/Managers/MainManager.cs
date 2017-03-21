using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class MainManager : GenericSingleton<MainManager>
{
	[HideInInspector]
	public GameObjectPool       GAME_OBJECT_POOL;
	[HideInInspector]
	public DynamicConfig DYNAMIC_CONFIG;
	[HideInInspector]
	public LevelManager LEVEL_MANAGER;
	[HideInInspector]
	public TimeManager TIME_MANAGER;
	[HideInInspector]
	public InputManager INPUT_MANAGER;
	[HideInInspector]
	public CoolDownManager COOLDOWN_MANAGER;
	[HideInInspector]
	public GameManager GAME_MANAGER;
	[HideInInspector]
	public LoadingScreen LOADING_MANAGER;
	[HideInInspector]
	public ArenaManager ARENA_MANAGER;
	[HideInInspector]
	public ServerManager SERVER_MANAGER;
	[HideInInspector]
	public CameraManager OriginalCameraManager;

	protected override void Awake ()
	{
		base.Awake();
		//DontDestroyOnLoad(gameObject);

		Cursor.visible = false;
		Application.targetFrameRate = 60;

		GAME_OBJECT_POOL = GameObjectPool.Instance;
		DYNAMIC_CONFIG      = DynamicConfig.Instance;
		LEVEL_MANAGER       = LevelManager.Instance;
		TIME_MANAGER        = TimeManager.Instance;
		INPUT_MANAGER       = InputManager.Instance;
		COOLDOWN_MANAGER    = CoolDownManager.Instance;
		GAME_MANAGER        = GameManager.Instance;
		SERVER_MANAGER      = ServerManager.Init();
		OriginalCameraManager = CameraManager.Instance;

		Dictionary<string, Object> dic = new Dictionary<string, Object>()
		{
			{ "GAME_OBJECT_POOL", GAME_OBJECT_POOL },
			{ "DYNAMIC_CONFIG", DYNAMIC_CONFIG },
			{ "LEVEL_MANAGER", LEVEL_MANAGER },
			{ "INPUT_MANAGER", INPUT_MANAGER },
			{ "COOLDOWN_MANAGER", COOLDOWN_MANAGER },
			{ "GAME_MANAGER", GAME_MANAGER },
			{ "TIME_MANAGER", TIME_MANAGER }
		};

		foreach(KeyValuePair<string, Object> value in dic)
		{
			if(value.Value == null)
			{
				Debug.LogError("[" + value.Key + "] is Required to launch the game");
				Debug.Break();
			}
		}

		LEVEL_MANAGER.OpenMenu(true,"Title");
		MenuPanelNew.GlobalInputDelay = 4;
	}

	void Update()
	{
		if(Input.GetKey(KeyCode.LeftShift))
		{
			if (Input.GetKeyDown(KeyCode.F1))
				Screen.SetResolution(1920, 1080, true);
			if (Input.GetKeyDown(KeyCode.F2))
				Screen.SetResolution(1600, 900, true);
			if (Input.GetKeyDown(KeyCode.F3))
				Screen.SetResolution(1280, 720, true);
			if (Input.GetKeyDown(KeyCode.F4))
				Screen.SetResolution(800, 450, false);
		}

		if(Input.GetKeyDown(KeyCode.F7) && NetworkServer.active)
		{
			if (Player.LocalPlayer != null)
			{
				Player.LocalPlayer.RpcToggleNoClip();
			}
			else
				MessageManager.Log("No Local Player can't toggle noclip");
		}

		if(Input.GetKeyDown(KeyCode.F8))
			Cursor.visible = !Cursor.visible;
	}
}
