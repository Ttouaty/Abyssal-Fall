using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GrowAndFade : MonoBehaviour {
	public float AnimationTime = 0.3f;
	public Vector3 TargetScale = new Vector3(1.5f,1.5f,1.5f);
	[Space]
	public bool DublicateSprite = true;
	public bool IsDetached = true;

	public void Activate()
	{
		if (DublicateSprite)
		{
			GameObject particle = (GameObject)Instantiate(gameObject, transform.position, Quaternion.identity);
			if (!IsDetached) 
				particle.transform.SetParent(transform.parent);
			particle.transform.localScale = transform.localScale;
			particle.transform.localRotation = transform.localRotation;
			StartCoroutine(ActivateCoroutine(particle.GetComponent<Image>()));
		}
		else
			StartCoroutine(ActivateCoroutine(GetComponent<Image>()));

	}

	IEnumerator ActivateCoroutine(Image target)
	{
		float targetStamp = Time.time + AnimationTime;

		target.CrossFadeAlpha(0.5f, 0, false);
		target.CrossFadeAlpha(0, AnimationTime, false);

		while (Time.time < targetStamp)
		{
			target.transform.localScale = Vector3.Lerp(target.transform.localScale, TargetScale, 0.05f);
			yield return null;
		}
		Destroy(target);
	}

}
