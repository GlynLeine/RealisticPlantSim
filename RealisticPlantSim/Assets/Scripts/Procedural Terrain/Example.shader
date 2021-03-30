Shader "HDRP/Custom/Example"
{
    Properties
    {
        // TODO
    }
    HLSLINCLUDE

    #pragma target 4.6

    // Indicate that this shader requires tessellation hardware.
    #pragma require tessellation tessHW

    // Report what features this shader supports.
    {
        #pragma shader_feature_local _ALPHATEST_ON
        #pragma shader_feature_local _DEPTHOFFSET_ON
        #pragma shader_feature_local _DOUBLESIDED_ON
        #pragma shader_feature_local _ _TESSELLATION_DISPLACEMENT _PIXEL_DISPLACEMENT
        #pragma shader_feature_local _VERTEX_DISPLACEMENT_LOCK_OBJECT_SCALE
        #pragma shader_feature_local _DISPLACEMENT_LOCK_TILING_SCALE
        #pragma shader_feature_local _PIXEL_DISPLACEMENT_LOCK_OBJECT_SCALE
        #pragma shader_feature_local _TESSELLATION_PHONG
        #pragma shader_feature_local _ _REFRACTION_PLANE _REFRACTION_SPHERE _REFRACTION_THIN

        #pragma shader_feature_local _ _EMISSIVE_MAPPING_PLANAR _EMISSIVE_MAPPING_TRIPLANAR
        #pragma shader_feature_local _ _MAPPING_PLANAR _MAPPING_TRIPLANAR
        #pragma shader_feature_local _NORMALMAP_TANGENT_SPACE
        #pragma shader_feature_local _ _REQUIRE_UV2 _REQUIRE_UV3

        #pragma shader_feature_local _NORMALMAP
        #pragma shader_feature_local _MASKMAP
        #pragma shader_feature_local _BENTNORMALMAP
        #pragma shader_feature_local _EMISSIVE_COLOR_MAP

        #pragma shader_feature_local _ENABLESPECULAROCCLUSION
        #pragma shader_feature_local _ _SPECULAR_OCCLUSION_NONE _SPECULAR_OCCLUSION_FROM_BENT_NORMAL_MAP

        #ifdef _ENABLESPECULAROCCLUSION
            #define _SPECULAR_OCCLUSION_FROM_BENT_NORMAL_MAP
        #endif

        #pragma shader_feature_local _HEIGHTMAP
        #pragma shader_feature_local _TANGENTMAP
        #pragma shader_feature_local _ANISOTROPYMAP
        #pragma shader_feature_local _DETAIL_MAP
        #pragma shader_feature_local _SUBSURFACE_MASK_MAP
        #pragma shader_feature_local _THICKNESSMAP
        #pragma shader_feature_local _IRIDESCENCE_THICKNESSMAP
        #pragma shader_feature_local _SPECULARCOLORMAP
        #pragma shader_feature_local _TRANSMITTANCECOLORMAP

        #pragma shader_feature_local _DISABLE_DECALS
        #pragma shader_feature_local _DISABLE_SSR
        #pragma shader_feature_local _ADD_PRECOMPUTED_VELOCITY
        #pragma shader_feature_local _ENABLE_GEOMETRIC_SPECULAR_AA

        // MaterialFeature are used as shader feature to allow compiler to optimize properly
        #pragma shader_feature_local _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
        #pragma shader_feature_local _MATERIAL_FEATURE_TRANSMISSION
        #pragma shader_feature_local _MATERIAL_FEATURE_ANISOTROPY
        #pragma shader_feature_local _MATERIAL_FEATURE_CLEAR_COAT
        #pragma shader_feature_local _MATERIAL_FEATURE_IRIDESCENCE
        #pragma shader_feature_local _MATERIAL_FEATURE_SPECULAR_COLOR
    }

    // Enable dithering LOD crossfade
    #pragma multi_compile _ LOD_FADE_CROSSFADE
    
    //enable GPU instancing support
    #pragma multi_compile_instancing
    #pragma instancing_options renderinglayer
    #pragma multi_compile _ DOTS_INSTANCING_ON

    // Shader defines
    {
        #define TESSELLATION_ON

        // This shader support vertex modification
        #define HAVE_VERTEX_MODIFICATION
        #define HAVE_TESSELLATION_MODIFICATION

        // If we use subsurface scattering, enable output split lighting (for forward pass)
        #if defined(_MATERIAL_FEATURE_SUBSURFACE_SCATTERING)
            #define OUTPUT_SPLIT_LIGHTING
        #endif
    }

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitProperties.hlsl"

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"

    ENDHLSL

    SubShader
    {
        Tags // Mark shader as a HDRP lit shader.
        {
            "RenderPipeline" = "HDRenderPipeline"
            "RenderType" = "HDLitShader"
        }

        UsePass "HDRP/LitTessellation/ScenePickingPass"
        UsePass "HDRP/LitTessellation/SceneSelectionPass"

        pass // Geometry buffer pass to supply deferred lighting data.
        {
            Name "GBuffer"
            Tags{"LightMode" = "GBuffer"}

            // Apply default face culling and depth testing behaviour.
            Cull[_Cull]
            ZTest[_ZWrite]

            // Setup stencil buffer to write "disableSSR" to prevent SSR in the forward pass.
            Stencil
            {
                WriteMask [_StencilWriteMaskGBuffer]
                Ref  [_StencilRefGBuffer]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM

            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            // Setup DECALS_OFF so the shader stripper can remove variants.
            #pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT
            #pragma multi_compile _ LIGHT_LAYERS

            #define SHADERPASS SHADERPASS_GBUFFER

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitSharePass.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"

            #include "GBufferPass.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma hull Hull
            #pragma domain Domain

            ENDHLSL
        }

        UsePass "HDRP/LitTessellation/META"

        Pass // Shadow pass that renders only depth information to the shadow maps.
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            Cull[_CullMode]

            ZClip [_ZClip]
            ZWrite On
            ZTest LEqual

            ColorMask 0

            HLSLPROGRAM

            #define SHADERPASS SHADERPASS_SHADOWS
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitDepthPass.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"

            #include "DepthOnlyPass.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma hull Hull
            #pragma domain Domain

            ENDHLSL
        }
        
        Pass
        {
            Name "MotionVectors"
            Tags{ "LightMode" = "MotionVectors" } // Caution, this need to be call like this to setup the correct parameters by C++ (legacy Unity)

            // If velocity pass (motion vectors) is enabled we tag the stencil so it don't perform CameraMotionVelocity
            Stencil
            {
                WriteMask [_StencilWriteMaskMV]
                Ref [_StencilRefMV]
                Comp Always
                Pass Replace
            }

            Cull[_CullMode]
            AlphaToMask [_AlphaToMask]

            ZWrite On

            HLSLPROGRAM
            #pragma multi_compile _ WRITE_NORMAL_BUFFER
            #pragma multi_compile _ WRITE_DECAL_BUFFER
            #pragma multi_compile _ WRITE_MSAA_DEPTH

            #define SHADERPASS SHADERPASS_MOTION_VECTORS
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
            #ifdef WRITE_NORMAL_BUFFER // If enabled we need all regular interpolator
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitSharePass.hlsl"
            #else
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/ShaderPass/LitMotionVectorPass.hlsl"
            #endif
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl"
            
            #include "MotionVectorsPass.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma hull Hull
            #pragma domain Domain

            ENDHLSL
        }
        
    }
    
    CustomEditor "Rendering.HighDefinition.LitGUI"
}