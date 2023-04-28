Shader "GPUTrails/GPUTrails" {

Properties {
	_Width("Width", Float) = 0.1
	_StartColor("StartColor", Color) = (1,1,1,1)
	_EndColor("EndColor", Color) = (0,0,0,1)
	_MainTex("MainTex", 2D) = "white" {}
}
   
SubShader {
Pass{
	Cull Off Fog { Mode Off } ZWrite Off 
	// Blend One One
	Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
	 Blend SrcAlpha OneMinusSrcAlpha 

	CGPROGRAM
	#pragma target 5.0
	#pragma vertex vert
	#pragma geometry geom
	#pragma fragment frag

	#include "UnityCG.cginc"
	#include "GPUTrails.cginc"

	float _Width;
	float _Life;
	float4 _StartColor;
	float4 _EndColor;
	sampler2D _MainTex;
	StructuredBuffer<Trail> _TrailBuffer;
	StructuredBuffer<Node> _NodeBuffer;

	Node GetNode(int trailIdx, int nodeIdx)
	{
		return _NodeBuffer[ToNodeBufIdx(trailIdx, nodeIdx)];
	}

	struct vs_out {
		float4 pos : POSITION0;
		float3 dir : TANGENT0;
		float4 col : COLOR0;
		float4 posNext: POSITION1;
		float3 dirNext : TANGENT1;
		float4 colNext : COLOR1;
	};

	struct gs_out {
		float4 pos : SV_POSITION;
		float4 col : COLOR;
		// float2 uv : TEXCOORD0;
	};

	vs_out vert (uint id : SV_VertexID, uint instanceId : SV_InstanceID)
	{
		vs_out Out;
		Trail trail = _TrailBuffer[instanceId];
		int currentNodeIdx = trail.currentNodeIdx;

		Node node0 = GetNode(instanceId, id-1);
		Node node1 = GetNode(instanceId, id); // current
		Node node2 = GetNode(instanceId, id+1);
		Node node3 = GetNode(instanceId, id+2);

		bool isLastNode = (currentNodeIdx == (int)id);

		if ( isLastNode || !IsValid(node1))
		{
			node0 = node1 = node2 = node3 = GetNode(instanceId, currentNodeIdx);
		}

		float3 pos1 = node1.position;
		float3 pos0 = IsValid(node0) ? node0.position : pos1;
		float3 pos2 = IsValid(node2) ? node2.position : pos1;
		float3 pos3 = IsValid(node3) ? node3.position : pos2;

		Out.pos = float4(pos1, 1);
		Out.posNext = float4(pos2, 1);

		Out.dir = normalize(pos2 - pos0);
		Out.dirNext = normalize(pos3 - pos1);

		float ageRate = saturate((_Time.y - node1.time) / _Life);
		float ageRateNext = saturate((_Time.y - node2.time) / _Life);
		Out.col = lerp(_StartColor, _EndColor, ageRate);
		Out.colNext = lerp(_StartColor, _EndColor, ageRateNext);

		return Out;
	}

	[maxvertexcount(4)]
	void geom (point vs_out input[1], inout TriangleStream<gs_out> outStream)
	{
		gs_out output0, output1, output2, output3;
		float3 pos = input[0].pos; 
		float3 dir = input[0].dir;
		float3 posNext = input[0].posNext; 
		float3 dirNext = input[0].dirNext;

		float3 camPos = _WorldSpaceCameraPos;
		float3 toCamDir = normalize(camPos - pos);
		float3 sideDir = normalize(cross(toCamDir, dir));

		float3 toCamDirNext = normalize(camPos - posNext);
		float3 sideDirNext = normalize(cross(toCamDirNext, dirNext));
		float width = _Width * 0.5;

		output0.pos = UnityWorldToClipPos(pos + (sideDir * width));
		output1.pos = UnityWorldToClipPos(pos - (sideDir * width));
		
		output2.pos = UnityWorldToClipPos(posNext + (sideDirNext * width));
		output3.pos = UnityWorldToClipPos(posNext - (sideDirNext * width));
		
		float vivid = 0.4;
		float lightup = 0.1;
		float3 dirColor = input[0].dir;
		float3 dirColorNext = input[0].dirNext;
		float up = max(0, dot(float3(0,1,0), input[0].dir));
		float upNext =  max(0, dot(float3(0,1,0), input[0].dirNext));

		dirColor = up * float3(1,0.9,0.8) + (1-up) * float3(1, 0.8, 1) * 0.7;
		dirColorNext = upNext *  float3(1,0.9,0.8) +  (1-upNext) * float3(1, 0.8, 1) * 0.7;

		output0.col =
		output1.col = input[0].col;
		output0.col.xyz =
		output1.col.xyz = dirColor*(vivid*2) - vivid + lightup;
		output0.col.w = output1.col.w = 1;

		output2.col =
		output3.col = input[0].colNext;
		output2.col.xyz =
		output3.col.xyz = dirColorNext*(vivid*2) - vivid + lightup;
		output2.col.w = output3.col.w = 1;

		outStream.Append (output0);
		outStream.Append (output1);
		outStream.Append (output2);
		outStream.Append (output3);
	
		outStream.RestartStrip();
	}

	fixed4 frag (gs_out In) : COLOR
	{
		// fixed4 col = tex2D(_MainTex, In.pos.xy*0.1);
		//In.col.xyz = float3(0, 1, 0);
		In.col.w = 0.2;
		return In.col;
	}

	ENDCG
   
   }
}
}
