using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TutorialSequence
{
	[HideInInspector]
	public string name = "TutoSequence";
	public Message[] _messageKeys;
}

public class TutorialManager : GenericSingleton<TutorialManager> {

	public Transform _respawnPoint;
	public DialogBox DialogBoxObject;
	private int _lastRespawnPointIndex = -1;
	public PlayerController TutorialCharacter;
	private PlayerController TutorialCharacterInstance;
	public TutorialSequence[] _tutorialSequences;

	private Player TempPlayer;
	void Start()
	{
		Init();
	}

	public override void Init()
	{
		Debug.Log("Le tutoriel est initialisé ici.");
		CameraManager.Instance.Reset();
		CameraManager.Instance.SetCenterPoint(_respawnPoint);
		CameraManager.Instance.SetCamAngle(30, Vector3.right);
	}

	public void SpawnPlayer(JoystickNumber joyStick)
	{
		PoolConfiguration[] assets = TutorialCharacter._characterData.OtherAssetsToLoad;
		MainManager.Instance.GAME_OBJECT_POOL.DropAll();
		for (int j = 0; j < assets.Length; ++j)
		{
			MainManager.Instance.GAME_OBJECT_POOL.AddPool(assets[j]);
		}
		MainManager.Instance.GAME_OBJECT_POOL.Load();
		MainManager.Instance.GAME_OBJECT_POOL.LoadEnd.AddListener(PopPlayer);

		TempPlayer = new Player();
		TempPlayer.PlayerNumber = 1;
		TempPlayer.JoystickNumber = joyStick;
	}

	private void PopPlayer(float unused)
	{
		TutorialCharacterInstance = (PlayerController)Instantiate(TutorialCharacter, _respawnPoint.position, Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.up.ZeroY().normalized) * Quaternion.AngleAxis(180, Vector3.up), transform);
		TempPlayer.Controller = TutorialCharacterInstance;
		TutorialCharacterInstance.Init(TempPlayer.gameObject);
		TutorialCharacterInstance.AddStun(1f);
	}

	public void SetNewRespawnPoint(RespawnPoint newRespawn)
	{

		if (newRespawn.RespawnIndex > _lastRespawnPointIndex)
		{
			_respawnPoint = newRespawn.targetRespawnPoint;
			CameraManager.Instance.SetCenterPoint(_respawnPoint);
		}
	}

	public void OnplayerFall()
	{
		CancelInvoke("RespawnPlayer");

		CameraManager.Instance.ClearTrackedTargets();

		Invoke("RespawnPlayer", 1);
	}

	private void RespawnPlayer()
	{
		TutorialCharacterInstance.transform.position = _respawnPoint.position;
		TutorialCharacterInstance.Freeze();
		TutorialCharacterInstance.UnFreeze();
		CameraManager.Instance.AddTargetToTrack(TutorialCharacterInstance.transform);
	}

	public void StartSequence(int sequenceNumber)
	{
		if (_tutorialSequences.Length > sequenceNumber)
			StartCoroutine(SequenceCoroutine(sequenceNumber));
	}

	IEnumerator SequenceCoroutine(int sequenceNumber)
	{
		while (!TutorialCharacterInstance.IsGrounded) { yield return null; }
		TutorialCharacterInstance.Freeze();

		yield return StartCoroutine(DialogBoxObject.LaunchDialog(_tutorialSequences[sequenceNumber]._messageKeys));

		Debug.Log("Sequence over");
		TutorialCharacterInstance.UnFreeze();
		TutorialCharacterInstance.WaitForDashRelease = true;
		DialogBoxObject.gameObject.SetActive(false);
	}

	public void EndTutorial()
	{
		PlayerPrefs.SetInt("FTUEDone", 1);
		PlayerPrefs.Save();

		StartCoroutine(ReopenMenu());
	}

	private IEnumerator ReopenMenu()
	{
		yield return StartCoroutine(AutoFade.StartFade(1,
			DoigtDansLeCul(),
			AutoFade.EndFade(0.2f, 0.7f, Color.white),
			Color.white));
	}


	IEnumerator DoigtDansLeCul()
	{
		yield return LevelManager.Instance.OpenMenu();
	}

	public void SetPlayerDash(bool value){ TutorialCharacterInstance.AllowDash = value; }
	public void SetPlayerSpecial(bool value){ TutorialCharacterInstance.AllowSpecial = value; }
	public void SetInputLockTime(float value){ InputManager.AddInputLockTime(value); }
}