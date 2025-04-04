Shader "Hidden/SC Post Effects/Caustics"
{
	HLSLINCLUDE

	#define REQUIRE_DEPTH
	#include "../../Shaders/Pipeline/Pipeline.hlsl"

	TEXTURE2D(_CausticsTex);
	SAMPLER(sampler_CausticsTex);
	
	float4 _FadeParams;
	float4 _CausticsParams;
	float4 _HeightParams;
	float _LuminanceThreshold;

	float4x4 unity_WorldToLight;

	#define MIN_HEIGHT _HeightParams.x
	#define MIN_HEIGHT_FALLOFF _HeightParams.y - 0.01
	
	#define MAX_HEIGHT _HeightParams.z
	#define MAX_HEIGHT_FALLOFF _HeightParams.w + 0.01

	float HeightRangeMask(float height, float minHeight, float minHeightFalloff, float maxHeight, float maxHeightFalloff)
	{
		float minEnd = minHeight - minHeightFalloff;
		float min = saturate((minEnd - (height - minHeight) ) / (minEnd-minHeight));
		float maxEnd = maxHeight + maxHeightFalloff;
		float max = saturate((maxEnd - (height - maxHeight) ) / (maxEnd-maxHeight));

		return saturate(max * (min));
	}
	
	float4 Frag(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		half4 sceneColor = SCREEN_COLOR(SCREEN_COORDS);
		float depth = SAMPLE_DEPTH(SCREEN_COORDS);
		float3 worldPos = GetWorldPosition(SCREEN_COORDS, depth);

		float2 projection = worldPos.xz;
		if(_CausticsParams.z == 1) projection = mul((float4x4)unity_WorldToLight, float4(worldPos, 1.0)).xy; 

		float2 uv = projection * _CausticsParams.x;

		float3 caustics1 = SAMPLE_TEXTURE2D_LOD(_CausticsTex, sampler_CausticsTex, uv + (_Time.y * float2(_CausticsParams.y, _CausticsParams.y)), 0).rgb;
		float3 caustics2 = SAMPLE_TEXTURE2D_LOD(_CausticsTex, sampler_CausticsTex, (uv * 0.8) -(_Time.y * float2(_CausticsParams.y, _CausticsParams.y)), 0).rgb;

		#if UNITY_COLORSPACE_GAMMA
		caustics1 = SRGBToLinear(caustics1);
		caustics2 = SRGBToLinear(caustics2);
		#endif
		
		float3 caustics = min(caustics1, caustics2);
		//return float4(caustics.rgb, 1.0);
		
		//Height range filter
		float heightMask = HeightRangeMask(worldPos.y, MIN_HEIGHT, MIN_HEIGHT_FALLOFF, MAX_HEIGHT, MAX_HEIGHT_FALLOFF);
		//return heightMask;

		//Intensity
		caustics *= _CausticsParams.w;
		
		float luminance = smoothstep(0,  _LuminanceThreshold, Luminance(sceneColor));
		caustics *= heightMask * luminance;
		
		//Clip skybox
		if (LINEAR_DEPTH(depth) > 0.99) caustics = 0;

		float fadeDist = LinearDistanceFade(worldPos, _FadeParams.x, _FadeParams.y, 0.0, _FadeParams.w);
		//return fadeDist;
		caustics *= fadeDist;

		return float4(sceneColor.rgb + caustics.rgb, sceneColor.a);
	}


	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "Caustics"
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}
	}
}