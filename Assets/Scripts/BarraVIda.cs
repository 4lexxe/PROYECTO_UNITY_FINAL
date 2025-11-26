using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Image rellenoBarraVida;
    private PlayerController playerController;

    void Start()
    {
        if (rellenoBarraVida == null)
        {
            rellenoBarraVida = GetComponent<Image>();
            if (rellenoBarraVida == null) rellenoBarraVida = GetComponentInChildren<Image>();
        }
        if (playerController == null) playerController = Object.FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (playerController == null)
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo != null) playerController = pgo.GetComponent<PlayerController>();
        }
        if (playerController == null || rellenoBarraVida == null) return;
        float ratio = 0f;
        if (playerController.vidaMaxima > 0f) ratio = playerController.vida / playerController.vidaMaxima;
        rellenoBarraVida.fillAmount = Mathf.Clamp01(ratio);
    }
}
