using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionTrigger : MonoBehaviour
{
    public string sceneName;
    public int sceneBuildIndex = -1;
    public bool useBuildIndex = false;
    public bool autoIsTrigger = true;
    public string playerTag = "Player";
    public bool useAsync = false;
    public bool useOverlayTransition = true;
    public Color overlayColor = new Color(0f, 0f, 0f, 1f);
    public float overlayFadeInSeconds = 0.6f;
    public float overlayFadeOutSeconds = 0.6f;
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
        if (useOverlayTransition)
        {
            DontDestroyOnLoad(gameObject);
            StartCoroutine(TransitionAndLoad());
        }
        else
        {
            Load();
        }
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

    System.Collections.IEnumerator TransitionAndLoad()
    {
        string targetName = null;
        int targetIndex = -1;
        if (useBuildIndex)
        {
            int count = SceneManager.sceneCountInBuildSettings;
            if (sceneBuildIndex >= 0 && sceneBuildIndex < count)
            {
                targetIndex = sceneBuildIndex;
            }
            else if (!string.IsNullOrEmpty(sceneName) && IsSceneInBuild(sceneName))
            {
                targetName = sceneName;
            }
            else
            {
                Debug.LogError($"SceneTransitionTrigger: índice {sceneBuildIndex} fuera de rango y escena '{sceneName}' no válida. Añade la escena a Build Settings o configura 'sceneName'.");
                yield break;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(sceneName) && IsSceneInBuild(sceneName))
            {
                targetName = sceneName;
            }
            else
            {
                Debug.LogError($"SceneTransitionTrigger: la escena '{sceneName}' no está en Build Settings. Usa File > Build Settings > Add Open Scenes.");
                yield break;
            }
        }

        GameObject overlayRoot = GameObject.Find("SceneTransitionOverlay");
        Image img = null;
        if (overlayRoot == null)
        {
            overlayRoot = new GameObject("SceneTransitionOverlay");
            Object.DontDestroyOnLoad(overlayRoot);
            var canvas = overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            var cg = overlayRoot.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;
            var goImg = new GameObject("Image");
            goImg.transform.SetParent(overlayRoot.transform);
            var rect = goImg.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            img = goImg.AddComponent<Image>();
            img.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
            img.raycastTarget = true;
        }
        else
        {
            img = overlayRoot.GetComponentInChildren<Image>(true);
            if (img == null)
            {
                var goImg = new GameObject("Image");
                goImg.transform.SetParent(overlayRoot.transform);
                var rect = goImg.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                img = goImg.AddComponent<Image>();
                img.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
                img.raycastTarget = true;
            }
        }

        float t = 0f;
        float inDur = Mathf.Max(0f, overlayFadeInSeconds);
        float targetA = overlayColor.a;
        while (t < inDur)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(0f, targetA, inDur > 0f ? (t / inDur) : 1f);
            var c = img.color; c.a = a; img.color = c;
            yield return null;
        }
        var cc = img.color; cc.a = targetA; img.color = cc;

        SceneManager.sceneLoaded += OnSceneLoadedResetPlayer;
        AsyncOperation op;
        if (targetIndex >= 0)
        {
            op = SceneManager.LoadSceneAsync(targetIndex, LoadSceneMode.Single);
        }
        else
        {
            op = SceneManager.LoadSceneAsync(targetName, LoadSceneMode.Single);
        }
        while (op != null && !op.isDone) yield return null;

        t = 0f;
        float outDur = Mathf.Max(0f, overlayFadeOutSeconds);
        while (t < outDur)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(targetA, 0f, outDur > 0f ? (t / outDur) : 1f);
            var c2 = img.color; c2.a = a; img.color = c2;
            yield return null;
        }
        var c3 = img.color; c3.a = 0f; img.color = c3;
        Object.Destroy(overlayRoot);
        Destroy(gameObject);
    }
    void OnSceneLoadedResetPlayer(Scene s, LoadSceneMode m)
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
        {
            var pc = p.GetComponent<PlayerController>();
            if (pc == null) pc = p.GetComponentInParent<PlayerController>();
            if (pc != null) pc.vida = pc.vidaMaxima;
            if (pc != null) pc.muerto = false;
            var pm = p.GetComponent<PlayerMovement>();
            if (pm == null) pm = p.GetComponentInParent<PlayerMovement>();
            if (pm != null) pm.enabled = true;
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
