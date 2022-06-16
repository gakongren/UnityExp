#ifndef FOW_CGINC_INCLUDED
#define FOW_CGINC_INCLUDED

sampler2D _FOW_PREV_TEX;
sampler2D _FOW_CURRENT_TEX;
float4 _FOW_MAP_AREA;
float _FOW_BLEND_FACTOR;

#if defined(UNITY_NO_FULL_STANDARD_SHADER)
#   define UNITY_STANDARD_SIMPLE 1
#endif

#include "UnityStandardConfig.cginc"
#include "UnityStandardCoreForward.cginc"

float uvInFowArea(float2 fow_uv) {
    float2 is_in_fow = step(fow_uv, 1) - step(fow_uv, 0);
    return is_in_fow.x * is_in_fow.y;
}

float FowFactor(float3 pos){
    float2 fow_uv = (pos.xz - _FOW_MAP_AREA.xy)/_FOW_MAP_AREA.zw;
    float factor = lerp(
        tex2D(_FOW_PREV_TEX, fow_uv).r,
        tex2D(_FOW_CURRENT_TEX, fow_uv).r, 
        _FOW_BLEND_FACTOR
    );
    return uvInFowArea(fow_uv) * factor;
}

#if UNITY_STANDARD_SIMPLE
half4 fragFow (VertexOutputBaseSimple i) : SV_Target 
{ 
    float4 col = fragBase(i);
    FRAGMENT_SETUP(s);
    return float4(col.rgb * lerp(0.5, 1, FowFactor(s.posWorld)), col.a);
    // return col;
}

half4 fragFowDebug(VertexOutputBaseSimple i) : SV_Target
{ 
    float4 col = float4(1, 1, 1, 1);
    FRAGMENT_SETUP(s);
    return float4(col.rgb * lerp(0.5, 1, FowFactor(s.posWorld)), col.a);
}
#else
half4 fragFow(VertexOutputForwardBase i) : SV_Target
{ 
    float4 col = fragBase(i);
    FRAGMENT_SETUP(s);
    return float4(col.rgb * lerp(0.3, 1, FowFactor(s.posWorld)), col.a);
    // return col;
}

half4 fragFowDebug(VertexOutputForwardBase i) : SV_Target
{
    float4 col = float4(1, 1, 1, 1);
    FRAGMENT_SETUP(s);
    return float4(col.rgb * lerp(0.5, 1, FowFactor(s.posWorld)), col.a);
}
#endif

#endif