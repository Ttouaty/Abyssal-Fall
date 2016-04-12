using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(ArenaGenerator))]
public class CustomEditorArenaGenerator : Editor
{
	public override void OnInspectorGUI()
	{
		ArenaGenerator myTarget = (ArenaGenerator)target;
		EditorGUILayout.BeginVertical("box");
		{
			EditorGUILayout.LabelField("ArenaGenerator Options", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Global Options", EditorStyles.boldLabel);
				myTarget.Size = EditorGUILayout.IntSlider("Arena Size: ", myTarget.Size, 20, 100);
				myTarget.TileScale = EditorGUILayout.Slider("Ground bloc scale: ", myTarget.TileScale, 1, 5);
				myTarget.SpawnDistanceFromBorder = EditorGUILayout.IntSlider("Spawn distance from border: ", myTarget.SpawnDistanceFromBorder, 0, 5);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Obstacles Options", EditorStyles.boldLabel);
				myTarget.ObstaclesQuantity = EditorGUILayout.IntSlider("Obstacles Quantity: ", myTarget.ObstaclesQuantity, 0, 100);
				myTarget.ObstaclesDecrease = EditorGUILayout.IntSlider("Obstacle grow decrease: ", myTarget.ObstaclesDecrease, 10, 100);
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical("box");	
		{
			EditorGUILayout.LabelField("ArenaGenerator References", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
				for (var i = 0; i < 4; ++i)
				{
					EditorGUILayout.BeginVertical("box");
					{
						EditorGUILayout.LabelField("Player " + (i + 1), EditorStyles.boldLabel);
						myTarget.PlayerRef[i] = (GameObject)EditorGUILayout.ObjectField("Prefab: ", myTarget.PlayerRef[i], typeof(GameObject));
						myTarget.SpritesIDMaterialRef[i] = (Material)EditorGUILayout.ObjectField("SpriteID Material: ", myTarget.SpritesIDMaterialRef[i], typeof(Material));
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Separator();

			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Pools Required", EditorStyles.boldLabel);
				for (var i = 0; i < myTarget.RequiredPools.Count; ++i)
				{
					EditorGUILayout.BeginVertical("box");
					{
						myTarget.RequiredPools[i] = EditorGUILayout.TextField("Pool name:", myTarget.RequiredPools[i]);
						if (GUILayout.Button("Delete Pool", GUILayout.Width(80)))
						{
							myTarget.RequiredPools.Remove(myTarget.RequiredPools[i]);
						}
					}
					EditorGUILayout.EndVertical();
				}
				if (GUILayout.Button("Add Pool"))
				{
					myTarget.RequiredPools.Add("");
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndVertical();
	}
}
