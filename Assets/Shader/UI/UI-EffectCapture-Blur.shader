Shader "UI/Hidden/UI-EffectCapture-Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" { }
    }
    SubShader
    {
        // No culling or depth
        ZTest Always
        Cull Off
        ZWrite Off
        Fog
        {
            Mode off
        }
        
        UsePass "Hidden/UI-EffectCapture-Base/EFFECTCAPTURE-BASE"
        
        Pass
        {
            Name "EffectCapture-Blur"
            
            CGPROGRAM
            
            #pragma vertex vert_img
            #pragma fragment frag_blur
            #pragma target 2.0
            
            #pragma shader_feature __ FASTBLUR MEDIUMBLUR DETAILBLUR
            
            #include "UnityCG.cginc"
            #include "UI-EffectCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            half4 _EffectFactor;
            
            fixed4 frag_blur(v2f_img IN): SV_Target
            {
                half2 blurFactor = _EffectFactor.xy;
                half4 color = Tex2DBlurring1D(_MainTex, IN.uv, blurFactor * _MainTex_TexelSize.xy * 2);
                color.a = 1;
                return color;
            }
            ENDCG
            
        }
    }
}
