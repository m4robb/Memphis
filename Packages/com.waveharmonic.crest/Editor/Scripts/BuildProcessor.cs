// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using WaveHarmonic.Crest.Editor.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace WaveHarmonic.Crest.Editor.Build
{
    sealed class BuildProcessor : IPreprocessComputeShaders, IPreprocessShaders, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        int _VariantCount;
        int _VariantCountStripped;

        ProjectSettings _Settings;
        WaterResources _Resources;

        void Logger(string message)
        {
            Debug.Log(message);
        }

        bool StripShader(Object shader, IList<ShaderCompilerData> data)
        {
            _Settings = ProjectSettings.Instance;
            _Resources = WaterResources.Instance;

            if (!AssetDatabase.GetAssetPath(shader).StartsWithNoAlloc("Packages/com.waveharmonic.crest"))
            {
                return false;
            }

            if (_Settings.DebugEnableStrippingLogging)
            {
                Logger($"Shader: '{shader.name}' @ {AssetDatabase.GetAssetPath(shader)}");
            }

            _VariantCount += data.Count;

            if (ShouldStripShader(shader))
            {
                if (_Settings.DebugEnableStrippingLogging)
                {
                    Logger($"Stripping Shader: {shader.name}");
                }

                _VariantCountStripped += data.Count;
                data.Clear();
                return false;
            }

            return true;
        }

        bool ShouldStripVariant(Object shader, ShaderCompilerData data, string[] keywords)
        {
            return false;
        }

        bool ShouldStripVariant(ProjectSettings.State state, ShaderCompilerData data, string[] keywords, LocalKeyword keyword, Object shader0, Object shader1)
        {
            if (shader0 != shader1)
            {
                return false;
            }

            return state switch
            {
                ProjectSettings.State.Disabled => data.shaderKeywordSet.IsEnabled(keyword),
                // Strip if keyword is not enabled and appears in one other variant.
                ProjectSettings.State.Enabled => !data.shaderKeywordSet.IsEnabled(keyword) && ArrayUtility.Contains(keywords, keyword.name),
                _ => false,
            };
        }

        bool ShouldStripVariant(ProjectSettings.State state, ShaderCompilerData data, string[] keywords, ShaderKeyword keyword)
        {
            return state switch
            {
                ProjectSettings.State.Disabled => data.shaderKeywordSet.IsEnabled(keyword),
                // Strip if keyword is not enabled and appears in one other variant.
                ProjectSettings.State.Enabled => !data.shaderKeywordSet.IsEnabled(keyword) && ArrayUtility.Contains(keywords, keyword.name),
                _ => false,
            };
        }

        bool ShouldStripVariant(Object shader, ShaderKeyword[] keywords)
        {
            // Strip debug variants.
            if (!EditorUserBuildSettings.development)
            {
                foreach (var keyword in keywords)
                {
                    if (keyword.name.StartsWithNoAlloc("_DEBUG"))
                    {
                        if (_Settings.DebugEnableStrippingLogging)
                        {
                            Logger($"Stripping Keyword: {keyword.name}");
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        bool ShouldStripShader(Object shader)
        {
            if (!EditorUserBuildSettings.development)
            {
                if (shader.name.Contains("Debug"))
                {
                    return true;
                }
            }

            return false;
        }

        void StripKeywords(Object shader, IList<ShaderCompilerData> data)
        {
            // Get all keywords for this kernel/stage.
            string[] keywords;
            {
                var set = new HashSet<ShaderKeyword>();
                for (var i = 0; i < data.Count; i++)
                {
                    // Each ShaderCompilerData is a variant which is a combination of keywords. Since each list will be
                    // different, simply getting a list of all keywords is not possible. This also appears to be the only
                    // way to get a list of keywords without trying to extract them from shader property names. Lastly,
                    // shader_feature will be returned only if they are enabled.
                    set.UnionWith(data[i].shaderKeywordSet.GetShaderKeywords());
                }

                keywords = set.Select(x => x.name).ToArray();
            }

            for (var i = data.Count - 1; i >= 0; --i)
            {
                if (_Settings.DebugEnableStrippingLogging)
                {
                    Logger($"Keywords: {string.Join(", ", data[i].shaderKeywordSet.GetShaderKeywords())}");
                }

                if (ShouldStripVariant(shader, data[i].shaderKeywordSet.GetShaderKeywords()))
                {
                    _VariantCountStripped++;
                    data.RemoveAt(i);
                    continue;
                }

                if (ShouldStripVariant(shader, data[i], keywords))
                {
                    _VariantCountStripped++;
                    data.RemoveAt(i);
                    continue;
                }
            }
        }

        bool ShouldStripSubShader(Shader shader, ShaderSnippetData snippet)
        {
            if (!shader.name.StartsWithNoAlloc("Crest/") && !shader.name.StartsWithNoAlloc("Hidden/Crest/"))
            {
                return false;
            }

            // There will be at least three sub-shaders if one per render pipeline.
            if (shader.subshaderCount <= 2)
            {
                return false;
            }

            // Strip BIRP sub-shader if not using BIRP as Unity only strips HDRP/URP sub-shaders.
            if (!RenderPipelineHelper.IsLegacy && !shader.TryGetRenderPipelineTag(snippet, out _))
            {
                return true;
            }

            return false;
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            if (!StripShader(shader, data))
            {
                return;
            }

            if (ShouldStripSubShader(shader, snippet))
            {
                _VariantCountStripped += data.Count;
                data.Clear();
                return;
            }

            if (_Settings.DebugEnableStrippingLogging)
            {
                Logger($"Pass {snippet.passName} Type {snippet.passType} Stage {snippet.shaderType}");
            }

            // TODO: Add stripping specific to pixel shaders here.

            StripKeywords(shader, data);
        }

        public void OnProcessComputeShader(ComputeShader shader, string kernel, IList<ShaderCompilerData> data)
        {
            if (!StripShader(shader, data))
            {
                return;
            }

            if (_Settings.DebugEnableStrippingLogging)
            {
                Logger($"Kernel {kernel}");
            }

            // TODO: Add stripping specific to compute shaders here.
            StripKeywords(shader, data);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            _Settings = ProjectSettings.Instance;
            _Resources = WaterResources.Instance;

            if (_Settings.DebugEnableStrippingLogging)
            {
                Debug.Log($"Crest: {_VariantCountStripped} / {_VariantCount} stripped from Crest. Total variants: {_VariantCount - _VariantCountStripped}");
            }
        }
    }
}
