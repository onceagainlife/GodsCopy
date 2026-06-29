Shader "UI/GridByPixelSize"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}

        _BgColor   ("Background Color", Color) = (1,1,1,1)
        _GridColor ("Grid Color", Color) = (0,0,0,0.25)

        _GridSizeX ("Grid Pixel Width", Float) = 50
        _GridSizeY ("Grid Pixel Height", Float) = 50
        _LineWidth ("Line Width", Range(0.001, 0.05)) = 0.003
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _BgColor;
            fixed4 _GridColor;

            float _GridSizeX;
            float _GridSizeY;
            float _InvUiWidth;
            float _InvUiHeight;
            float _LineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 cellUV = float2(
                    _InvUiWidth  * _GridSizeX,
                    _InvUiHeight * _GridSizeY
                );

                float2 grid = frac(i.uv / cellUV);

                float2 lines =
                    step(grid, _LineWidth) +
                    step(1.0 - _LineWidth, grid);

                float mask = saturate(lines.x + lines.y);

                fixed4 col = lerp(_BgColor, _GridColor, mask);

                fixed4 tex = tex2D(_MainTex, i.uv);
                col.rgb = lerp(tex.rgb, col.rgb, col.a);

                return col;
            }
            ENDCG
        }
    }
}