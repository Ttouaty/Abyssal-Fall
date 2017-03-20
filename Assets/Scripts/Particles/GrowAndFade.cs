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
			GameObject particle = new GameObject(name); /*(GameObject)Instantiate(gameObject, transform.position, Quaternion.identity);*/

			particle.CopyComponentFrom(GetComponent<Image>());
			particle.CopyComponentFrom(GetComponent<GrowAndFade>());

			if (IsDetached)
				particle.transform.SetParent(
					GetComponentInParent<Canvas>() == null ?
					MessageManager.Instance.GetComponentInChildren<Canvas>().transform : 
					GetComponentInParent<Canvas>().transform,
					false
				);
			else
				particle.transform.SetParent(transform, false);

			particle.transform.localScale = IsDetached ? transform.localScale : Vector3.one;
			particle.transform.localRotation = IsDetached ? transform.localRotation : Quaternion.identity;
			particle.GetComponent<GrowAndFade>().enabled = true;
			particle.GetComponent<GrowAndFade>().DublicateSprite = false;
			particle.GetComponent<GrowAndFade>().Activate();
		}
		else
		{
			Graphic[] affectedImages = GetComponentsInChildren<Graphic>();

			for (int i = 0; i < affectedImages.Length; i++)
			{
				StartCoroutine(ActivateCoroutine(affectedImages[i]));
			}
		}

	}

	IEnumerator ActivateCoroutine(Graphic target)
	{
		float eT = 0;

		target.CrossFadeAlpha(0.5f, 0, false);
		target.CrossFadeAlpha(0, AnimationTime, false);
		Vector3 startScale = target.transform.localScale;

		while (eT < AnimationTime)
		{
			eT += Time.deltaTime;
			target.transform.localScale = Vector3.Lerp(startScale, TargetScale, eT / AnimationTime);
			yield return null;
		}
		StopAllCoroutines();
		Destroy(target.gameObject);
	}

}
