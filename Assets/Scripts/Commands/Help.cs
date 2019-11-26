using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain;
using UnityEngine;
using Util;

namespace Commands
{
    public class Help : Command
    {
        private TerminalInputManager _inputManager;
        private TerminalHistory _terminalHistory;

        private IDictionary<string, string> _availableCommandList;

        void Start()
        {
            _inputManager = FindObjectOfType<TerminalInputManager>();
            _terminalHistory = FindObjectOfType<TerminalHistory>();
            
            _availableCommandList = new Dictionary<string, string>();
            
            var commands = TerminalUtil.GetAvailableCommandList();
            if (commands == null || !commands.commands.Any())
            {
                return;
            }
            
            foreach (var command in commands.commands)
            {
                if (_availableCommandList.ContainsKey(command.key))
                {
                    continue;
                }
                
                _availableCommandList.Add(command.key, command.summary);
            }
        }
        
        public override void Execute(string[] args)
        {
            var commandSet = _inputManager.GetListOfCommands();
            
            var sb = new StringBuilder();
            sb.Append("\n");
            
            foreach (var command in commandSet)
            {
                sb.Append(!_availableCommandList.ContainsKey(command)
                    ? $"-{command}\n"
                    : $"-{command}: {_availableCommandList[command]}\n");
            }
            
            _terminalHistory.Append(sb.ToString());
        }
        
    }
}