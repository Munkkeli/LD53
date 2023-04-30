using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
    public static Controller Current;
    
    public GameObject DeliveryCar;

    private float _timer;

    private void Awake() {
        Current = this;
    }

    private void Start() {
        
    }

    private void Update() {
        if (_timer <= 0) {
            var end = Map.Current.GetRandomFreeEnd();
            var house = Map.Current.GetRandomHouse();
            if (end) {
                var obj = Instantiate(DeliveryCar, end.transform.position, Quaternion.identity);
                var car = obj.GetComponent<DeliveryCar>();
                car.house = house;
            }

            _timer = 10f;
        }
        _timer -= Time.deltaTime;
    }
}
