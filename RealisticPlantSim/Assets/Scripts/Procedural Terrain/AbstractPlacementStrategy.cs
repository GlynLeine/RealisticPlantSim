using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class AbstractPlacementStrategy : MonoScript
{
    public abstract void OnGeneratorStart(PlantGenerator plantGenerator);

    public abstract Vector3 RandomizePosition(PlantGenerator plantGenerator);

    public abstract void OnGeneratorFinish(PlantGenerator plantGenerator);

    public abstract void OnInspectorGUI();
}
