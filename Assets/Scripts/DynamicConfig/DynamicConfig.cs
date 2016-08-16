using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicConfig : GenericSingleton<DynamicConfig>
{
    public const string VERSION = "1.1.0";

    public List<ArenaConfiguration> ArenaConfigurations;
    public Dictionary<string, ArenaConfiguration_SO> ArenaConfigurationsDic;

    public List<ModeConfiguration> ModeConfigurations;
    public Dictionary<string, ModeConfiguration_SO> ModeConfigurationsDic;

    public List<CharacterConfiguration> CharacterConfigurations;
    public Dictionary<string, SO_Character> CharacterConfigurationsDic;

    public List<MapConfiguration> MapsConfigurations;
    public Dictionary<string, TextAsset> MapsConfigurationsDic;

    void Awake ()
    {
        ArenaConfigurationsDic      = new Dictionary<string, ArenaConfiguration_SO>();
        ModeConfigurationsDic       = new Dictionary<string, ModeConfiguration_SO>();
        CharacterConfigurationsDic  = new Dictionary<string, SO_Character>();
        MapsConfigurationsDic       = new Dictionary<string, TextAsset>();

        for (int i = 0; i < ArenaConfigurations.Count; ++i)
        {
            ArenaConfiguration config = ArenaConfigurations[i];
            ArenaConfigurationsDic.Add(config.Name, config.Configuration);
        }

        for (int i = 0; i < ModeConfigurations.Count; ++i)
        {
            ModeConfiguration config = ModeConfigurations[i];
            ModeConfigurationsDic.Add(config.Name, config.Configuration);
        }

        for (int i = 0; i < CharacterConfigurations.Count; ++i)
        {
            CharacterConfiguration config = CharacterConfigurations[i];
            CharacterConfigurationsDic.Add(config.Name, config.Configuration);
        }

        for (int i = 0; i < MapsConfigurations.Count; ++i)
        {
            MapConfiguration config = MapsConfigurations[i];
            MapsConfigurationsDic.Add(config.Name, config.Configuration);
        }
    }

    public ArenaConfiguration_SO GetArenaConfig(string configName)
    {
        ArenaConfiguration_SO config;
        ArenaConfigurationsDic.TryGetValue(configName, out config);
        return config;
    }

    public ModeConfiguration_SO GetModeConfig(string configName)
    {
        ModeConfiguration_SO config;
        ModeConfigurationsDic.TryGetValue(configName, out config);
        return config;
    }

    public SO_Character GetCharacterConfig(string configName)
    {
        SO_Character config;
        CharacterConfigurationsDic.TryGetValue(configName, out config);
        return config;
    }

    public TextAsset GetMapConfig(string configName)
    {
        TextAsset config;
        MapsConfigurationsDic.TryGetValue(configName, out config);
        return config;
    }

#if UNITY_EDITOR
    public void AddArenaConfiguration()
    {
        ArenaConfiguration config = new ArenaConfiguration();
        ArenaConfigurations.Add(config);
    }

    public void RemoveArenaConfiguration(ArenaConfiguration config)
    {
        ArenaConfigurations.Remove(config);
    }

    public void AddModeConfiguration()
    {
        ModeConfiguration config = new ModeConfiguration();
        ModeConfigurations.Add(config);
    }

    public void RemoveModeConfiguration(ModeConfiguration config)
    {
        ModeConfigurations.Remove(config);
    }

    public void AddCharacterConfiguration()
    {
        CharacterConfiguration config = new CharacterConfiguration();
        CharacterConfigurations.Add(config);
    }

    public void RemoveCharacterConfiguration(CharacterConfiguration config)
    {
        CharacterConfigurations.Remove(config);
    }

    public void AddMapConfiguration()
    {
        MapConfiguration config = new MapConfiguration();
        MapsConfigurations.Add(config);
    }

    public void RemoveMapConfiguration(MapConfiguration config)
    {
        MapsConfigurations.Remove(config);
    }
#endif
}

[System.Serializable]
public struct ArenaConfiguration
{
    public string Name;
    public ArenaConfiguration_SO Configuration;
}

[System.Serializable]
public struct ModeConfiguration
{
    public string Name;
    public ModeConfiguration_SO Configuration;
}

[System.Serializable]
public struct CharacterConfiguration
{
    public string Name;
    public SO_Character Configuration;
}

[System.Serializable]
public struct MapConfiguration
{
    public string Name;
    public TextAsset Configuration;
    public Vector2 MapSize;
}