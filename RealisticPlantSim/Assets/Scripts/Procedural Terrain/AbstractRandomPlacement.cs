using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class AbstractRandomPlacement : MonoScript
{

    public abstract Vector3 randomizePosition(PlantGenerator plantGenerator);
}
