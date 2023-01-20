using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PageView
{
    enum PAGE_TYPE
    {
        PatternTemplates,
        UserTemplates,
        CommunityTemplates,
        TemplateOverview
    }
    private VisualElement windowBody;
    private HttpLoader loader;
    private PAGE_TYPE currentPage = PAGE_TYPE.PatternTemplates;
    private bool isLoadingTemplates = false;
    private VisualTreeAsset template_item_preset;

    private VisualElement template_overview;
    private VisualElement templates_view;

    private bool closePageView = false;
    private int reloadCounter = 0;
    public PageView()
    {
        SBDebugger.Log("Page view initialize...");
        windowBody = ScriptBox.root;
        loader = ScriptBox.loader;

        template_item_preset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ScriptBox/Script_Item_Slot.uxml");

        template_overview = windowBody.Q<VisualElement>("template-overview");
        templates_view = windowBody.Q<VisualElement>("templates-view");

        ButtonsActivity();

        LoadPage();
    }

    private void ButtonsActivity()
    {
        Button patternTemplates = windowBody.Query<Button>("update-from-remoute");
        patternTemplates.clicked += LoadPage;

        Button hard_update_btn = windowBody.Query<Button>("hard-update-from-remoute");
        hard_update_btn.clicked += loader.ClearCache;
        hard_update_btn.clicked += LoadPage;

        Button btn_pattern_templates = windowBody.Query<Button>("pattern-scripts");
        btn_pattern_templates.clicked += delegate { ChangePage(PAGE_TYPE.PatternTemplates); };

        Button btn_community_templates = windowBody.Query<Button>("community-scripts");
        btn_community_templates.clicked += delegate { ChangePage(PAGE_TYPE.CommunityTemplates); };

        Button btn_user_templates = windowBody.Query<Button>("user-scripts");
        btn_user_templates.clicked += delegate { ChangePage(PAGE_TYPE.UserTemplates); };
    }

    private void LoadPage()
    {
        string templatesGroup = "";
        switch (currentPage)
        {
            case PAGE_TYPE.PatternTemplates:
                SetPageLayout(templates_view.name);
                templatesGroup = "patterns";
                LoadTemplates(templatesGroup); // Loading templates from remote
                break;
            case PAGE_TYPE.UserTemplates:
                SetPageLayout(templates_view.name);
                templatesGroup = "patterns"; // TODO: Load user templates from local storage
                LoadTemplates(templatesGroup); // Loading templates from remote
                break;
            case PAGE_TYPE.CommunityTemplates:
                SetPageLayout(templates_view.name);
                templatesGroup = "community-templates";
                LoadTemplates(templatesGroup); // Loading templates from remote
                break;
            case PAGE_TYPE.TemplateOverview:
                // Change ui
                SetPageLayout(template_overview.name);
                break;
        }
    }

    private void SetPageLayout(string layoutName)
    {
        template_overview.SetEnabled(layoutName == template_overview.name);
        templates_view.SetEnabled(layoutName == template_overview.name);
    }

    private void ChangePage(PAGE_TYPE page)
    {
        currentPage = page;
        LoadPage();
    }

    public async void LoadTemplates(string group)
    {
        if (isLoadingTemplates) return;
        isLoadingTemplates = true;

        JToken map = await loader.GetRemoteMap();
        JArray patternTemplates = (JArray)map["folders"];

        VisualElement grid = windowBody.Query<VisualElement>("script-slots-grid");
        grid.Clear();

        try
        {
            for (int i = 0; i < patternTemplates.Count; i++)
            {
                // Cycle through template groups
                if (patternTemplates[i]["name"].ToString() == group)
                {
                    JArray groupFolders = (JArray)patternTemplates[i]["folders"];

                    for (int j = 0; j < groupFolders.Count; j++)
                    {
                        // Cycle group folders
                        string folderName = groupFolders[j].ToString();
                        string optionsResult = await (loader.LoadFile(group, folderName, HttpLoader.LoadFileType.OPTIONS));
                        SBDebugger.Log("optionsResult", optionsResult);
                        JToken options = JToken.Parse(optionsResult);

                        SBDebugger.Log(options["name"]);

                        VisualElement script_item_copy = template_item_preset.Instantiate().ElementAt(0);
                        // Set item NAME
                        Label itemName = script_item_copy.Query<Label>("template-item-name");
                        itemName.text = options["name"].ToString();
                        // Set item AUTHOR
                        Label authorName = script_item_copy.Query<Label>("template-author-name");
                        string author = options["author"].ToString();
                        authorName.text = author == "none" || string.IsNullOrEmpty(author) ? "Unknown author" : author;
                        // Set item ICON
                        VisualElement icon = script_item_copy.Query<VisualElement>("template-item-icon");
                        icon.style.backgroundImage = new StyleBackground(png2sprite(await loader.LoadTemplateIcon(group, folderName)));
                        // Set item BUTTONS
                        Button more_btn = script_item_copy.Query<Button>("btn-more");
                        more_btn.clicked += delegate { ChangePage(PAGE_TYPE.TemplateOverview); };
                        // Add item to grid object
                        grid.Add(script_item_copy);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            SBDebugger.Warning(ex);
            TryLoadTemplate(group);
        }
        finally
        {
            reloadCounter = 0;
        }
        SBDebugger.Log("Finished loading");
        isLoadingTemplates = false;
    }
    private async void TryLoadTemplate(string templatesGroup)
    {
        await Task.Delay(1000);
        if (closePageView) return;
        if (reloadCounter > 3)
        {
            SBDebugger.Warning("Stoped tring reload!");
            return;
        }
        reloadCounter++;
        LoadTemplates(templatesGroup);
    }

    ~PageView()
    {
        closePageView = true;
        SBDebugger.Log("Page viewer destroyed");
    }

    private Sprite png2sprite(byte[] imageData, float pixelsPerUnit = 100.0f)
    {
        SBDebugger.Log("png2sprite", imageData);
        Sprite sprite;

        // LoadImage will replace with the size of the incoming image.
        Texture2D tex = new Texture2D(2, 2);
        // Load data into the texture.
        tex.LoadImage(imageData);
        SBDebugger.Log("Texture2D", tex);
        sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), pixelsPerUnit);
        return sprite;
    }
}