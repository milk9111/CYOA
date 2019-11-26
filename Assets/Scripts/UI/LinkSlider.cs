using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LinkSlider : MonoBehaviour
    {
        public Text count;

        private Slider _slider;
        private NodeEditorManager _nodeEditorManager;

        private bool _hasStarted;

        void Start()
        {
            Init();
        }

        void Init()
        {
            _slider = GetComponent<Slider>();
            _nodeEditorManager = FindObjectOfType<NodeEditorManager>();
            _hasStarted = false;
        }

        void Update()
        {
            if (!_hasStarted)
            {
                UpdateCount();
                Confirm();
                _hasStarted = true;
            }
        }

        public void UpdateCount()
        {
            count.text =  _slider.value + "";
        }

        public void Confirm()
        {
            _nodeEditorManager.UpdateSize((int)_slider.value);   
        }

        public void Reset()
        {
            _slider.value = 0;
            UpdateCount();
            Confirm();
        }

        public void SetValue(int value)
        {
            if (_slider == null)
            {
                Init();
            }
            _slider.value = value;
        }
    }
}
