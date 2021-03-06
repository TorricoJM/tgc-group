//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))
//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;

    Output.Position = mul(Input.Position, matWorldViewProj);
   
    Output.Texcoord = Input.Texcoord;

    Output.Color = Input.Color;

    return (Output);
}

VS_OUTPUT vs_main2(VS_INPUT Input)
{
    VS_OUTPUT Output;

    Output.Position = mul(Input.Position, matWorldViewProj);

    Output.Texcoord = Input.Texcoord;

    Input.Color.r = 0.74;
	Input.Color.g = 0.84;
	Input.Color.b = 0.84;

    Output.Color = Input.Color;

    return (Output);
}

float4 ps_main(VS_OUTPUT Input) : COLOR0
{
    return tex2D(diffuseMap, Input.Texcoord);
}

//Pixel Shader
float4 ps_main2(float2 Texcoord : TEXCOORD0, float4 Color : COLOR0) : COLOR0
{
    float4 fvBaseColor = tex2D(diffuseMap, Texcoord);
    return 0.25 * fvBaseColor + 0.75 * Color;
}

// ------------------------------------------------------------------
technique Unfreeze
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}

technique Freeze
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main2();
        PixelShader = compile ps_3_0 ps_main2();
    }
}