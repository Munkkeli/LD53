using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : MonoBehaviour {
    public SpriteRenderer red;
    public SpriteRenderer green;
    public Color offColor;
    private Color _redColor;
    private Color _greenColor;
    private bool _state = true;

    public bool State {
        get => _state;
        set {
            if (value == _state) return;
            red.color = !value ? _redColor : offColor;
            green.color = value ? _greenColor : offColor;
            _state = value;
        }
    }

    void Awake() {
        _redColor = red.color;
        _greenColor = green.color;
        State = false;
    }
    
    void Update()
    {
        
    }
}
