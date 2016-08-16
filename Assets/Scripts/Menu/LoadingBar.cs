using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
	private float originalSize;

	private Image _loadBarProgress;

	void Awake ()
    {
        _loadBarProgress = transform.GetChild(0).GetComponent<Image>();
	}

    void Start ()
    {
        originalSize = _loadBarProgress.rectTransform.rect.width;
        _loadBarProgress.rectTransform.sizeDelta = new Vector2(0, _loadBarProgress.rectTransform.rect.height);
    }

	public void SetPercent(float newPercent)
    {
        _loadBarProgress.rectTransform.sizeDelta = new Vector2(newPercent * originalSize, _loadBarProgress.rectTransform.rect.height);
	}
}
