// 创建 Unlit Shader 命名为 "PulseShader"
Shader "Unlit/PulseShader" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Speed ("Pulse Speed", Range(0,3)) = 2
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

            #define PI 3.14159265359
            
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
                float pulse = fmod(_Time.y*_Speed,1.0) * 0.5 + 0.1;
                fixed4 col = _Color;
                float d = sdCircle(i.uv*2 -1.0, pulse);
                pulse-=0.1;
                col.a *= (1.0-pulse*1.6); // fade out the color
                float4 color = 0.0;//d>0.0 ? fixed4(0,0,0,0) : col;
                color = lerp(color, fixed4(0.9,0.9,0.9,col.a), 1.0-smoothstep(0.0, 0.1, abs(d)));
                return color;
            }
            ENDCG
        }
    }
}