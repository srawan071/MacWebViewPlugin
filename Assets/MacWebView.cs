using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MacWebView : MonoBehaviour
{
    // Declare the DLL import (native plugin)
    private const string DLL_NAME = "MacWebViewPlugin";  // The .dylib file generated from your macOS plugin

    // Unity callback to log messages
    [DllImport(DLL_NAME)]
    private static extern void setUnityLoggerCallback(Action<string> callback);

    // Native function to initialize the WebView
    [DllImport(DLL_NAME)]
    private static extern void initializeWebView();

    // Native function to load a URL in the WebView
    [DllImport(DLL_NAME)]
    private static extern void loadWebViewURL(string url);

    // Native function to render WebView content to a texture
    [DllImport(DLL_NAME)]
    private static extern void renderWebViewToTexture();

    [DllImport(DLL_NAME)]
    private static extern void destroyWebView();


    // The texture that we will apply to the Unity material
    private RenderTexture renderTexture;

    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        // Set Unity logger callback to print logs from the native plugin
        setUnityLoggerCallback(LogMessageFromNative);

        // Initialize the WebView
        initializeWebView();

        // Create a RenderTexture to display the WebView content
        renderTexture = new RenderTexture(1024/2, 768/2, 24);
        renderTexture.Create();

        // Set the RenderTexture on the material of a Quad (for example)
        mat.mainTexture = renderTexture;

        // Load a URL into the WebView
        loadWebViewURL("https://www.youtube.com");
    }

    // Update is called once per frame
    void Update()
    {
        // Capture WebView content and render it to the Unity texture
        renderWebViewToTexture();
    }

    // Log messages from the native code
    private void LogMessageFromNative(string message)
    {
        Debug.Log("Native: " + message);
    }

    // Call this to clean up and destroy the WebView when no longer needed
    public void CleanupWebView()
    {
        destroyWebView();
    }

    // Ensure cleanup when the object is destroyed
    private void OnDestroy()
    {
        CleanupWebView();
    }
}
