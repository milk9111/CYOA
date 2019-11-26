using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;
using Util;

namespace UI
{
    public class InputGroups : MonoBehaviour
    {
        public InputField keys;
        public InputField link;
        public LinkSlider slider;
        
        private int _size = 1;
        private List<InputGroup> _inputs;
        private InputGroup _selectedInput;

        public List<Button> inputSelectorButtons;

        private Color _startingNormalColor;
        private int _lastIndex = -1;
        private int _lastSize;

        private bool _isInitialized;

        void Start()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Init();
            }
        }

        void Update()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Init();
            }
        }

        private void Init()
        {
            _inputs = new List<InputGroup>(_size);
            _lastSize = _size;
            _startingNormalColor = inputSelectorButtons[0].gameObject.GetComponent<Image>().color;
            for (var i = 0; i < inputSelectorButtons.Count; i++)
            {
                _inputs.Add(new InputGroup
                {
                    keys = "",
                    link = ""
                });
            }
        }

        public void UpdateSize(int size)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Init();
            }
            
            _lastSize = _size;
            _size = size;

            if (inputSelectorButtons != null)
            {
                for (var i = 0; i < inputSelectorButtons.Count; i++)
                {
                    inputSelectorButtons[i].gameObject.SetActive(i < _size);
                }
            }

            for (var i = Math.Min(_size, _lastSize); i < _inputs.Count; i++)
            {
                _inputs[i].keys = "";
                _inputs[i].link = "";
            }

            SelectInput(0);
        }

        public void UpdateSelectedKeys()
        {
            _selectedInput.keys = keys.text;
        }

        public void UpdateSelectedLink()
        {
            link.text = NodeUtil.ValidateInput(link.text);
            _selectedInput.link = link.text;
        }

        public int GetSize()
        {
            return _size;
        }

        public void Reset()
        {
            if (_inputs == null)
            {
                return;
            }
            
            slider.Reset();
            
            foreach (var inputGroup in _inputs)
            {
                inputGroup.keys = "";
                inputGroup.link = "";
            }
            
            
            SelectInput(0);
        }

        public void LoadExistingNode(StoryNode node)
        {
            UpdateSize(node.nodes.Length == 0 ? 1 : node.nodes.Length);
            
            for (var i = 0; i < node.nodes.Length; i++)
            {
                var nodeLink = node.nodes[i];
                _inputs[i].keys = string.Join(",", nodeLink.keys);
                _inputs[i].link = NodeUtil.StripFileExtension(nodeLink.link);
            }
            
            slider.SetValue(node.nodes.Length);
            
            SelectInput(0);
        }

        public List<InputGroup> GetInputGroups()
        {
            return _inputs;
        }

        public void SelectInput(int index)
        {
            if (index >= _inputs.Count)
            {
                return;
            }

            if (_lastIndex != -1)
            {
                inputSelectorButtons[_lastIndex].gameObject.GetComponent<Image>().color = _startingNormalColor;
            }

            _lastIndex = index;
            
            inputSelectorButtons[index].gameObject.GetComponent<Image>().color = Color.gray;
            _selectedInput = _inputs[index];
            keys.text = _selectedInput.keys;
            link.text = _selectedInput.link;
        }
    }
}