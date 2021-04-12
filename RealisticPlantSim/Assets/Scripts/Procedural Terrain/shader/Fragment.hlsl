
// Fragment shader main function
void Frag(  PackedVaryingsToPS packedInput
            #if (SHADERPASS == SHADERPASS_GBUFFER)
                // Automatically sort out output buffers based on what shaderpass this is.
                , OUTPUT_GBUFFER(outGBuffer)
            #else
                #ifdef WRITE_MSAA_DEPTH
                    // We need the depth color as SV_Target0 for alpha to coverage
                    , out float4 depthColor : SV_Target0
                    #if (SHADERPASS == SHADERPASS_MOTION_VECTORS)
                        , out float4 outMotionVector : SV_Target1
                    #endif
                #elif (SHADERPASS == SHADERPASS_MOTION_VECTORS)
                    // When no MSAA, the motion vector is always the first buffer
                    , out float4 outMotionVector : SV_Target0
                #endif
                
                // Decal buffer must be last as it is bind but we can optionally write into it (based on _DISABLE_DECALS)
                #ifdef WRITE_NORMAL_BUFFER
                    , out float4 outNormalBuffer : SV_TARGET_NORMAL
                #endif

                // Decal buffer must be last as it is bind but we can optionally write into it (based on _DISABLE_DECALS)
                #if defined(WRITE_DECAL_BUFFER) && !defined(_DISABLE_DECALS)
                , out float4 outDecalBuffer : SV_TARGET_DECAL
                #endif
            #endif

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

    #ifndef FRAGMENT_NO_DEFAULT_INPUTS
            GetSurfaceAndBuiltinData(input, V, posInput, surfaceData, builtinData);
    #endif

    #if (SHADERPASS != SHADERPASS_DEPTH_ONLY && SHADERPASS != SHADERPASS_SHADOWS)
        FragmentProgram(input, V, posInput, surfaceData, builtinData);
    #endif

    #if (SHADERPASS == SHADERPASS_GBUFFER)
        ENCODE_INTO_GBUFFER(surfaceData, builtinData, posInput.positionSS, outGBuffer);                 
    #else
        #if (SHADERPASS == SHADERPASS_MOTION_VECTORS)
            VaryingsPassToPS inputPass = UnpackVaryingsPassToPS(packedInput.vpass);                            
            #ifdef _DEPTHOFFSET_ON                                                                              
                inputPass.positionCS.w += builtinData.depthOffset;                                              
                inputPass.previousPositionCS.w += builtinData.depthOffset;                                      
            #endif                                                                                              
                                                                                                                
            float2 motionVector = CalculateMotionVector(inputPass.positionCS, inputPass.previousPositionCS);    
                                                                                                                
            EncodeMotionVector(motionVector * 0.5, outMotionVector);                                            
                                                                                                                
            bool forceNoMotion = unity_MotionVectorsParams.y == 0.0;                                            
                                                                                                                
            if (forceNoMotion)                                                                                  
                outMotionVector = float4(2.0, 0.0, 0.0, 0.0);     
        #endif

        #ifdef WRITE_MSAA_DEPTH                                                                             
            depthColor = packedInput.vmesh.positionCS.z;                                                    
                                                                                                            
            #ifdef _ALPHATOMASK_ON                                                                          
                depthColor.a = SharpenAlpha(builtinData.opacity, builtinData.alphaClipTreshold);            
            #endif                                                                                          
        #endif                                                                                              
                                                                                                        
        #if defined(WRITE_NORMAL_BUFFER)                                                                    
            EncodeIntoNormalBuffer(ConvertSurfaceDataToNormalData(surfaceData), outNormalBuffer);           
        #endif                                                                                              
                                                                                                            
        #if defined(WRITE_DECAL_BUFFER)                                                                     
            DecalPrepassData decalPrepassData;                                                              
            #ifdef _DISABLE_DECALS                                                                          
                ZERO_INITIALIZE(DecalPrepassData, decalPrepassData);                                       
            #else                                                                                           
                decalPrepassData.geomNormalWS = surfaceData.geomNormalWS;                                  
                decalPrepassData.decalLayerMask = GetMeshRenderingDecalLayer();                             
            #endif                                                                                          
            EncodeIntoDecalPrepassBuffer(decalPrepassData, outDecalBuffer);                                 
        #endif
    #endif

    #ifdef _DEPTHOFFSET_ON
        outputDepth = posInput.deviceDepth;
    #endif
}