using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Image rellenoBarraVida;
    private PlayerController playerController;

    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        rellenoBarraVida.fillAmount =
            playerController.vida / playerController.vidaMaxima;
    }
}
