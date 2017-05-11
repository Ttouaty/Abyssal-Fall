using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;

[Serializable]
public class Message
{
	public string TextKey;
	[HideInInspector]
	public string TranslatedText;
	public float DelayAfter;
	[Tooltip("Does the message skips itself after the end of Delay After ?")]
	public bool AutoSkip;
	[Tooltip("Can the message be instantly displayed on user Input ?")]
	public bool IsSkippable = true;
	[Tooltip("How fast the text is displayed (character per second)")]
	public float TextSpeed = 30;

	public string TaglessText
	{
		get
		{
			return Regex.Replace(TranslatedText, "<[^>]*>", ""); ;
		}
	}
}

public class MessageManager : GenericSingleton<MessageManager>
{
	private Canvas _canvas;
	private RectTransform _messageContainer;

	[SerializeField]
	private TextMessage _messagePrefab;

	private int _numberMessagesDisplayed = 0;
	private int _numberMessagesDestroyed = 0;

	private float _messageMargin = 5;

	private float TargetHeight = 0;
	[HideInInspector]
	public float AppendHeight = 0;
	void Start()
	{
		_canvas = GetComponentInChildren<Canvas>();

		if (_canvas.transform.childCount != 0)
			_messageContainer = (RectTransform)_canvas.transform.GetChild(0).GetChild(0);
		else
			Debug.LogError("No message container found in messageContainerCanvas");
	}

	void Update()
	{
		_messageContainer.localPosition = Vector3.Lerp(_messageContainer.localPosition, Vector3.up * TargetHeight, 10 * Time.deltaTime);

		if(_numberMessagesDisplayed != 0)
		{
			if (_numberMessagesDestroyed == _numberMessagesDisplayed)
			{
				CleanUp();
			}
		}
	}


	public static void Log(string message, float timeout, bool translated)
	{
		Instance.StartCoroutine(Instance.LogCo(message, timeout, translated)); 
	}
	
	IEnumerator LogCo(string message, float timeout, bool translated)
	{
		GameObject TextMessageGO = (GameObject)Instantiate(Instance._messagePrefab.gameObject, Instance._messageContainer);
		TextMessage messageRef = TextMessageGO.GetComponent<TextMessage>();
		//TextMessageGO.GetComponent<RectTransform>().sizeDelta = new Vector2(250, TEXT_HEIGHT);
		TextMessageGO.transform.localScale = Vector3.one;

		yield return null;

		TextMessageGO.GetComponent<RectTransform>().localPosition = -Vector3.right * Instance._messageContainer.sizeDelta.x - Vector3.up * Instance.AppendHeight;

		messageRef.MoveTo(-Vector3.up * Instance.AppendHeight, 0.5f, true);

		Instance.AddAppendHeight(TextMessageGO.GetComponent<RectTransform>().sizeDelta.y);

		if (translated)
			message = Localizator.LanguageManager.Instance.GetText(message);

		Instance._numberMessagesDisplayed++;
		messageRef.Activate(message, timeout);

	}

	public static void Log(string message, bool translated){ Log(message, 5, translated); }
	public static void Log(string message, float timeout){ Log(message, timeout, false); }
	public static void Log(string message){ Log(message, 5, false);	}

/*	public static void LogCustom(string message, float timeout, Vector3 startLocalPos, Vector3 localPosition = default(Vector3), Vector2 newSizeDelta = default(Vector2), Vector2 newPivot = default(Vector2))
	{
		GameObject TextMessageGO = (GameObject)Instantiate(Instance._messagePrefab.gameObject, Instance._canvas.transform);
		TextMessage messageRef = TextMessageGO.GetComponent<TextMessage>();
		TextMessageGO.transform.localScale = Vector3.one;

		if (newSizeDelta != default(Vector2))
			TextMessageGO.GetComponent<RectTransform>().sizeDelta = newSizeDelta;
		else
			TextMessageGO.GetComponent<RectTransform>().sizeDelta = new Vector2(250, TEXT_HEIGHT);

		if (newPivot != default(Vector2))
			TextMessageGO.GetComponent<RectTransform>().pivot = newPivot;


		TextMessageGO.GetComponent<RectTransform>().localPosition = startLocalPos;

		if(localPosition != default(Vector3))
			messageRef.MoveTo(localPosition, 0.5f, true);

		message = Localizator.LanguageManager.Instance.GetText(message);
		messageRef.Activate(message, timeout);
	}

	public static void LogCustom(string message, float timeout, Vector3 startLocalPos) { LogCustom(message, timeout, startLocalPos, default(Vector3), default(Vector2), Vector2.one * 0.5f); }
	public static void LogCustom(string message, float timeout, Vector3 startLocalPos, Vector2 newSizeDelta) { LogCustom(message, timeout, startLocalPos, default(Vector3), newSizeDelta, Vector2.one * 0.5f); }
*/
	public void OnMessageDestroyed(TextMessage targetMessage)
	{
		_numberMessagesDestroyed++;
		TargetHeight += targetMessage.GetComponent<RectTransform>().sizeDelta.y + _messageMargin;
	}

	public void CleanUp()
	{
		_numberMessagesDisplayed = 0;
		_numberMessagesDestroyed = 0;

		TargetHeight = 0;
		AppendHeight = 0;


		_messageContainer.localPosition = Vector3.zero;
	}

	public void AddAppendHeight(float newHeight)
	{
		AppendHeight += newHeight + _messageMargin;
	}

}
