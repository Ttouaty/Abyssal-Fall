﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct SceneField
{
    public Object SceneAsset;
    public string SceneName;

    public bool IsNull
    {
        get
        {
            return SceneAsset == null && SceneName == "";
        }
    }

    public static implicit operator string(SceneField sceneField)
    {
        return sceneField.SceneName;
    }
}