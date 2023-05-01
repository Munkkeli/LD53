using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class Car : MonoBehaviour {
    public Map.Road Road;
    public Map.Road _lastRoad;
    public Map.Road _nextRoad;
    public Map.Road _nextNextRoad;
    public Vector2 _direction;
    public Vector2 _lastDirection;
    public Vector2 _lastPosition;
    protected bool _firstMove = true;
    protected bool _canMove = true;
    protected float _distance = 0;
    protected Vector2 _target;
    protected Random _random = new Random();
    private Vector3 _rotationVel;

    public Color[] colors;
    protected Color? _color;
    
    public float Speed = 1f;
    public Transform Trigger;
    public LayerMask collisionMask;

    public Transform sprite;

    protected virtual void Start() {
        var road = Map.Current.GetRoadFromPosition(transform.position);
        if (road == null) {
            Destroy(gameObject);
            return;
        }
        this.Road = road;
        _lastRoad = this.Road;
        transform.position = this.Road.Position;

        _nextRoad = road;
        _nextNextRoad = PickNextRoad(Road);
        _direction = (_nextNextRoad.Position - Road.Position).normalized;
        _target = CalculateTarget();
        CompleteMove();
        // _distance = 1f;
        _firstMove = true;
        
        var sprite = GetComponentInChildren<SpriteRenderer>();
        sprite.color = _color ?? colors[_random.Next(colors.Length)];
    }

    protected virtual void Update() {
        _target = CalculateTarget();
        
        if (_canMove) _distance += Speed * Time.deltaTime;
        var (position, rotation) = (_direction == _lastDirection)
            ? LerpForward(_lastPosition, _target)
            : LerpCorner(_lastPosition, _target, _lastDirection);
        
        var smoothRotation = Quaternion.RotateTowards(transform.rotation, rotation, 200f * Time.deltaTime);

        transform.position = position;
        transform.rotation = smoothRotation;
        
        if (_distance >= 1f) {
            if (!_nextRoad.CheckCarSpace(_direction)) return;
            var _nextDirection = (_nextNextRoad.Position - _nextRoad.Position).normalized;
            if (_nextRoad.IsIntersection && !_nextNextRoad.CheckCarSpace(_nextDirection, true)) return;
            CompleteMove();
        }

        if (Road.IsEnd && !_firstMove) {
            Map.Current.DeleteCar(this);
        }
    }

    private void FixedUpdate() {
        var hit = Physics2D.OverlapCircle(Trigger.position, 0.1f, collisionMask);
        _canMove = !hit;
    }

    private Quaternion LookTowards(Vector2 current, Vector2 target) {
        var rotatedVectorToTarget = Quaternion.Euler(0, 0, 0) * (target - current).normalized;
        return Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);
    }

    private (Vector2, Quaternion) LerpForward(Vector2 one, Vector2 two) {
        var position = Vector2.Lerp(one, two, _distance);
        var rotation = LookTowards(one, two);
        return (position, rotation);
    }

    private (Vector2, Quaternion) LerpCorner(Vector2 one, Vector2 two, Vector2 direction) {
        var a1 = two - one;
        var corner = one + new Vector2(Mathf.Abs(a1.x), Mathf.Abs(a1.y)) * direction;
        var m1 = Vector2.Lerp( one, corner, _distance);
        var m2 = Vector2.Lerp( corner, two, _distance);
        var position = Vector2.Lerp(m1, m2, _distance);
        var rotation = LookTowards(position, m2);
        return (position, rotation);
    }

    protected virtual Vector2 CalculateTarget() {
        return (Vector2)Road.Position + (_direction * 0.8f) + (new Vector2(0.4f, 0.4f) * (Quaternion.Euler(0, 0, -90) * _direction));
    }

    protected virtual void CompleteMove() {
        Road.RemoveCar(this);
        _lastRoad = Road;
        Road = _nextRoad;
        _nextRoad = _nextNextRoad;
        _nextNextRoad = PickNextRoad(_nextRoad, Road);
        _firstMove = false;

        _lastDirection = _direction;
        _lastPosition = _target;
        _direction = (_nextRoad.Position - Road.Position).normalized;
        
        Road.AddCar(this, _direction);
        _distance = 0;
    }

    protected virtual Map.Road PickNextRoad(Map.Road road, Map.Road fallback = null) {
        var neighbors =
            road.Neighbors.Where(item => !item.Equals(road) && !item.Equals(_lastRoad) && !item.Equals(Road));
        if (!neighbors.Any()) return fallback ?? _lastRoad;
        return neighbors.ElementAt(_random.Next(neighbors.Count()));
    }

    private void OnDrawGizmos() {
        if (!Map.Current) return;
        Gizmos.color = Color.red;
        var local = Map.Current.WorldToMap(transform.position);
        // Gizmos.DrawCube( Map.Current.MapToWorld((int)local.x, (int)local.y), new Vector3(2, 2));
        Gizmos.DrawWireSphere(Trigger.position, 0.2f);
        
        Gizmos.color = Color.cyan;
        var a1 = _target - _lastPosition;
        var corner = _lastPosition + new Vector2(Mathf.Abs(a1.x), Mathf.Abs(a1.y)) * _lastDirection;
        // Gizmos.DrawSphere(corner, 0.02f);
        Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(_lastPosition, 0.02f);
        // Gizmos.DrawWireSphere(_target, 0.02f);
        
        
        Gizmos.color = Color.green;
        // Gizmos.DrawCube( _nextRoad.Position, new Vector3(2, 2));
        Gizmos.color = Color.blue;
        // Gizmos.DrawCube( _nextNextRoad.Position, new Vector3(2, 2));
        foreach (var neighbor in Road.Neighbors) {
            // Gizmos.DrawCube( neighbor.Position, new Vector3(2, 2));
        }
    }
}
