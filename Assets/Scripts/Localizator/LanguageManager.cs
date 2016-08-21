using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
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

		private Dictionary<SystemLanguage, Dictionary<string, string>> _languagesDictionaries;
		private Dictionary<string, string> _currentDictionary;

        private SystemLanguage _currentLanguage = SystemLanguage.Unknown;
        public SystemLanguage CurrentLanguage
        {
            get
            {
                #if UNITY_EDITOR
                if(!Application.isPlaying && File.Exists(Path.DefaultLanguageFilePath))
                {
                    _currentLanguage = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), File.ReadAllText(Path.DefaultLanguageFilePath));
                }
                #endif
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

        public LocalizationTextChangeEvent OnChangeLanguage;

        void Awake ()
        {
            DefaultLanguage = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), File.ReadAllText(Path.DefaultLanguageFilePath));
            _currentLanguage = DefaultLanguage;

            _currentDictionary = new Dictionary<string, string>();
            _languagesDictionaries = new Dictionary<SystemLanguage, Dictionary<string, string>>();

            LoadLanguage();
        }

		public void LoadLanguage()
        {
            Dictionary<string, string> target;
			if (!_languagesDictionaries.TryGetValue(_currentLanguage, out target))
            {
                Debug.LogWarning("Localizator.LanguageManager -> Try to load language <" + _currentLanguage.ToString() + ">");
                if (File.Exists(Localizator.Path.DatabaseRootPath + _currentLanguage.ToString() + ".csv"))
                {
                    string[] lines = File.ReadAllLines(Localizator.Path.DatabaseRootPath + _currentLanguage.ToString() + ".csv");
                    target = new Dictionary<string, string>();
                    for (int i = 1; i < lines.Length; ++i)
                    {
                        string[] parts = lines[i].Split(';');
                        target.Add(parts[0], parts[1]);
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
	}
}