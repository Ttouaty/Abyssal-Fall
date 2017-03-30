using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CountdownManager : GenericSingleton<CountdownManager>
{
	public GameObject[] Elements = new GameObject[4];
	public float TotalTimeTaken = 3;

	public float DistanceFromCam = 20;

	public IEnumerator Countdown ()
	{
		yield return new WaitUntil(() => Player.LocalPlayer.Controller != null);

		Elements[0].transform.parent.SetParent(Camera.main.transform, false);
		Elements[0].transform.parent.localRotation = Quaternion.identity;
		Elements[0].transform.parent.localPosition = new Vector3(0,0, 6.5f);
		Elements[0].transform.parent.localScale = new Vector3(0.25f, 0.25f, 0.25f);

		TimeManager.Pause();
		StartCoroutine(PlayerFocus());

		for (int i = 0; i < Elements.Length; i++)
		{
			Elements[i].SetActive(true);
			Elements[i].GetComponentInChildren<Animator>().SetTriggerAfterInit("Activate");
			Elements[i].GetComponentInChildren<Animator>().SetFloatAfterInit("TimeMultiplier", Elements.Length / TotalTimeTaken);
			yield return new WaitForSeconds((TotalTimeTaken) / Elements.Length);
			Elements[i].SetActive(false);
		}

		TimeManager.Resume();
		MenuPauseManager.Instance.CanPause = true;
	}

	IEnumerator PlayerFocus(float TimeBeforeLaunch = 1)
	{
		for (int i = 0; i < Player.PlayerList.Length; i++)
		{
			Player.PlayerList[i].Controller._animator.speed = 1;
			Player.PlayerList[i].Controller._animator.SetTriggerAfterInit("WaitForEnter");
		}

		yield return new WaitForSeconds(0.2f);

		Transform[] PreviousCamInterestPoint = CameraManager.Instance.TargetsTracked.ToArray();

		CameraManager.Instance.TargetsTracked.Clear();
		CameraManager.Instance.AddTargetToTrack(ArenaManager.Instance.ArenaRoot);

		for (int i = 0; i < Player.PlayerList.Length; i++)
		{
			CameraManager.Instance.AddTargetToTrack(Player.PlayerList[i].Controller.transform);
			Player.PlayerList[i].Controller._animator.SetTriggerAfterInit("Enter");
			yield return new WaitForSeconds((TotalTimeTaken - TimeBeforeLaunch) / Player.PlayerList.Length);
			CameraManager.Instance.RemoveTargetToTrack(Player.PlayerList[i].Controller.transform);
		}

		CameraManager.Instance.TargetsTracked = PreviousCamInterestPoint.ToList();
	}
}
