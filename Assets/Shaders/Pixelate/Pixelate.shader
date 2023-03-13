// This shader fills the mesh shape with a color predefined in the code.
Shader "Pixelate/Pixelate"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        [HiddenInspector] _MainTex("Main Texture", 2D) = "white"
        //[HiddenInspector] _PixelationScale("Pixelation Scale", Integer) = 1
        
    }

        // The SubShader block containing the Shader code.
        SubShader
    {
        // markers that specify that we don't need culling
        // or reading/writing to the depth buffer
        Cull Off
        ZWrite Off
        ZTest Always

        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        // Tags { "LightMode" = "Pixelate" }

        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // The DeclareDepthTexture.hlsl file contains utilities for sampling the Camera
            // depth texture.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // To make the Unity shader SRP Batcher compatible, declare all
            // properties related to a Material in a a single CBUFFER block with
            // the name UnityPerMaterial.
            // CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseColor variable, so that you
                // can use it in the fragment shader.
                // float4 _BaseColor;
                // float4 _MainTex_ST;                
            // CBUFFER_END

            

            int _SmallScreenWidth;
            int _SmallScreenHeight;

            int _PixelationScale;

            float _OutlineStrength;

            float _DepthOutlineMultiplier;
            float _DepthOutlineBias;

            float _NormalOutlineMultiplier;
            float _NormalOutlineBias;
    
            float _BandingFactor;

            bool _ShowOutlineOnly;
            bool _ShowDepthOutline;
            bool _ShowNormalOutline;
           
            // The vertex shader definition with properties defined in the Varyings
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous clip space.
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                
                // Returning the output.
                return OUT;
            }

            float readDepth(float2 uv, float offsetX, float offsetY)
            {
                float2 screenSize = float2(_SmallScreenWidth, _SmallScreenHeight);
                float2 texelSize = 1. / screenSize * 0.5f;
                float2 UV = uv + texelSize * float2(offsetX, offsetY);
                #if UNITY_REVERSED_Z
                    float depth = SampleSceneDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif
                return 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y);
            }

            float3 readNormal(float2 uv, float offsetX, float offsetY)
            {

                float2 screenSize = float2(_SmallScreenWidth, _SmallScreenHeight);
                float2 texelSize = 1. / screenSize * 0.5f;
                float2 UV = uv + texelSize * float2(offsetX, offsetY);
                float3 normal = SampleSceneNormals(UV);
                normal = 0.5 * normal + 0.5;
                normal = normalize(normal);
                return normal;
            }

            float sobelDepth(float2 uv)
            {
                float depth_tl = readDepth(uv, -1, -1);
                float depth_tm = readDepth(uv, 0, -1);
                float depth_tr = readDepth(uv, 1, -1);

                float depth_ml = readDepth(uv, 1, 0);
                // float depth_mm = readDepth(uv, 0, 0);
                float depth_mr = readDepth(uv, 1, 0);

                float depth_bl = readDepth(uv, -1, 1);
                float depth_bm = readDepth(uv, 0, 1);
                float depth_br = readDepth(uv, 1, 1);

                float sobelX = depth_tl + 2 * depth_ml + depth_bl
                    - depth_tr - 2 * depth_mr - depth_br;

                float sobelY = depth_tl + 2 * depth_tm + depth_tr
                    - depth_bl - 2 * depth_bm - depth_br;

                return sqrt(sobelX * sobelX + sobelY * sobelY);
            }

            float sobelNormal(float2 uv)
            {
                float3 normal_tl = readNormal(uv, -1, -1);
                float3 normal_tm = readNormal(uv, 0, -1);
                float3 normal_tr = readNormal(uv, 1, -1);

                float3 normal_ml = readNormal(uv, 1, 0);
                // float normal_mm = readNormal(uv, 0, 0);
                float3 normal_mr = readNormal(uv, 1, 0);

                float3 normal_bl = readNormal(uv, -1, 1);
                float3 normal_bm = readNormal(uv, 0, 1);
                float3 normal_br = readNormal(uv, 1, 1);

                float3 sobelX = normal_tl + 2 * normal_ml + normal_bl
                              - normal_tr - 2 * normal_mr - normal_br;
                    
                float3 sobelY = normal_tl + 2 * normal_tm + normal_tr
                              - normal_bl - 2 * normal_bm - normal_br;

                float sobel =  sqrt(dot(sobelX,sobelX) + dot(sobelY,sobelY));
                return sobel;
            }

            // The fragment shader definition.
            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                color = (floor(_BandingFactor * color) / _BandingFactor + color) / 2;

                float depth = readDepth(IN.uv, 0, 0);
                float3 normal = readNormal(IN.uv, 0, 0);
         
                float depthOutline = sobelDepth(IN.uv);
                depthOutline = saturate(pow(_DepthOutlineMultiplier * depthOutline, _DepthOutlineBias));

                if (_ShowOutlineOnly && !_ShowDepthOutline)
                    depthOutline = 0;

                float normalOutline = sobelNormal(IN.uv);
                normalOutline = saturate(pow(_NormalOutlineMultiplier * normalOutline, _NormalOutlineBias));

                if (_ShowOutlineOnly && !_ShowNormalOutline)
                    normalOutline = 0;

                float outline = max(depthOutline, normalOutline);
                //outline = saturate(pow(normalOutline * _OutlineMultiplier, _OutlineBias));
         
                color = (floor(_BandingFactor * color) / _BandingFactor + color) / 2;

                float4 outlineColor = color;
                outlineColor.rgb *= outline * _OutlineStrength;

                 //float debug = normalOutline; return float4(debug, debug, debug, 1);
                // float3 vec = forwardCamera; return float4(vec, 1); sobelNormal(IN.uv) + sobelDepth(IN.uv) * depth *
                
                if (_ShowOutlineOnly)
                    return outline;
            
                return color + outlineColor;

                
            }
            ENDHLSL
        }
    }
}