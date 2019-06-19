Shader "Hidden/UI-EffectCapture-Base"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
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
        
		Pass
        {
            Name "EffectCapture-Base"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            v2f_img vert(appdata_img v)
            {
                v2f_img o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                #if UNITY_UV_STARTS_AT_TOP
                    o.uv.y = 1 - o.uv.y;
                #endif
                return o;
            }
            
            fixed4 frag(v2f_img IN): SV_Target
            {
                half4 color = tex2D(_MainTex, IN.uv);
                color.a = 1;
                return color;
            }
            ENDCG
            
        }
	}
}
