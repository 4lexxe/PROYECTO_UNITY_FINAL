using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button reiniciarBoton;
    public Button menuBoton;

    private bool gameOverActivo = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (reiniciarBoton != null)
            reiniciarBoton.onClick.AddListener(ReiniciarEscena);

        if (menuBoton != null)
            menuBoton.onClick.AddListener(IrAlMenu);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOverActivo)
        {
            if(Input.GetKeyUp(KeyCode.R)) 
            {
                ReiniciarEscena();
            }
            if(Input.GetKeyUp(KeyCode.Escape)) 
            {
                IrAlMenu();
            }
        }
    }

    public void GameOver()
    {
        if (gameOverActivo) return;

        gameOverActivo=true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\n \nR - Reiniciar\nESC - Menu Principal";
        }
    }

    public void ReiniciarEscena()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
}
