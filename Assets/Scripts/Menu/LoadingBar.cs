using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour {


	private float originalSize;

	private Image _loadBarProgress;

	// Use this for initialization
	void Start () {
		_loadBarProgress = transform.GetChild(0).GetComponent<Image>();
		originalSize = _loadBarProgress.rectTransform.rect.width;
		_loadBarProgress.rectTransform.sizeDelta = new Vector2(0, _loadBarProgress.rectTransform.rect.height);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetPercent(float newPercent)
	{
		_loadBarProgress.rectTransform.sizeDelta = new Vector2(newPercent * originalSize, _loadBarProgress.rectTransform.rect.height);
	
	}
}
