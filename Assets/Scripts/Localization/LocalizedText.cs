using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    private Text _text;
    public string TextID = "";

    void Start()
    {
        _text = GetComponent<Text>();
        OnChangeLanguage();
        LocalizationManager.Instance.OnChangeLanguage.AddListener(OnChangeLanguage);
    }

    public void SetText(string value)
    {
        TextID = value;
        OnChangeLanguage();
    }

    public void OnChangeLanguage()
    {
        if(TextID != null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                _text.text = LocalizationManager.Instance.GetText(TextID);
            }
            else
            {
                GetComponent<Text>().text = TextID;
                gameObject.SetActive(false);
                gameObject.SetActive(true);
#else
             _text.text = LocalizationManager.Instance.GetText(TextID);
#endif
            }
        }
    }
}