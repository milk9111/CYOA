using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ViewInputManager : MonoBehaviour
{
    public float glideSpeed = 2f;
    public float zoomSpeed = 2f;
    public int zoomMin = 30;
    public int zoomMax = 80;
    public bool isPaused;
    
    private Vector3 _lastPosition;
    private bool _isFirstRun = true;
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }
    
    void Update()
    {        
        if (isPaused)
        {
            return;
        }
        
        if (Input.GetMouseButton(0) && EventSystem.current.currentSelectedGameObject == null)
        {
            MoveCamera();
        }

        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Math.Abs(scroll) > 0)
        {
            ZoomCamera(scroll);
        }
    }

    private void ZoomCamera(float scroll)
    {
        _mainCamera.fieldOfView = Math.Min(zoomMax, Math.Max(zoomMin, _mainCamera.fieldOfView + scroll * -zoomSpeed));
    }

    private void MoveCamera()
    {
        if (_isFirstRun)
        {
            _lastPosition = Input.mousePosition;
            _isFirstRun = false;
        }
        
        var dir = (_lastPosition - Input.mousePosition).normalized;
        
       _mainCamera.transform.Translate(dir * glideSpeed);
        
        _lastPosition = Input.mousePosition;
    }

    public void SetPause(bool pause)
    {
        isPaused = pause;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public void ExitEditor()
    {
        SceneManager.LoadScene("start_screen");
    }
}
