Shader "SpaceShooter/FlatColor"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex : POSITION; };
            struct Output { float4 vertex : SV_POSITION; };
            fixed4 _Color;
            Output vert(Input input)
            {
                Output output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                return output;
            }
            fixed4 frag(Output input) : SV_Target { return _Color; }
            ENDCG
        }
    }
}
