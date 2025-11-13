#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
#if UNITY_RENDER_PIPELINE_URP
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

/// <summary>
/// Ensures a default URP asset/renderers/global-settings exist and are wired into the project
/// the next time Unity opens the repository.
/// </summary>
[InitializeOnLoad]
public static class EnsureURPSetup
{
    private const string SettingsFolder = "Assets/Settings/URP";
    private const string RendererPath = SettingsFolder + "/ForwardRenderer.asset";
    private const string PipelinePath = SettingsFolder + "/UniversalRenderPipelineAsset.asset";
    private const string GlobalSettingsPath = SettingsFolder + "/URPGlobalSettings.asset";

    static EnsureURPSetup()
    {
        EditorApplication.update += Configure;
    }

    private static void Configure()
    {
        EditorApplication.update -= Configure;

#if UNITY_RENDER_PIPELINE_URP
        if (EditorApplication.isUpdating || EditorApplication.isCompiling)
        {
            EditorApplication.update += Configure;
            return;
        }

        Directory.CreateDirectory(SettingsFolder);

        var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (rendererData == null)
        {
            rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            rendererData.name = "ForwardRenderer";
            AssetDatabase.CreateAsset(rendererData, RendererPath);
        }

        var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (pipelineAsset == null)
        {
            pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            pipelineAsset.name = "UniversalRenderPipelineAsset";
            AssetDatabase.CreateAsset(pipelineAsset, PipelinePath);
        }

        var pipelineSO = new SerializedObject(pipelineAsset);
        var rendererListProp = pipelineSO.FindProperty("m_RendererDataList");
        if (rendererListProp != null)
        {
            rendererListProp.arraySize = 1;
            rendererListProp.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
        }

        var defaultRendererProp = pipelineSO.FindProperty("m_DefaultRendererIndex");
        if (defaultRendererProp != null)
        {
            defaultRendererProp.intValue = 0;
        }
        pipelineSO.ApplyModifiedPropertiesWithoutUndo();

        if (GraphicsSettings.defaultRenderPipeline != pipelineAsset)
        {
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        }

        var qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
#if UNITY_2022_1_OR_NEWER
            if (QualitySettings.GetRenderPipelineAssetAt(i) != pipelineAsset)
            {
                QualitySettings.SetRenderPipelineAsset(i, pipelineAsset);
            }
#else
            QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
            QualitySettings.renderPipeline = pipelineAsset;
#endif
        }

#if UNITY_2022_2_OR_NEWER
        var globalSettings = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineGlobalSettings>(GlobalSettingsPath);
        if (globalSettings == null)
        {
            globalSettings = ScriptableObject.CreateInstance<UniversalRenderPipelineGlobalSettings>();
            globalSettings.name = "URPGlobalSettings";
            AssetDatabase.CreateAsset(globalSettings, GlobalSettingsPath);
        }

        if (RenderPipelineGlobalSettings.currentRenderPipelineGlobalSettings != globalSettings)
        {
            RenderPipelineGlobalSettings.SetRenderPipelineGlobalSettingsAsset<UniversalRenderPipeline>(globalSettings);
        }
#endif

        AssetDatabase.SaveAssets();
#endif // UNITY_RENDER_PIPELINE_URP
    }
}
#endif
