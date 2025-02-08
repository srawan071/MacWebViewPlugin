using UnityEngine;
using UnityEditor;

public class CustomEditorWindow : EditorWindow
{
    public static void ShowWindow(Vector2 position, Vector2 size)
    {
        // Create the window
        CustomEditorWindow window = GetWindow<CustomEditorWindow>("My Window");

        // Set position and size
        Rect rect = new Rect(position, size);
        window.position = rect;

        // Show window
        window.Show();
    }

    // Draw UI inside the window
    private void OnGUI()
    {
        GUILayout.Label("This is a custom editor window", EditorStyles.boldLabel);
    }
}
