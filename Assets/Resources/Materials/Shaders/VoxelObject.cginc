#include "UnityCG.cginc"
#define M_PI 3.1415926535897932384626433832795

float _VoxelateX;
float _VoxelateY;
float _VoxelateZ;
float _VoxelateStrength;

float _Gradient;

float _BaseLight;
float _AmbientLight;
float _Sunshine;
float _Specular;
float _EdgeSharp;
float _Attenuation;

sampler2D _MainTex;
float4 _MainTex_ST;

static float2 _TextureSize = float2(64, 64);
static float2 _SubImageSize = float2(16, 8);

static float2 _UVAtlasOnePixel = 1.0f / _TextureSize;
static float2 _UVAtlasHalfPixel = _UVAtlasOnePixel / 2;
static float _ClampOffset = 0.0001;
static fixed4 red = fixed4(1, 0, 0, 1);
static float3 _SunPos = normalize(float3(0, 0, 1));

static float _NormalCodeMaxValue = 9;
static float _VoxelDepthMaxValue = 100;

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
	float3 objNormal : NORMAL1;
	float3 uvAtlas : POSITION2;
	float3 uvPixel : POSITION3;
};

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

inline float if_eq(float x, float y)
{
	return 1.0 - abs(sign(x - y));
}

inline float if_neq(float x, float y)
{
	return abs(sign(x - y));
}

inline float if_gt(float x, float y)
{
	return max(sign(x - y), 0.0);
}

inline float if_lt(float x, float y)
{
	return max(sign(y - x), 0.0);
}

inline float if_else(float testValue, float ifExpr, float elseExpr)
{
	return elseExpr + (if_neq(testValue, 0) * (ifExpr - elseExpr));
}

inline float3 uvClamped(v2f i)
{
	float diffX = i.uvAtlas.x - i.uvPixel.x;
	float diffY = i.uvAtlas.y - i.uvPixel.y;
	float3 uvAtlasClamped = i.uvAtlas;
	uvAtlasClamped.x -= if_gt(diffX, _UVAtlasOnePixel.x - _ClampOffset) * _UVAtlasHalfPixel.x;
	uvAtlasClamped.y -= if_gt(diffY, _UVAtlasOnePixel.y - _ClampOffset) * _UVAtlasHalfPixel.y;
	uvAtlasClamped.x += if_lt(diffX, _ClampOffset) * _UVAtlasHalfPixel.x;
	uvAtlasClamped.y += if_lt(diffY, _ClampOffset) * _UVAtlasHalfPixel.y;
	return uvAtlasClamped;
}

inline int isBackface(float4 vertex, float3 worldNormal)
{
	float3 worldPos = mul(_Object2World, vertex).xyz;
	float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
    return if_else(if_gt(dot(worldNormal, worldViewDir), 0), 0, 1); 
}

inline v2f voxelobject_vert(appdata v)
{
	int normalCode = round(v.cubeDesc.b * _NormalCodeMaxValue);
	float voxelDepth = round(v.cubeDesc.a * _VoxelDepthMaxValue);

	v2f o;
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
	o.normal = mul(_Object2World, float4(v.normal, 0)).xyz;
	o.objNormal = normalForCode[normalCode];
	o.uvAtlas = float3(v.uvAtlas, depthForCode[normalCode]);
	o.uvPixel = float3(v.uvPixel, voxelDepth);

	#ifndef NO_CULL
		// For objects with voxelDepth == 0, we don't cull away
		// backface triangles, but flip their normals instead
		int cull = v.cubeDesc.r;
		int backface = isBackface(v.vertex, o.normal);
		o.normal *= if_else(backface, -1, 1);
		o.vertex *= if_else(backface, if_else(cull, 0, 1), 1);
	#endif

	return o;
}

inline fixed4 voxelobject_frag(v2f i)
{
	float3 uvAtlasClamped = uvClamped(i);

	float depth = i.uvPixel.z;
	float3 textureSize = float3(_TextureSize, depth);
	float3 subImageSize = float3(_SubImageSize, depth);

	float3 uvAtlasSubImageSize = subImageSize / textureSize;
	float3 subImageIndex = float3(floor(uvAtlasClamped / uvAtlasSubImageSize).xy, 0);
	float3 uvSubImageBottomLeft = subImageIndex * uvAtlasSubImageSize;

	float3 uvSubImage = (i.uvAtlas - uvSubImageBottomLeft) / uvAtlasSubImageSize;
	float3 voxel = min(uvSubImage * subImageSize, subImageSize - 1);
	float3 uvVoxel = frac(voxel);

 	float isLeftOrRightSide = if_neq(i.objNormal.x, 0);
 	float isBottomOrTopSide = if_neq(i.objNormal.y, 0);
 	float isFrontOrBackSide = if_neq(i.objNormal.z, 0);

	fixed4 c = tex2Dlod(_MainTex, float4(uvAtlasClamped.xy, 0, 0));

	#ifndef NO_DISCARD
		if (c.a == 0) {
			discard;
			return c;
		}
	#endif

	#ifndef NO_LIGHT
		float sunDist = dot(normalize(i.normal), _SunPos);
		float normalizedSunDist = (sunDist + 1) / 2;
		float sunLight = _Sunshine * normalizedSunDist;
		c *= _AmbientLight + sunLight;
		c *= _BaseLight;
	#endif
		
	#ifndef NO_VOXELATE
		int3 voxelate = int3(voxel * float3(_VoxelateX, _VoxelateY, _VoxelateZ));
		c *= 1 + (((voxelate.x + voxelate.y + voxelate.z) % 2) * _VoxelateStrength);
	#endif

	#ifndef NO_SIDESHARP
		c *= 1 + if_else(isFrontOrBackSide, _EdgeSharp, 0);
	#endif

	#ifndef NO_GRADIENT
		c *= if_else(isLeftOrRightSide, 1 - ((1 - uvSubImage.y) * _Gradient), 1);
		c *= if_else(isBottomOrTopSide, 1 - ((1 - uvSubImage.x) * _Gradient), 1);
		c *= if_else(isFrontOrBackSide, 1 - ((1 - uvSubImage.y) * _Gradient), 1);
	#endif

	return c;
}
