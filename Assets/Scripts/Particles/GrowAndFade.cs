using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GrowAndFade : MonoBehaviour {
	public float AnimationTime = 0.3f;
	public Vector3 TargetScale = new Vector3(1.5f,1.5f,1.5f);
	[Space]
	public bool IsDetached = true;

	[HideInInspector]
	public bool DublicateSprite = true;
	[HideInInspector]
	public bool IsLocked = false;

	public void Activate()
	{
		if (IsLocked)
			return;
		if (DublicateSprite)
		{
			GameObject particle = (GameObject)Instantiate(gameObject, transform.position, Quaternion.identity);

			MonoBehaviour[] tempComponents = particle.GetComponents<MonoBehaviour>();
			for (int i = 0; i < tempComponents.Length; i++)
			{
				tempComponents[i].enabled = false;
			}
			particle.GetComponent<Image>().enabled = true;

			if (IsDetached)
				particle.transform.SetParent(
					GetComponentInParent<Canvas>() == null ?
					MessageManager.Instance.GetComponentInChildren<Canvas>().transform : 
					GetComponentInParent<Canvas>().transform
				);
			else
				particle.transform.SetParent(transform.parent);

			particle.transform.localScale = transform.localScale;
			particle.transform.localRotation = transform.localRotation;
			particle.GetComponent<GrowAndFade>().enabled = true;
			particle.GetComponent<GrowAndFade>().DublicateSprite = false;
			particle.GetComponent<GrowAndFade>().Activate();
		}
		else
		{
			Image[] affectedImages = GetComponentsInChildren<Image>();

			for (int i = 0; i < affectedImages.Length; i++)
			{
				StartCoroutine(ActivateCoroutine(affectedImages[i]));
			}
		}

	}

	IEnumerator ActivateCoroutine(Image target)
	{
		float targetStamp = Time.time + AnimationTime;

		target.CrossFadeAlpha(0.5f, 0, false);
		target.CrossFadeAlpha(0, AnimationTime, false);
		Vector3 startScale = target.transform.localScale;
		while (Time.time < targetStamp)
		{
			target.transform.localScale = Vector3.Lerp(startScale, TargetScale, 1 - (targetStamp - Time.time));
			yield return null;
		}
		Destroy(target.gameObject);
	}

}
