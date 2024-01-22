Shader "Unlit/BillboardGrass"
{
    Properties
    {
        [NoScaleOffset] _GrassTexure ("GrassTexture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ColorMultiplier ("ColorMultiplier", Range(0.1, 1.0)) = 1.0
        _AlphaClip ("AlphaClip", Range(0.1, 1.0)) = 0.5
    }
    SubShader
    {
        Cull off
        Zwrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc" 
            #include "Assets/Shaders/Grass/Random.cginc"

            struct VertexData
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float noiseVal : TEXCOORD2;
            };

            struct GrassData {
                float3 pos;
                float4 rot;
                float3 scale;
                float2 uv;
            };

            sampler2D _GrassTexure;
            float4 _GrassTexure_ST;
            float4 _Color;
            float _ColorMultiplier;
            float _AlphaClip;
            StructuredBuffer<GrassData> positionBuffer;

            float4 RotateAroundYInDegrees(float4 vertex, float degrees) {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }

            v2f vert (VertexData v, uint instanceID : SV_INSTANCEID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                float4 grassPosition = float4(positionBuffer[instanceID].pos.xyz, 1.0f);

                float idHash = randValue(abs(grassPosition.x * 10000 + grassPosition.y * 100 + grassPosition.z * 0.05 + 2));
                idHash = randValue(idHash * 100000);
                
                float4 worldUV = float4(positionBuffer[instanceID].uv, 0, 0);
                float4 localPosition = RotateAroundYInDegrees(v.vertex, 0);

                //localPosition.xz += lerp(0.5f, 1.0f, idHash) * (v.uv.y * v.uv.y);
                
                float4 worldPosition = float4(grassPosition.xyz + localPosition, 1.0f);
                o.vertex = UnityObjectToClipPos(worldPosition);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_GrassTexure, i.uv);
                clip(-(_AlphaClip - col.a));
                return col;
            }
            ENDCG
        }
    }
}
