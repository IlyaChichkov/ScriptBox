using UnityEngine;
using UnityEditor;
using System.Net.Http;

public class ScriptBoxTest : EditorWindow
{
    static HttpClient httpClient = new HttpClient();
    string getContent = "";

    private void OnEnable()
    {
        // Repository();
    }

    [MenuItem("Window/ScriptBox")]
    private static void ShowWindow()
    {
        var window = GetWindow<ScriptBoxTest>();
        window.titleContent = new GUIContent("ScriptBox");
        window.Show();
    }

    async void Repository()
    {
        httpClient.DefaultRequestHeaders.Add("User-Agent", "ScriptBox");
        httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

        using HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/IlyaChichkov/freeos_uart_transactions/contents/tests");

        // получаем ответ
        getContent = await response.Content.ReadAsStringAsync();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
        MainView();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Script Box (Test v0.0.1) by RedEyeGames");
    }


    private Vector2 scroll = Vector2.zero;
    private void MainView()
    {


        EditorGUILayout.BeginVertical(GUILayout.Width(312));
        if (GUILayout.Button("Change client"))
        {
            httpClient = new HttpClient();
        }

        if (GUILayout.Button("Make Request"))
        {
            Repository();
            getContent = "Loading...";
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(getContent);
        EditorGUILayout.TextArea(getContent, EditorStyles.textArea);
        EditorGUILayout.EndVertical();

    }
}