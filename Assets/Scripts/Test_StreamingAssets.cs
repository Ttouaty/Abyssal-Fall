using UnityEngine;
using System.IO;
using System.Collections;

public class Test_StreamingAssets : MonoBehaviour
{
    public string StreamFilePath;
    public string Result;

    // Use this for initialization
    void Start () {
        StartCoroutine(Test());
	}

    IEnumerator Test ()
    {
        CoroutineWithResult<string> cd = new CoroutineWithResult<string>(this, LoadFile());
        yield return cd.coroutine;
        Result = cd.result;  //  'success' or 'fail'

    }

    IEnumerator LoadFile()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, StreamFilePath);
        if (filePath.Contains("://"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            yield return www.text;
        }
        else
            yield return File.ReadAllText(filePath);
    }
}
