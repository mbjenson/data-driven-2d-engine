#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


float3 AmbientLightColor = float3(0.5, 0.5, 0.5);

float3 PointLightPosition = float3(0.0, 0.0, 0.0);
float3 PointLightColor = float3(0.0, 1.0, 0.0);
float PointLightRadius = 100.0;

//float3 LightDirection = 1.0;

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 Pos : SV_Position;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

/*
float attenuation(float distance, float radius, float max_intensity, float falloff)
{
    float s = distance / radius;
    if (s >= 1.0)
    {
        return 0.0;
    }
	
    float s2 = pow(s, 2);
	
    return max_intensity * pow(1 - s2, 2) / (1 + falloff * s);
}
*/

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 ambientLight = AmbientLightColor * input.Color.rgb;
    float4 texColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    
	
	
	// add on the light value based on how far the point is from the light

	// light source attenuation
    
    //float b = 0.01;
    //float radius = sqrt(1.0 / (b * minLight));
	
	
    
    
    
    
    
    float dist = distance(PointLightPosition.xy, input.Pos.xy);
    
    // attenuation technique 1
    //float b = 0.01;
    //float a = 0.1;
    //float att = 1.0 / (1.0 + a * dist + b * dist * dist);
    
    // attenuation technique 2
    // float minLight = 0.01; // cuts light off when attenuation reaches this value
    // float b = 1.0 / (PointLightRadius * PointLightRadius * minLight); // calculate b based on radius and minline value 
    // float a = 0.1;
    
    // attenuation technique 3
    float att = clamp(1.0 - dist / PointLightRadius, 0.0, 1.0);
    
    
    if (dist >= PointLightRadius)
    {
        return texColor * float4(ambientLight, input.Color.w);
    }
    
    
    //float gradient = smoothstep(0.0, 1.0, dist);
    
    // add lights together
    float3 finalLight = ambientLight + (PointLightColor * att);
    
    // multiply texture color and light value
    return texColor * float4(finalLight, input.Color.w);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};