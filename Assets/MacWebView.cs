using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections;

public static class MyScreen
{
    public static float Width
    {
#if UNITY_EDITOR
        
        get => MacWebView.CheckGameViewPositionAndSize().size.x;
#else
        get => Screen.width;
#endif
    }

    public static float Height
    {
#if UNITY_EDITOR

        get => MacWebView.CheckGameViewPositionAndSize().size.y;
#else
        get => Screen.height;
#endif
    }
    public static float PosX
    {
#if UNITY_EDITOR

        get => MacWebView.CheckGameViewPositionAndSize().x;
#else
get=>100;
#endif
    }
    public static float PosY
    {
#if UNITY_EDITOR

        get => MacWebView.CheckGameViewPositionAndSize().y;
#else
get=>100;
#endif
    }
}

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
        StartCoroutine(Updatee());
    }
    
    IEnumerator Updatee()
    {
        while (true)
        {
         
            if (Input.GetKeyDown(KeyCode.Space)) // For debugging, update frame when space is pressed
            {
                MatchTextureSizeToRectTransform(webViewRectTransform);
                yield return new WaitForSeconds(2);
                UpdateWebViewFrame();
                //  UpdateMask();

            }
            if (Input.GetKeyDown(KeyCode.M))
            {
#if UNITY_EDITOR
                CheckGameViewPositionAndSize();
                yield return new WaitForSeconds(2);
                UpdateWebViewFrame();
#endif
            }
            yield return null;
        }
        
    }

    void UpdateWebViewFrame()
    {

       
       Debug.Log($"Setting WebView Frame: X={x}, Y={y}, Width={width}, Height={height}");
        SetWebViewFrame(x, y, width, height); // Set the WebView's position and size
    }
   void UpdateMask()
    {
#if UNITY_EDITOR
        Rect editorWindowRect = GetEditorWindowRect();
        Debug.Log(" Editor Window rect"+ editorWindowRect);
        Rect gameViewRect = CheckGameViewPositionAndSize();
        Mask.x = gameViewRect.x;
        Mask.y = gameViewRect.y;
        Mask.z = editorWindowRect.size.x - (gameViewRect.size.x+ Mask.x);
        Mask.w = editorWindowRect.size.y-(gameViewRect.size.y+Mask.y);
        Debug.Log("Ediotr.x " + editorWindowRect.size.x);
         Debug.Log($" Update Mask {Mask}");
        Mask += Vector4.one * 50;
         SetMaskView(Mask.x,Mask.y,Mask.z,Mask.w,VisibleMask);
#endif
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
        //   Debug.Log("Only Scale is " + scale);
        Vector2 pivot = rectTransform.pivot;
        Debug.Log("Final Scale Factor is " + scaleFactor + "Canter" + center + " EffectiveScale" + GetCanvasRelativeLocalScale(rectTransform)+ "Piviot is: "+ pivot);
        SetCenterPositionWithScale(center, scale / scaleFactor, pivot);

    }
    // Use this function instead of SetMargins to easily set up a centered window
    // NOTE: for historical reasons, `center` means the lower left corner and positive y values extend up.
    public void SetCenterPositionWithScale(Vector2 center, Vector2 scale, Vector2 pivot)
    {
        
        Vector2 Screen = new Vector2(1920, 1080);
        Screen = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);

        Vector2 screenSize = Screen;


        float left = (Screen.x - scale.x) / 2 + center.x;
        float right = Screen.x - (left + scale.x);
        float bottom = (Screen.y - scale.y) / 2 + center.y;
        float top = Screen.y - (bottom + scale.y);
        Debug.Log("Left before" + left+ "Bottom "+bottom);

        left += (0.5f - pivot.x)*scale.x;
        bottom += (0.5f - pivot.y) * scale.y;
        Debug.Log("Left After" + left + "Bottom " + bottom);

        //  Debug.Log($" Margins left{left} top{top} right{right} bottom{bottom} center{center} scale{scale} pivot {pivot}");

#if UNITY_EDITOR
        Screen = new Vector2(1920, 1080);
        Vector2 Screenorg = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
        Screen = new Vector2(MyScreen.Width, MyScreen.Height);
       
        Vector2 scaleMultiplayer =  Screen/Screenorg;
          scale = scale * scaleMultiplayer;
         left = left * scaleMultiplayer.x+ MyScreen.PosX;
         bottom = bottom * scaleMultiplayer.y + MyScreen.PosY;

       // scale = new Vector2(MyScreen.Width, MyScreen.Height);
       // left = MyScreen.PosX;
        //bottom = MyScreen.PosY;
      
#endif
        x = left;
        y = bottom;
        this.width = scale.x;
        this.height = scale.y;

        // SetMargins((int)left, (int)top, (int)right, (int)bottom);

    }
    
    Vector2 GetPivot(RectTransform rectTransform)
    {
        return rectTransform.pivot;

    }

    /*
    public void SetMargins(int left, int top, int right, int bottom, bool relative = false)
    {
        Vector2 Screen = new Vector2(1920, 1080);
       Vector2  Screenorg = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
        Screen = new Vector2(MyScreen.Width, MyScreen.Height);
        Screen = Screenorg;
      //  Debug.Log(" Before Screen org= " + Screenorg + "Screen =" + Screen);
        Vector2 scaleMultiplayer = Screen/Screenorg;
       // Screen = Screen * scaleMultiplayer;
       // Debug.Log(" After Screen org= " + Screenorg + "Screen =" + Screen + " ScaleMultiplayer=" + scaleMultiplayer);


        int width = (int)(Screen.x - (left + right));
        int height = (int)(Screen.y - (bottom + top));
       
       // rect = new Rect(left, bottom, width, height);
        Debug.Log($"Left{left} Bottom {bottom} Width {width} Height{height} ");

        x = left;
        y = bottom;
        this.width = width;
        this.height = height;
        
    }
    */

#if UNITY_EDITOR
    public static Rect CheckGameViewPositionAndSize()
    {
        Rect finalRect = new Rect();
        var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
       // gameViewType = System.Type.GetType("UnityEditor.PlayModeView,UnityEditor");
        var gameView = EditorWindow.GetWindow(gameViewType);

        if (gameView != null)
        {
            //Type gameViewType = gameView.GetType();
            var targetSizeProperty = gameViewType.GetProperty("targetSize", BindingFlags.NonPublic | BindingFlags.Instance);
            if (targetSizeProperty == null)
            {
                Debug.LogError("Failed to access Game Viewport size!");
                return finalRect;
            }
            Vector2 targetSize = (Vector2)targetSizeProperty.GetValue(gameView);

            // Calculate the aspect ratio of the Game View
            float aspectRatio = targetSize.x / targetSize.y;
           // Debug.Log(" Target size =" + targetSize + " Aspect ratio =+" + aspectRatio);



            Rect gameViewRect = gameView.position;

            // Fix DPI Scaling Issues
            float scale = EditorGUIUtility.pixelsPerPoint;
           // Debug.Log("DPI SCALE IS+"+ scale);

            float adjustedX = gameViewRect.x * scale;
            float adjustedY = gameViewRect.y * scale;
            float adjustedWidth = gameViewRect.width * scale;
            float adjustedHeight = gameViewRect.height * scale;

            // Fix Bottom-Left Position
            float editorHeight = EditorGUIUtility.GetMainWindowPosition().height;
            adjustedY= editorHeight - (adjustedY + adjustedHeight);
            Vector2 editorPos = EditorGUIUtility.GetMainWindowPosition().position;


            float yPadding = 20;
           
            adjustedHeight -= yPadding;
            adjustedX -= editorPos.x -1+1;
            adjustedY += editorPos.y - yPadding;


            Vector2 renderedSize = GetGameViewRenderedSize(adjustedWidth, adjustedHeight, aspectRatio);
          // renderedSize = new Vector2(adjustedWidth, adjustedHeight);

            // Now, calculate the offsets to center the rendered view
            float centerX = adjustedX + (adjustedWidth - renderedSize.x) / 2;
            float centerY = adjustedY + (adjustedHeight - renderedSize.y) / 2;
            
            finalRect = new Rect(new Vector2(centerX, centerY), renderedSize);

             

            return finalRect;
            
        }
        else
        {
            Debug.LogError("Game View window not found!");
        }

        return finalRect;
    }


    static Vector2 GetGameViewRenderedSize(float width, float height, float aspectRatio)
    {

        Vector2 renderedSize = Vector2.zero;
         renderedSize.x = height * aspectRatio; // Rendered width based on aspect ratio
         renderedSize.y = height; // Height remains the same

        // Check if the rendered width exceeds the Game View width
        if (renderedSize.x > width)
        {
            // If the rendered width exceeds, we adjust the height based on the width
            renderedSize.x = width;
            renderedSize.y = width / aspectRatio;
        }

       
       UnityEditor.PlayModeWindow. GetRenderingResolution(out uint widthX, out uint heightY);
        Vector2 playmoderect = new Vector2(widthX, heightY);
        Vector2 scale = new Vector2(width, height) / playmoderect;
        Vector2 HandleSize = Handles.GetMainGameViewSize();

        Vector2 newScale = GetGameViewScale();
        Vector2 afterScale = HandleSize * newScale;
    //   Debug.Log(" Playmode Window is +" + playmoderect+ " renderSize ="+ renderedSize +"Scale "+scale+ "Handle Size"+ HandleSize+ " After scale "+afterScale);

        renderedSize = afterScale;
        return renderedSize;
    }
    public static Vector2 GetGameViewScale()
    {
        Vector2 scale = Vector2.zero;
        var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        if (gameViewType == null) return scale;

        var gameView = EditorWindow.GetWindow(gameViewType);
        if (gameView == null) return scale;

        var zoomAreaField = gameViewType.GetField("m_ZoomArea", BindingFlags.NonPublic | BindingFlags.Instance);
        if (zoomAreaField == null) return scale;

        var zoomArea = zoomAreaField.GetValue(gameView);
        if (zoomArea == null) return scale;

        var scaleProperty = zoomArea.GetType().GetProperty("scale", BindingFlags.Public | BindingFlags.Instance);
        if (scaleProperty == null) return
                scale;

         scale = (Vector2)scaleProperty.GetValue(zoomArea);
      //  Debug.Log("Game View Scale: " + scale);
        return scale;
    }
  private  Rect GetEditorWindowRect()
    {
        Type containerWindowType = Type.GetType("UnityEditor.ContainerWindow,UnityEditor");
        if (containerWindowType == null) return new Rect(0, 0, 0, 0);

        FieldInfo showModeField = containerWindowType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
        PropertyInfo positionProperty = containerWindowType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);

        if (showModeField == null || positionProperty == null) return new Rect(0, 0, 0, 0);

        object[] windows = Resources.FindObjectsOfTypeAll(containerWindowType);
        foreach (object window in windows)
        {
            int showMode = (int)showModeField.GetValue(window);
            if (showMode == 4) // 4 corresponds to the main Unity Editor window
            {
                return (Rect)positionProperty.GetValue(window, null);
            }
        }

        return new Rect(0, 0, 0, 0);
    }
#endif

}