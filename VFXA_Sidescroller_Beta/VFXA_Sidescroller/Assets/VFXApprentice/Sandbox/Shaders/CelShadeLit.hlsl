void CelShadeLit_float(in float3 normal, in float cel_ramp_smoothness, in float3 clip_space_position,
    in float3 world_position, in float4 cel_ramp_tint, in float cel_ramp_offset, out float3 cel_ramp_output, out float3 direction)
{
    #ifdef SHADERGRAPH_PREVIEW
        cel_ramp_output = float3(0.5, 0.5, 0);
        direction = float3(0.5, 0.5, 0);
    #else
        #if SHADOWS_SCREEN
            half4 shadow_coord = ComputeScreenPos(clip_space_position);
        #else
            half4 shadow_coord = TransformWorldToShadowCoord(world_position);
        #endif

        #if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS
            Light light = GetMainLight(shadow_coord);
        #else
            Light light = GetMainLight();
        #endif

        half d = dot(normal, light.direction) * 0.5 + 0.5;

        half cel_ramp = smoothstep(cel_ramp_offset, cel_ramp_offset + cel_ramp_smoothness, d);

        cel_ramp *= light.shadowAttenuation;

        cel_ramp_output = light.color * (cel_ramp + cel_ramp_tint);

        direction = light.direction;
        
    #endif
}