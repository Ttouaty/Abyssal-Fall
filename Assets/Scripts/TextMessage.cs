using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TextMessage : MonoBehaviour
{
	private Text _text;
	private CanvasGroup _canvasGroup;

	void Awake()
	{
		_text = GetComponentInChildren<Text>();
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	void Start()
	{

	}

	public void Activate(string newText, float timeLeft)
	{
		_text.text = newText;

		StartCoroutine(FadeCoroutine(timeLeft));
	}

	

	IEnumerator FadeCoroutine(float time)
	{
		time += Time.time;
		while (Time.time < time - 1)
		{
			yield return null;
		}

		while (Time.time < time)
		{
			_canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, time - Time.time, 0.2f); // la triche avec le time mais bon...
			yield return null;
		}
		_canvasGroup.alpha = 0;
		MessageManager.Instance.OnMessageDestroyed();
		Destroy(gameObject);
	}
}
