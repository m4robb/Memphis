// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace WaveHarmonic.Crest.Editor.Settings
{
    [FilePath(k_Path, FilePathAttribute.Location.ProjectFolder)]
    sealed class ProjectSettings : ScriptableSingleton<ProjectSettings>
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [@Heading("Variant Stripping", Heading.Style.Settings)]

        [@Group]

        [@DecoratedField, SerializeField]
        bool _DebugEnableStrippingLogging;

        [@Heading("Features", Heading.Style.Settings)]

        [@Group]

        [Tooltip("Whether to sample shadow maps for built-in renderer. Can cause problems when trying to build. If you see \"redefinition of '_ShadowMapTexture'\" error, either disable this or do a Clean Build first.")]
        [@DecoratedField, SerializeField]
        bool _BuiltInRendererSampleShadowMaps = true;

        internal const string k_Path = "ProjectSettings/Packages/com.waveharmonic.crest/Settings.asset";

        internal enum State
        {
            Dynamic,
            Disabled,
            Enabled,
        }

        internal static ProjectSettings Instance => instance;

        internal bool DebugEnableStrippingLogging => _DebugEnableStrippingLogging;
        internal bool BuiltInRendererSampleShadowMaps => _BuiltInRendererSampleShadowMaps;


        void OnEnable()
        {
            // Fixes not being editable.
            hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
        }


        internal static void Save()
        {
            instance.Save(saveAsText: true);
        }

        [@OnChange(skipIfInactive: false)]
        void OnChange(string path, object previous)
        {
            switch (path)
            {
                case nameof(_BuiltInRendererSampleShadowMaps):
                    ShaderSettingsGenerator.Generate();
                    break;
            }
        }
    }

    sealed class SettingsProvider : UnityEditor.SettingsProvider
    {
        UnityEditor.Editor _Editor;

        SettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope)
        {
            // Empty
        }

        static bool IsSettingsAvailable()
        {
            return File.Exists(ProjectSettings.k_Path);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            _Editor = UnityEditor.Editor.CreateEditor(ProjectSettings.Instance);
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Helpers.Destroy(_Editor);
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        void OnUndoRedo()
        {
            ProjectSettings.Save();
        }

        public override void OnGUI(string searchContext)
        {
            if (_Editor.target == null)
            {
                Helpers.Destroy(_Editor);
                _Editor = UnityEditor.Editor.CreateEditor(ProjectSettings.Instance);
                return;
            }

            // Reset foldout values.
            DecoratedDrawer.s_IsFoldout = false;
            DecoratedDrawer.s_IsFoldoutOpen = false;

            EditorGUI.BeginChangeCheck();

            // Pad similar to settings header.
            var style = new GUIStyle();
            style.padding.left = 8;

            // Same label with as other settings.
            EditorGUIUtility.labelWidth = 251;

            EditorGUILayout.BeginVertical(style);
            _Editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();

            // Commit all changes. Normally settings are written when user hits save or exits
            // without any undo/redo entry and dirty state. No idea how to do the same.
            // SaveChanges and hasUnsavedChanges on custom editor did not work.
            // Not sure if hooking into EditorSceneManager.sceneSaving is correct.
            if (EditorGUI.EndChangeCheck())
            {
                ProjectSettings.Save();
            }
        }

        [SettingsProvider]
        static UnityEditor.SettingsProvider Create()
        {
            if (ProjectSettings.Instance)
            {
                var provider = new SettingsProvider("Project/Crest", SettingsScope.Project);
                provider.keywords = GetSearchKeywordsFromSerializedObject(new(ProjectSettings.Instance));
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}
