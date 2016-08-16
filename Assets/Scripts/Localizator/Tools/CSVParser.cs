using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Localizator
{
    public static class CSVTools
    {
        const string    SPLIT_REGEX         = @";(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        const string    LINE_SPLIT_REGEX    = @"\r\n|\n\r|\n|\r";
        const char      TRIM_CHAR           = '\"';

        public static List<string[]> Read (string filePath)
        {
            List<string[]> list = new List<string[]>();
            if (filePath.IndexOf(".csv") >= 0 && File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length <= 1)
                {
                    return list;
                }
                for (int i = 0; i < lines.Length; ++i)
                {
                    string[] line = Regex.Split(lines[i], SPLIT_REGEX);
                    for (int j = 0; j < line.Length; ++j)
                    {
                        line[j] = line[j].TrimStart(TRIM_CHAR).TrimEnd(TRIM_CHAR);
                    }
                    list.Add(line);
                }
            }
            return list;
        }

        public static void Write (string filePath, List<string[]> list)
        {
            string[] data = new string[list.Count];
            for (int i = 0; i < list.Count; ++i)
            {
                string[] line = list[i];
                for(int j = 0; j < line.Length; ++j)
                {
                    if(line[j].IndexOf(';') >= 0)
                    {
                        line[j] = TRIM_CHAR + line[j] + TRIM_CHAR;
                    }
                }
                data[i] = string.Join(";", line);
            }
            File.WriteAllLines(filePath, data);
        }
    }
}