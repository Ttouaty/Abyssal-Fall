using UnityEngine;
using UnityEngine.UI;
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

		public string Fragment = "none";

		void Awake()
		{
			_text = GetComponent<Text>();
		}

		void Start()
		{
			if (LanguageManager.Instance == null)
			{
				LanguageManager.CreateInstance();
			}
			LanguageManager.Instance.OnChangeLanguage.AddListener(OnChangeLanguage);
			OnChangeLanguage();
		}

		public void SetText (string fragment, params KeyValuePair<string, string>[] replace)
		{
			Fragment = fragment;
			OnChangeLanguage();
			SetText(replace);
		}

		public void SetText (params KeyValuePair<string, string>[] replace)
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
			_text.text = _fragmentTextRef;
		}
	}

}