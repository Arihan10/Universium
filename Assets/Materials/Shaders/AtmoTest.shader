Shader "Custom/VolumetricSphere" {
    Properties{
        _Color("Color", Color) = (1,1,1,1)
        _Position("Position", Vector) = (0, 0, 0)
        _SunPosition("Sun Position", Vector) = (0, 0, 0)
    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            Cull Off

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float4 uv : TEXCOORD0;
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float3 viewVector : TEXCOORD1;
                };

                v2f vert(appdata v) {
                    v2f output;
                    output.pos = UnityObjectToClipPos(v.vertex);
                    output.uv = v.uv;
                    // float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
                    // output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                    // output.viewVector = v.uv - _WorldSpaceCameraPos;

                    float3 viewDir = UnityWorldSpaceViewDir(v.vertex.xyz);
                    output.viewVector = mul(UNITY_MATRIX_V, float4(viewDir, 0)).xyz;

                    //float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                    //float3 viewPos = _WorldSpaceCameraPos - worldPos.xyz;
                    //output.viewVector = mul(unity_CameraToWorld, float4(viewPos, 0.0)).xyz;

                    return output;
                }

                bool raySphereIntersection(float ro, float rd, float so, float sr) {
                    float t = dot((so - ro), rd); 
                    float3 P = ro + rd * t; 
                    float y = length(so - P); 

                    return y <= sr; 
                }

                float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir) {
                    float3 offset = rayOrigin - sphereCentre;
                    float a = dot(rayDir, rayDir);
                    float b = 2 * dot(offset, rayDir);
                    float c = dot(offset, offset) - sphereRadius * sphereRadius;
                    float d = b * b - 4 * a * c; // Discriminant from quadratic formula

                    // Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
                    if (d > 0) {
                        float s = sqrt(d);
                        float dstToSphereNear = (-b - s) / (2 * a);
                        float dstToSphereFar = (-b + s) / (2 * a);

                        // Ignore intersections that occur behind the ray
                        if (dstToSphereFar >= 0 && dstToSphereNear >= 0) {
                            return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
                        }
                    }
                    // Ray did not intersect sphere
                    return float2(3.402823466e+38, 0);
                }


                float4 _Color; 
                float4 _Position; 
                float4 _SunPosition; 

                float4 frag(v2f i) : SV_Target {
                    float3 ro = _WorldSpaceCameraPos; 
                    float3 rd = normalize(i.viewVector); 

                    // return raySphereIntersection(ro, rd, _Position, 0.5); 
                    return raySphere(_Position, 0.5, ro, rd).y / 2; 
                    // return float4(rd, 1); 
                }

                ENDCG
            }
    }
        FallBack "Diffuse"
}
