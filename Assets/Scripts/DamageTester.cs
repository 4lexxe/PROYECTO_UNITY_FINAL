using UnityEngine;

public class DamageTester : MonoBehaviour
{
    public float intervaloDanio = 1f;
    public float cantidadDanio = 1f;

    private float tiempo;
    private PlayerController pc;

    void Start()
    {

    }

    void Update()
    {
        tiempo += Time.deltaTime;

        if (tiempo >= intervaloDanio)
        {
            pc.RecibirDanio(cantidadDanio);
            tiempo = 0f;
        }
    }
}
