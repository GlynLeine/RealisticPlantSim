/// <summary>
/// Custom editor window for generating simplified custom shaders.
/// Author: Glyn Marcus Leine
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ShaderBuilder : EditorWindow
{
    [MenuItem("Assets/Create/Shader/Simplified Custom Shader", false, 1)]
    public static void CreateShader()
    {
        ShaderBuilder window = GetWindow<ShaderBuilder>();
        window.CenterOnMainWin();
        window.Show();
    }

    enum TargetPipeline
    {
        HDRP, URP
    }

    string shaderName = "New Simplified Shader";
    TargetPipeline targetPipeline = TargetPipeline.HDRP;

    int lastKeyboardControl = -1;

    void OnGUI()
    {
        EditorGUILayout.Space();

        targetPipeline = (TargetPipeline)EditorGUILayout.EnumPopup("Target Pipeline", targetPipeline);

        shaderName = EditorGUILayout.TextField("Shader Name", shaderName);

        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            int kbdCtrlId  = GUIUtility.keyboardControl;
            if (kbdCtrlId != lastKeyboardControl)
            {
                // check to see if the focused control is this text area...
                string focusedControl = GUI.GetNameOfFocusedControl();
                
                if (focusedControl == "Shader Name")
                {
                    // It is!  Now, get the editor state and tweak it.
                    TextEditor textEditor = GUIUtility.GetStateObject(typeof(TextEditor), kbdCtrlId) as TextEditor;
                    textEditor.SelectAll();
                    lastKeyboardControl = kbdCtrlId;
                }
            }
        }

        EditorGUILayout.Space();
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Create"))
        {
            switch (targetPipeline)
            {
                case TargetPipeline.HDRP:
                    ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Assets/Simplified Custom Shaders/Base/HDRP/CustomProgram.template", shaderName + " Program.shaderprogram");
                    ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Assets/Simplified Custom Shaders/Base/HDRP/CustomShader.template", shaderName + ".shader");
                    break;
                case TargetPipeline.URP:
                    Debug.LogError("[SCS] Pipeline not supported yet.");
                    break;
            }
            Close();
        }
    }
}
