#import <Cocoa/Cocoa.h>
#import <WebKit/WebKit.h>
#import <AppKit/AppKit.h>
#import <QuartzCore/QuartzCore.h>


static WKWebView *webView = nil;
static NSWindow *unityWindow = nil;
static NSView *maskView = nil;  // Track the mask view
static NSMutableDictionary *customHeaders = nil; // Store custom headers globally

// Initialize the WebView within Unity's window
void InitializeWebView(void) {
    @autoreleasepool {
        // Get Unity's window from the current app
        unityWindow = [NSApplication sharedApplication].mainWindow;
        
        // Create the WebView with no separate window (inside Unity's window)
        WKWebViewConfiguration *configuration = [[WKWebViewConfiguration alloc] init];
        webView = [[WKWebView alloc] initWithFrame:unityWindow.contentView.bounds configuration:configuration];
        
        // Add WebView to Unity's window content view
        [unityWindow.contentView addSubview:webView];
        
        // Initialize the customHeaders dictionary
                customHeaders = [[NSMutableDictionary alloc] init];
    }
}

// Set the position and size of the WebView (relative to Unity's window)
void SetWebViewFrame(float x, float y, float width, float height) {
    @autoreleasepool {
        if (webView) {
            // Update WebView's frame within Unity's window
            webView.frame = NSMakeRect(x, y, width, height);
        }
    }
}
// Add a single custom header
void AddCustomHeader(const char *headerKey, const char *headerValue) {
    @autoreleasepool {
        // Convert C strings to NSString
        NSString *key = [NSString stringWithUTF8String:headerKey];
        NSString *value = [NSString stringWithUTF8String:headerValue];

        // Add the custom header to the global dictionary
        [customHeaders setObject:value forKey:key];
    }
}

// Load a URL in the WebView using previously set custom headers
void LoadURL(const char *url) {
    @autoreleasepool {
        NSString *urlString = [NSString stringWithUTF8String:url];
        NSURL *urlObject = [NSURL URLWithString:urlString];
        NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:urlObject];

        // Add custom headers to the request
        for (NSString *key in customHeaders) {
            [request setValue:customHeaders[key] forHTTPHeaderField:key];
        }

        // Load the request in the WebView
        [webView loadRequest:request];

        // Clear the custom headers after loading the URL
        [customHeaders removeAllObjects]; // Clear headers after loading the URL
    }
}
void DestroyWebView(void) {
    @autoreleasepool {
        [webView stopLoading]; // Stop loading any content
        [webView removeFromSuperview]; // Remove WebView from Unity's window
        webView = nil; // Release WebView object
    }
    // Remove the mask view if it exists
    if (maskView) {
        [maskView removeFromSuperview];  // Remove mask view from the superview
        maskView = nil;  // Release the mask view reference
    }
}
void SetMaskView(float leftMargin, float topMargin, float rightMargin, float bottomMargin, bool visibleMask) {
    @autoreleasepool {
        if (unityWindow) {
            // Get Unity window's content view bounds
            NSRect windowFrame = unityWindow.contentView.bounds;

            // Calculate the mask's frame with the provided margins
            NSRect maskFrame = NSMakeRect(leftMargin, topMargin,
                                          windowFrame.size.width - (leftMargin + rightMargin),
                                          windowFrame.size.height - (topMargin + bottomMargin));

            // Convert the maskFrame from Unity window's coordinate space to WebView's coordinate space
            NSRect convertedMaskFrame = [unityWindow.contentView convertRect:maskFrame toView:webView];

            // Create a clipping path in the WebView's coordinate space
            CGMutablePathRef path = CGPathCreateMutable();
            CGPathAddRect(path, NULL, CGRectMake(0, 0, convertedMaskFrame.size.width, convertedMaskFrame.size.height));

            // Create a shape layer with the converted path
            CAShapeLayer *clipLayer = [CAShapeLayer layer];
            clipLayer.frame = convertedMaskFrame;
            clipLayer.path = path;
            CGPathRelease(path); // Release the path after use

            // Remove the previous clipping mask if it exists
            if (webView.layer.mask) {
                webView.layer.mask = nil;
            }

            // Apply the new clipping mask to the WebView
            webView.layer.mask = clipLayer;

            // Handle the visibility of the mask view
            if (visibleMask) {
                // Create the mask view for visualization
                if (!maskView) {
                    maskView = [[NSView alloc] initWithFrame:maskFrame];
                } else {
                    maskView.frame = maskFrame; // Update the frame if maskView already exists
                }

                [maskView setWantsLayer:YES];
                [maskView.layer setBackgroundColor:[[NSColor redColor] CGColor]]; // Red background for mask
                maskView.layer.opacity = 0.5; // 50% opacity

                // Add the mask view to Unity window's content view if not already added
                if (![unityWindow.contentView.subviews containsObject:maskView]) {
                    [unityWindow.contentView addSubview:maskView];
                }
            } else {
                // Remove the mask view if visibleMask is false
                if (maskView && [unityWindow.contentView.subviews containsObject:maskView]) {
                    [maskView removeFromSuperview];
                }
            }
        }
    }
}



