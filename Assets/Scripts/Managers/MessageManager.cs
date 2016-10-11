using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageManager : GenericSingleton<MessageManager>
{
	private Canvas _canvas;
	private RectTransform _messageContainer;

	[SerializeField]
	private TextMessage _messagePrefab;

	private int _numberMessagesDisplayed = 0;
	private int _numberMessagesDestroyed = 0;

	private const int TEXT_HEIGHT = 60;

	private float _messageOffset;

	void Start()
	{
		_messageOffset = TEXT_HEIGHT * 1.05f;
		_canvas = GetComponentInChildren<Canvas>();

		if (_canvas.transform.childCount != 0)
			_messageContainer = (RectTransform)_canvas.transform.GetChild(0).GetChild(0);
		else
			Debug.LogError("No message container found in messageContainerCanvas");
	}

	void Update()
	{
		_messageContainer.localPosition = Vector3.Lerp(_messageContainer.localPosition, Vector3.up * _numberMessagesDestroyed * _messageOffset, 10 * Time.deltaTime);
		//if (Input.GetKeyDown(KeyCode.F6))
		//{
		//	Log("Je suis le message numero: "+_numberMessagesDisplayed);
		//}
	}


	public static void Log(string message)
	{
		GameObject TextMessageGO = (GameObject) Instantiate(Instance._messagePrefab.gameObject, Instance._messageContainer);
		TextMessage messageRef = TextMessageGO.GetComponent<TextMessage>();
		TextMessageGO.GetComponent<RectTransform>().sizeDelta = new Vector3(Instance._messageContainer.sizeDelta.x, TEXT_HEIGHT);
		TextMessageGO.transform.localScale = Vector3.one;
		
		TextMessageGO.GetComponent<RectTransform>().localPosition = - Vector3.right * Instance._messageContainer.sizeDelta.x - Vector3.up * (Instance._numberMessagesDisplayed * Instance._messageOffset);

		messageRef.MoveTo(- Vector3.up * (Instance._numberMessagesDisplayed * Instance._messageOffset), 0.5f, true);

		messageRef.Activate(message, 5);
		Instance._numberMessagesDisplayed++;
	}

	public void OnMessageDestroyed()
	{
		_numberMessagesDestroyed++;
	}

}
