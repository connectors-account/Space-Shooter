Shader "SpaceShooter/SolidColor"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct VertexInput
            {
                float4 vertex : POSITION;
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
            };

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                return output;
            }

            fixed4 frag(VertexOutput input) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
    Fallback Off
}
