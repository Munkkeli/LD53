using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

public class Intersection : MonoBehaviour {
    public Map.Road Road;
    public GameObject Light;
    public GameObject SwitchButton;
    
    private Dictionary<Map.Direction, Light> _lights = new();

    private bool _lightDirection = false;
    private bool _switching = false;
    private bool _request = false;
    private float _timer = 0;

    private void Start() {
        var button = Instantiate(SwitchButton, transform.position, Quaternion.identity);
        button.GetComponent<Change>().Intersection = this;
        
        foreach (var neighbor in Road.Neighbors) {
            var _direction = (neighbor.Position - Road.Position).normalized;
            var light = Instantiate(Light, Road.Position + _direction, Quaternion.identity);
            _lights.Add(Map.Vector2ToDirection(_direction), light.GetComponent<Light>());
        }
    }

    private void Update()
    {
        if (_timer <= 0) {
            if (_request && !_switching) {
                _switching = true;
            } else if (_switching) {
                _lightDirection = !_lightDirection;
                _switching = false;
                _request = false;
            }

            var s = !_switching;
            if (_lights.TryGetValue(Map.Direction.UP, out var up)) up.State = s && _lightDirection;
            if (_lights.TryGetValue(Map.Direction.DOWN, out var down)) down.State = s && _lightDirection;
            if (_lights.TryGetValue(Map.Direction.LEFT, out var left)) left.State = s && !_lightDirection;
            if (_lights.TryGetValue(Map.Direction.RIGHT, out var right)) right.State = s && !_lightDirection;
            Road.LightState[Map.Direction.UP] = s && _lightDirection;
            Road.LightState[Map.Direction.DOWN] = s && _lightDirection;
            Road.LightState[Map.Direction.LEFT] = s && !_lightDirection;
            Road.LightState[Map.Direction.RIGHT] = s && !_lightDirection;
            _timer = 1.5f;
        }

        if (_switching || _request) _timer -= Time.deltaTime;
    }

    public void Switch() {
        _request = true;
        _timer = 0;
    }
}
