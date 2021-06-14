/// <summary>
/// Vertex shader code in order to simplify making custom shaders for HDRP.
/// Author: Glyn Marcus Leine
/// </summary>

// Vertex shader main function
PackedVaryingsType Vert(    AttributesMesh inputMesh
                            #if (SHADERPASS == SHADERPASS_MOTION_VECTORS)
                                , AttributesPass inputPass
                            #endif
                    )
{
    VaryingsType varyingsType;

#if defined(HAVE_RECURSIVE_RENDERING) && (SHADERPASS == SHADERPASS_DEPTH_ONLY || SHADERPASS == SHADERPASS_GBUFFER || SHADERPASS == SHADERPASS_FORWARD) && !defined(SCENESELECTIONPASS) && !defined(SCENEPICKINGPASS)
    // If we have a recursive raytrace object, we will not render it.
    // As we don't want to rely on renderqueue to exclude the object from the list,
    // we cull it by settings position to NaN value.
    // TODO: provide a solution to filter dyanmically recursive raytrace object in the DrawRenderer
    if (_EnableRecursiveRayTracing && _RayTracing > 0.0)
    {
        ZERO_INITIALIZE(VaryingsType, varyingsType); // Divide by 0 should produce a NaN and thus cull the primitive.
    }
    else
#endif
    {
        varyingsType.vmesh = VertMesh(VertexProgram(inputMesh));
    }

#if (SHADERPASS == SHADERPASS_MOTION_VECTORS)
    return MotionVectorVS(varyingsType, inputMesh, inputPass);
#else
    return PackVaryingsType(varyingsType);
#endif
}
