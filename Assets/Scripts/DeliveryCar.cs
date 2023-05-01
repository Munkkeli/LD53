using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryCar : Car {
  public House house;
  public bool _isAtTarget;
  public int DeliveryLayer;
  public GameObject Label;

  public RectTransform _label;
  public Text _timeText;
  private float _timeLimit;

  protected override void Start() {
    house = Map.Current.GetFarthestHouse(transform.position);

    _timeLimit = 13 + Mathf.Round(Vector2.Distance(transform.position, house.Position) * 1.1f);
    
    var obj = Instantiate(Label, Vector3.zero, Quaternion.identity);
    _label = obj.GetComponent<RectTransform>();
    _label.SetParent(Controller.Current.Canvas);
    _color = house.color;
    _timeText = _label.Find("time").GetComponentInChildren<Text>();
    
    base.Start();
  }

  protected override void Update() {
    if (Vector2.Distance(transform.position, house.Position) < 3f && !_isAtTarget) {
      _isAtTarget = true;
      _lastDirection = _direction;
      _lastPosition = _target;
      _direction = (_lastPosition - house.Position).normalized;
      _distance = 0f;
      gameObject.layer = DeliveryLayer;
      foreach (Transform child in transform)
      {
        child.gameObject.layer = DeliveryLayer;
      }
    }
    
    var position = Camera.main.WorldToScreenPoint(transform.position);
    _label.position = new Vector2(position.x, position.y + 32f);
    
    var mouse = Input.mousePosition;
    var visible = Vector2.Distance(mouse, _label.position) > 100 &&
                  Controller.Current.state == Controller.GameState.Running;
    _label.gameObject.SetActive(visible);

    _label.GetComponentInChildren<Text>().text = house.letter;
    foreach (var image in _label.GetComponentsInChildren<Image>()) {
      if (image.gameObject.name != "color") continue;
      image.color = house.color;
    }

    _timeText.text = $"{(int)_timeLimit}s";
    _timeLimit -= Time.deltaTime;

    if (_timeLimit <= 0 && Controller.Current.state == Controller.GameState.Running) {
      Controller.Current.state = Controller.GameState.End;
      // Map.Current.DeleteCar(this);
      return;
    }

    base.Update();
  }

  private void OnDestroy() {
    Destroy(_label.gameObject);
  }

  protected override Vector2 CalculateTarget() {
    if (_isAtTarget) return house.Position;
    return base.CalculateTarget();
  }

  protected override void CompleteMove() {
    if (_isAtTarget) {
      Map.Current.DeleteCar(this);
      if (Controller.Current.state == Controller.GameState.Running) {
        Config.Current.currentDeliveries++;
      }
      return;
    }

    base.CompleteMove();
  }

  protected override Map.Road PickNextRoad(Map.Road road, Map.Road fallback = null) {
    var neighbors =
      road.Neighbors.Where(item => !item.Equals(road) && !item.Equals(_lastRoad) && !item.Equals(Road))
        .OrderBy(item => item.DistanceToHouse[house]);
    if (!neighbors.Any()) return fallback ?? _lastRoad;
    return neighbors.First();
  }
}
