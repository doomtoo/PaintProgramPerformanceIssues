Shader "Custom/BrushShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BrushColor("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				// UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _BrushColor;

			//brush size in uv size- but width and height might be different, so have to pass both
			uniform float _Brush_size_UV_half_x;//brush size- in percent of total UV
			uniform float _Brush_size_UV_half_y;//brush size- in percent of total UV

			// We also have to signal to the shader that these variables will be modified from outside, hence the uniform qualifier. https://www.alanzucconi.com/2016/01/27/arrays-shaders-heatmaps-in-unity3d/
			uniform int _Points_Length;

			uniform float _Points_x[200]; //MAX 224 or doesn't work on Android!!!!!
			uniform float _Points_y[200]; //MAX 224 or doesn't work on Android!!!!!


			//for knowing the aspect ratio of the canvas for making the circle look correctly
			uniform float _aspect_ratio = 1;

			//for drawing multiple points, using a texture as the brush
			void DrawMultiPointsFromSquareBrush(inout fixed4 col, v2f i)
			{
				float x = i.uv.x;
				float y = i.uv.y;

				for (int i = 0; i < _Points_Length; i++)
				{

					if (x>_Points_x[i] - _Brush_size_UV_half_x && x<_Points_x[i] + _Brush_size_UV_half_x)
						if (y > _Points_y[i] - _Brush_size_UV_half_y && y < _Points_y[i] + _Brush_size_UV_half_y)
						{
							col.r = _BrushColor.r;
							col.g = _BrushColor.g;
							col.b = _BrushColor.b;
							col.a = _BrushColor.a;							
						}
				}
			}
			
			
			v2f vert (appdata v)
			{
				v2f o;
				 o.vertex = UnityObjectToClipPos(v.vertex);
				 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);		
				// fixed4 col = float4(0,0,0,1);

				DrawMultiPointsFromSquareBrush(col, i);

				return col;
			}
			
			ENDCG
		}
	}
}
