Shader "MyShader/ScreenWaterDrop" {
    Properties {
        _BumpAmt ("Distortion", range (0,128)) = 10
        // MainTex는 이제 틴트 색상(RGB)과 모양(Alpha)에 모두 사용됩니다.
        _MainTex ("Tint Color (RGB) / Alpha (A)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
    }

    SubShader {
        // URP 관련 태그 사용
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        // 투명 오브젝트는 일반적으로 깊이 버퍼에 쓰지 않습니다.
        ZWrite Off
        // 양쪽 면을 모두 보이게 하려면 컬링을 끕니다. 효과에 따라 필요 없을 수도 있습니다.
        Cull Off
        // **블렌드 모드를 표준 알파 블렌딩으로 변경합니다.**
        // Blend SrcAlpha OneMinusSrcAlpha: 출력 색상 * 소스 알파 + 배경 색상 * (1 - 소스 알파)
        // 이렇게 하면 알파가 0인 부분은 완전히 투명해지고, 알파가 1인 부분은 불투명해집니다.
        Blend SrcAlpha OneMinusSrcAlpha


        Pass {
            Name "DISTORT"
            // URP LightMode 태그 (이 언릿 효과에는 크게 중요하지 않지만 관례적으로 사용)
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // #pragma multi_compile_fog // URP의 MixFog 사용 시 필요 없을 수 있습니다.

            // URP 코어 라이브러리 포함 - 기본적인 유니폼/정의 포함 (_CameraOpaqueTexture, 변환 행렬 등)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // URP 라이팅 라이브러리 포함 - 안개 관련 함수/유니폼 포함
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // ScreenSpaceCoords.hlsl이나 SpaceTransforms.hlsl 대신 Core.hlsl에 포함된 기본 함수 활용

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord: TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                // URP 화면 텍스처 접근을 위한 screenPos 사용
                // W 컴포넌트에 원근 보정 값이 포함됩니다. (ComputeScreenPos 사용)
                float4 screenPos : TEXCOORD0;
                float2 uvbump : TEXCOORD1;
                float2 uvmain : TEXCOORD2;
                // 안개 적용을 위해 클립 공간 위치를 프래그먼트 셰이더로 전달합니다.
                float4 positionCS : TEXCOORD3;
            };

            // 개별 변환 행렬들은 Core.hlsl에 정의되어 있으므로 여기서는 별도 선언하지 않습니다.
            // 이들은 코드 내에서 바로 사용할 수 있어야 합니다.
            // 예: unity_ObjectToWorld, unity_MatrixV, UNITY_MATRIX_P


            float _BumpAmt;
            float4 _BumpMap_ST;
float4 _MainTex_ST; // 추가: MainTex의 Tiling/Offset 정보 필요

            // URP 텍스처 및 샘플러 선언
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            // URP 불투명(Opaque) 텍스처 및 텍셀 크기 선언
            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);
            float4 _CameraOpaqueTexture_TexelSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                // 모델 공간 정점(v.vertex)을 클립 공간으로 변환
                // Core.hlsl에 정의된 개별 행렬 사용 (P * V * M)
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex); // 모델 -> 월드
                float4 viewPos = mul(unity_MatrixV, worldPos);       // 월드 -> 뷰
                o.vertex = mul(UNITY_MATRIX_P, viewPos);             // 뷰 -> 투영 (클립 공간)

                // URP 화면 텍스처 접근을 위해 화면 공간 위치 계산
                // ComputeScreenPos는 Core.hlsl에 정의되어 있을 가능성이 높습니다.
                o.screenPos = ComputeScreenPos(o.vertex);

                // 범프 및 메인 텍스처의 텍스처 좌표 변환 (타일링/오프셋 적용)
                o.uvbump = TRANSFORM_TEX( v.texcoord, _BumpMap );
                o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );

                // 안개 적용을 위해 정점의 클립 공간 위치를 프래그먼트로 전달
                o.positionCS = o.vertex; // 클립 공간 위치를 그대로 전달

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // 화면 공간 위치(screenPos)에서 원근 보정된 화면 UV를 계산합니다.
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                // 노말 맵을 샘플링하고 언팩합니다. (왜곡에는 RG 채널만 사용)
                half2 bump = UnpackNormal(SAMPLE_TEXTURE2D( _BumpMap, sampler_BumpMap, i.uvbump )).rg;

                // 화면 UV 공간에서 왜곡 오프셋을 계산합니다.
                float2 offset = bump * _BumpAmt * _CameraOpaqueTexture_TexelSize.xy;

                // 화면 UV에 오프셋을 적용합니다.
                screenUV += offset;

                // 왜곡된 UV를 사용하여 _CameraOpaqueTexture를 샘플링합니다.
                half4 col = SAMPLE_TEXTURE2D( _CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV);

                // 틴트 및 알파 텍스처를 샘플링합니다.
                half4 tint = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvmain);

                // 배경 색상에 틴트 색상을 곱하여 적용합니다.
                col.rgb *= tint.rgb; // RGB는 틴트로 사용

                // **셰이더 출력 알파 값을 텍스처의 알파 값으로 설정합니다.**
                col.a = tint.a; // Alpha는 투명도(모양)로 사용

                // URP 안개 적용 (RGB에만 적용)
                col.rgb = MixFog(col.rgb, i.positionCS);

                return col;
            }
            ENDHLSL
        }
    }
}