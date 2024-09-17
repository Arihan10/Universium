Shader "Custom/VolumetricSphere" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _Color2("Color2", Color) = (1,1,1,1)
        _Radius("Radius", Range(0, 10)) = 1
        _Density("Density", Range(0, 1)) = 0.1
        _ExtinctionCoefficient("Extinction Coefficient", Range(0, 1)) = 0.1
        _ScatteringCoefficient("Scattering Coefficient", Range(0, 1)) = 0.1
        _Position("Position", Vector) = (0, 0, 0)
        _SunPosition("Sun Position", Vector) = (0, 0, 0)
    }

        SubShader{
            Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "ForceNoShadowCasting" = "True" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float3 worldPos : TEXCOORD0;
                    float3 worldNormal : TEXCOORD1;
                };

                float _Radius;
                float _Density;
                float _ExtinctionCoefficient;
                float _ScatteringCoefficient;
                float3 _Position;
                float3 _SunPosition;
                float3 _Color;
                float3 _Color2; 

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz - _Position;
                    o.worldNormal = mul((float3x3)unity_WorldToObject, normalize(o.worldPos));
                    return o;
                }

                float3 CalculateAttenuation(float3 ray, float3 sunDir) {
                    float distance = length(ray);
                    float3 extinction = _ExtinctionCoefficient * _Density * distance;
                    float3 attenuation = exp(-extinction);
                    float3 scatter = exp(-_ScatteringCoefficient * _Density * distance);
                    float3 absorption = exp(-_Density * distance);
                    float3 sunScatter = exp(-_ScatteringCoefficient * _Density * distance * dot(sunDir, ray));
                    return attenuation * (scatter + sunScatter) * absorption;
                }

                float3 CalculateFalloff(float3 ray, float3 sunDir) {
                    float radius = _Radius + _Position.z;
                    float curvature = dot(ray, sunDir) * radius;
                    float t = clamp(1.0 - (curvature / radius), 0.0, 1.0);
                    float falloff = t * t * t * (t * (t * 6.0 - 15.0) + 10.0) * t * t * t;
                    return falloff * CalculateAttenuation(ray, sunDir);
                }

                fixed4 frag(v2f i) : SV_Target{
                    // Calculate the angle between the sun direction and the surface normal
                    float3 sunDir = normalize(_SunPosition - i.worldPos);
                    float sunAngle = max(dot(i.worldNormal, sunDir), 0.0);

                    float3 ray = normalize(i.worldPos);
                    float3 falloff = CalculateFalloff(ray, sunDir);

                    // Modify the color of the shader based on the sun angle
                    fixed4 finalColor = fixed4(_Color, 1.0);
                    fixed4 col2 = fixed4(_Color2, 1.0); 
                    finalColor.rgb *= lerp(1.0, 0.2, pow(sunAngle, 3.0));

                    float3 scattering = CalculateAttenuation(ray, sunDir);
                    float alpha = lerp(0.28, 0.135, falloff * scattering);
                    fixed4 hello = fixed4(lerp(col2.rgb, finalColor.rgb, falloff * scattering), 1 - alpha); 
                    // hello = (falloff * scattering, 1.0); 
                    return hello; 
                    // return fixed4(finalColor.rgb * falloff * scattering, 1.0);
                }

                ENDCG
            }
    }
        FallBack "Diffuse"
}
