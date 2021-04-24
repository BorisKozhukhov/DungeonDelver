using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum EType
    {
        key,
        health,
        grappler
    }

    public static float COLLIDER_DELAY = 0.5f;

    [Header("Set in Inspector")]
    public EType itemType;

    // Awake и Activate деактивируют коллайдер на 0.5 секунды
    private void Awake()
    {
        GetComponent<Collider>().enabled = false;
        Invoke("Activate", COLLIDER_DELAY);
    }

    void Activate() => GetComponent<Collider>().enabled = true;
}
