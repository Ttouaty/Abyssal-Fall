using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSelectPedestal : MonoBehaviour
{
	public float RotatePerSec	= 180;
	public float TimeToScaleUp	= 0.2f;

	private bool _previousState = false;
	private List<Transform> _lights = new List<Transform>();


	void Start()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			if(transform.GetChild(i).tag != "3DMask")
				_lights.Add(transform.GetChild(i));
		}
	}

	void Update()
	{
		for (int i = 0; i < _lights.Count; i++)
		{
			//Every light alternate rotation (+180° / -180°) /s
			_lights[i].rotation = Quaternion.AngleAxis(RotatePerSec * Time.deltaTime * Mathf.Cos(i%2 * Mathf.PI), transform.up) * _lights[i].rotation; 
		}
	}

	public void SetSelect(bool active)
	{
		StopAllCoroutines();

		for (int i = 0; i < _lights.Count; i++)
		{
			_lights[i].gameObject.SetActive(active);
			if (_previousState != false)
			{
				StartCoroutine(ScaleUp(_lights[i], _lights[i].localScale));
			}
		}

		_previousState = active;
	}

	private IEnumerator ScaleUp(Transform target, Vector3 targetScale)
	{
		Vector3 startScale = targetScale.ZeroY();
		target.localScale = startScale;
		float eT = 0;
		while(eT < TimeToScaleUp)
		{
			target.localScale = Vector3.Lerp(startScale, targetScale, eT / TimeToScaleUp);
			eT += Time.deltaTime;
			yield return null;
		}
		target.localScale = targetScale;
	}
}
