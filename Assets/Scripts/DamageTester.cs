using UnityEngine;

public class DamageTester : MonoBehaviour
{
    public float intervaloDanio = 1f;
    public float cantidadDanio = 1f;
    public bool autoDamage = false;

    private float tiempo;
    private PlayerController pc;

    void Start()
    {
        pc = Object.FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (!autoDamage) return;
        tiempo += Time.deltaTime;

        if (tiempo >= intervaloDanio)
        {
            pc.RecibirDanio(cantidadDanio);
            tiempo = 0f;
        }
    }
}
