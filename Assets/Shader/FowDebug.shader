Shader "Debug/FOW_Debug"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color of FOW Visible Area", Color) = (1,0,0,1)
        _ColorFowHide("Color of FOW Invisible Area", Color) = (0,1,0,1)
    }
        SubShader
        {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            fixed4 _Color;
            fixed4 _ColorFowHide;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                col.rgb = lerp(_ColorFowHide, _Color, col.r);
                return col;
            }
            ENDCG
        }
    }
}
