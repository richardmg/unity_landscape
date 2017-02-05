#include "TestFunctions.cginc"

#define M_PI 3.1415926535897932384626433832795

static float2 _TextureSize = float2(1024, 512);
static float2 _SubImageSize = float2(1024, 512);
static float2 _UVAtlasOnePixel = 1.0f / _TextureSize;
static float2 _UVAtlasHalfPixel = _UVAtlasOnePixel / 2;
static float _ClampOffset = 0.00001;

inline float2 uvClamped(float2 uvAtlas, float2 uvPixel)
{
	float diffX = uvAtlas.x - uvPixel.x;
	float diffY = uvAtlas.y - uvPixel.y;
	float2 uvAtlasClamped = uvAtlas;
	uvAtlasClamped.x -= if_gt(diffX, _UVAtlasOnePixel.x - _ClampOffset) * _UVAtlasHalfPixel.x;
	uvAtlasClamped.y -= if_gt(diffY, _UVAtlasOnePixel.y - _ClampOffset) * _UVAtlasHalfPixel.y;
	uvAtlasClamped.x += if_lt(diffX, _ClampOffset) * _UVAtlasHalfPixel.x;
	uvAtlasClamped.y += if_lt(diffY, _ClampOffset) * _UVAtlasHalfPixel.y;
	return uvAtlasClamped;
}

inline int isBackface(float4 vertex, float3 worldNormal)
{
	float3 worldPos = mul(unity_ObjectToWorld, vertex).xyz;
	float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
    return if_else(if_gt(dot(worldNormal, worldViewDir), 0), 0, 1); 
}

