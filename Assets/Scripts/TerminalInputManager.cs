using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Commands;
using UnityEngine;

public class TerminalInputManager : MonoBehaviour
{
    public string commandReservedChar = "-";

    private IDictionary<string, Command> _commands;
    
    private NodeManager _nodeManager;
    
    void Start()
    {
        _nodeManager = FindObjectOfType<NodeManager>();
        InitCommands();
    }

    private void InitCommands()
    {
        _commands = new Dictionary<string, Command>
        {
            {"help", GetComponent<Help>()},
            {"history", GetComponent<History>()},
            {"resume", GetComponent<Resume>()},
            {"exit", GetComponent<Exit>()}
        }; 
    }

    public void ReceiveInput(string input)
    {
        if (input.Contains(commandReservedChar))
        {
            var commands = input.Substring(input.IndexOf(commandReservedChar, StringComparison.Ordinal) + 1).Split(' ');
            var command = commands[0];
            if (!_commands.ContainsKey(command))
            {
                return;
            }
            
            _commands[command].Execute(commands.Skip(1).ToArray());
            return;
        }
        
        var nextNode = _nodeManager.GetNextNode(input);
        if (string.IsNullOrEmpty(nextNode))
        {
            return;
        }
        
        _nodeManager.LoadNextNode(nextNode);
    }

    public ISet<string> GetListOfCommands()
    {
        var commandSet = new HashSet<string>();
        foreach (var command in _commands.Keys)
        {
            if (commandSet.Contains(command))
            {
                continue;
            }

            commandSet.Add(command);
        }
        
        return commandSet;
    }
}
