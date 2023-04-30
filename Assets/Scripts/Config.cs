using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour {
    public static Config Current;
    
    public AnimationCurve spawnRateOverTime;
    public AnimationCurve deliverySpawnRateOverTime;
    public float estimatedLevelDurationInMinutes = 2f;
    public float maximumSpawnRateInMinute = 80f;
    public int maximumDeliveryDelay = 40;
    
    public int currentDeliveries;
    public int targetDeliveries = 10;

    private void Awake() {
        Current = this;
    }
}
