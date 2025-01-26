#ifndef WEBVIEW_H
#define WEBVIEW_H

#ifdef __cplusplus
extern "C" {
#endif

// Initializes the WebView within Unity's window
void InitializeWebView();

// Sets the position and size of the WebView (relative to Unity's window)
void SetWebViewFrame(float x, float y, float width, float height);

void AddCustomHeader(const char *headerKey, const char *headerValue);
// Loads a URL in the WebView
void LoadURL(const char* url);

// Destroys the WebView and removes it from Unity's window
void DestroyWebView();

// Sets a mask view for clipping the WebView and optionally displays it for debugging
// - leftMargin, topMargin, rightMargin, bottomMargin: Margins to define the mask area
// - visibleMask: If true, a red semi-transparent mask view is displayed; otherwise, only clipping is applied
void SetMaskView(float leftMargin, float topMargin, float rightMargin, float bottomMargin, bool visibleMask);

#ifdef __cplusplus
}
#endif

#endif // WEBVIEW_H
