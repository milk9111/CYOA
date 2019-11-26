using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

public class TerminalInput : MonoBehaviour
{
    public TerminalManager terminalManager;
    public Text innerText;
    
    private InputFieldSubmitOnly _input;

    void Start()
    {
        _input = GetComponent<InputFieldSubmitOnly>();
        _input.Select();
        _input.ActivateInputField();
    }

    private void FixedUpdate()
    {
        if (!_input.isFocused)
        {
            _input.Select();
            _input.ActivateInputField();
        }
    }

    public void Setup(int fontSize)
    {
        innerText.fontSize = fontSize;
    }

    public int GetFontSize()
    {
        return innerText.fontSize;
    }

    public void OnEndEdit()
    {
        terminalManager.ReceiveInput(_input.text);
        _input.text = "";
        _input.Select();
        _input.ActivateInputField();
    }
}
