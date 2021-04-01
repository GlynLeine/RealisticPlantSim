
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
