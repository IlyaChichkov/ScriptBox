using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptBox : EditorWindow
{
    static ScriptBox window;
    public static VisualElement root;
    public static HttpLoader loader;
    PageView pageView;

    private void OnEnable()
    {
        if (loader == null)
        {
            loader = new HttpLoader();
        }

    }

    private void OnDisable()
    {
        loader.DisposeClient();
    }

    [MenuItem("Script Box Asset/Script Box")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        window = (ScriptBox)EditorWindow.GetWindow(typeof(ScriptBox));
        window.titleContent = new GUIContent("ScriptBox");
        window.Show();
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        root = rootVisualElement;

        // Import UXML
        var assetTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScriptBox/ScriptBox.uxml");
        VisualElement visualTree = assetTree.CloneTree();
        root.Add(visualTree.ElementAt(0));

        Label pathText = root.Query<Label>("current-path");
        pathText.text = Application.dataPath;

        pageView = new PageView();
    }
}