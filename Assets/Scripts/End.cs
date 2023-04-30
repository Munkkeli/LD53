using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class End : MonoBehaviour {
    public GameObject Car;

    public float Interval = 5f;

    private float _timer;
    
    // Start is called before the first frame update
    void Start() {
        _timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (_timer <= 0) {
            var hit = Physics2D.OverlapBox(transform.position, new Vector2(1, 1), 0);
            if (hit) return;
            Instantiate(Car, transform.position, Quaternion.identity);
            _timer = Interval + (Interval * Random.value);
        }

        _timer -= Time.deltaTime;
    }
}
