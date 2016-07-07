using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(GameObjectPool))]
public class CustomEditorGameObjectPool : Editor
{
	[MenuItem("GameObject/Create GameObject Pool")]
	public static void CreateGameObjecrPool ()
	{
		GameObjectPool test = (GameObjectPool)FindObjectOfType(typeof(GameObjectPool));
		if(test == null)
		{
			GameObject gameObjectPool = new GameObject("GameObjectPool");
			gameObjectPool.AddComponent<GameObjectPool>();
		}
	}

	private GameObjectPool _myTarget;
	private Color _defaultColor;
	private Color _defaultBackgroundColor;

	void OnEnable ()
	{
		_myTarget = (GameObjectPool)target;
		for(var i = 0; i < _myTarget.Pools.Count; ++i)
		{
			Pool pool = _myTarget.Pools[i];
			pool.bIsOpen = false;
			_myTarget.Pools[i] = pool;
		}
		
		_defaultColor = GUI.color;
		_defaultBackgroundColor = GUI.backgroundColor;
	}
	
	public override void OnInspectorGUI()
	{
		SetGUIBackgroundColor(Color.white * 0.75f);
		EditorGUILayout.BeginVertical("box");
		{
			List<Pool> poolsToRemove = new List<Pool>();

			SetGUIBackgroundColor(Color.white);
			EditorGUILayout.BeginVertical("box");
			{
				GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
				style.richText = true;
				style.alignment = TextAnchor.MiddleCenter;
				style.fontSize = 15;
				GUILayout.Label("GameObject Pool Manager v" + GameObjectPool.VERSION, style, GUILayout.Height(20));
			}
			EditorGUILayout.EndVertical();
			SetGUIBackgroundColor(Color.white * 0.75f);

			EditorGUILayout.Separator();
			
			SetGUIBackgroundColor(Color.white);
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.BeginVertical();
				{
					for (var p = 0; p < _myTarget.Pools.Count; ++p)
					{
						Pool pool = _myTarget.Pools[p];

						EditorGUILayout.BeginVertical("box");
						{
							EditorGUILayout.BeginHorizontal();
							{
								EditorGUILayout.LabelField(pool.Name + "  " + (pool.Reserve != null ? pool.Reserve.Count : 0) + "/" + pool.Quantity, EditorStyles.boldLabel);
								GUILayout.FlexibleSpace();
								GUIStyle styleButton = new GUIStyle(EditorStyles.miniButtonRight);
								SetGUIBackgroundColor(Color.cyan);
								if(GUILayout.Button("", styleButton, GUILayout.Width(25), GUILayout.Height(20)))
								{
									pool.bIsOpen = !pool.bIsOpen;
								}
								SetGUIBackgroundColor(Color.white);
							}
							EditorGUILayout.EndHorizontal();
							
							Rect progressBarRect = GUILayoutUtility.GetRect(300,25);
							EditorGUI.ProgressBar(progressBarRect, pool.Reserve != null ? (float)pool.Reserve.Count / (float)pool.Quantity : 0, "");
							
							if(pool.bIsOpen)
							{
								GUILayout.Space(8);
								
								pool.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab: ", pool.Prefab, typeof(GameObject), false);
								pool.Quantity = EditorGUILayout.IntField("Quantity: ", pool.Quantity);
								EditorGUILayout.BeginHorizontal();
								{
									if (GUILayout.Button("Duplicate Pool"))
									{
										_myTarget.DuplicatePool(pool);
									}
									if (GUILayout.Button("Delete Pool"))
									{
										poolsToRemove.Add(pool);
									}
								}
								EditorGUILayout.EndHorizontal();
							}
						}
						EditorGUILayout.EndVertical();
						_myTarget.Pools[p] = pool;
					}
				}
				EditorGUILayout.EndVertical();
				DropAreaNewPool(_myTarget);

				for (var p = 0; p < poolsToRemove.Count; ++p)
				{
					_myTarget.RemovePool(poolsToRemove[p]);
				}
			}
			EditorGUILayout.EndVertical();
			SetGUIBackgroundColor(Color.white * 0.75f);

			EditorGUILayout.Separator();
			
			SetGUIBackgroundColor(Color.white);
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
				SerializedProperty loadStart = serializedObject.FindProperty("LoadStart");
				SerializedProperty loadProgress = serializedObject.FindProperty("LoadProgress");
				SerializedProperty loadEnd = serializedObject.FindProperty("LoadEnd");

				EditorGUILayout.PropertyField(loadStart);
				EditorGUILayout.PropertyField(loadProgress);
				EditorGUILayout.PropertyField(loadEnd);

				serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Separator();

			SetGUIBackgroundColor(Color.white);
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
				_myTarget.NumberOfInstancesPerFrame = EditorGUILayout.IntField("Objects Generated per frame: ", _myTarget.NumberOfInstancesPerFrame);
				_myTarget.bInitOnLoad = EditorGUILayout.Toggle("Load on Start", _myTarget.bInitOnLoad);
			}
			EditorGUILayout.EndVertical();
			SetGUIBackgroundColor(Color.white * 0.75f);
		}
		EditorGUILayout.EndVertical();

		SetDefaultGUIBackgroundColor();
		SceneView.RepaintAll();
	}
 
    void DropAreaNewPool (GameObjectPool myTarget)
    {
		GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
		style.normal.textColor = Color.black;
		style.richText = true;
		style.alignment = TextAnchor.MiddleCenter;
		style.fontSize = 15;
		
		Event evt = Event.current;
		Rect drop_area = EditorGUILayout.BeginHorizontal("box");
		{
			EditorGUILayout.LabelField("Drop Prefab to create new pool", style, GUILayout.Height(50));
		}
		EditorGUILayout.EndVertical();
     
        switch (evt.type) {
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if (!drop_area.Contains (evt.mousePosition))
					return;
				
				DragAndDrop.visualMode = DragAndDropVisualMode.Link;
			
				if (evt.type == EventType.DragPerform) {
					DragAndDrop.AcceptDrag ();
				
					foreach (Object draggedObject in DragAndDrop.objectReferences) {
						if((GameObject)draggedObject != null)
						{
							myTarget.AddPool((GameObject)draggedObject);
						}
					}
				}
			break;
        }
    }
	
	void SetDefaultGUIBackgroundColor ()
	{
		GUI.backgroundColor = _defaultBackgroundColor;
	}
	
	void SetGUIBackgroundColor (Color color)
	{
		GUI.backgroundColor = color;
	}
	
	void SetDefaultGUIColor ()
	{
		GUI.color = _defaultColor;
	}
	
	void SetGUIColor (Color color)
	{
		GUI.color = color;
	}
}