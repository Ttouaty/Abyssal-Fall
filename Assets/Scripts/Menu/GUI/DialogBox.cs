﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[RequireComponent(typeof(AudioSource))]
public class DialogBox : MonoBehaviour
{
	public Transform ControlDiv;
	public Text TextDiv;
	public AudioClip TypeSound;
	private Message[] _activeMessageSequence;
	private int _activeSequenceIndex = 0;
	private Coroutine _activeDisplayCoroutine;
	private bool _advance = false;
	private bool _isDisplaying = false;
	private AudioSource _audioSource;

	void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		_audioSource.spatialize = false;
	}

	public IEnumerator LaunchDialog(Message[] messagesSequence)
	{
		Restart();
		_activeMessageSequence = messagesSequence;

		for (int i = 0; i < _activeMessageSequence.Length; i++)
		{
			_activeMessageSequence[i].TranslatedText = Localizator.LanguageManager.Instance.GetText(_activeMessageSequence[i].TextKey);
		}

		_activeDisplayCoroutine = StartCoroutine(DialogCoroutine());
		yield return _activeDisplayCoroutine;
	}



	private IEnumerator DialogCoroutine()
	{
		while (_activeSequenceIndex < _activeMessageSequence.Length)
		{
			yield return StartCoroutine(DisplayTextOverTime(_activeMessageSequence[_activeSequenceIndex]));

			while (!_advance)
			{
				yield return null;
			}

			_activeSequenceIndex++;
			_advance = false;
		}
	}


	private int _messageProgressionIndex;
	private int _displayedTagIndex;
	private int _displayedTextOffset;
	private bool skipMessage;
	private IEnumerator DisplayTextOverTime(Message message)
	{
		//ControlDiv.gameObject.SetActive(false);
		DeactivateControls();
		TextDiv.text = "";
		string textToAdd = "";
		int nextIndexToWrite = 0;
		int nextTextOffset = 0;
		_messageProgressionIndex = 0;
		_displayedTagIndex = 0;
		_displayedTextOffset = 0;
		_isDisplaying = true;
		skipMessage = false;
		tagMatches = Regex.Matches(message.TranslatedText, @"<(.*?)>");

		yield return null;

		while (_messageProgressionIndex < message.TranslatedText.Length)
		{

			nextIndexToWrite = _messageProgressionIndex;
			nextTextOffset = _displayedTextOffset;
			textToAdd = FilterTag(message);

			//Debug.Log(nextIndexToWrite - nextTextOffset);
			//Debug.Log(textToAdd);
			//Debug.Log(TextDiv.text.Length);

			TextDiv.text = TextDiv.text.Insert(nextIndexToWrite - nextTextOffset, textToAdd);
			if (!skipMessage)
			{
				_audioSource.PlayOneShot(TypeSound);
				yield return new WaitForSeconds(1 / message.TextSpeed);
			}
		}

		yield return new WaitForSeconds(message.DelayAfter);

		_isDisplaying = false;

		if (message.AutoSkip)
			MoveNext();
		else
			ActivateControls();
	}

	private List<int> tagIndexToAdd = new List<int>();
	private MatchCollection tagMatches;
	private string FilterTag(Message message)
	{
		if (message.TranslatedText[_messageProgressionIndex] == '<')
		{
			string stringToWrite = "";

			if (message.TranslatedText[_messageProgressionIndex + 1] == '/')
			{
				//Closing

				Debug.Log("detected closing");
				_messageProgressionIndex += tagIndexToAdd[tagIndexToAdd.Count - 1];
				tagIndexToAdd.RemoveAt(tagIndexToAdd.Count - 1);
				_displayedTagIndex++;
				return "";
			}
			else
			{
				string tagName = Regex.Matches(tagMatches[_displayedTagIndex].Value, @"(\w+)")[0].Value;
				string closingTagString = "";
				//Opening

				if (tagName == "br")
				{
					stringToWrite = "\n";
					_displayedTextOffset += tagMatches[_displayedTagIndex].Value.Length - 1;
				}
				else
				{
					for (int i = _displayedTagIndex + 1; i < tagMatches.Count; i++)
					{
						if (Regex.Match(tagMatches[i].Value, @"(/" + tagName + ")").Success)
						{
							closingTagString = tagMatches[i].Value;
							tagIndexToAdd.Add(closingTagString.Length);
							break;
						}
					}

					stringToWrite = tagMatches[_displayedTagIndex].Value + closingTagString;
				}

				_messageProgressionIndex += tagMatches[_displayedTagIndex].Value.Length;

				_displayedTagIndex++;
				return stringToWrite;
			}
			//start <\s*\w.*?>

			//start & closing <(.*?)>

			// tag name (\w+) [0]

		}

		_messageProgressionIndex++;
		if (_messageProgressionIndex <= message.TranslatedText.Length)
			return message.TranslatedText[_messageProgressionIndex - 1] + "";
		else
			return "";
	}

	private IEnumerator DisplayInTag(Message message, string tag)
	{
		yield return null;
	}

	public void MoveNext()
	{
		if (_isDisplaying)
		{
			if (!_activeMessageSequence[_activeSequenceIndex].IsSkippable)
				return;

			skipMessage = true;
		}
		else
			_advance = true;
	}

	public void SkipSequence()
	{
		if (_activeDisplayCoroutine != null)
			StopCoroutine(_activeDisplayCoroutine);


		Debug.Log("Activate End Dialog (Skip)");
	}

	private void Restart()
	{
		if(TextDiv == null)
			Debug.LogError("NO TEXT LINKED TO THE TEXTDIV => "+gameObject.name);

		gameObject.SetActive(true);
		DeactivateControls();
		_activeSequenceIndex = 0;

		if (_activeDisplayCoroutine != null)
			StopCoroutine(_activeDisplayCoroutine);
		Debug.Log("Dialog box was reset.");
	}

	public void ActivateControls()
	{
		ControlDiv.GetComponent<CanvasGroup>().alpha = 1;
	}

	public void DeactivateControls()
	{
		ControlDiv.GetComponent<CanvasGroup>().alpha = 0.3f;
	}
}
