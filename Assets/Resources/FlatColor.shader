Shader "SpaceShooter/FlatColor"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Cull Off
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct VertexInput { float4 vertex : POSITION; };
            struct VertexOutput { float4 position : SV_POSITION; };
            fixed4 _Color;
            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                return output;
            }
            fixed4 frag(VertexOutput input) : SV_Target { return _Color; }
            ENDCG
        }
    }
}
