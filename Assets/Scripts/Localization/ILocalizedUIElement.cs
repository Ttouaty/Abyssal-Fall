using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public interface ILocalizedUIElement
{
    void SetText(string value);
    void OnChangeLanguage();
}
