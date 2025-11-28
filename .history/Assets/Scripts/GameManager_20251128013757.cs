using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button reiniciarBoton;
    public Button menuBoton;
    public GameObject panelVictoria;
    public bool applyVictoryEnvironment = true;
    public Material victorySkybox;
    public Color victoryAmbientLight = new Color(0.9f, 0.9f, 0.9f, 1f);
    public float victoryDirectionalIntensity = 1.2f;
    public Vector3 victoryLightEuler = new Vector3(50f, -30f, 0f);
    public Color victoryDirectionalColor = Color.white;
    public bool fadeVictoryLight = true;
    public float victoryLightFadeSeconds = 1.5f;
    public bool showVictoryPanel = true;
    public float victoryPanelDelaySeconds = 15f;
    private bool victoryApplied = false;

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

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

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
    public void Victory()
    {
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (applyVictoryEnvironment && !victoryApplied)
        {
            ApplyVictoryEnvironment();
            victoryApplied = true;
        }
        if (showVictoryPanel)
        {
            StartCoroutine(ShowVictoryPanelDelayed());
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

    public bool IsGameOver()
    {
        return gameOverActivo;
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

    void ApplyVictoryEnvironment()
    {
        if (victorySkybox != null)
        {
            RenderSettings.skybox = victorySkybox;
        }
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = victoryAmbientLight;
        var lights = Object.FindObjectsOfType<Light>(true);
        Light dir = null;
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].type == LightType.Directional)
            {
                dir = lights[i];
                break;
            }
        }
        if (dir == null)
        {
            var go = new GameObject("VictoryDirectionalLight");
            dir = go.AddComponent<Light>();
            dir.type = LightType.Directional;
        }
        if (fadeVictoryLight)
        {
            StartCoroutine(FadeLight(dir, victoryDirectionalColor, victoryDirectionalIntensity, victoryLightFadeSeconds));
        }
        else
        {
            dir.intensity = victoryDirectionalIntensity;
            dir.color = victoryDirectionalColor;
        }
        dir.shadows = LightShadows.None;
        dir.transform.rotation = Quaternion.Euler(victoryLightEuler);
    }

    IEnumerator FadeLight(Light l, Color toColor, float toIntensity, float seconds)
    {
        if (l == null) yield break;
        seconds = Mathf.Max(0f, seconds);
        Color fromColor = l.color;
        float fromIntensity = l.intensity;
        if (seconds <= 0f)
        {
            l.color = toColor;
            l.intensity = toIntensity;
            yield break;
        }
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = seconds > 0f ? (t / seconds) : 1f;
            l.color = Color.Lerp(fromColor, toColor, k);
            l.intensity = Mathf.Lerp(fromIntensity, toIntensity, k);
            yield return null;
        }
        l.color = toColor;
        l.intensity = toIntensity;
    }

    IEnumerator ShowVictoryPanelDelayed()
    {
        float d = Mathf.Max(0f, victoryPanelDelaySeconds);
        yield return new WaitForSeconds(d);
        if (panelVictoria != null) panelVictoria.SetActive(true);
    }
}
