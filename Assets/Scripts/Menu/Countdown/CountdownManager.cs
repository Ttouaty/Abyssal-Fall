using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CountdownManager : GenericSingleton<CountdownManager>
{
	public GameObject[] Elements = new GameObject[4];
	public float TotalTimeTaken = 3;

	public float DistanceFromCam = 20;

	public IEnumerator Countdown ()
	{
		Elements[0].transform.parent.SetParent(Camera.main.transform, false);
		Elements[0].transform.parent.localRotation = Quaternion.identity;
		Elements[0].transform.parent.localPosition = new Vector3(0,0, 6.5f);
		Elements[0].transform.parent.localScale = new Vector3(0.25f, 0.25f, 0.25f);

		TimeManager.Pause();

		for (int i = 0; i < Elements.Length; i++)
		{
			Elements[i].SetActive(true);
			Elements[i].GetComponentInChildren<Animator>().SetTriggerAfterInit("Activate");
			Elements[i].GetComponentInChildren<Animator>().SetFloatAfterInit("TimeMultiplier", Elements.Length / TotalTimeTaken);
			yield return new WaitForSeconds(TotalTimeTaken / Elements.Length);
			Elements[i].SetActive(false);
		}

		TimeManager.Resume();
		MenuPauseManager.Instance.CanPause = true;
	}
}
