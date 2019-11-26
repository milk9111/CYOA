using System;

namespace Commands
{
    public class History : Command
    {
        private TerminalHistory _terminalHistory;

        void Start()
        {
            _terminalHistory = FindObjectOfType<TerminalHistory>();
        }
        
        public override void Execute(string[] args)
        {
            try
            {
                _terminalHistory.ShowHistory(Convert.ToBoolean(args[0]));
                _terminalHistory.Append(null);
            }
            catch (Exception e)
            {
                return;
            }
        }
    }
}