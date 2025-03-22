// 创建 Unlit Shader 命名为 "PulseShader"
Shader "Unlit/PulseShader" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Speed ("Pulse Speed", Range(1,5)) = 2
        _radius ("Radius", Range(0,1)) = 0.5
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _Speed;
            float _radius;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float sdCircle( float2 p, float r )
            {
                return length(p) - r;
            }

            fixed4 frag (v2f i) : SV_Target {
                float d = sdCircle(i.uv*2 -1.0, _radius);
                // 计算脉冲效果
                float pulse = sin(_Time.y * _Speed) * 0.5 + 0.5;
                fixed4 col = _Color;
                col.a *= pulse * 0.8; // 控制透明度变化幅度
                float4 color = d>0.0 ? fixed4(0,0,0,0) : col;
                color = lerp(color, fixed4(0.8,0.8,0.8,col.a), 1.0-smoothstep(0.0, 0.1, abs(d)));
                return color;
            }
            ENDCG
        }
    }
}