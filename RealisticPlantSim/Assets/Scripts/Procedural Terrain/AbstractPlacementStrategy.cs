using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Abstract class that any placement strategy needs to extend to be able to work.
/// Author: Robin Dittrich
/// </summary>
public abstract class AbstractPlacementStrategy : MonoScript
{
    /// <summary>
    /// Gets called before plants are gnerated.
    /// </summary>
    /// <param name="plantGenerator"></param>
    public abstract void OnGeneratorStart(PlantGenerator plantGenerator);

    /// <summary>
    /// Gets called for each plant to randomize their position based on this current placement strategy.
    /// </summary>
    /// <param name="plantGenerator"></param>
    /// <param name="chunk"></param>
    /// <param name="xmin"></param>
    /// <param name="xmax"></param>
    /// <param name="zmin"></param>
    /// <param name="zmax"></param>
    /// <returns></returns>
    public abstract Vector3 RandomizePosition(PlantGenerator plantGenerator, TerrainChunk chunk, float xmin, float xmax, float zmin, float zmax);

    /// <summary>
    /// Gets called after all plants have been generated.
    /// </summary>
    /// <param name="plantGenerator"></param>
    public abstract void OnGeneratorFinish(PlantGenerator plantGenerator);

    /// <summary>
    /// Exposed Inspector GUI capability
    /// </summary>
    /// <param name="plantGenerator"></param>
    public abstract void OnInspectorGUI(PlantGenerator plantGenerator);
}
