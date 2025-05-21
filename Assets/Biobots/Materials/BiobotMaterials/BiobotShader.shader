Shader "Rahinii/BiobotShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionIntensity ("Emission Intensity", Float) = 0
        _BioluminescencePattern ("Bioluminescence Pattern (0-1)", Range(0,1)) = 0.0
        _WaveFunctionModulation ("Wave Function Modulation", Range(0,1)) = 0.0

        // Placeholder for future dimensional/quantum effects
        _DimensionalInfluence ("Dimensional Influence", Range(0,1)) = 0.0
        _EntanglementPulse ("Entanglement Pulse", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="UniversalLit" } // Or Standard, if not using URP/HDRP
            Name "Unlit"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // For GPU instancing

            #include "UnityCG.cginc"
            #include "Lighting.cginc" // If using standard lighting or URP/HDRP includes

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_INSTANCE_ID
            };

            fixed4 _BaseColor;
            fixed4 _EmissionColor;
            float _EmissionIntensity;
            float _BioluminescencePattern;
            float _WaveFunctionModulation;
            float _DimensionalInfluence;
            float _EntanglementPulse;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                // --- Base Color ---
                fixed4 col = _BaseColor;

                // --- Emission / Bioluminescence ---
                fixed4 emission = _EmissionColor;
                float finalEmissionIntensity = _EmissionIntensity;

                // Conceptual pattern logic (this is where complex patterns would be implemented)
                // For example, a simple pulse:
                if (_BioluminescencePattern > 0.1 && _BioluminescencePattern < 0.3) // "pulse_blue"
                {
                    finalEmissionIntensity *= (sin(_Time.y * 5.0 * _WaveFunctionModulation) * 0.5 + 0.5);
                }
                // More patterns could be added with if/else if checks based on _BioluminescencePattern ranges.

                emission *= finalEmissionIntensity;

                // --- Combined result ---
                // For a simple unlit shader, emission is just added to base color
                col.rgb += emission.rgb;

                // --- Placeholder for advanced effects ---
                // Dimensional distortion (conceptual: distort color or add noise based on _DimensionalInfluence)
                // Entanglement pulse (conceptual: overlay color or flicker based on _EntanglementPulse)
                // This would be complex, potentially influencing UVs or vertex positions for distortion.

                return col;
            }
            ENDCG
        }
    }
    FallBack "Standard" // Fallback to Unity's built-in Standard shader if this one fails
}
