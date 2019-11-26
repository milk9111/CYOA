using System.Collections.Generic;
using System.Linq;
using Domain;
using UnityEngine;
using Util;

public class NodeManager : MonoBehaviour
{
    private Node _node;
    private TerminalManager _terminalManager;
    private bool _isAnyKey;
    private bool _hasStarted;
    private string _input;
    private string _currNode;

    void Start()
    {
        _node = NodeUtil.GetNodeData<StoryNode>("entry.json");
        _terminalManager = FindObjectOfType<TerminalManager>();
        _currNode = "entry.json";
    }

    void Update()
    {
        if (!_hasStarted)
        {
            _terminalManager.AppendHistory(_node.text);
            _hasStarted = true;
        }
    }

    public string GetNextNode(string key)
    {
        _input = key;
        
        var lookup = _isAnyKey ? "*" : key.ToLower();
        return (from node in _node.nodes where node.keys.Any(k => string.Equals(lookup, k)) select node.link).FirstOrDefault();
    }

    public void LoadNextNode(string node)
    {
        _currNode = node;
        _node = NodeUtil.GetNodeData<StoryNode>(node);
        _isAnyKey = false;
        foreach (var keys in _node.nodes.Select(n => n.keys))
        {
            if (keys.Any() && !keys.Contains("*"))
            {
                continue;
            }
            
            _isAnyKey = true;
            break;
        }

        if (!_node.nodes.Any())
        {
            _isAnyKey = true;
        }
        
        _terminalManager.AppendHistory("\n> " + _input);
        _terminalManager.AppendHistory(TerminalHistory.Separator);
        _terminalManager.AppendHistory(_node.text);
    }

    public void LoadCurrentNode()
    {
        LoadNextNode(_currNode);
    }
}
