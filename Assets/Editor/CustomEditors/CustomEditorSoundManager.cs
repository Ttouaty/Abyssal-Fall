using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SoundManager))]
public class CustomEditorSoundManager : Editor
{
	[MenuItem("GameObject/Create Sound Manager")]
	public static void CreateSoundManager()
	{
		SoundManager test = (SoundManager)FindObjectOfType(typeof(SoundManager));
		if (test == null)
		{
			GameObject soundManager = new GameObject("SoundManager");
			soundManager.AddComponent<SoundManager>();
		}
	}

	private SoundManager _myTarget;
	private Color _defaultColor;
	private Color _defaultBackgroundColor;

	void OnEnable()
	{
		_myTarget = (SoundManager)target;
		_defaultColor = GUI.color;
		_defaultBackgroundColor = GUI.backgroundColor;
	}

	public override void OnInspectorGUI ()
    {
        SetGUIBackgroundColor(Color.white * 0.75f);
		EditorGUILayout.BeginVertical("box");
		{
			SetGUIBackgroundColor(Color.white);
			EditorGUILayout.BeginVertical("box");
			{
				GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
				style.richText = true;
				style.alignment = TextAnchor.MiddleCenter;
				style.fontSize = 15;
				GUILayout.Label("Sound Manager v" + SoundManager.VERSION, style, GUILayout.Height(20));
			}
			EditorGUILayout.EndVertical();
			SetGUIBackgroundColor(Color.white * 0.75f);

			EditorGUILayout.Separator();

			SetGUIBackgroundColor(Color.white);
			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("AudioClips", EditorStyles.boldLabel);

				if(_myTarget.SoundList != null)
				{
					List<SoundData> toRemove = new List<SoundData>();
					for (int i = 0; i < _myTarget.SoundList.Count; ++i)
					{
						SoundData data = _myTarget.SoundList[i];
						GUILayout.Space(8);
						EditorGUILayout.BeginVertical("box");
						{
							data.Name = data.Clip.name;
							EditorGUILayout.SelectableLabel(data.Name, EditorStyles.boldLabel);
							data.Clip = (AudioClip)EditorGUILayout.ObjectField("", data.Clip, typeof(AudioClip), false);

							EditorGUILayout.BeginHorizontal();
							{
								if (GUILayout.Button("Delete Clip"))
								{
									toRemove.Add(data);
								}
							}
							EditorGUILayout.EndHorizontal();
						}
						EditorGUILayout.EndVertical();
						_myTarget.SoundList[i] = data;
					}

					for (int j = 0; j < toRemove.Count; ++j)
					{
						_myTarget.RemoveSoundClip(toRemove[j]);
					}
				}

				EditorGUILayout.Separator();

				DropAreaNewAudioClip(_myTarget);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Separator();

			EditorGUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Chanels", EditorStyles.boldLabel);

				if (_myTarget.AudioSourceChanels != null)
				{
					List<ChanelData> toRemove = new List<ChanelData>();
					for (int i = 0; i < _myTarget.AudioSourceChanels.Count; ++i)
					{
						ChanelData data = _myTarget.AudioSourceChanels[i];
						GUILayout.Space(8);
						EditorGUILayout.BeginVertical("box");
						{
							string oldName = data.Name;
							data.Name = GUILayout.TextField(data.Name, EditorStyles.textField);
							if(data.Name == SoundManager.DEFAULT_CHANEL_NAME)
							{
								data.Name = oldName;
							}
							data.Loop = EditorGUILayout.Toggle("Loop Chanel: ", data.Loop);

							if (GUILayout.Button("x"))
							{
								toRemove.Add(data);
							}
						}
						EditorGUILayout.EndVertical();
						_myTarget.AudioSourceChanels[i] = data;
					}

					for (int j = 0; j < toRemove.Count; ++j)
					{
						_myTarget.RemoveChanel(toRemove[j]);
					}
				}

				EditorGUILayout.Separator();

				if(GUILayout.Button("Add Chanel"))
				{
					_myTarget.AddChanel();
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Separator();

		}
		EditorGUILayout.EndVertical();

		SetDefaultGUIBackgroundColor();
		SceneView.RepaintAll();
	}

	void DropAreaNewAudioClip(SoundManager myTarget)
	{
		GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
		style.normal.textColor = Color.black;
		style.richText = true;
		style.alignment = TextAnchor.MiddleCenter;
		style.fontSize = 15;

		Event evt = Event.current;
		Rect drop_area = EditorGUILayout.BeginHorizontal("box");
		{
			EditorGUILayout.LabelField("Drop AudioClip to create Clip entry", style, GUILayout.Height(50));
		}
		EditorGUILayout.EndVertical();

		switch (evt.type)
		{
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if (!drop_area.Contains(evt.mousePosition))
					return;

				DragAndDrop.visualMode = DragAndDropVisualMode.Link;

				if (evt.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();

					foreach (Object draggedObject in DragAndDrop.objectReferences)
					{
						if ((AudioClip)draggedObject != null)
						{
							myTarget.AddSoundClip((AudioClip)draggedObject);
						}
					}
				}
				break;
		}
	}

	void SetDefaultGUIBackgroundColor()
	{
		GUI.backgroundColor = _defaultBackgroundColor;
	}

	void SetGUIBackgroundColor(Color color)
	{
		GUI.backgroundColor = color;
	}

	void SetDefaultGUIColor()
	{
		GUI.color = _defaultColor;
	}

	void SetGUIColor(Color color)
	{
		GUI.color = color;
	}
}