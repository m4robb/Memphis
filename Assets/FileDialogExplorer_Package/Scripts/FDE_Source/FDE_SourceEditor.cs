#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FDE_Source))]
public class FDE_SourceEditor : Editor
{
    //Official editor for FDE Source - no documentation & commentary

    private FDE_Source fde;

    public Texture MouseIcon;
    public Texture FDE_Logo;

    private bool showChangeNotes = false;

    private readonly List<string> drivers = new List<string>();

    private void OnEnable()
    {
        fde = (FDE_Source)target;

        ActionOBJ_ReadTo3DText = serializedObject.FindProperty("ActionOBJ_ReadTo3DText");
        ActionOBJ_ReadToUIText = serializedObject.FindProperty("ActionOBJ_ReadToUIText");
        ActionOBJ_ReadToUGUITMP = serializedObject.FindProperty("ActionOBJ_ReadToUGUITMP");
        ActionOBJ_ReadToSprite = serializedObject.FindProperty("ActionOBJ_ReadToSprite");
        ActionOBJ_ReadToUIImage = serializedObject.FindProperty("ActionOBJ_ReadToUIImage");
        ActionOBJ_ReadToRenderer = serializedObject.FindProperty("ActionOBJ_ReadToRenderer");
        ActionOBJ_VideoPlayer = serializedObject.FindProperty("ActionOBJ_VideoPlayer");

        ActionOBJ_ReadToVariableMonoBeh = serializedObject.FindProperty("ActionOBJ_ReadToVariableMonoBeh");
        ActionOBJ_ReadToVariableVar = serializedObject.FindProperty("ActionOBJ_ReadToVariableVar");

        Action_CustomEvent = serializedObject.FindProperty("Action_CustomEvent");

        drivers.Clear();
        foreach (string d in System.IO.Directory.GetLogicalDrives())
        {
            if (System.IO.Directory.Exists(d))
                drivers.Add(d);
        }
        drivers.Add(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
        drivers.Add(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label(FDE_Logo);
        BH();
        if (B("Documentation"))
            Application.OpenURL("https://docs.google.com/presentation/d/1AEBz5pIbTXIvJsGpspBT1tGYQv1tF5iic65kYndViNA/edit?usp=sharing");
        if (B("Discord"))
            Application.OpenURL("https://discord.gg/Ztr8ghQKqC");
        if (B("Support"))
            Application.OpenURL("https://matejvanco.com/contact/");
        EH();
        showChangeNotes = EditorGUILayout.Toggle("Show Change-Log", showChangeNotes);
        if(showChangeNotes)
            EditorGUILayout.HelpBox("File Dialog Explorer version 7.0 [25/04/2022 (dd/mm/yyyy)]\n\nChangeLog:\n- Updated API\n- Updated Browser\n- Minor Code Refactor\n- Updated Asynchronous Loading\n- Updated Custom Event Trigger", MessageType.Info);
        S(10);

        DrawProperty("DefaultStartup_ApplicationStartUp", "Default Path is App Startup","If enabled, default startup path for Dialog Explorer will be the application's startup path - where is your application located");
        if (!fde.DefaultStartup_ApplicationStartUp)
        {
            DrawProperty("MainPath", "Main Path", "Main default start-up path");
            L("Available Drivers");
            BH();
            foreach (string driver in drivers)
            {
                if (B(driver))
                    fde.MainPath = driver;
            }
            EH();
        }
        else DrawProperty("MobilePlatform", "Mobile Platform", "If enabled, the system is ready for mobile platforms - persistence data path will be preferred (instead of DataPath)");

        S(10);

        BV();
        BV();
        DrawProperty("EnableDialogAfterStart", "Enable Dialog On Startup", "Show up dialog on startup");
        DrawProperty("KeepDialogAfterAction", "Keep Dialog On Action", "If disabled, dialog will be closed after Action [After item click]");
        EV();
        S(10);
        BV();
        DrawProperty("EnableDataCustomization", "Enable Data Customization", "If enabled, user will be able to use Right Mouse Button to create, edit or copy files/ folders in drives");
        if (fde.EnableDataCustomization)
        {
            DrawProperty("HighProtectionLevel", "High Protection Level", "[Recommended: enabled] If enabled, you won't be able to manipulate with existing files/folders in your computer. But you will be able to manipulate with files/folders created in Dialog Explorer by you");
            EditorGUILayout.HelpBox("Please be careful using the 'Data Customization' feature. You may easily remove any kind of data of any size. It's recommended to keep the HighProtectionLevel enabled for your own safety.", MessageType.Warning);
        }
        EV();
        S(10);
        BV();
        DrawProperty("EnableHistoryDialog", "Enable History Dialog", "If enabled, user will be able to use history dialog");
        if (fde.EnableHistoryDialog)
        {
            DrawProperty("ShowHistoryDialogOnStart", "Show History Dialog On Start", "If enabled, the History Dialog will appear on application startup");
            DrawProperty("ShowHistoryFoldersNameOnly", "Show History Folders Name Only", "If enabled, the generated history folders will contain just names without a full path");
        }
        EV();
        S(10);
        DrawProperty("ShowLoadingPanel", "Show Loading Panel", "If enabled, the loading panel with Cancel button will appear while loading large folders");
        EV();

        S(10);

        BV();
        DrawProperty("DefaultExtension", "Default Extension", "Default extension while creating a new file WITHOUT dot!");
        DrawProperty("ICON_Files", "Default Files");
        DrawProperty("ICON_Folders", "Default Folders");
        DrawProperty("MaxStoredHistoryFolders", "Max Stored History Folders", "Maximum amount of recently opened folders");

        S(5);
        DrawProperty("MaxImageDisplaySize", "Max Image Size [kb]", "When images are too big, it may take a longer time to load... Set the size of maximum image file size into dialog[default 1024 kb = 1 mb]. Otherwise the image will be replaced by the image below...");
        DrawProperty("ICON_DefaultImageHolder", "Default Image");
        S(5);
        BV();
        DrawProperty("ShowFilesWithRegisteredExtensionsOnly", "Filter Registered Extensions Only", "If enabled, files that contains one of the registered extensions below will show up, other files will be hidden");
        DrawProperty("registeredExtensions", "Registered Extensions", "", true);
        EV();
        S(5);
        DrawProperty("useCustomFont", "Use Custom Font", "Use custom font in the File Dialog");
        if (fde.useCustomFont)
            DrawProperty("customFont");
        EV();

        S(10);
        BV();
        DrawProperty("FDE_ItemPrefab", "FDE File Prefab", "Prefab of generated files in FDE");
        EV();

        S(10);
        BV();
        InternalActions();
        EV();
    }

    private SerializedProperty ActionOBJ_ReadTo3DText;
    private SerializedProperty ActionOBJ_ReadToUIText;
    private SerializedProperty ActionOBJ_ReadToUGUITMP;
    private SerializedProperty ActionOBJ_ReadToSprite;
    private SerializedProperty ActionOBJ_ReadToUIImage;
    private SerializedProperty ActionOBJ_ReadToRenderer;
    private SerializedProperty ActionOBJ_ReadToVariableMonoBeh;
    private SerializedProperty ActionOBJ_ReadToVariableVar;
    private SerializedProperty ActionOBJ_VideoPlayer;
    private SerializedProperty Action_CustomEvent;

    private void InternalActions()
    {
        S(5);
        GUILayout.Label(new GUIContent("Selected Action - " + fde.File_Action.ToString(), MouseIcon));

        DrawProperty("File_Action", "Action Type", "Select an action type that will occur after file click");

        if (fde.File_Action == FDE_Source.FileAction.Open)
            L("After click - selected file will be opened as file.");
        else if(fde.File_Action == FDE_Source.FileAction.OpenInExplorer)
            L("After click - selected file will open it's directory.");
        else if(fde.File_Action == FDE_Source.FileAction.Text_ReadToVariable)
        {
            EditorGUILayout.PropertyField(ActionOBJ_ReadToVariableMonoBeh, new GUIContent("Enter MonoBehaviour Target Script"), true);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(ActionOBJ_ReadToVariableVar, new GUIContent("Enter Variable Name In Target Script"), true);
            serializedObject.ApplyModifiedProperties();
            DrawProperty("Read_Type", "Read Type", "Value that will be received from the clicked file");
            S(5);
            L("After click - selected file will transfer data to the selected variable value.");
        }
        else if(fde.File_Action == FDE_Source.FileAction.Text_ReadTo3DText)
        {
            EditorGUILayout.PropertyField(ActionOBJ_ReadTo3DText, new GUIContent("Enter 3D Text Mesh Object"), true);
            serializedObject.ApplyModifiedProperties();
            DrawProperty("Read_Type", "Read Type", "Value that will be received from the clicked file");
            S(5);
            L("After click - selected file will transfer data to the 3D text mesh.");
        }
        else if(fde.File_Action == FDE_Source.FileAction.Text_ReadToUIText)
        {
            EditorGUILayout.PropertyField(ActionOBJ_ReadToUIText, new GUIContent("Enter UI Text Object"), true);
            serializedObject.ApplyModifiedProperties();
            DrawProperty("Read_Type", "Read Type", "Value that will be received from the clicked file");
            S(5);
            L("After click - selected file will transfer data to the UI text.");
        }
        else if (fde.File_Action == FDE_Source.FileAction.Text_ReadToUGUITextMeshPro)
        {
            EditorGUILayout.PropertyField(ActionOBJ_ReadToUGUITMP, new GUIContent("Enter TMPro Text Object"), true);
            serializedObject.ApplyModifiedProperties();
            DrawProperty("Read_Type", "Read Type", "Value that will be received from the clicked file");
            S(5);
            L("After click - selected file will transfer data to the UGUI TMPro text.");
        }
        else if(fde.File_Action == FDE_Source.FileAction.Image_ReadImageToSprite)
        {
            EditorGUILayout.PropertyField(ActionOBJ_ReadToSprite, new GUIContent("Enter Sprite Renderer Object"), true);
            serializedObject.ApplyModifiedProperties();
            S(5);
            L("After click - selected file will transfer image data to the Sprite Renderer.\n[Allowed extensions: png, jpg, bmp, gif, tga]");
        }
        else if(fde.File_Action == FDE_Source.FileAction.Image_ReadImageToUIImage)
        {
            EditorGUILayout.PropertyField(ActionOBJ_ReadToUIImage, new GUIContent("Enter UI Image Object"), true);
            serializedObject.ApplyModifiedProperties();
            S(5);
            L("After click - selected file will transfer image data to the UI image.\n[Allowed extensions: png, jpg, bmp, gif, tga]");
        }
        else if(fde.File_Action == FDE_Source.FileAction.Image_ReadImageToRenderer)
        {
            EditorGUILayout.PropertyField(ActionOBJ_ReadToRenderer, new GUIContent("Enter Mesh Renderer Object"), true);
            serializedObject.ApplyModifiedProperties();
            S(5);
            L("After click - selected file will transfer image data to the Mesh Renderer.\n[Allowed extensions: png, jpg, bmp, gif, tga]");
        }
        else if(fde.File_Action == FDE_Source.FileAction.Video_ReadVideoToVideoPlayer)
        {
            EditorGUILayout.PropertyField(ActionOBJ_VideoPlayer, new GUIContent("Enter Video Player Object"), true);
            serializedObject.ApplyModifiedProperties();
            S(5);
            L("After click - selected file will read the video-url path to the video player.\nThe video will be played afterwards.");
        }
        else if(fde.File_Action == FDE_Source.FileAction.CustomEvent)
        {
            EditorGUILayout.PropertyField(Action_CustomEvent, new GUIContent("Enter Custom Event"), true);
            serializedObject.ApplyModifiedProperties();
            S(5);
            L("After click - the custom event will be rendered immediately.");
        }
    }

    private void DrawProperty(string p, string txt = "", string tooltip = "", bool includeChilds = false)
    {
        if (string.IsNullOrEmpty(txt))
            txt = p;
        EditorGUILayout.PropertyField(serializedObject.FindProperty(p), new GUIContent(txt, tooltip), includeChilds, null);
        serializedObject.ApplyModifiedProperties();
    }

    private bool B(string txt)
    {
        return GUILayout.Button(txt);
    }

    private void S(float v)
    {
        GUILayout.Space(v);
    }

    private void L(string txt)
    {
        GUILayout.Label(txt);
    }

    private void BV()
    {
        GUILayout.BeginVertical("Box");
    }

    private void EV()
    {
        GUILayout.EndVertical();
    }

    private void BH()
    {
        GUILayout.BeginHorizontal("Box");
    }

    private void EH()
    {
        GUILayout.EndHorizontal();
    }
}
#endif