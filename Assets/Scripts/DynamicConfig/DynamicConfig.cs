using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicConfig : GenericSingleton<DynamicConfig>
{
    public const string VERSION = "1.0.0";

    public List<ArenaConfiguration> ArenaConfigurations;
    public Dictionary<string, ArenaConfiguration_SO> ArenaConfigurationsDic;
    public ArenaConfiguration_SO DefaultArena;

    void Awake ()
    {
        ArenaConfigurationsDic = new Dictionary<string, ArenaConfiguration_SO>();

        for (int i = 0; i < ArenaConfigurations.Count; ++i)
        {
            ArenaConfiguration config = ArenaConfigurations[i];
            ArenaConfigurationsDic.Add(config.Name, config.Configuration);
        }
    }

    public ArenaConfiguration_SO GetConfig (string configName)
    {
        ArenaConfiguration_SO config;
        ArenaConfigurationsDic.TryGetValue(configName, out config);
        return config;
    }

#if UNITY_EDITOR
    public void AddArenaConfiguration ()
    {
        ArenaConfiguration config = new ArenaConfiguration();
        ArenaConfigurations.Add(config);
    }

    public void RemoveArenaConfiguration (ArenaConfiguration config)
    {
        ArenaConfigurations.Remove(config);
    }
#endif
}

[System.Serializable]
public struct ArenaConfiguration
{
    public string Name;
    public ArenaConfiguration_SO Configuration;
}