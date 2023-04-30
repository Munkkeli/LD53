using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryCar : Car {
  public House house;
  public bool _isAtTarget;

  protected override void Update() {
    if (Vector2.Distance(transform.position, house.Position) < 3f && !_isAtTarget) {
      _isAtTarget = true;
      _lastDirection = _direction;
      _lastPosition = _target;
      _direction = (_lastPosition - house.Position).normalized;
      _distance = 0f;
    }

    base.Update();
  }

  protected override Vector2 CalculateTarget() {
    if (_isAtTarget) return house.Position;
    return base.CalculateTarget();
  }

  protected override void CompleteMove() {
    if (_isAtTarget) {
      Map.Current.DeleteCar(this);
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
