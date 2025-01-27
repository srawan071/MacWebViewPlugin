using System.Runtime.InteropServices;
using UnityEngine;

public class MacWebView : MonoBehaviour
{
    private const string DLL_NAME = "MacWebView";

    [DllImport(DLL_NAME, EntryPoint = "InitializeWebView")]
    private static extern void InitializeWebView();

    [DllImport(DLL_NAME, EntryPoint = "SetWebViewFrame")]
    private static extern void SetWebViewFrame(float x, float y, float width, float height);

    [DllImport(DLL_NAME, EntryPoint = "LoadURL")]
    private static extern void LoadURL(string url);

    [DllImport(DLL_NAME, EntryPoint = "DestroyWebView")]
    private static extern void DestroyWebView();

    [DllImport(DLL_NAME)]
    private static extern void SetMaskView(float leftMargin, float topMargin, float rightMargin, float bottomMargin, bool visibleMask);

    [DllImport(DLL_NAME)]
    private static extern void AddCustomHeader(string headerKey, string headerValue);

    public RectTransform webViewRectTransform; // The RectTransform defining the WebView's size and position
    public string url = "https://connect.alter-learning.com/members/srawan071/create-event/"; // URL to load

    private bool isWebViewInitialized;
    public float x, y, width, height;
    public Vector4 Mask;
    public bool VisibleMask;
    
    void Start()
    {
        Debug.Log("Initializing WebView...");
        InitializeWebView(); // Initialize the WebView
        isWebViewInitialized = true;
        UpdateWebViewFrame();
        Debug.Log($"Loading URL: {url}");
        //AddCustomHeader("Authorization", "Bearer " + token );
        LoadURL(url); // Load the URL
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // For debugging, update frame when space is pressed
        {
            UpdateWebViewFrame();
           // UpdateMask();
            MatchTextureSizeToRectTransform(webViewRectTransform);
        }
    }

    void UpdateWebViewFrame()
    {
        
        
        Debug.Log($"Setting WebView Frame: X={x}, Y={y}, Width={width}, Height={height}");
        SetWebViewFrame(x, y, width, height); // Set the WebView's position and size
    }
   void UpdateMask()
    {
        Debug.Log($" Update Mask {Mask}");
         SetMaskView(Mask.x,Mask.y,Mask.z,Mask.w,VisibleMask);
    }

    void OnDestroy()
    {
        if (isWebViewInitialized)
        {
           // Debug.Log("Destroying WebView...");
            DestroyWebView(); // Destroy WebView when application closes
            isWebViewInitialized = false;
        }
    }

    public Vector3 GetCanvasRelativeLocalPosition(RectTransform rectTransform)
    {
        // Get the root Canvas
        Canvas rootCanvas = rectTransform.GetComponentInParent<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogWarning("No Canvas found in parent hierarchy.");
            return Vector3.zero;
        }

        // Get the world position of the target RectTransform
        Vector3 worldPosition = rectTransform.position;

        // Convert the world position to the Canvas's local position
        RectTransform canvasRectTransform = rootCanvas.GetComponent<RectTransform>();
        Vector3 canvasRelativeLocalPosition = canvasRectTransform.InverseTransformPoint(worldPosition);

        return canvasRelativeLocalPosition;
    }
    public Vector3 GetCanvasRelativeLocalScale(RectTransform rectTransform)
    {
        // Find the root Canvas
        Canvas rootCanvas = rectTransform.GetComponentInParent<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogWarning("No Canvas found in parent hierarchy.");
            return Vector3.one;
        }

        // Start with the target's local scale
        Vector3 effectiveScale = rectTransform.localScale;

        // Traverse up the hierarchy to the Canvas, accumulating scale
        Transform current = rectTransform.parent;
        while (current != null && current != rootCanvas.transform)
        {
            // Multiply with each parent's local scale
            effectiveScale = Vector3.Scale(effectiveScale, current.localScale);
            current = current.parent;
        }

        return effectiveScale;
    }
    public void MatchTextureSizeToRectTransform(RectTransform rectTransform, float quality=1)
    {
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;
        float scaleFactor = 1f / canvas.transform.localScale.x;
        //  scaleFactor = 1f / GetCanvasRelativeLocalScale(rectTransform).x;
        Vector2 scale = rectTransform.rect.size * GetCanvasRelativeLocalScale(rectTransform);
        Vector2 center = GetCanvasRelativeLocalPosition(rectTransform) / scaleFactor;
        Debug.Log("Only Scale is " + scale);

        Debug.Log("Final Scale Factor is " + scaleFactor + "Canter" + center + " EffectiveScale" + GetCanvasRelativeLocalScale(rectTransform));
        SetCenterPositionWithScale(center, scale / scaleFactor);
    }
    // Use this function instead of SetMargins to easily set up a centered window
    // NOTE: for historical reasons, `center` means the lower left corner and positive y values extend up.
    public void SetCenterPositionWithScale(Vector2 center, Vector2 scale)
    {
#if UNITY_WEBPLAYER || UNITY_WEBGL
        //TODO: UNSUPPORTED
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
        //TODO: UNSUPPORTED
#else
        Vector2 Screen = new Vector2(1920, 1080);
        Screen = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
        float left = (Screen.x - scale.x) / 2.0f + center.x;
        float right = Screen.x - (left + scale.x);
        float bottom = (Screen.y - scale.y) / 2.0f + center.y;
        float top = Screen.y - (bottom + scale.y);
        Debug.Log(" Size is " + scale);
        Debug.Log($" Margins left{left} top{top} right{right} bottom{bottom}");
        SetMargins((int)left, (int)top, (int)right, (int)bottom);
#endif
    }

    public void SetMargins(int left, int top, int right, int bottom, bool relative = false)
    {
        Vector2 Screen = new Vector2(1920, 1080);
        Screen = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
        int width = (int)(Screen.x - (left + right));
        int height = (int)(Screen.y - (bottom + top));
       
       // rect = new Rect(left, bottom, width, height);
        Debug.Log($"Left{left} Bottom {bottom} Width {width} Height{height} ");

        x = left;
        y = bottom;
        this.width = width;
        this.height = height;
        
    }
    }
