Shader "Unlit/StencilGeom"
{
    Properties
    {
[IntRange] _StencilID ("Stencil ID", Range(0,255))= 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100

        Pass
        {
           Blend Zero One
           Zwrite Off

            Stencil
            {
                Ref [_StencilID]
                Comp Always
                Pass Replace
            }
        }
    }
}
