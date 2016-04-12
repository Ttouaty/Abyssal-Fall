using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(GameObjectPool))]
public class CustomEditorGameObjectPool : Editor
{
	private Vector2 _scrollPosition;

	public override void OnInspectorGUI()
	{
		GameObjectPool myTarget = (GameObjectPool)target;
		List<Pool> poolsToRemove = new List<Pool>();

		EditorGUILayout.BeginVertical("box");
		{
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Pools availables", EditorStyles.boldLabel);
				// int height = myTarget.Pools.Count > 3 ? 350 : 

				_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(330));
				{
					for (var p = 0; p < myTarget.Pools.Count; ++p)
					{
						Pool pool = myTarget.Pools[p];
						EditorGUILayout.BeginVertical("box");
						{
							EditorGUILayout.LabelField(pool.Name != "" ? pool.Name : "Unamed Pool", EditorStyles.boldLabel);
							pool.Name = EditorGUILayout.TextField("Name: ", pool.Name);
							pool.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab: ", pool.Prefab, typeof(GameObject));
							pool.Quantity = EditorGUILayout.IntField("Quantity: ", pool.Quantity);
							EditorGUILayout.BeginHorizontal();
							{
								if (GUILayout.Button("Duplicate Pool"))
								{
									myTarget.DuplicatePool(pool);
								}
								if (GUILayout.Button("Delete Pool"))
								{
									poolsToRemove.Add(pool);
								}
							}
							EditorGUILayout.EndHorizontal();
						}
						EditorGUILayout.EndVertical();
						myTarget.Pools[p] = pool;
					}
				}
				EditorGUILayout.EndScrollView();
			}
			EditorGUILayout.EndVertical();
			if (GUILayout.Button("Add Pool"))
			{
				myTarget.AddPool();
			}

			for (var p = 0; p < poolsToRemove.Count; ++p)
			{
				myTarget.RemovePool(poolsToRemove[p]);
			}
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.Separator();

		EditorGUILayout.BeginVertical("box");
		{
			EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
			myTarget.NumberOfInstancesPerFrame = EditorGUILayout.IntField("Quantity Generated per frame: ", myTarget.NumberOfInstancesPerFrame);
		}
		EditorGUILayout.EndVertical();
	}
}