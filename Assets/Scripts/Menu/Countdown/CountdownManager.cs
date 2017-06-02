using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CountdownManager : GenericSingleton<CountdownManager>
{
	public GameObject[] Elements = new GameObject[4];
	public float TotalTimeTaken = 3;

	public Vector3 DistanceFromCam;

	public IEnumerator Countdown ()
	{
		yield return new WaitUntil(() => FindObjectOfType<PlayerController>() != null);

		Elements[0].transform.parent.SetParent(Camera.main.transform, false);
		Elements[0].transform.parent.localRotation = Quaternion.identity;
		Elements[0].transform.parent.localPosition = DistanceFromCam;

		TimeManager.Pause();
		StartCoroutine(PlayerFocus());

		for (int i = 0; i < Elements.Length; i++)
		{
			Elements[i].SetActive(true);
			Elements[i].GetComponentInChildren<Animator>().SetTriggerAfterInit("Activate");
			Elements[i].GetComponentInChildren<Animator>().SetFloatAfterInit("TimeMultiplier", Elements.Length / TotalTimeTaken);
			if(i != Elements.Length -1)
				yield return new WaitForSeconds((TotalTimeTaken) / Elements.Length);
			else
				yield return new WaitForSeconds(0.15f);
		}

		TimeManager.Resume();
	}

	IEnumerator PlayerFocus(float TimeBeforeLaunch = 1)
	{
		PlayerController[] ActiveControllers = FindObjectsOfType<PlayerController>();
		for (int i = 0; i < ActiveControllers.Length; i++)
		{
			ActiveControllers[i]._animator.speed = 1;
			if(ActiveControllers[i]._isLocalPlayer)
				ActiveControllers[i]._networkAnimator.BroadCastTrigger("WaitForEnter");
		}

		yield return new WaitForSeconds(0.2f);
		ActiveControllers = FindObjectsOfType<PlayerController>(); //dégueux mais nike !

		Transform[] PreviousCamInterestPoint = CameraManager.Instance.TargetsTracked.ToArray();

		CameraManager.Instance.TargetsTracked.Clear();
		CameraManager.Instance.AddTargetToTrack(ArenaManager.Instance.ArenaRoot);

		for (int i = 0; i < ActiveControllers.Length; i++)
		{
			CameraManager.Instance.AddTargetToTrack(ActiveControllers[i].transform);
			if(ActiveControllers[i]._isLocalPlayer)
				ActiveControllers[i]._networkAnimator.BroadCastTrigger("Enter");
			yield return new WaitForSeconds((TotalTimeTaken - TimeBeforeLaunch) / Player.PlayerList.Length);
			CameraManager.Instance.RemoveTargetToTrack(ActiveControllers[i].transform);
		}

		CameraManager.Instance.TargetsTracked = PreviousCamInterestPoint.ToList();
	}
}
