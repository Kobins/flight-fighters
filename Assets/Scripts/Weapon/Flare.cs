using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

public class Flare : MonoBehaviour {
    
    public static readonly Dictionary<int, Flare> Flares = new Dictionary<int, Flare>();

    [CanBeNull]
    public static Flare GetOrNull(Object obj) {
        var id = obj.GetInstanceID();
        return Flares.TryGetValue(id, out var flare) ? flare : null;
    }
    
    public float force = 10f;
    public float lifetime = 20f;

    private Rigidbody _rigidbody;

    public FlareLauncher Launcher { get; private set; }
    public ArmoredVehicle Shooter { get; private set; }

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Init(FlareLauncher launcher, ArmoredVehicle shooter, Vector3 direction) {
        Launcher = launcher;
        Shooter = shooter;
        var vehicleVelocity = Vector3.zero;
        if(shooter is Aeroplane aeroplane) {
            vehicleVelocity = aeroplane.rigidbody.velocity;
        }
        _rigidbody.AddForce(direction * force + vehicleVelocity, ForceMode.VelocityChange);

        Flares.Add(GetInstanceID(), this);
        Destroy(gameObject, lifetime);
    }

    private void Update() {
        transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
    }

    private void OnDisable() {
        Flares.Remove(GetInstanceID());
    }
}