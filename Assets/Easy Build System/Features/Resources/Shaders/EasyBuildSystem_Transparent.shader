Shader "Easy Build System/EasyBuildSystem_Transparent"
{
	Properties
	{
		_BaseColor("BaseColor", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard alpha:fade
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		fixed4 _BaseColor;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = _BaseColor;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		ENDCG

		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		/*ZWrite Off
		ZTest Greater*/

		CGPROGRAM
		#pragma surface surf Standard alpha:fade
		#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		fixed4 _BaseColor;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = _BaseColor;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		ENDCG

	}
		FallBack "Transparent"
}