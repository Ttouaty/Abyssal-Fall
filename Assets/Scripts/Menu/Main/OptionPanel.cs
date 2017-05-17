using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.IO;

public class OptionPanel : ButtonPanelNew
{
	public static string OptionFilePath = @"\StreamingAssets\AbyssalFallOptions.txt";


	//public Color AmbiantDarkest;
	//public Color AmbiantLightest;

	private AbyssalFallOptions _optionsObj;
	public override void Open()
	{
		base.Open();

		if(System.IO.File.Exists(Application.dataPath + OptionFilePath))
			_optionsObj = JsonUtility.FromJson<AbyssalFallOptions>(System.IO.File.ReadAllText(Application.dataPath + OptionFilePath));

		if (_optionsObj == null)
			_optionsObj = new AbyssalFallOptions();

		GlobalOptionButton tempOption;
		for (int i = 0; i < transform.childCount; i++)
		{
			tempOption = transform.GetChild(i).GetComponent<GlobalOptionButton>();

			if (tempOption == null)
				continue;

			if (!tempOption.isActiveAndEnabled)
				continue;


			tempOption.Init(Convert.ToInt32(_optionsObj.GetType().GetField(tempOption.TargetOptionName).GetValue(_optionsObj)));
		}
	}

	void OnDisable()
	{
		Debug.Log("Written options in => "+Application.dataPath + OptionFilePath);
		System.IO.File.WriteAllText(Application.dataPath + OptionFilePath, JsonUtility.ToJson(_optionsObj));
	}

	public override void SelectNewButton(UIDirection newDirection)
	{
		base.SelectNewButton(newDirection);

		if(newDirection == UIDirection.Right)
			AddValue();
		else if(newDirection == UIDirection.Left)
			DecreaseValue();
	}

	public void AddValue()
	{
		if(ActiveElement.GetComponent<GlobalOptionButton>() != null)
			ActiveElement.GetComponent<GlobalOptionButton>().ChangeValue(1);
	}

	public void DecreaseValue()
	{
		if(ActiveElement.GetComponent<GlobalOptionButton>() != null)
			ActiveElement.GetComponent<GlobalOptionButton>().ChangeValue(-1);
	}

	public void ChangeTextureQuality(int value)
	{
		QualitySettings.masterTextureLimit = value;
		_optionsObj.TextureQuality = value;
	}

	public void ChangeVsync(int value)
	{
		QualitySettings.vSyncCount = value;
		_optionsObj.Vsync = value;
	}

	public void ChangeScreenResolution(int value)
	{
		Screen.SetResolution((int)MainManager.Instance.AvailableResolutions[value].x, (int)MainManager.Instance.AvailableResolutions[value].y, Screen.fullScreen);
		_optionsObj.ScreenResolution = value;
	}

	//public void ChangeFullScreen(int value)
	//{
	//	Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, Convert.ToBoolean(value));
	//	_optionsObj.FullScreen = Convert.ToBoolean(value);
	//}

	public void ChangeAntiAliasing(int value)
	{
		QualitySettings.antiAliasing = (int) Mathf.Pow(2,value);
		_optionsObj.AntiAliasing = value;
	}

	public void ChangeGammaValue()
	{	
		//RenderSettings.ambientLight = Color.Lerp(AmbiantDarkest, AmbiantLightest, transform.Find("Gamma").GetComponent<Slider>().value);
		//_optionsObj.GammaValue = transform.Find("Gamma").GetComponent<Slider>().value;
	}

	public void ChangeLanguage(int value)
	{
		_optionsObj.Language = value;
		Localizator.LanguageManager.Instance.CurrentLanguage = (SystemLanguage) value;
	}

	public void ChangeMasterSoundVolume(float value)
	{
		Debug.Log("Master sound volume set to => " + value);
		_optionsObj.MasterSoundVolume = value;
	}

	public void ChangeSFXVolume(float value)
	{
		Debug.Log("SFX volume set to => " + value);
		_optionsObj.SFXVolume = value;
	}

	public void ChangeMusicVolume(float value)
	{
		Debug.Log("Music volume set to => " + value);
		_optionsObj.MusicVolume = value;
	}
}

[Serializable]
public class AbyssalFallOptions
{
	public int TextureQuality = 0;
	public int Vsync = 0;
	public int AntiAliasing = 0;
	public int Language = 10;
	public float GammaValue = 0.5f;
	public float MasterSoundVolume = 1f;
	public float SFXVolume = 1f;
	public float MusicVolume = 1f;
	//public bool FullScreen = true;
	public int ScreenResolution = 0;
}
