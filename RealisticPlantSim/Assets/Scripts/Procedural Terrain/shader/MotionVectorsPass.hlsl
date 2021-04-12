#if SHADERPASS != SHADERPASS_MOTION_VECTORS
#error SHADERPASS_is_not_correctly_define
#endif

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/MotionVectorVertexShaderCommon.hlsl"

#if defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)
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
#include "Fragment.hlsl"
