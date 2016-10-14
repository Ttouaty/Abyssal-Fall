using UnityEngine;
using System.Collections;

public class AutoFade : MonoBehaviour
{
	private static AutoFade m_Instance = null;
	private Material m_Material = null;
	private bool m_Fading = false;

	private static AutoFade Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = (new GameObject("AutoFade")).AddComponent<AutoFade>();
				m_Instance.transform.SetParent(GameManager.Instance.transform);
			}
			return m_Instance;
		}
	}
	public static bool Fading
	{
		get { return Instance.m_Fading; }
	}

	private void Awake()
	{
		DontDestroyOnLoad(this);
		m_Instance = this;
		m_Material = Resources.Load<Material>("Plane_No_zTest");
#if UNITY_EDITOR
		if (m_Material == null)
		{
			var resDir = new System.IO.DirectoryInfo(System.IO.Path.Combine(Application.dataPath, "Resources"));
			if (!resDir.Exists)
				resDir.Create();
			Shader s = Shader.Find("Plane/No zTest");
			if (s == null)
			{
				string shaderText = "Shader \"Plane/No zTest\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Fog { Mode Off } BindChannels { Bind \"Color\",color } } } }";
				string path = System.IO.Path.Combine(resDir.FullName, "Plane_No_zTest.shader");
				Debug.Log("Shader missing, create asset: " + path);
				System.IO.File.WriteAllText(path, shaderText);
				UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
				UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>("Resources/Plane_No_zTest.shader");
				s = Shader.Find("Plane/No zTest");
			}
			var mat = new Material(s);
			mat.name = "Plane_No_zTest";
			UnityEditor.AssetDatabase.CreateAsset(mat, "Assets/Resources/Plane_No_zTest.mat");
			m_Material = mat;

		}
#endif
	}

	private void DrawQuad(Color aColor, float aAlpha)
	{
		aColor.a = aAlpha;
		m_Material.SetPass(0);
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Begin(GL.QUADS);
		GL.Color(aColor);   // moved here, needs to be inside begin/end
		GL.Vertex3(0, 0, -1);
		GL.Vertex3(0, 1, -1);
		GL.Vertex3(1, 1, -1);
		GL.Vertex3(1, 0, -1);
		GL.End();
		GL.PopMatrix();
	}

	private IEnumerator Fade(float start, float wait, float end, Color aColor)
	{
		float t = 0.0f;
		while (t < start)
		{
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			DrawQuad(aColor, t / start);
		}

		t = 0;
		while (t < wait)
		{
			t += Time.deltaTime;
			DrawQuad(aColor, 1);
			yield return new WaitForEndOfFrame();
		}

		DrawQuad(aColor, 1);
		t = 0;
		while (t < end)
		{
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
			DrawQuad(aColor, 1 - (t / end));
		}
		m_Fading = false;
	}

	public static void StartFade(float start, float wait, float end)
	{
		StartFade(start, wait, end, Color.black);
	}

	public static void StartFade(float start, float wait, float end, Color aColor)
	{
		Instance.m_Fading = true;
		Instance.StartCoroutine(Instance.Fade(start, wait, end, aColor));
	}

	
}