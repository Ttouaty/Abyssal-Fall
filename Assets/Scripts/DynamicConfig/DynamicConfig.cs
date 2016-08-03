using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicConfig : GenericSingleton<DynamicConfig>
{
    public const string VERSION = "1.0.0";

    public List<ArenaConfiguration> ArenaConfigurations;
    public Dictionary<string, ArenaConfiguration_SO> ArenaConfigurationsDic;

    public List<ModeConfiguration> ModeConfigurations;
    public Dictionary<string, ModeConfiguration_SO> ModeConfigurationsDic;

    void Awake ()
    {
        ArenaConfigurationsDic = new Dictionary<string, ArenaConfiguration_SO>();
        ModeConfigurationsDic = new Dictionary<string, ModeConfiguration_SO>();

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