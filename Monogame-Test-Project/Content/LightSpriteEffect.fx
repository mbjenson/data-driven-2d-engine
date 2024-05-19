#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


// light parameters
int LightCount = 2;
float3 PointLightPositions[2];
float3 PointLightColors[2];
float PointLightRadii[2];

float3 AmbientLightColor = float3(0.5, 0.5, 0.5);

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 Pos : SV_Position; // must use this pos if it is going to be accessed inside the pixel shader
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
    float3 finalLight = 0.0;
    
    for (int i = 0; i < 2; i++)
    {
        // calculate distance from the light
        float dist = distance(PointLightPositions[i].xy, input.Pos.xy);
        if (dist >= PointLightRadii[i])
        {
            continue;
        }
    
        // calculate attenuation
        float a = 0.8;
        float att = clamp(a - dist / PointLightRadii[i], 0.0, 1.0);
    
        // add lights together
        finalLight += (PointLightColors[i] * att);
    }
    
    finalLight += ambientLight;
    // multiply texture color and light value
    return texColor * float4(finalLight, input.Color.w);
}


/*
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 ambientLight = AmbientLightColor * input.Color.rgb;
    float4 texColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    float3 finalLight = 0.0;
    
    for (int i = 0; i < 2; i++)
    {
        // calculate distance from the light
        float dist = distance(PointLightPositions[i].xy, input.Pos.xy);
        if (dist >= PointLightRadius)
        {
            continue;
        }
        
    
        // calculate attenuation
        float a = 0.8;
        float att = clamp(a - dist / PointLightRadius, 0.0, 1.0);
    
        // add lights together
        finalLight += (PointLightColors[i] * att);
    }
    
    finalLight += ambientLight;
    // multiply texture color and light value
    return texColor * float4(finalLight, input.Color.w);
}
*/

/*
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 ambientLight = AmbientLightColor * input.Color.rgb;
    float4 texColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    float3 finalLight = 0.0;
    //float3(1.0, 0.4, 0.0);
    for (int i = 0; i < LightCount; i++)
    {
        float dist = distance(PointLightPositions[i].xy, input.Pos.xy);
        float att = clamp(1.0 - dist / PointLightRadius, 0.0, 1.0);
        
        //if (dist >= PointLightRadius) // don't account for this light
        //{
        //    continue;
        //}
        // add lights together
        
        finalLight += (PointLightColor * att);
        
        // multiply texture color and light value
        //return texColor * float4(finalLight, input.Color.w);
    }
    finalLight += ambientLight;
    
    return texColor * float4(finalLight, input.Color.w);
    //return texColor * float4(ambientLight, input.Color.w);
    
	// add on the light value based on how far the point is from the light

	// light source attenuation
    
    //float b = 0.01;
    //float radius = sqrt(1.0 / (b * minLight));
	
	
    
    
    
    
    
    
    
    // attenuation technique 1
    //float b = 0.01;
    //float a = 0.1;
    //float att = 1.0 / (1.0 + a * dist + b * dist * dist);
    
    // attenuation technique 2
    // float minLight = 0.01; // cuts light off when attenuation reaches this value
    // float b = 1.0 / (PointLightRadius * PointLightRadius * minLight); // calculate b based on radius and minline value 
    // float a = 0.1;
    
    // attenuation technique 3
    
    
    
    
    
    
    //float gradient = smoothstep(0.0, 1.0, dist);
    
    
    
    
}
*/

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};