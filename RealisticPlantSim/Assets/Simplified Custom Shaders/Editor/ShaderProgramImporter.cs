/// <summary>
/// Custom importer for the simplified shader program.
/// Author: Glyn Marcus Leine
/// </summary>

using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(1, "shaderprogram")]
public class ShaderProgramImporter : ScriptedImporter
{
    private TextAsset shaderProgram;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        string fileText = File.ReadAllText(ctx.assetPath);
        shaderProgram = new TextAsset(fileText);

        Texture2D icon = Resources.Load<Texture2D>("Icons/ShaderProgram Icon");
        ctx.AddObjectToAsset("Shader Program", shaderProgram, icon);
        ctx.SetMainObject(shaderProgram);
    }
}
