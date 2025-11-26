using UnityEngine;

public class EnemyFloat : MonoBehaviour
{
    public float floatAmplitude = 0.3f; // cuánta altura sube/baja
    public float floatSpeed = 2f;      // qué tan rápido flota

    private float originalY;

    void Start()
    {
        originalY = transform.position.y;
    }

    void Update()
    {
        float newY = originalY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
