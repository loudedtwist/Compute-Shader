Shader "MyShaders/Optimized Shader" {
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
	}
		Subshader{
		Pass{
		Tags{ "LightMode" = "ForwardBase" "RenderType" = "Opaque"}
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

		//Lighting
		//dot product
		fixed nDotL = saturate(dot(normalDirection, In.lightDirection.xyz));

		fixed3 diffuseReflection = In.lightDirection.w * _LightColor0.xyz * nDotL;

		fixed3 specularReflection = In.lightDirection.w * _LightColor0.xyz * _MySpecColor * saturate(dot(In.lightDirection.xyz, normalDirection)) * pow(saturate(dot(reflect(-In.lightDirection.xyz,normalDirection), In.viewDirection)), _Shininess);

		fixed rim = 1 - saturate(dot(In.viewDirection, normalDirection));
		fixed3 rimLighting = _LightColor0.xyz * _RimColor * nDotL * pow(rim, _RimPower);

		fixed3 lightFinal = rimLighting + diffuseReflection + (specularReflection * tex.a) + UNITY_LIGHTMODEL_AMBIENT.xyz;

		return fixed4(tex.xyz * lightFinal * _Color.xyz,1.0);
	}

		ENDCG
	}
			Pass{
		Tags{ "LightMode" = "ForwardAdd" "RenderType" = "Opaque" }
		Blend One One
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
		Out.binormalWorld = normalize(cross(Out.normalWorld, Out.tangentWorld) * tangentSign/*In.tangent.w*/);

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
	fixed3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));

	//Lighting
	//dot product
	fixed nDotL = saturate(dot(normalDirection, In.lightDirection.xyz));

	fixed3 diffuseReflection = In.lightDirection.w * _LightColor0.xyz * nDotL;

	fixed3 specularReflection = In.lightDirection.w * _LightColor0.xyz * _MySpecColor * saturate(dot(In.lightDirection.xyz, normalDirection)) * pow(saturate(dot(reflect(-In.lightDirection.xyz,normalDirection), In.viewDirection)), _Shininess);

	fixed rim = 1 - saturate(dot(In.viewDirection, normalDirection));
	fixed3 rimLighting = _LightColor0.xyz * _RimColor.xyz * nDotL * pow(rim, _RimPower);

	fixed3 lightFinal = rimLighting + diffuseReflection + (specularReflection * tex.a);

	return fixed4(lightFinal, 1.0);
	}

		ENDCG
	}
	}
	Fallback "Diffuse"
}