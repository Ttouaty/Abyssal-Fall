using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace Localizator
{
	enum LanguageFragment {none}

	[CustomEditor(typeof(LocalizedText))]
	public class CustomInspector_LocalizedText : Editor
	{
		private LocalizedText _target;
		private string[] _rootLines;
		//private int _selectedIndex = 0;

		void OnEnable ()
		{
			_target = (LocalizedText)target;
			if(_rootLines == null)
			{
				_rootLines = File.ReadAllLines(Localizator.Path.DatabaseRootPath + "Root.csv");
			}

			for(int i = 0; i < _rootLines.Length; ++i)
			{
				if(_rootLines[i].Equals(_target._fragmentInternal))
				{
					//_selectedIndex = i;
					break;
				}
			}
		}

		public override void OnInspectorGUI()
		{
			if(_rootLines.Length == 0)
			{
				EditorGUILayout.LabelField("No Labels set in the label file", EditorStyles.boldLabel);
				if(GUILayout.Button("Open Localization Editor Window"))
				{
					CustomEditorWindow_Localizator.OpenWindow();
				}
			}
			else
			{
				string value = EditorGUILayout.TextField("Current Fragment :", _target._fragmentInternal);
				if(!_target._fragmentInternal.Equals(value))
				{
					_target._fragmentInternal = value;

					if(Application.isPlaying)
					{
						_target.OnChangeLanguage();
					}
					else
					{
						//_target.GetComponent<Text>().text = "<" + _target.Fragment + ">";
						_target.gameObject.SetActive(false);
						_target.gameObject.SetActive(true);
					}
				}
			}
		}
	}
}