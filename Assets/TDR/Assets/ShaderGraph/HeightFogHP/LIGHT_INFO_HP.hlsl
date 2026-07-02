#ifndef LIGHT_INFO_HP

#define LIGHT_INFO_HP


void LightInfo_float(float3 worldPos, out float3 direction, out float3 color, out float shadow)
{
#ifdef SHADERGRAPH_PREVIEW
    direction = normalize(float3(-0.5, 0.5, -0.5));
    color = float3(1, 1, 1);
    shadow = 1;
#else
    Light mainLight = GetMainLight(TransformWorldToShadowCoord(worldPos), worldPos, unity_ProbesOcclusion);
    shadow = mainLight.shadowAttenuation;
    direction = mainLight.direction;
    color = mainLight.color;
#endif
}

void LightInfo_half(float3 worldPos, out float3 direction, out float3 color, out float shadow)
{
#ifdef SHADERGRAPH_PREVIEW
    direction = normalize(float3(-0.5, 0.5, -0.5));
    color = float3(1, 1, 1);
    shadow = 1;
#else
    Light mainLight = GetMainLight(TransformWorldToShadowCoord(worldPos), worldPos, unity_ProbesOcclusion);
    shadow = mainLight.shadowAttenuation;
    direction = mainLight.direction;
    color = mainLight.color;
#endif
}

#endif