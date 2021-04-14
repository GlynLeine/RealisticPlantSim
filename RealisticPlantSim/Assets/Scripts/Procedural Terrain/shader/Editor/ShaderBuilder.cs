using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ShaderBuilder : EditorWindow
{
    [MenuItem("Assets/Create/Shader/Simplified Custom HDRP Shader", false, 1)]
    public static void CreateShader()
    {
        Debug.Log("Tadaa!");

        ShaderBuilder window = GetWindow<ShaderBuilder>();
        window.CenterOnMainWin();
        window.Show();
        //ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Assets/Simplified Custom Shaders/Base/HDRP/CustomShader.template", "New Simplified Shader.shader");
        //ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Assets/Simplified Custom Shaders/Base/HDRP/CustomProgram.template", "New Simplified Program.hlsl");
    }

    enum TargetPipeline
    {
        HDRP, URP
    }

    void OnGUI()
    {
        EditorGUILayout.Space();

        TargetPipeline targetPipeline = (TargetPipeline)EditorGUILayout.EnumPopup("Target Pipeline", TargetPipeline.HDRP);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Shader Name");
        string shaderName = GUILayout.TextField("New Simplified Shader");
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Create"))
        {
            Debug.Log(shaderName);
            Close();
        }
    }
}
