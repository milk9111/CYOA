using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Util
{
    public class NodeUtil
    {
        private static readonly string FilePath = "Assets/Nodes/";
        private static readonly string DefaultNode = "default.json";
        
        public static T GetNodeData<T>(string node)
        {
            var filePath = FilePath + node;
            if(!File.Exists(filePath))
            {
                filePath = FilePath + DefaultNode;
            }
            
            var dataAsJson = File.ReadAllText(filePath);    
            return JsonUtility.FromJson<T>(dataAsJson);
        }

        public static List<string> GetAvailableNodes()
        {
            return Directory.GetFiles(FilePath).Where(s => !s.Contains(".meta"))
                .Select(s => s.Replace(FilePath, "")).ToList();
        }

        public static void WriteNodeData(string node, object nodeData)
        {
            if (!node.Contains(".json"))
            {
                node += ".json";
            }
            
            var jsonString = JsonUtility.ToJson(nodeData);
            var filePath = "Assets/Nodes/" + node;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            File.WriteAllText(filePath, jsonString);
        }

        public static void DeleteNode(string node)
        {
            var filePath = "Assets/Nodes/" + node;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        
        public static string ValidateInput(string input)
        {
            var charArray = input.ToCharArray();
            if (charArray.Length == 0)
            {
                return "";
            }
        
            var lastChar = charArray[input.Length - 1];
            if (TerminalUtil.InvalidChars.Contains(lastChar))
            {
                var length = input.Length - 1 > 0 ? input.Length - 1 : 0;
                return length == 0 ? "" : input.Substring(0, length);
            }

            return input;
        }
        
        public static string StripFileExtension(string file)
        {
            return file.Substring(0, file.IndexOf('.'));
        }
    }
}