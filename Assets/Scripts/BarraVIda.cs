using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Image rellenoBarraVida;
    private PlayerController playerController;

    void Start()
    {
    }

    void Update()
    {
        rellenoBarraVida.fillAmount =
            playerController.vida / playerController.vidaMaxima;
    }
}
