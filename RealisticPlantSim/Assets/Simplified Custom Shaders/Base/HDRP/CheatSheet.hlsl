/// <summary>
/// This is a cheat-sheet and not made actual use. Including this file into any shader will likely cause errors.
/// The definition of any commonly used unity shader data-structures are copied into this file for reference.
/// Author: Glyn Marcus Leine
/// </summary>

struct AttributesMesh
{
    float3 positionOS   : POSITION;
#ifdef ATTRIBUTES_NEED_NORMAL
    float3 normalOS     : NORMAL;
#endif
#ifdef ATTRIBUTES_NEED_TANGENT
    float4 tangentOS    : TANGENT; // Store sign in w
#endif
#ifdef ATTRIBUTES_NEED_TEXCOORD0
    float2 uv0          : TEXCOORD0;
#endif
#ifdef ATTRIBUTES_NEED_TEXCOORD1
    float2 uv1          : TEXCOORD1;
#endif
#ifdef ATTRIBUTES_NEED_TEXCOORD2
    float2 uv2          : TEXCOORD2;
#endif
#ifdef ATTRIBUTES_NEED_TEXCOORD3
    float2 uv3          : TEXCOORD3;
#endif
#ifdef ATTRIBUTES_NEED_COLOR
    float4 color        : COLOR;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Varying for domain shader
// Position and normal are always present (for tessellation) and in world space
struct VaryingsMeshToDS
{
    float3 positionRWS;
    float3 normalWS;
#ifdef VARYINGS_DS_NEED_TANGENT
    float4 tangentWS;
#endif
#ifdef VARYINGS_DS_NEED_TEXCOORD0
    float2 texCoord0;
#endif
#ifdef VARYINGS_DS_NEED_TEXCOORD1
    float2 texCoord1;
#endif
#ifdef VARYINGS_DS_NEED_TEXCOORD2
    float2 texCoord2;
#endif
#ifdef VARYINGS_DS_NEED_TEXCOORD3
    float2 texCoord3;
#endif
#ifdef VARYINGS_DS_NEED_COLOR
    float4 color;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct PackedVaryingsMeshToDS
{
    float3 interpolators0 : INTERNALTESSPOS; // positionRWS
    float3 interpolators1 : NORMAL; // NormalWS

#ifdef VARYINGS_DS_NEED_TANGENT
    float4 interpolators2 : TANGENT;
#endif

    // Allocate only necessary space if shader compiler in the future are able to automatically pack
#ifdef VARYINGS_DS_NEED_TEXCOORD1
    float4 interpolators3 : TEXCOORD0;
#elif defined(VARYINGS_DS_NEED_TEXCOORD0)
    float2 interpolators3 : TEXCOORD0;
#endif

#ifdef VARYINGS_DS_NEED_TEXCOORD3
    float4 interpolators4 : TEXCOORD1;
#elif defined(VARYINGS_DS_NEED_TEXCOORD2)
    float2 interpolators4 : TEXCOORD1;
#endif

#ifdef VARYINGS_DS_NEED_COLOR
    float4 interpolators5 : TEXCOORD2;
#endif

     UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct PackedVaryingsToDS
{
    PackedVaryingsMeshToDS vmesh;
};

struct FragInputs
{
    // Contain value return by SV_POSITION (That is name positionCS in PackedVarying).
    // xy: unormalized screen position (offset by 0.5), z: device depth, w: depth in view space
    // Note: SV_POSITION is the result of the clip space position provide to the vertex shaders that is transform by the viewport
    float4 positionSS; // In case depth offset is use, positionRWS.w is equal to depth offset
    float3 positionRWS; // Relative camera space position
    float4 texCoord0;
    float4 texCoord1;
    float4 texCoord2;
    float4 texCoord3;
    float4 color; // vertex color

    // TODO: confirm with Morten following statement
    // Our TBN is orthogonal but is maybe not orthonormal in order to be compliant with external bakers (Like xnormal that use mikktspace).
    // (xnormal for example take into account the interpolation when baking the normal and normalizing the tangent basis could cause distortion).
    // When using tangentToWorld with surface gradient, it doesn't normalize the tangent/bitangent vector (We instead use exact same scale as applied to interpolated vertex normal to avoid breaking compliance).
    // this mean that any usage of tangentToWorld[1] or tangentToWorld[2] outside of the context of normal map (like for POM) must normalize the TBN (TCHECK if this make any difference ?)
    // When not using surface gradient, each vector of tangentToWorld are normalize (TODO: Maybe they should not even in case of no surface gradient ? Ask Morten)
    float3x3 tangentToWorld;

    uint primitiveID; // Only with fullscreen pass debug currently - not supported on all platforms

    // For two sided lighting
    bool isFrontFace;
};

struct PositionInputs
{
    float3 positionWS;  // World space position (could be camera-relative)
    float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    float  linearDepth; // View space Z coordinate                              : [Near, Far]
};

struct SurfaceData
{
    uint materialFeatures;
    real3 baseColor;
    real specularOcclusion;
    float3 normalWS;
    real perceptualSmoothness;
    real ambientOcclusion;
    real metallic;
    real coatMask;
    real3 specularColor;
    uint diffusionProfileHash;
    real subsurfaceMask;
    real thickness;
    float3 tangentWS;
    real anisotropy;
    real iridescenceThickness;
    real iridescenceMask;
    real3 geomNormalWS;
    real ior;
    real3 transmittanceColor;
    real atDistance;
    real transmittanceMask;
};

struct BuiltinData
{
    real opacity;
    real alphaClipTreshold;
    real3 bakeDiffuseLighting;
    real3 backBakeDiffuseLighting;
    real shadowMask0;
    real shadowMask1;
    real shadowMask2;
    real shadowMask3;
    real3 emissiveColor;
    real2 motionVector;
    real2 distortion;
    real distortionBlur;
    uint renderingLayers;
    float depthOffset;
    real4 vtPackedFeedback;
};
