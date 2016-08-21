using UnityEngine;
using UnityEngine.UI;

namespace Localizator
{
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        private Text _text;

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

        public void SetText (string fragment)
        {
            Fragment = fragment;
        }

        public void OnChangeLanguage()
        {
            _text.text = LanguageManager.Instance.GetText(Fragment);
        }
    }

}