using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Localizator
{
	[System.Serializable]
	public class LocalizationTextChangeEvent : UnityEvent { }

	[System.Serializable]
	public struct LangStruct
	{
		public SystemLanguage ID;
		public Dictionary<string, string> Values;
	}

	public class LanguageManager : MonoBehaviour
	{
		public const string VERSION = "1.3.0";
		public static SystemLanguage DefaultLanguage = SystemLanguage.Unknown;

		private static LanguageManager _instance;
		public static LanguageManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType<LanguageManager>();
				}
				return _instance;
			}
		}

		public static void CreateInstance ()
		{
			GameObject go = new GameObject("LanguageManager");
			_instance = go.AddComponent<LanguageManager>();
		}

		private Dictionary<SystemLanguage, Dictionary<string, string>>	_languagesDictionaries;
		private Dictionary<string, string>								_currentDictionary;
		private string													_tmpResult = null;

		private SystemLanguage _currentLanguage = SystemLanguage.Unknown;
		public SystemLanguage CurrentLanguage
		{
			get
			{
				return _currentLanguage;
			}
			set
			{
				SystemLanguage old = _currentLanguage;
				_currentLanguage = value;

				if (Application.isPlaying && _currentLanguage != old)
				{
					LoadLanguage();
				}
			}
		}

		public LocalizationTextChangeEvent		OnChangeLanguage;
		public bool								bIsLoaded { get; private set; }

		void Awake ()
		{
			_instance = this;
			bIsLoaded = false;
			StartCoroutine(Awake_Implementation());
		}

		IEnumerator Awake_Implementation()
		{
			yield return StartCoroutine(LoadLangAsset_Implementation(Path.DefaultLanguageFilePath));
			DefaultLanguage = (SystemLanguage)System.Enum.Parse(typeof(SystemLanguage), _tmpResult);
			_currentLanguage = DefaultLanguage;

			_currentDictionary = new Dictionary<string, string>();
			_languagesDictionaries = new Dictionary<SystemLanguage, Dictionary<string, string>>();

			StartCoroutine(LoadLanguage_Implementation());
		}

		public void LoadLanguage()
		{
			StartCoroutine(LoadLanguage_Implementation());
		}

		IEnumerator LoadLanguage_Implementation ()
		{
			Dictionary<string, string> target;
			if (!_languagesDictionaries.TryGetValue(_currentLanguage, out target))
			{
				Debug.LogWarning("Localizator.LanguageManager -> Try to load language <" + _currentLanguage.ToString() + ">");
				yield return StartCoroutine(LoadLangAsset_Implementation(_currentLanguage.ToString() + ".csv"));

				if (_tmpResult != null)
				{
					string[] lines = System.Text.RegularExpressions.Regex.Split(_tmpResult, CSVReader.LINE_SPLIT_RE);
					target = new Dictionary<string, string>();

					for (int i = 1; i < lines.Length; ++i)
					{
						if(!string.IsNullOrEmpty(lines[i]))
						{
							string[] parts = lines[i].Split(';');
							target.Add(parts[0], parts[1]);
						}
					}
					_languagesDictionaries.Add(_currentLanguage, target);
					LoadLanguage();
				}
				else
				{
					Debug.LogError("Localizator.LanguageManager -> Language <" + _currentLanguage + "> not translated. Lanugage <" + DefaultLanguage.ToString() + "> used instead");
					_currentDictionary = _languagesDictionaries[DefaultLanguage];
				}
			}
			else
			{
				Debug.Log("Localizator.LanguageManager -> CurrentLanguage set to <" + _currentLanguage.ToString() + ">");
				_currentDictionary = target;
			}

			if(!bIsLoaded)
			{
				bIsLoaded = true;
			}
			OnChangeLanguage.Invoke();
		}

		public string GetText(string id)
		{
			if(_currentDictionary == null)
			{
				LoadLanguage();
			}

			string target;
			if (_currentDictionary.TryGetValue(id, out target))
			{
				return target;
			}
			return "Not_Found_" + id;
		}

		IEnumerator LoadLangAsset_Implementation (string fileName)
		{
			string filePath = System.IO.Path.Combine(Path.DatabaseRootPath, fileName);
			if (filePath.Contains("://"))
			{
				WWW www = new WWW(filePath);
				yield return www;
				_tmpResult = www.text;
			}
			else
			{
				_tmpResult = System.IO.File.ReadAllText(filePath);
			}
		}
	}
}