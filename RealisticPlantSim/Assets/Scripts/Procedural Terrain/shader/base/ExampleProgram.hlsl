
#if (SHADERPASS == SHADERPASS_FULL_SCREEN_DEBUG)
    #define DEBUG_DISPLAY
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/DebugDisplay.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Debug/FullScreenDebug.hlsl"
#endif

#if (SHADERPASS == SHADERPASS_MOTION_VECTORS)
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/MotionVectorVertexShaderCommon.hlsl"
#else
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"
#endif

#if (SHADERPASS != SHADERPASS_GBUFFER) && defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalPrepassBuffer.hlsl"
#endif

AttributesMesh VertexProgram(AttributesMesh input)
{
    // Vertex shader.
    return input;
}

void TesselationVertexProgram(inout VaryingsMeshToDS input, const OutputPatch<PackedVaryingsToDS, 3> patch, float3 baryCoords)
{
}

void FragmentProgram(FragInputs input, float3 viewdir, inout PositionInputs posInput, inout SurfaceData surfaceData, inout BuiltinData builtinData)
{
    // Fragment shader.
}

#include "Vertex.hlsl"
#include "Tessellation.hlsl"

#if (SHADERPASS == SHADERPASS_FORWARD)
    #include "FragmentForward.hlsl"
#else
    #include "Fragment.hlsl"
#endif
