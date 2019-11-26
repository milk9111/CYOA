using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Domain
{
    public class LineHolder : MonoBehaviour
    {
        public GameObject directionIndicator;
        public GameObject selfIndicator;
        public bool onlyUpdateOnce;
        [ReadOnly]
        public string lineName;

        [ReadOnly]
        public int direction;

        private LineRenderer _lineRenderer;
        public GameObject _indicatorLast;
        public GameObject _indicatorFirst;
        public GameObject _selfIndicator;
        private Transform _parent;
        private bool _hasUpdated;
        private bool _isIndicator1Enabled;
        private bool _isIndicator2Enabled;
        private bool _isSelfIndicatorEnabled;

        void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _indicatorLast = null;
            _indicatorFirst = null;
            _selfIndicator = null;
            _parent = GameObject.FindGameObjectWithTag("Graph").transform;
        }

        void LateUpdate()
        {
            if (_selfIndicator == null)
            {
                _selfIndicator = Instantiate(selfIndicator, _parent);
                _selfIndicator.gameObject.name = "self - " + lineName;
            }
            
            if (_indicatorLast == null)
            {
                _indicatorLast = Instantiate(directionIndicator, _parent);
                _indicatorLast.gameObject.name = "last - " + lineName;
            }

            if (_indicatorFirst == null)
            {
                _indicatorFirst = Instantiate(directionIndicator, _parent);
                _indicatorFirst.gameObject.name = "first - " + lineName;
            }
            
            _indicatorLast.SetActive(_isIndicator1Enabled);
            _indicatorFirst.SetActive(_isIndicator2Enabled);
            _selfIndicator.SetActive(_isSelfIndicatorEnabled);
            
            // I want to at least instantiate the indicators first
            if (onlyUpdateOnce && _hasUpdated)
            {
                return;
            }

            var size = _lineRenderer.positionCount;
            var firstPoint = _lineRenderer.GetPosition(0);
            var secondPoint = _lineRenderer.GetPosition(1);
            var lastPoint = _lineRenderer.GetPosition(size - 1);
            var pointBeforeLast = _lineRenderer.GetPosition(size - 2);

            var localPointLast = _indicatorLast.transform.InverseTransformPoint(lastPoint);
            var localPointBeforeLast = _indicatorLast.transform.InverseTransformPoint(pointBeforeLast);
            var localPointFirst = _indicatorFirst.transform.InverseTransformPoint(firstPoint);
            var localPointSecond = _indicatorFirst.transform.InverseTransformPoint(secondPoint);

            var localPointSelf = _selfIndicator.transform.InverseTransformPoint(firstPoint);
            
            //var xLength = (localPointLast.x - localPointFirst.x) / 4;
            //var yLength = (localPointLast.y - localPointFirst.y) / 4;
            
            _indicatorLast.transform.localPosition = new Vector3(localPointLast.x - CalculateXLength(localPointLast, localPointBeforeLast), 
                localPointLast.y - CalculateYLength(localPointLast, localPointBeforeLast), 0);
            _indicatorFirst.transform.localPosition = new Vector3(localPointFirst.x + CalculateXLength(localPointSecond, localPointFirst), 
                localPointFirst.y + CalculateYLength(localPointSecond, localPointFirst), 0);
            
            _selfIndicator.transform.localPosition = new Vector3(localPointSelf.x - 100, localPointSelf.y, 0);
            
            var lookAtPoint1 = (new Vector3(localPointLast.x, localPointLast.y, 0) - _indicatorLast.transform.localPosition).normalized;
            _indicatorLast.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(lookAtPoint1.y, lookAtPoint1.x) * Mathf.Rad2Deg);
            
            var lookAtPoint2 = (new Vector3(localPointFirst.x, localPointFirst.y, 0) - _indicatorFirst.transform.localPosition).normalized;
            _indicatorFirst.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(lookAtPoint2.y, lookAtPoint2.x) * Mathf.Rad2Deg);
            
            _hasUpdated = true;
        }

        private float CalculateXLength(Vector3 point1, Vector3 point2)
        {
            return (point1.x - point2.x) / 4;
        }
        
        private float CalculateYLength(Vector3 point1, Vector3 point2)
        {
            return (point1.y - point2.y) / 4;
        }

        public LineRenderer GetLineRenderer()
        {
            return _lineRenderer;
        }

        public IList<Vector3> GetPositions()
        {
            var pos = new List<Vector3>();
            for (var i = 0; i < _lineRenderer.positionCount; i++)
            {
                pos.Add(_lineRenderer.GetPosition(i));
            }

            return pos;
        }
        
        public void SetPositionCount(int positionCount)
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.GetComponent<LineRenderer>();
            }
            _lineRenderer.positionCount = positionCount;
        }

        public void SetPositions(Vector3[] positions)
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.GetComponent<LineRenderer>();
            }
            _lineRenderer.SetPositions(positions);
        }

        public void SetIsIndicatorEnabled(int index, bool isEnabled)
        {
            switch (index)
            {
                case 0:
                    _isIndicator1Enabled = isEnabled;
                    break;
                case 1:
                    _isIndicator2Enabled = isEnabled;
                    break;
                case 2:
                    _isSelfIndicatorEnabled = isEnabled;
                    break;
            }
        }

        public void Destroy()
        {
            Destroy(_indicatorLast);
            Destroy(_indicatorFirst);
            Destroy(_selfIndicator);
        }
    }
}
