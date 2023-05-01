using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour {
    public GameObject Label;
    public Vector2 Position;
    public Color color;
    public string letter;

    private RectTransform _label;

    private void Start() {
        (letter, color) = Controller.Current.GetNextHouseBranding();
        
        var obj = Instantiate(Label, Vector3.zero, Quaternion.identity);
        _label = obj.GetComponent<RectTransform>();
        _label.SetParent(Controller.Current.Canvas);
        _label.GetComponentInChildren<Text>().text = letter;
        foreach (var image in _label.GetComponentsInChildren<Image>()) {
            image.color = color;
        }
    }

    private void Update() {
        var position = Camera.main.WorldToScreenPoint(Position);
        _label.position = new Vector2(position.x, position.y);
        
        var visible = Controller.Current.state == Controller.GameState.Running;
        _label.gameObject.SetActive(visible);
    }

    private void OnDestroy() {
        Destroy(_label.gameObject);
    }
}
