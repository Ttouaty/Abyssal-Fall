using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public struct LocalizationField
{
    public string ID;
    public string EN;
    public string FR;
    public string ES;
}

[System.Serializable]
public enum LocalizationLang
{
    EN, FR, ES
}

public class LocalizationManager : GenericSingleton<LocalizationManager>
{
    [System.Serializable]
    public class LocalizationTextChangeEvent : UnityEvent { }

    [SerializeField]
    private LocalizationLang _currentLanguage = LocalizationLang.EN;

    public TextAsset LocalizationFile;
    public LocalizationTextChangeEvent  OnChangeLanguage;

    [SerializeField]
    private Dictionary<string, LocalizationField> _localizationFields;

    void Awake ()
    {
        if(LocalizationFile == null)
        {
            Debug.LogError("A localization file must be provided");
            Debug.Break();
        }
        else
        {
            _localizationFields = new Dictionary<string, LocalizationField>();
            List<Dictionary<string,string>> result = CSVReader.Read(LocalizationFile);

            for(int i = 0; i < result.Count; ++i)
            {
                LocalizationField localizationField = new LocalizationField();
                Dictionary<string, string> dic = result[i];
                foreach (KeyValuePair<string, string> entry in dic)
                {
                    switch(entry.Key.ToUpper())
                    {
                        case "ID": localizationField.ID = entry.Value; break;
                        case "EN": localizationField.EN = entry.Value; break;
                        case "FR": localizationField.FR = entry.Value; break;
                        case "ES": localizationField.ES = entry.Value; break;
                    }
                }

                _localizationFields.Add(localizationField.ID, localizationField);
            }
        }
    }

    public void SetCurrentLocale (LocalizationLang value)
    {
        _currentLanguage = value;
        if (Application.isPlaying)
        {
            OnChangeLanguage.Invoke();
        }
        else
        {
            LocalizedText[] list = GameObject.FindObjectsOfType<LocalizedText>();
            for (int i = 0; i < list.Length; ++i)
            {
                list[i].OnChangeLanguage();
            }
        }
    }
    
    public string GetText (string key)
    {
        LocalizationField value;
        if(_localizationFields.TryGetValue(key, out value))
        {
            string frag = "<" + key + "->" + _currentLanguage + "> NOT FOUND";
            switch(_currentLanguage)
            {
                case LocalizationLang.EN: frag = value.EN; break;
                case LocalizationLang.FR: frag = value.FR; break;
                case LocalizationLang.ES: frag = value.ES; break;
            }
            return frag;
        }
        else
        {
            return "<" + key + "> NOT FOUND";
        }
    }



#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetCurrentLocale(LocalizationLang.EN);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            SetCurrentLocale(LocalizationLang.FR);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SetCurrentLocale(LocalizationLang.ES);
        }
    }
#endif
}