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
    public bool showPortal = true;
    public int portalElementCount = 24;
    public float portalRadius = 1.2f;
    public float portalRotationSpeed = 30f;
    public Color portalColor = new Color(0.4f, 0.8f, 1f, 1f);
    public float portalLightIntensity = 2f;
    public float portalLightRange = 6f;
    public string portalMessage = "Ayuda al reino de los bosques a vencer contra el rey de la oscuridad";
    public float portalMessageYOffset = 2.0f;
    public float portalMessageSize = 0.6f;
    public bool showPortalText = false;
    Transform portalRoot;
    Transform textRoot;
    void Awake()
    {
        var c2d = GetComponent<Collider2D>();
        if (autoIsTrigger && c2d != null) c2d.isTrigger = true;
        if (showPortal) CreatePortalFX();
    }
    void OnValidate()
    {
        var c2d = GetComponent<Collider2D>();
        if (autoIsTrigger && c2d != null) c2d.isTrigger = true;
        if (showPortal) CreatePortalFX();
    }
    void Update()
    {
        if (portalRoot != null)
        {
            portalRoot.Rotate(Vector3.forward, portalRotationSpeed * Time.deltaTime, Space.Self);
        }
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
    void CreatePortalFX()
    {
        if (portalRoot == null)
        {
            var pr = new GameObject("PortalFX");
            pr.transform.SetParent(transform);
            pr.transform.localPosition = Vector3.zero;
            portalRoot = pr.transform;
        }
        for (int i = portalRoot.childCount - 1; i >= 0; i--)
        {
            var c = portalRoot.GetChild(i);
            if (c != null && c.name.StartsWith("Orb")) Destroy(c.gameObject);
        }
        int count = Mathf.Max(6, portalElementCount);
        for (int i = 0; i < count; i++)
        {
            float ang = (Mathf.PI * 2f) * (i / (float)count);
            Vector3 pos = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * Mathf.Max(0.2f, portalRadius);
            var orb = new GameObject("Orb" + i);
            orb.transform.SetParent(portalRoot);
            orb.transform.localPosition = pos;
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(orb.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 0.2f;
            var mr = sphere.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mpb = new MaterialPropertyBlock();
                mr.GetPropertyBlock(mpb);
                mpb.SetColor("_Color", portalColor);
                mr.SetPropertyBlock(mpb);
            }
            var sc = sphere.GetComponent<SphereCollider>();
            if (sc != null) Destroy(sc);
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(orb.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = portalColor;
            light.intensity = portalLightIntensity;
            light.range = portalLightRange;
            light.shadows = LightShadows.None;
        }
        if (!showPortalText)
        {
            if (textRoot != null)
            {
                Destroy(textRoot.gameObject);
                textRoot = null;
            }
        }
        else
        {
            if (textRoot == null)
            {
                var tr = new GameObject("PortalText");
                tr.transform.SetParent(transform);
                tr.transform.localPosition = new Vector3(0f, portalMessageYOffset, 0f);
                textRoot = tr.transform;
                var tm = tr.AddComponent<TextMesh>();
                tm.text = portalMessage;
                tm.fontSize = 48;
                tm.characterSize = Mathf.Max(0.1f, portalMessageSize);
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = portalColor;
            }
        }
    }
    void OnDestroy()
    {
        if (portalRoot != null) Destroy(portalRoot.gameObject);
        if (textRoot != null) Destroy(textRoot.gameObject);
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
