#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using UnityEngine;
using Random = System.Random;

public class Map : MonoBehaviour
{
  public static Map Current;

  public enum Direction {
    UP,
    DOWN,
    LEFT,
    RIGHT,
  }

  public class Road {
    public Road[] Neighbors;
    public Vector3 Position;
    public bool IsEnd = false;
    public bool IsIntersection = false;

    public Dictionary<Direction, List<Car>> _carsInDirection = new() {
      { Direction.UP, new List<Car>() },
      { Direction.DOWN, new List<Car>() },
      { Direction.LEFT, new List<Car>() },
      { Direction.RIGHT, new List<Car>() },
    };
    
    public Dictionary<Direction, bool> LightState = new() {
      { Direction.UP, true },
      { Direction.DOWN, true },
      { Direction.LEFT, true },
      { Direction.RIGHT, true },
    };

    public Dictionary<House, float> DistanceToHouse = new();

    public void SetNeighbors(Road[] neighbors) {
      Neighbors = neighbors;
    }

    public void AddCar(Car car, Vector2 direction) {
      _carsInDirection[Vector2ToDirection(direction)].Add(car);
    }
    
    public bool CheckCarSpace(Vector2 direction, bool strict = false) {
      var _direction = Vector2ToDirection(direction);
      if (!LightState[_direction]) return false;
      if (Neighbors.Length > 2) {
        var count = _carsInDirection[Direction.UP].Count + _carsInDirection[Direction.DOWN].Count +
          _carsInDirection[Direction.LEFT].Count + _carsInDirection[Direction.RIGHT].Count;
        return count < 1;
      }

      return _carsInDirection[_direction].Count <= (strict ? 0 : 2);
    }

    public void RemoveCar(Car car) {
      _carsInDirection[Direction.UP].Remove(car);
      _carsInDirection[Direction.DOWN].Remove(car);
      _carsInDirection[Direction.LEFT].Remove(car);
      _carsInDirection[Direction.RIGHT].Remove(car);
    }

    public override bool Equals(object obj) {
      if (!(obj is Road)) return false;
      return Position == ((Road)obj).Position;
    }
  }

  private int _size;
  private int _width;
  private int _height;
  private Grid _grid;
  private Tilemap _tilemap;
  private TileBase[] tiles;
  private Road?[,] _roads;
  private End[] _ends;
  private House[] _houses;
  private Random _random = new Random();

  public GameObject End;
  public GameObject Intersection;
  public GameObject House;
  
  private void Awake() {
    Current = this;

    _grid = GetComponentInParent<Grid>();
    _tilemap = GetComponent<Tilemap>();
    _size = (int)_tilemap.cellSize.x;

    BoundsInt bounds = _tilemap.cellBounds;
    tiles = _tilemap.GetTilesBlock(bounds);
    _width = bounds.size.x;
    _height = bounds.size.y;

    _roads = new Road?[bounds.size.x, bounds.size.y];
    var ends = new List<End>();
    var houses = new List<House>();

    for (int x = 0; x < bounds.size.x; x++)
    {
      for (int y = 0; y < bounds.size.y; y++)
      {
        TileBase tile = tiles[x + y * bounds.size.x];
        if (tile == null) {
          _roads[x, y] = null;
          continue;
        }
        
        if (!tile.name.StartsWith("grass") && !tile.name.StartsWith("house")) {
          Road road = new Road();
          road.Position = MapToWorld(x, y);
          _roads[x, y] = road;

          if (tile.name == "end") {
            var obj = Instantiate(End, road.Position, Quaternion.identity);
            ends.Add(obj.GetComponent<End>());
            road.IsEnd = true;
          }
        }

        if (tile.name.StartsWith("house")) {
          var obj = Instantiate(House, MapToWorld(x, y), Quaternion.identity);
          var house = obj.GetComponent<House>();
          house.Position = MapToWorld(x, y);
          houses.Add(house);
        }
      }
    }

    _ends = ends.ToArray();
    _houses = houses.ToArray();

    for (int x = 0; x < bounds.size.x; x++) {
      for (int y = 0; y < bounds.size.y; y++) {
        var road = GetRoad(x, y);
        if (road == null) continue;

        var top = GetRoad(x, y + 1);
        var bottom = GetRoad(x, y - 1);
        var left = GetRoad(x - 1, y);
        var right = GetRoad(x + 1, y);
        var neighbors = new List<Road?>() {
          top, bottom, left, right
        };

        road.SetNeighbors(neighbors.OfType<Road>().ToArray());

        if (road.Neighbors.Length > 2) {
          var intersection = Instantiate(Intersection, road.Position, Quaternion.identity);
          intersection.GetComponent<Intersection>().Road = road;
          road.IsIntersection = true;
        }
      }
    }

    foreach (var house in _houses) {
      foreach (var road in _roads) {
        if (road == null) continue;
        road.DistanceToHouse.Add(house, Vector2.Distance(house.Position, road.Position));
      }
    }
  }

  private void OnDrawGizmos() {
    for (int x = 0; x < _width; x++) {
      for (int y = 0; y < _height; y++) {
        var road = _roads[x, y];
        if (road == null) continue;

        Gizmos.DrawWireCube(road.Position, new Vector3(_size, _size));
      }
    }
  }

  public Vector3 MapToWorld(int x, int y) {
    var offset = new Vector2(_width, _height);
    return (Vector2)_grid.GetCellCenterWorld(new Vector3Int(x, y)) - offset;
  }
  
  public Vector3 WorldToMap(Vector3 position) {
    var offset = new Vector3(_width, _height);
    return _grid.WorldToCell(position + offset);
  }

  private TileBase? GetTile(int x, int y) {
    var tile = _tilemap.GetTile(new Vector3Int(x, y, 0));
    return tile.name == "grass" ? null : tile;
  }
  
  public Road? GetRoad(int x, int y) {
    if (x < 0 || x >= _width) return null;
    if (y < 0 || y >= _height) return null;
    return _roads[x, y];
  }

  public End? GetRandomFreeEnd() {
    return _ends.ElementAt(_random.Next(_ends.Length));
  }
  
  public House GetRandomHouse() {
    return _houses.ElementAt(_random.Next(_houses.Length));
  }

  public Road? GetRoadFromPosition(Vector3 position) {
    var local = WorldToMap(position);
    var x = (int)Mathf.Clamp(local.x, 0, _width);
    var y = (int)Mathf.Clamp(local.y, 0, _height);
    return _roads[x, y];
  }

  public void DeleteCar(Car car) {
    foreach (var road in _roads) {
      if (road != null) road.RemoveCar(car);
      Destroy(car.gameObject);
    }
  }

  public static Direction Vector2ToDirection(Vector2 direction) {
    if (direction.y > 0) return Direction.UP;
    if (direction.y < 0) return Direction.DOWN;
    if (direction.x > 0) return Direction.RIGHT;
    if (direction.x < 0) return Direction.LEFT;
    return Direction.UP;
  }
}
