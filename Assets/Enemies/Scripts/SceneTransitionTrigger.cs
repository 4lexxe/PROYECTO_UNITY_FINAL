using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    public string sceneName;
    public int sceneBuildIndex = -1;
    public bool useBuildIndex = false;
    public bool autoIsTrigger = true;
    public string playerTag = "Player";
    public bool useAsync = false;
    void Awake()
    {
        var c2d = GetComponent<Collider2D>();
        if (autoIsTrigger && c2d != null) c2d.isTrigger = true;
    }
    void OnValidate()
    {
        var c2d = GetComponent<Collider2D>();
        if (autoIsTrigger && c2d != null) c2d.isTrigger = true;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        Load();
    }
    void Load()
    {
        if (useBuildIndex)
        {
            int count = SceneManager.sceneCountInBuildSettings;
            if (sceneBuildIndex >= 0 && sceneBuildIndex < count)
            {
                SceneManager.sceneLoaded += OnSceneLoadedResetPlayer;
                if (useAsync) SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Single);
                else SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
            }
            else
            {
                if (!string.IsNullOrEmpty(sceneName) && IsSceneInBuild(sceneName))
                {
                    SceneManager.sceneLoaded += OnSceneLoadedResetPlayer;
                    if (useAsync) SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                    else SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                }
                else
                {
                    Debug.LogError($"SceneTransitionTrigger: índice {sceneBuildIndex} fuera de rango y escena '{sceneName}' no válida. Añade la escena a Build Settings o configura 'sceneName'.");
                }
            }
            return;
        }
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (IsSceneInBuild(sceneName))
            {
                SceneManager.sceneLoaded += OnSceneLoadedResetPlayer;
                if (useAsync) SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                else SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError($"SceneTransitionTrigger: la escena '{sceneName}' no está en Build Settings. Usa File > Build Settings > Add Open Scenes.");
            }
        }
    }
    void OnSceneLoadedResetPlayer(Scene s, LoadSceneMode m)
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
        {
            var pc = p.GetComponent<PlayerController>();
            if (pc == null) pc = p.GetComponentInParent<PlayerController>();
            if (pc != null) pc.vida = pc.vidaMaxima;
        }
        SceneManager.sceneLoaded -= OnSceneLoadedResetPlayer;
    }
    bool IsSceneInBuild(string name)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string bn = System.IO.Path.GetFileNameWithoutExtension(path);
            if (bn == name) return true;
        }
        return false;
    }
}
