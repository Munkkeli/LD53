using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class End : MonoBehaviour {
    public float Interval = 5f;
    public int AmountOfDeliveriesSpawned;
    
    private float _timer;

    private void Update() {
        if (_timer <= 0) {
            _timer = Interval + (Interval * Random.value);
            
            var hit = Physics2D.OverlapBox(transform.position, new Vector2(1, 1), 0);
            if (hit) return;

            // Try to spawn deliveries into different ends each time
            if (!Controller.Current._spawnQueue.TryPeek(out var carTypePeek)) return;
            if (carTypePeek == Controller.CarType.Delivery &&
                AmountOfDeliveriesSpawned > Map.Current.GetDeliveriesPerEndAverage()) {
                return;
            }

            if (!Controller.Current._spawnQueue.TryDequeue(out var carType)) return;
            
            Instantiate(Controller.Current.CarTypeToPrefab[carType], transform.position, Quaternion.identity);

            if (carType == Controller.CarType.Delivery) {
                AmountOfDeliveriesSpawned++;
            }
        }

        _timer -= Time.deltaTime;
    }
}
