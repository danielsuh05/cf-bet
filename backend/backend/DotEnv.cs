namespace backend;

using System;
using System.IO;

public static class DotEnv
{
    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (string line in File.ReadAllLines(filePath))
        {
            int index = line.IndexOf('=');
            if (index == -1)
                continue;

            string key = line[..index].Trim();
            string value = line[(index + 1)..].Trim();

            Environment.SetEnvironmentVariable(key, value); 
        }
    }
}