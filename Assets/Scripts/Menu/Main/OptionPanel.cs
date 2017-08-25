using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.IO;

public class OptionPanel : ButtonPanelNew
{
	public static string OptionFilePath = @"\StreamingAssets\AbyssalFallOptions.txt";

	public static AbyssalFallOptions OptionObj
	{
		get
		{
			if (System.IO.File.Exists(Application.dataPath + OptionFilePath))
				return JsonUtility.FromJson<AbyssalFallOptions>(System.IO.File.ReadAllText(Application.dataPath + OptionFilePath));

			return new AbyssalFallOptions();
		}
	}

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

		Slider[] sliderChildren = GetComponentsInChildren<Slider>();
		for (int i = 0; i < sliderChildren.Length; i++)
		{
			sliderChildren[i].value = Convert.ToSingle(_optionsObj.GetType().GetField(sliderChildren[i].gameObject.name).GetValue(_optionsObj));
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
		if (ActiveElement.GetComponent<GlobalOptionButton>() != null)
			ActiveElement.GetComponent<GlobalOptionButton>().ChangeValue(1);
		else
			ActiveElement.GetComponent<Slider>().value += 0.05f;
	}

	public void DecreaseValue()
	{
		if(ActiveElement.GetComponent<GlobalOptionButton>() != null)
			ActiveElement.GetComponent<GlobalOptionButton>().ChangeValue(-1);
		else
			ActiveElement.GetComponent<Slider>().value -= 0.05f;
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

	public void ChangeMasterSoundVolume(Single value)
	{
		_optionsObj.MasterVolume = value;
		FMODUnity.RuntimeManager.GetVCA("vca:/Master").setVolume(value);
	}

	public void ChangeSFXVolume(Single value)
	{
		_optionsObj.SFXVolume = value;
		FMODUnity.RuntimeManager.GetVCA("vca:/SFX").setVolume(value);
	}

	public void ChangeMusicVolume(Single value)
	{
		_optionsObj.MusicVolume = value;
		FMODUnity.RuntimeManager.GetVCA("vca:/Music").setVolume(value);
	}

	public void ChangeAmbianceVolume(Single value)
	{
		_optionsObj.AmbianceVolume = value;
		FMODUnity.RuntimeManager.GetVCA("vca:/Ambiance").setVolume(value);
	}

	public void ChangeShowGameId(int value)
	{
		_optionsObj.ShowGameId = value;
	}
}

[Serializable]
public class AbyssalFallOptions
{
	public int TextureQuality = 0;
	public int Vsync = 0;
	public int AntiAliasing = 0;
	public int Language = 10;
	public float MasterVolume = 0.8f;
	public float SFXVolume = 1f;
	public float MusicVolume = 0.8f;
	public float AmbianceVolume = 0.8f;
	public int ShowGameId = 1;
	
	//public bool FullScreen = true;
	public int ScreenResolution = 0;
}
