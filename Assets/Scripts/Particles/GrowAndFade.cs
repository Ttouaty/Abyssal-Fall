using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GrowAndFade : MonoBehaviour {
	public float AnimationTime = 0.3f;
	public Vector3 TargetScale = new Vector3(1.5f,1.5f,1.5f);

	[HideInInspector]
	public bool DublicateSprite = true;
	[HideInInspector]
	public bool IsLocked = false;

	public void Activate()
	{
		if (IsLocked || !gameObject.activeInHierarchy)
			return;
		if (DublicateSprite)
		{
			GameObject particle = new GameObject(name); /*(GameObject)Instantiate(gameObject, transform.position, Quaternion.identity);*/

			particle.AddComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
			particle.CopyComponentFrom(GetComponent<Image>());
			particle.CopyComponentFrom(GetComponent<GrowAndFade>());

			particle.transform.SetParent(transform, false);
			particle.transform.SetAsFirstSibling();

			particle.GetComponent<GrowAndFade>().enabled = true;
			particle.GetComponent<GrowAndFade>().DublicateSprite = false;
			particle.GetComponent<GrowAndFade>().Activate();
		}
		else
		{
			StartCoroutine(ActivateCoroutine(GetComponent<Graphic>()));
			Destroy(gameObject, AnimationTime);
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
	}

}
