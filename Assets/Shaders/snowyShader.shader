Shader "MyShaders/Snow Shader" {
	Properties{
		_Color("Color", Color) = (1.0,1.0,1.0,1.0)
		_MainTex("Diffuse Texture", 2D) = "white"{}
		_BumpMap("Normal Texture", 2D) = "bump"{}
		_BumpDepth("Bump Depth", Range(0.0,2.0)) = 1.0
		_MySpecColor("SpecularColor", Color) = (1.0,1.0,1.0,1.0)
		_Shininess("Shininess", Float) = 10
		_RimColor("Rim Color", Color) = (1.0,1.0,1.0,1.0)
		_RimPower("Rim Range", Range(0.1,10.0)) = 3.0
		_AttenDist1("AttDist1", Float) = 1.0
		_AttenDist2("AttDist2", Float) = 1.0
		_AttenDist3("AttDist3", Float) = 1.0
		_SnowPercVert("SnowAnzVert", Range(0,0.1)) = 0.1
		_SnowPerc("SnowAnzTex", Range(0,1)) = 0.0
		_SnowAngle("Snow Angle", Range(0.5,1)) = 0.1
	}
		Subshader{
		Pass{
		Tags{ "LightMode" = "ForwardBase" }
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"
		#include "UnityLightingCommon.cginc" //for _LightColor0


		fixed4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		float4 _MainTex_ST;
		float4 _BumpMap_ST;
		fixed4 _MySpecColor;
		fixed4 _RimColor;
		float _Shininess;
		float _RimPower;
		float _AttenDist1;
		float _AttenDist2;
		float _AttenDist3;
		float _BumpDepth;
		float _SnowPerc;
		float _SnowPercVert;
		float _SnowAngle;

	struct v2f {
		float4 pos : SV_POSITION;
		float4 tex : TEXCOORD0;
		fixed4 lightDirection : TEXCOORD1;
		fixed3 viewDirection : TEXCOORD2;
		fixed3 normalWorld : TEXCOORD3;
		fixed3 tangentWorld : TEXCOORD4;
		fixed3 binormalWorld : TEXCOORD5;

	};

	v2f vert(appdata_tan In) {
		v2f Out;

		Out.normalWorld = UnityObjectToWorldNormal(In.normal);
		Out.tangentWorld = UnityObjectToWorldDir(In.tangent.xyz);
		float tangentSign = In.tangent.w * unity_WorldTransformParams.w;
		Out.binormalWorld = normalize( cross(Out.normalWorld, Out.tangentWorld) * tangentSign/*In.tangent.w*/);


		Out.pos = UnityObjectToClipPos(In.vertex);
		float4 posWorld = mul(unity_ObjectToWorld, In.vertex);
		Out.tex = In.texcoord;

		Out.viewDirection = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);

		float3 fragmentToLightSource = _WorldSpaceLightPos0.xyz - posWorld.xyz;
		float dist = length(fragmentToLightSource);
		
		float tmp = min((1 / _AttenDist1 + _AttenDist2*dist + _AttenDist3*dist*dist), 1.0);
		Out.lightDirection = fixed4(
				normalize(lerp(_WorldSpaceLightPos0.xyz, fragmentToLightSource, _WorldSpaceLightPos0.w)),
				lerp(1.0, tmp, _WorldSpaceLightPos0.w)
			);

		if (dot(Out.normalWorld, fixed3(0,1,0)) > _SnowAngle){
			Out.pos.xyz = float3(Out.pos + (Out.normalWorld * dot(Out.normalWorld, fixed3(0,1,0))) * _SnowPercVert);
		}
		return Out;
	}

	fixed4 frag(v2f In) : COLOR{
		//Texture maps
		fixed4 tex = tex2D(_MainTex, In.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);
		fixed4 texN = tex2D(_BumpMap, In.tex.xy * _BumpMap_ST.xy + _BumpMap_ST.zw);

		//unpack Normal from image
		fixed3 localCoords = float3(2.0 * texN.ag - float2(1.0,1.0), _BumpDepth);

		//normal transpose matrix
		float3x3 local2WorldTranspose = float3x3(
			In.tangentWorld,
			In.binormalWorld,
			In.normalWorld
			);

		//calculate NormalDirection
		fixed3 normalDirection = normalize( mul(localCoords, local2WorldTranspose) ); 

		fixed4 finalColor = fixed4( _Color.xyz,1.0);

		if (dot(normalDirection, fixed3(0,1,0)) > _SnowAngle){
			finalColor = finalColor + (fixed4(1,1,1,1) * dot(normalDirection, fixed3(0,1,0))) * _SnowPerc;
		}

		return finalColor;
	}

		ENDCG
	}
	}
	Fallback "Diffuse"
}