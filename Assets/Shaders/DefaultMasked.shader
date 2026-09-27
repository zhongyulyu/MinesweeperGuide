Shader "Sprites/DefaultMasked"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture (Alpha)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Toggle] _UseMaskR ("Use Mask Red Channel", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Sprite" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _USEMASKR_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            sampler2D _MaskTex;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord);

                #ifdef _USEMASKR_ON
                    // 用遮罩图的红色通道作为遮罩强度（适配纯黑白 RGB 遮罩图）
                    c.a *= tex2D(_MaskTex, IN.texcoord).r;
                #else
                    // 用遮罩图的 alpha 通道作为遮罩强度
                    c.a *= tex2D(_MaskTex, IN.texcoord).a;
                #endif

                c.rgb *= c.a;
                return c * IN.color;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
