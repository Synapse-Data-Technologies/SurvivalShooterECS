using UnityEngine;
using UnityEngine.UI;

public class SphereEnemyDebugUI : MonoBehaviour
{
    [Header("Debug UI Settings")]
    public GameObject debugCanvasPrefab;
    public Vector3 uiOffset = new Vector3(0, 2f, 0);
    public bool showDebugUI = true;
    
    private Canvas debugCanvas;
    private Text versionText;
    private Text modeText;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (showDebugUI)
        {
            CreateDebugUI();
        }
    }
    
    void Update()
    {
        if (debugCanvas != null && mainCamera != null)
        {
            // Make the debug UI face the camera
            Vector3 worldPosition = transform.position + uiOffset;
            debugCanvas.transform.position = worldPosition;
            debugCanvas.transform.LookAt(mainCamera.transform);
            debugCanvas.transform.Rotate(0, 180, 0); // Flip to face camera correctly
        }
    }
    
    private void CreateDebugUI()
    {
        // Create a world space canvas
        GameObject canvasGO = new GameObject("SphereEnemyDebugCanvas");
        canvasGO.transform.SetParent(transform);
        
        debugCanvas = canvasGO.AddComponent<Canvas>();
        debugCanvas.renderMode = RenderMode.WorldSpace;
        debugCanvas.worldCamera = mainCamera;
        
        // Set canvas size and scale
        RectTransform canvasRect = debugCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200, 100);
        canvasRect.localScale = Vector3.one * 0.01f; // Scale down for world space
        
        // Add CanvasScaler for better scaling
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        
        // Create background panel
        GameObject panelGO = new GameObject("DebugPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create version text
        GameObject versionGO = new GameObject("VersionText");
        versionGO.transform.SetParent(panelGO.transform, false);
        
        versionText = versionGO.AddComponent<Text>();
        versionText.text = "AI Version: 1.0";
        versionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        versionText.fontSize = 14;
        versionText.color = Color.white;
        versionText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform versionRect = versionGO.GetComponent<RectTransform>();
        versionRect.anchorMin = new Vector2(0, 0.5f);
        versionRect.anchorMax = new Vector2(1, 1);
        versionRect.offsetMin = new Vector2(5, 0);
        versionRect.offsetMax = new Vector2(-5, -5);
        
        // Create mode text
        GameObject modeGO = new GameObject("ModeText");
        modeGO.transform.SetParent(panelGO.transform, false);
        
        modeText = modeGO.AddComponent<Text>();
        modeText.text = "Mode: Patrol";
        modeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        modeText.fontSize = 14;
        modeText.color = Color.yellow;
        modeText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform modeRect = modeGO.GetComponent<RectTransform>();
        modeRect.anchorMin = new Vector2(0, 0);
        modeRect.anchorMax = new Vector2(1, 0.5f);
        modeRect.offsetMin = new Vector2(5, 5);
        modeRect.offsetMax = new Vector2(-5, 0);
    }
    
    public void UpdateDebugInfo(string version, string mode)
    {
        if (versionText != null)
        {
            versionText.text = $"AI Version: {version}";
        }
        
        if (modeText != null)
        {
            modeText.text = $"Mode: {mode}";
            
            // Change color based on mode
            switch (mode.ToLower())
            {
                case "idle":
                    modeText.color = Color.gray;
                    break;
                case "patrol":
                    modeText.color = Color.green;
                    break;
                case "chase":
                    modeText.color = Color.orange;
                    break;
                case "attack":
                    modeText.color = Color.red;
                    break;
                case "retreat":
                    modeText.color = Color.cyan;
                    break;
                default:
                    modeText.color = Color.white;
                    break;
            }
        }
    }
    
    public void ToggleDebugUI()
    {
        showDebugUI = !showDebugUI;
        if (debugCanvas != null)
        {
            debugCanvas.gameObject.SetActive(showDebugUI);
        }
    }
    
    void OnDestroy()
    {
        if (debugCanvas != null)
        {
            Destroy(debugCanvas.gameObject);
        }
    }
}