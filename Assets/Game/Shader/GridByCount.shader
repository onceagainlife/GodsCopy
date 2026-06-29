Shader "UI/GridByCount"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}

        _BgColor    ("Background Color", Color) = (1,1,1,1)
        _GridColor  ("Grid Color", Color) = (0,0,0,1)
        

        _GridCountX ("Grid Count X", Float) = 10
        _GridCountY ("Grid Count Y", Float) = 10
        _LineWidth  ("Line Width", Range(0.0005, 1)) = 0.004

    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

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
            float _GridCountX;
            float _GridCountY;
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
                // 网格计算
                float2 grid = frac(i.uv * float2(_GridCountX, _GridCountY));
                float2 lines = step(grid, _LineWidth) + step(1.0 - _LineWidth, grid);
                float gridMask = saturate(lines.x + lines.y);

                // 背景 + 网格颜色分离
                 fixed4 col = lerp(_BgColor, _GridColor, gridMask);
                // 计算网格 UV（和格子对齐）


                // 保留原 UI 纹理（可选）
                fixed4 tex = tex2D(_MainTex, i.uv);
                col.rgb = lerp(tex.rgb, col.rgb, col.a);

                return col;
            }
            ENDCG
        }
    }

    
}