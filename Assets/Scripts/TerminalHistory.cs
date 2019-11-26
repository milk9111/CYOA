using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class TerminalHistory : MonoBehaviour
{
    public static readonly string Separator = "\n\n----------------------------\n";
    
    public bool usePrebuiltHistory = true;
    
    private Stack<string> _historyStack;
    private Text _text;

    private Stack<string> _passedOverStack;

    private double _lineHeight;
    private int _maxLinesDisplayed = 30;
    private int _totalPassedOverAmount;
    private int _currNodeLines;
    private bool _showHistory = false;

    void Start()
    {
        _historyStack = usePrebuiltHistory ? TerminalUtil.PrebuiltHistory() : new Stack<string>();
        _historyStack = _historyStack ?? new Stack<string>();
        
        _passedOverStack = new Stack<string>();
        
        _text = GetComponent<Text>();
        _text.alignment = TextAnchor.LowerLeft;
    }

    public void Setup(int fontSize)
    {
        _text.fontSize = fontSize;
        
        DisplayHistory();
    }
    
    public void Append(string history)
    {
        var lines = history?.Split('\n').ToList() ?? new List<string>();

        _totalPassedOverAmount = 0;
        if (lines.Any())
        {
            _currNodeLines = 0;
        }

        foreach (var line in lines)
        {
            _currNodeLines++;
            _historyStack.Push(line);
        }
        
        DisplayHistory();
    }

    void Update()
    {
        var scrollWheel = (int)Math.Floor(10 * Input.GetAxis("Mouse ScrollWheel"));
        var arrowAmount = Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
        arrowAmount += Input.GetKey(KeyCode.DownArrow) ? -1 : 0;

        var passoverAmount = Math.Abs(scrollWheel) > 0 ? scrollWheel : arrowAmount;

        passoverAmount = !_showHistory && passoverAmount > _currNodeLines ? _currNodeLines : passoverAmount;
        
        if (passoverAmount != 0)
        {
            DisplayHistory(passoverAmount);
        }
    }

    private void DisplayHistory(int passoverAmount = 0)
    {
        Passover(passoverAmount);
        
        var tempStack = new Stack<string>();
        for (var i = 0; (_showHistory && i < _maxLinesDisplayed
            || !_showHistory && i < _currNodeLines) && _historyStack.Any(); i++)
        {
            tempStack.Push(_historyStack.Pop());
        }
        
        var displayedHistory = new StringBuilder();
        while (tempStack.Any())
        {
            var text = tempStack.Pop();
            _historyStack.Push(text);
            
            displayedHistory.Append(text);
            if (tempStack.Any())
            {
                displayedHistory.Append("\n");
            }
        }

        _text.text = displayedHistory.ToString();

        if (_passedOverStack.Any())
        {
            RestorePassover();
        }
    }

    private void Passover(int passoverAmount)
    {
        _totalPassedOverAmount = Math.Max(Math.Min(_totalPassedOverAmount + passoverAmount, _historyStack.Count - 1), 0);
        for (var i = 0; i < _totalPassedOverAmount; i++)
        {
            _passedOverStack.Push(_historyStack.Pop());
        }
    }

    private void RestorePassover()
    {
        while(_passedOverStack.Any())
        {
            _historyStack.Push(_passedOverStack.Pop());
        }
    }

    public void ShowHistory(bool showHistory)
    {
        _showHistory = showHistory;
    }
}
