// Dissolve: burns the mesh away along procedural noise, with a glowing
// edge. Shadows dissolve with it. URP 17 (Unity 6).
// Scripts For Unity, MIT licensed.
// https://scriptsforunity.com/shaders/dissolve/
Shader "Scripts For Unity/DissolveShader"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (0.55, 0.62, 0.7, 1)
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Range(0.5, 20)) = 4
        _EdgeWidth ("Edge Width", Range(0.001, 0.2)) = 0.06
        [HDR] _EdgeColor ("Edge Color", Color) = (4, 1.4, 0.3, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float _Dissolve;
            float _NoiseScale;
            float _EdgeWidth;
            half4 _EdgeColor;
            float _Cull;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        float SfuHash13(float3 p)
        {
            p = frac(p * 0.1031);
            p += dot(p, p.zyx + 31.32);
            return frac((p.x + p.y) * p.z);
        }

        float SfuValueNoise(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            float3 u = f * f * (3.0 - 2.0 * f);
            float n000 = SfuHash13(i);
            float n100 = SfuHash13(i + float3(1, 0, 0));
            float n010 = SfuHash13(i + float3(0, 1, 0));
            float n110 = SfuHash13(i + float3(1, 1, 0));
            float n001 = SfuHash13(i + float3(0, 0, 1));
            float n101 = SfuHash13(i + float3(1, 0, 1));
            float n011 = SfuHash13(i + float3(0, 1, 1));
            float n111 = SfuHash13(i + float3(1, 1, 1));
            return lerp(lerp(lerp(n000, n100, u.x), lerp(n010, n110, u.x), u.y),
                        lerp(lerp(n001, n101, u.x), lerp(n011, n111, u.x), u.y), u.z);
        }

        // Four octaves, normalised back to 0..1
        float SfuFractalNoise(float3 p)
        {
            float n = SfuValueNoise(p) * 0.5;
            n += SfuValueNoise(p * 2.03) * 0.25;
            n += SfuValueNoise(p * 4.01) * 0.125;
            n += SfuValueNoise(p * 8.05) * 0.0625;
            return n / 0.9375;
        }

        // How far a point is above the dissolve threshold; negative means burned away
        float SfuDissolveAmount(float3 positionOS)
        {
            return SfuFractalNoise(positionOS * _NoiseScale) - _Dissolve * 1.01;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };

            float4 SfuShadowCoord(float3 positionWS)
            {
            #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                return ComputeScreenPos(TransformWorldToHClip(positionWS));
            #else
                return TransformWorldToShadowCoord(positionWS);
            #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.positionOS = input.positionOS.xyz;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                return output;
            }

            half4 frag(Varyings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                float amount = SfuDissolveAmount(input.positionOS);
                clip(amount);

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                // Back faces show through the holes; light them from their own side
                float3 normalWS = normalize(input.normalWS) * IS_FRONT_VFACE(face, 1.0, -1.0);

                Light mainLight = GetMainLight(SfuShadowCoord(input.positionWS));
                float diffuse = saturate(dot(normalWS, mainLight.direction)) * mainLight.shadowAttenuation;
                float3 color = albedo.rgb * (mainLight.color * diffuse + SampleSH(normalWS));

                // Glowing band just above the threshold
                float edge = (1.0 - smoothstep(0.0, _EdgeWidth, amount)) * step(0.0001, _Dissolve);
                color = lerp(color, _EdgeColor.rgb, edge);

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // Shadows dissolve with the mesh
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            ShadowVaryings ShadowVert(ShadowAttributes input)
            {
                ShadowVaryings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                output.positionCS = positionCS;
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                clip(SfuDissolveAmount(input.positionOS));
                return 0;
            }
            ENDHLSL
        }

        // Depth with the same holes, for the depth texture and depth priming
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            DepthVaryings DepthVert(DepthAttributes input)
            {
                DepthVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half DepthFrag(DepthVaryings input) : SV_Target
            {
                clip(SfuDissolveAmount(input.positionOS));
                return input.positionCS.z;
            }
            ENDHLSL
        }

        // Normals with the same holes, for SSAO
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex NormalsVert
            #pragma fragment NormalsFrag

            struct NormalsAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct NormalsVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
            };

            NormalsVaryings NormalsVert(NormalsAttributes input)
            {
                NormalsVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half4 NormalsFrag(NormalsVaryings input, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                clip(SfuDissolveAmount(input.positionOS));
                float3 normalWS = normalize(input.normalWS) * IS_FRONT_VFACE(face, 1.0, -1.0);
                return half4(NormalizeNormalPerPixel(normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}