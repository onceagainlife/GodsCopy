Shader "UI/GridByCount_Texture"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}

        _BgColor    ("Background Color", Color) = (1,1,1,1)
        _GridColor  ("Grid Color", Color) = (0,0,0,1)
        
        _GridCountX ("Grid Count X", Float) = 10
        _GridCountY ("Grid Count Y", Float) = 10
        _LineWidth  ("Line Width", Range(0.0005, 1)) = 0.004
        
        // 新增：贴图
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        // 新增：贴图透明度控制（0~1）
        _OverlayAlpha ("Overlay Alpha", Range(0, 1)) = 1
        // 新增：贴图颜色 tint
        _OverlayColor ("Overlay Color", Color) = (1,1,1,1)
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
            
            // 新增变量
            sampler2D _OverlayTex;
            fixed _OverlayAlpha;
            fixed4 _OverlayColor;

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
                float gridMask = saturate(lines.x + lines.y); // 1=在网格线上，0=不在网格线上

                // 背景 + 网格颜色混合
                fixed4 col = lerp(_BgColor, _GridColor, gridMask);

                // 保留原 UI 纹理（可选）
                fixed4 tex = tex2D(_MainTex, i.uv);
                col.rgb = lerp(tex.rgb, col.rgb, col.a);

                // ============ 新增：贴图叠加逻辑 ============
                
                // 采样贴图
                fixed4 overlayTex = tex2D(_OverlayTex, i.uv);
                // 应用贴图颜色和透明度控制
                fixed4 overlay = overlayTex * _OverlayColor;
                overlay.a *= _OverlayAlpha;
                
                // 关键：用 gridMask 控制贴图只在网格线区域显示
                // 非重叠部分（gridMask=0）贴图透明度强制为0
                overlay.a *= gridMask;
                
                // 重叠部分：颜色叠加（这里用 alpha blend 方式混合）
                // 贴图的 RGB 只在 gridMask > 0 的区域贡献
                col.rgb = lerp(col.rgb, overlay.rgb, overlay.a);
                // 透明度叠加（1 - (1-a1)*(1-a2) 的简化形式）
                col.a = col.a + overlay.a * (1 - col.a);

                return col;
            }
            ENDCG
        }
    }
}