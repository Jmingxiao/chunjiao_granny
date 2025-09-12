Shader "Custom/TexturedRandomPattern" {
    Properties {
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        _Density ("Density", Range(1, 50)) = 10
        _PatternScale ("Pattern Scale", Range(0.1, 2)) = 0.5
        _RotationVariation ("Rotation Variation", Range(0, 1)) = 0.2
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _PatternTex;
            float4 _PatternTex_ST;
            fixed4 _BackgroundColor;
            float _Density;
            float _PatternScale;
            float _RotationVariation;

            // 改进版随机函数
            float hash(float2 st) {
                return frac(sin(dot(st, float2(12.9898,78.233)))*43758.5453123);
            }

            // 旋转矩阵
            float2 rotateUV(float2 uv, float angle) {
                float sina, cosa;
                sincos(angle, sina, cosa);
                return float2(
                    cosa * uv.x - sina * uv.y,
                    sina * uv.x + cosa * uv.y
                );
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 计算平铺网格
                float2 tiledUV = i.uv * _Density;
                float2 cell = floor(tiledUV);
                float2 localUV = frac(tiledUV);

                // 生成每个单元格的随机特征
                float3 rand = hash(cell);
                
                // 计算图案UV变换
                float2 patternUV = (localUV - 0.5) * 2.0; // [-1,1]空间
                
                // 应用随机旋转
                float maxRotation = _RotationVariation * 6.283; // 0到2PI
                patternUV = rotateUV(patternUV, rand.x * maxRotation);
                
                // 应用缩放
                patternUV *= lerp(0.8, 1.2, rand.y) * _PatternScale;
                
                // 转换为纹理采样坐标
                float2 sampleUV = patternUV * 0.5 + 0.5; // 转换回[0,1]范围

                // 采样图案纹理
                fixed4 pattern = tex2D(_PatternTex, sampleUV);
                
                // 混合背景色
                return lerp(_BackgroundColor, pattern, pattern.a);
            }
            ENDCG
        }
    }
}