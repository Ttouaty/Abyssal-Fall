Shader "Masked/Mask"
{
	Properties
	{
		_MainTex("Base (RGB) Transparency (A)", 2D) = "" {}
		_Cutoff("_Cutoff", Range(0,1)) = 0.5
	}
	SubShader
	{
		// Render the mask after regular geometry, but before masked geometry and
		// transparent things.

		Tags{ "Queue" = "Transparent+10" }
		
		// Don't draw in the RGBA channels; just the depth buffer

		ColorMask 0
		ZWrite On

	
		//ENDCG
		Pass 
		{
			//#pragma mutli_compile_instancing
			AlphaTest Greater [_Cutoff]
			SetTexture [_MainTex] { combine texture }
		}
	}
}
