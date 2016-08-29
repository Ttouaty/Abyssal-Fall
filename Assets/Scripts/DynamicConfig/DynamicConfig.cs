using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicConfig : GenericSingleton<DynamicConfig>
{
	public const string VERSION = "1.1.0";

	public List<ArenaConfiguration> ArenaConfigurations;
	public Dictionary<string, ArenaConfiguration_SO> ArenaConfigurationsDic;

	public List<AGameRulesConfiguration> ModeConfigurations;
	public Dictionary<string, AGameRules> ModeConfigurationsDic;

	public List<MapConfiguration> MapsConfigurations;
	public Dictionary<string, MapConfiguration_SO> MapsConfigurationsDic;

	public List<CharacterConfiguration> CharacterConfigurations;
	public Dictionary<string, PlayerController> CharacterConfigurationsDic;

	void Awake ()
	{
		ArenaConfigurationsDic      = new Dictionary<string, ArenaConfiguration_SO>();
		ModeConfigurationsDic       = new Dictionary<string, AGameRules>();
		MapsConfigurationsDic       = new Dictionary<string, MapConfiguration_SO>();
		CharacterConfigurationsDic  = new Dictionary<string, PlayerController>();

		for (int i = 0; i < ArenaConfigurations.Count; ++i)
		{
			ArenaConfiguration config = ArenaConfigurations[i];
			ArenaConfigurationsDic.Add(config.Name, config.Configuration);
		}

		for (int i = 0; i < ModeConfigurations.Count; ++i)
		{
			AGameRulesConfiguration config = ModeConfigurations[i];
			ModeConfigurationsDic.Add(config.Name, config.Configuration);
		}

		for (int i = 0; i < MapsConfigurations.Count; ++i)
		{
			MapConfiguration config = MapsConfigurations[i];
			MapsConfigurationsDic.Add(config.Name, config.Configuration);
		}

		for (int i = 0; i < CharacterConfigurations.Count; ++i)
		{
			CharacterConfiguration config = CharacterConfigurations[i];
			CharacterConfigurationsDic.Add(config.Name, config.Configuration);
		}
	}

	public void GetConfig(EArenaConfiguration configName, out ArenaConfiguration_SO config)	{ config = ArenaConfigurationsDic[configName.ToString()]; }
	public void GetConfig(string configName, out ArenaConfiguration_SO config)				{ config = ArenaConfigurationsDic[configName]; }
	public void GetConfigs(ref ArenaConfiguration_SO[] config)								{ ArenaConfigurationsDic.Values.CopyTo(config, 0); }

	public void GetConfig(EModeConfiguration configName, out AGameRules config)				{ config = ModeConfigurationsDic[configName.ToString()]; }
	public void GetConfig(string configName, out AGameRules config)							{ config = ModeConfigurationsDic[configName]; }
	public void GetConfigs(ref AGameRules[] config)											{ ModeConfigurationsDic.Values.CopyTo(config, 0); }

	public void GetConfig(EMapConfiguration configName, out MapConfiguration_SO config)		{ config = MapsConfigurationsDic[configName.ToString()]; }
	public void GetConfig(string configName, out MapConfiguration_SO config)				{ config = MapsConfigurationsDic[configName]; }
	public void GetConfigs(ref MapConfiguration_SO[] config)								{ MapsConfigurationsDic.Values.CopyTo(config, 0); }

	public void GetConfig(ECharacterConfiguration configName, out PlayerController config)	{ config = CharacterConfigurationsDic[configName.ToString()]; }
	public void GetConfig(string configName, out PlayerController config)					{ config = CharacterConfigurationsDic[configName]; }
	public void GetConfigs(ref PlayerController[] config)									{ CharacterConfigurationsDic.Values.CopyTo(config, 0); }
}

[System.Serializable]
public struct ArenaConfiguration
{
	public string Name;
	public ArenaConfiguration_SO Configuration;
}

[System.Serializable]
public struct AGameRulesConfiguration
{
	public string Name;
	public AGameRules Configuration;
}

[System.Serializable]
public struct CharacterConfiguration
{
	public string Name;
	public PlayerController Configuration;
}

[System.Serializable]
public struct MapConfiguration
{
	public string Name;
	public MapConfiguration_SO Configuration;
}