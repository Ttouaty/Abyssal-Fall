using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainManager : GenericSingleton<MainManager>
{
	public GameObjectPool       GAME_OBJECT_POOL;
	public DynamicConfig        DYNAMIC_CONFIG;
	public LevelManager         LEVEL_MANAGER;
	public TimeManager          TIME_MANAGER;
	public InputManager         INPUT_MANAGER;
	public CoolDownManager      COOLDOWN_MANAGER;
	public GameManager          GAME_MANAGER;
	public SoundManager         SOUND_MANAGER;
	public LoadingScreen        LOADING_MANAGER;
	public ArenaManager         ARENA_MANAGER;

	protected override void Awake ()
	{
		base.Awake();

		GAME_OBJECT_POOL    = GameObjectPool.Instance;
		DYNAMIC_CONFIG      = DynamicConfig.Instance;
		LEVEL_MANAGER       = LevelManager.Instance;
		TIME_MANAGER        = TimeManager.Instance;
		INPUT_MANAGER       = InputManager.Instance;
		COOLDOWN_MANAGER    = CoolDownManager.Instance;
		GAME_MANAGER        = GameManager.Instance;

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

		LEVEL_MANAGER.OpenMenu(true);
	}
}
