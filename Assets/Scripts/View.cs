using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour {
    public float minZoom = 5f;
    public float maxZoom = 15f;
    
    private Camera _camera;
    private Vector2? _mouseGrabPosition;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _camera = GetComponent<Camera>();
    }

    private void LateUpdate() {
        if (!Map.Current || Controller.Current.state != Controller.GameState.Running) return;
        
        var mousePosition = (Vector2)Input.mousePosition;
        if (Input.GetMouseButtonDown(1) && _mouseGrabPosition == null)
        {
            _mouseGrabPosition = _camera.ScreenToWorldPoint(mousePosition);
        }
        if (Input.GetMouseButtonUp(1)) _mouseGrabPosition = null;

        if (_mouseGrabPosition != null && Input.GetMouseButton(1))
        {
            var mouseWorldTranslation = (Vector2)transform.position - (Vector2)_camera.ScreenToWorldPoint(mousePosition);
            var position = (Vector2)_mouseGrabPosition + mouseWorldTranslation;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        var scale = (Map.Current._width / 2) * Map.Current._size;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -scale, scale),
            Mathf.Clamp(transform.position.y, -scale, scale), transform.position.z);

        var zoom = Mathf.Clamp(_camera.orthographicSize - Input.mouseScrollDelta.y * 2f, minZoom, maxZoom);
        _camera.orthographicSize = zoom;
    }
}
