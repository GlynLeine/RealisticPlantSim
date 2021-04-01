
// Fragment shader main function
void Frag(  PackedVaryingsToPS packedInput,
            // Automatically sort out output buffers based on what shaderpass this is.
            OUTPUT_GBUFFER(outGBuffer)
            #ifdef _DEPTHOFFSET_ON
            , out float outputDepth : SV_Depth
            #endif
        )
{
    FragInputs input = UnpackVaryingsMeshToFragInputs(packedInput.vmesh);

    // input.positionSS is SV_Position
    PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

    #ifdef VARYINGS_NEED_POSITION_WS
        float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
    #else
        // Unused
        float3 V = float3(1.0, 1.0, 1.0); // Avoid the division by 0
    #endif

    SurfaceData surfaceData;
    BuiltinData builtinData;

    #ifdef FRAGMENT_NO_DEFAULT_INPUTS
        FragmentProgram(input, V, posInput, surfaceData, builtinData);
    #else
        GetSurfaceAndBuiltinData(input, V, posInput, surfaceData, builtinData);
        FragmentProgram(input, V, posInput, surfaceData, builtinData);
    #endif

    // Automatically sort out how to write to the right output buffers based on which shaderpass this is.
    ENCODE_INTO_GBUFFER(surfaceData, builtinData, posInput.positionSS, outGBuffer);

    #ifdef _DEPTHOFFSET_ON
        outputDepth = posInput.deviceDepth;
    #endif
}