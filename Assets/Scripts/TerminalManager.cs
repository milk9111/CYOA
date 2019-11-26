using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalManager : MonoBehaviour
{
    public int fontSize = 28;
    public bool useInputFontSize;
    
    private TerminalHistory _history;
    private TerminalInput _input;
    private TerminalInputManager _inputManager;

    private bool _hasInitialized;

    void Start()
    {
        _history = FindObjectOfType<TerminalHistory>();
        _input = FindObjectOfType<TerminalInput>();
        _inputManager = FindObjectOfType<TerminalInputManager>();

        _hasInitialized = false;
    }

    void Update()
    {
        if (!_hasInitialized)
        {
            if (useInputFontSize)
            {
                fontSize = _input.GetFontSize();
            }
        
            _input.Setup(fontSize);
            _history.Setup(fontSize);

            _hasInitialized = true;
        }
    }

    public void ReceiveInput(string input)
    {
        _inputManager.ReceiveInput(input);
    }

    public void AppendHistory(string line)
    {
        _history.Append(line);
    }
}
