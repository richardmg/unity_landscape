
#include "UnityCG.cginc"
#include "VoxelObjectCommon.cginc"

#ifndef NO_LIGHT
#include "UnityLightingCommon.cginc" // for _LightColor0
#endif

#ifndef NO_SELF_SHADOW
// Docs: https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
//#pragma multi_compile_fwdbase
#include "AutoLight.cginc"
#endif

float _Gradient;

sampler2D _MainTex;
sampler2D _DetailTex;

float4 _MainTex_ST;

static fixed4 red = fixed4(1, 0, 0, 1);

static float _NormalCodeMaxValue = 9;
static float _VoxelDepthMaxValue = 100;

// We only set correct normals for the side exclusive vertices
// to be able to determine correct normals after interpolation
// in the fragment shader.
static float3 normalForCode[10] = {
	float3(-1, 0, 0),
	float3(-1, 0, 0),
	float3(1, 0, 0),
	float3(1, 0, 0),
	float3(0, -1, 0),
	float3(0, -1, 0),
	float3(0, 1, 0),
	float3(0, 1, 0),
	float3(0, 0, -1),
	float3(0, 0, 1)
};

static float depthForCode[10] = { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };

// ---------------------------------------------------------------

struct appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uvAtlas: TEXCOORD0;
	float2 uvPixel : TEXCOORD1;
	float4 cubeDesc : COLOR;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	float3 normal : NORMAL;
	float3 uvAtlas : POSITION2;
	float3 uvPixel : POSITION3;

#ifndef NO_DETAILS
	float3 objNormal : NORMAL1;
#endif

#ifndef NO_LIGHT
	fixed4 diffuse : COLOR0;
	fixed3 ambient : COLOR1;
#endif

#ifndef NO_SELF_SHADOW
	// Put shadows data into TEXCOORD1
	SHADOW_COORDS(1)
#endif
};

// ---------------------------------------------------------------

v2f vert(appdata v)
{
	int normalCode = round(v.cubeDesc.b * _NormalCodeMaxValue);
	float voxelDepth = round(v.cubeDesc.a * _VoxelDepthMaxValue);

	v2f o;
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
	o.normal = mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz;
	o.uvAtlas = float3(v.uvAtlas, depthForCode[normalCode]);
	o.uvPixel = float3(v.uvPixel, voxelDepth);

#ifndef NO_DETAILS
	o.objNormal = normalForCode[normalCode];
#endif

#ifndef NO_CULL
	int cull = v.cubeDesc.r;
	int backface = isBackface(v.vertex, o.normal);
	o.normal *= if_else(backface, -1, 1);
	o.vertex *= if_else(backface, if_else(cull, 0, 1), 1);
#endif

#ifndef NO_LIGHT
	half lightStrength0 = max(0, dot(o.normal, _WorldSpaceLightPos0.xyz));
	o.diffuse = lightStrength0 * _LightColor0;
    o.ambient = ShadeSH9(half4(o.normal, 1));
#endif

#ifndef NO_SELF_SHADOW
	TRANSFER_SHADOW(o);
#endif

	return o;
}

// ---------------------------------------------------------------

fixed4 frag(v2f i) : SV_Target
{
	float3 uvAtlasClamped = float3(uvClamped(i.uvAtlas.xy, i.uvPixel.xy), i.uvAtlas.z);
	fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy, 0, 0));

#ifndef NO_DETAILS
	float depth = i.uvPixel.z;
	float3 textureSize = float3(_TextureSize, depth);
	float3 subImageSize = float3(_SubImageSize, depth);

	float3 uvAtlasSubImageSize = subImageSize / textureSize;
	float3 subImageIndex = float3(floor(uvAtlasClamped / uvAtlasSubImageSize).xy, 0);
	float3 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;

	float3 uvSubImage = (i.uvAtlas - uvSubImageBottomLeft) / uvAtlasSubImageSize;
	float3 voxel = min(uvSubImage * subImageSize, subImageSize - _ClampOffset);
	float3 uvVoxel = frac(voxel);

 	float isLeftOrRightSide = if_neq(i.objNormal.x, 0);
 	float isBottomOrTopSide = if_neq(i.objNormal.y, 0);
 	float isFrontOrBackSide = if_neq(i.objNormal.z, 0);

//	if (isFrontOrBackSide)
//		c = tex2Dlod(_DetailTex, float4(uvVoxel.xy, 0, 0));
//	else if (isLeftOrRightSide)
//		c = tex2Dlod(_DetailTex, float4(uvVoxel.zy, 0, 0));
//	else
//		c = tex2Dlod(_DetailTex, float4(uvVoxel.xz, 0, 0));
#endif

#ifndef NO_DISCARD
	if (c.a == 0) {
		discard;
		return c;
	}
#endif

#ifndef NO_GRADIENT
	c *= if_else(isLeftOrRightSide, 1 - ((1 - uvSubImage.y) * _Gradient), 1);
	c *= if_else(isBottomOrTopSide, 1 - ((1 - uvSubImage.x) * _Gradient), 1);
	c *= if_else(isFrontOrBackSide, 1 - ((1 - uvSubImage.y) * _Gradient), 1);
#endif

#ifndef NO_LIGHT
	c.rgb *= (i.diffuse + i.ambient);
#endif

#ifndef NO_SELF_SHADOW
	c.rgb *= SHADOW_ATTENUATION(i);
#endif

	return c;
}
