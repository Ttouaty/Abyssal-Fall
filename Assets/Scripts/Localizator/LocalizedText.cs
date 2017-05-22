using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Localizator
{
	[RequireComponent(typeof(Text))]
	public class LocalizedText : MonoBehaviour
	{
		private string _fragmentTextRef;
		private Text _text;
		public Text Text
		{
			get
			{
				return _text;
			}
		}

		public string Fragment
		{
			get { return _fragmentInternal; }
			set
			{
				_fragmentInternal = value;
				OnChangeLanguage();
			}
		}
		[HideInInspector]
		public string _fragmentInternal = "none";

		void Awake()
		{
			_text = GetComponent<Text>();
		}

		void Start()
		{
			LanguageManager.Instance.OnChangeLanguage.AddListener(OnChangeLanguage);
			if (LanguageManager.Instance.bIsLoaded)
			{
				OnChangeLanguage();
			}
		}

		public void SetText(string fragment, params KeyValuePair<string, string>[] replace)
		{
			Fragment = fragment;
			SetText(replace);
		}

		public void SetText(params KeyValuePair<string, string>[] replace)
		{
			string frag = _fragmentTextRef;
			for (int i = 0; i < replace.Length; ++i)
			{
				frag = frag.Replace(replace[i].Key, replace[i].Value);
			}
			_text.text = frag;
		}

		public void OnChangeLanguage()
		{
			_fragmentTextRef = LanguageManager.Instance.GetText(Fragment);
			if (_fragmentTextRef != null)
				_text.text = _fragmentTextRef;
		}
	}

}