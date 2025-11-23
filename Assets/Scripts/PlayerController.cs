using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float vidaMaxima = 5f;
    public float vida;

    void Start()
    {
        vida = vidaMaxima;
    }

    public void RecibirDanio(float cantidad)
    {
        vida -= cantidad;
        vida = Mathf.Clamp(vida, 0, vidaMaxima);
    }
}
