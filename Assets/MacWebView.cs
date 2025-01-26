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
    private string token="eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL2Nvbm5lY3QuYWx0ZXItbGVhcm5pbmcuY29tIiwiaWF0IjoxNzM3OTI5MzM1LCJuYmYiOjE3Mzc5MjkzMzUsImV4cCI6MTczODUzNDEzNSwiZGF0YSI6eyJ1c2VyIjp7ImlkIjoiNTk1In19fQ.rtuwNYGBRkfOk9bnoEgQjJ8WJTYraOCS9lb_-BV_sVA";
    void Start()
    {
        Debug.Log("Initializing WebView...");
        InitializeWebView(); // Initialize the WebView
        isWebViewInitialized = true;
        UpdateWebViewFrame();
        Debug.Log($"Loading URL: {url}");
        AddCustomHeader("Authorization", "Bearer " +token);
        LoadURL(url); // Load the URL
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // For debugging, update frame when space is pressed
        {
            UpdateWebViewFrame();
            UpdateMask();
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
            Debug.Log("Destroying WebView...");
            DestroyWebView(); // Destroy WebView when application closes
            isWebViewInitialized = false;
        }
    }
}
