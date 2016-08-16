using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class CSVReader
{
    public static string    COMMA_SPLIT_RE =    @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    public static string    LINE_SPLIT_RE =     @"\r\n|\n\r|\n|\r";
    public static char      TRIM_CHAR =         '\"';

    public static List<List<string>> Read(TextAsset data)
    {
        List<List<string>> list = new List<List<string>>();
        string[] lines = Regex.Split(data.text, LINE_SPLIT_RE);

        for(int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i].Replace(TRIM_CHAR.ToString(), "");
            string[] values = Regex.Split(line, COMMA_SPLIT_RE);
            List<string> entry = new List<string>(values);
            list.Add(entry);
        }
        return list;
    }
}
