using System.Collections.Generic;
using System.IO;
using Domain;
using UnityEngine;

namespace Util
{
    public class TerminalUtil
    {
        public static List<char> InvalidChars = new List<char>
        {
            '/', '\\', '?', '%', '*', ':', '|', '"',
            '<', '>', '.'
        };
    
        public static double FONT_SIZE_TO_PIXEL_CONVERSION = 8.0 / 6.0;

        public static int GetScreenWidth()
        {
            return Screen.width;
        }

        public static int GetScreenHeight()
        {
            return Screen.height;
        }

        public static Stack<string> PrebuiltHistory()
        {
            var tempStack = new Stack<string>();
            using (var sr = new StreamReader("Assets/TestHistory/test.txt"))
            {
                while (!sr.EndOfStream)
                {
                    tempStack.Push(sr.ReadLine());
                }
            }

            return tempStack;
        }

        public static AvailableCommandList GetAvailableCommandList()
        {
            var filePath = "Assets/Commands/available_commands.json";
            if(!File.Exists(filePath))
            {
                return null;
            }
            
            var dataAsJson = File.ReadAllText(filePath);    
            return JsonUtility.FromJson<AvailableCommandList>(dataAsJson);
        }
    }
}
