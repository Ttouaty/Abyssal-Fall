using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GrowAndFade : MonoBehaviour {

	
	void Start () 
	{
		StartCoroutine(UpdateCoroutine());
	}
	
	
	void Update () {
		
	}

	IEnumerator UpdateCoroutine()
	{
		float totalTime = 0.3f;
		float targetStamp = Time.time + totalTime;

		float targetScale = 1.5f;

		GetComponent<Image>().CrossFadeAlpha(0.5f, 0, false);
		GetComponent<Image>().CrossFadeAlpha(0, totalTime, false);

		while (Time.time < targetStamp)
		{
			transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, 0.05f);
			yield return null;
		}
		Destroy(gameObject);
	}

}
