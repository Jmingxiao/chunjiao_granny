// This shader draws a texture on the mesh.
Shader "Example/URPUnlitShaderTexture"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
         _BaseMap("Base Map", 2D) = "white" {}
         _BaseMap1("Base Map", 2D) = "white" {}
         _BaseMap2("Base Map", 2D) = "white" {}

        _Blend("Blend", Range(0, 3)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            // This macro declares _BaseMap as a Texture2D object.
            sampler2D _BaseMap;
            sampler2D _BaseMap1;
            sampler2D _BaseMap2;

            
            CBUFFER_START(UnityPerMaterial)
            int _Blend;
            CBUFFER_END

            Varyings vert(Attributes i)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
                // The TransformObjectToHClip function transforms the vertex position
                OUT.uv = i.uv;
                return OUT;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // The tex2d function samples the texture using the UV coordinates
                half4 color = tex2D(_BaseMap, i.uv);
                half4 color1 = tex2D(_BaseMap1, i.uv);
                half4 color2 = tex2D(_BaseMap2, i.uv);

                // The tex2d function samples the texture using the UV coordinates
                half blend = (half)_Blend;

                half4 col = step(blend,0.9)*color + step(0.9,blend)*step(blend,1.8)*color1 + step(1.8, blend)*color2;

                return col;
            }
            ENDHLSL
        }
    }
}
