float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float4 pointsOfCollision[6];

//Material del mesh
float3 materialEmissiveColor; //Color RGB
float3 materialAmbientColor; //Color RGB
float4 materialDiffuseColor; //Color ARGB (tiene canal Alpha)
float3 materialSpecularColor; //Color RGB
float materialSpecularExp; //Exponente de specular

//Parametros de la Luz
float3 lightColor; //Color RGB de la luz
float4 lightPosition; //Posicion de la luz
float4 eyePosition; //Posicion de la camara
float lightIntensity; //Intensidad de la luz
float lightAttenuation; //Factor de atenuacion de la luz

float acumulateTime;

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


/* FREEZE */

struct FREEZE_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

struct FREEZE_OUTPUT
{
    float4 Position : POSITION0;
    float4 RealPosition : TEXCOORD1;
    float2 Texcoord : TEXCOORD0;
    float4 Color : COLOR0;
};

FREEZE_OUTPUT Freeze_vs(FREEZE_INPUT Input)
{
    FREEZE_OUTPUT Output;

    Output.RealPosition = Input.Position;

    Output.Position = mul(Input.Position, matWorldViewProj);

    Output.Texcoord = Input.Texcoord;

    Input.Color.r = 0.74;
    Input.Color.g = 0.84;
    Input.Color.b = 0.84;

    Output.Color = Input.Color;

    return (Output);
}

//Pixel Shader
float4 Freeze_ps(FREEZE_OUTPUT Input) : COLOR0
{
    float4 fvBaseColor = tex2D(diffuseMap, Input.Texcoord);
    if (Input.RealPosition.y < acumulateTime * 50)
    {
        return 0.25 * fvBaseColor + 0.75 * Input.Color;
    }
    else
    {
        return fvBaseColor;
    } 
}

/* FREEZE */

/* DEFORMACIONES */

struct DEFORMATION_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

struct DEFORMATION_OUTPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

DEFORMATION_OUTPUT Deformation_vs(DEFORMATION_INPUT Input)
{
    DEFORMATION_OUTPUT Output;
    float4 flag = float4(0,0,0,0);
    for (int i = 0; i < 6; i++)
    {
        if (distance(pointsOfCollision[i], flag) == 0)
        {
            Output.Position = mul(Input.Position, matWorldViewProj);
        }
    }

    Output.Texcoord = Input.Texcoord;

    Output.Color = Input.Color;

    return (Output);
}

//Pixel Shader
float4 Deformation_ps(float2 Texcoord : TEXCOORD0) : COLOR0
{
    return tex2D(diffuseMap, Texcoord);
}

/* DEFORMACIONES */

/**************************************************************************************/
/* DIFFUSE_MAP */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DIFFUSE_MAP
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR;
    float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT_DIFFUSE_MAP
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
    float3 LightVec : TEXCOORD3;
    float3 HalfAngleVec : TEXCOORD4;
};

//Vertex Shader
VS_OUTPUT_DIFFUSE_MAP vs_DiffuseMap(VS_INPUT_DIFFUSE_MAP input)
{
    VS_OUTPUT_DIFFUSE_MAP output;

	//Proyectar posicion
    output.Position = mul(input.Position, matWorldViewProj);

	//Enviar Texcoord directamente
    output.Texcoord = input.Texcoord;

	//Posicion pasada a World-Space (necesaria para atenuacion por distancia)
    output.WorldPosition = mul(input.Position, matWorld);

	/* Pasar normal a World-Space
	Solo queremos rotarla, no trasladarla ni escalarla.
	Por eso usamos matInverseTransposeWorld en vez de matWorld */
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
    output.LightVec = lightPosition.xyz - output.WorldPosition;

	//ViewVec (V): vector que va desde el vertice hacia la camara.
    float3 viewVector = eyePosition.xyz - output.WorldPosition;

	//HalfAngleVec (H): vector de reflexion simplificado de Phong-Blinn (H = |V + L|). Usado en Specular
    output.HalfAngleVec = viewVector + output.LightVec;

    return output;
}

//Input del Pixel Shader
struct PS_DIFFUSE_MAP
{
    float2 Texcoord : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
    float3 LightVec : TEXCOORD3;
    float3 HalfAngleVec : TEXCOORD4;
};

//Pixel Shader
float4 ps_DiffuseMap(PS_DIFFUSE_MAP input) : COLOR0
{
	//Normalizar vectores
    float3 Nn = normalize(input.WorldNormal);
    float3 Ln = normalize(input.LightVec);
    float3 Hn = normalize(input.HalfAngleVec);

	//Calcular intensidad de luz, con atenuacion por distancia
    float distAtten = length(lightPosition.xyz - input.WorldPosition) * lightAttenuation;
    float intensity = lightIntensity / distAtten; //Dividimos intensidad sobre distancia (lo hacemos lineal pero tambien podria ser i/d^2)

	//Obtener texel de la textura
    float4 texelColor = tex2D(diffuseMap, input.Texcoord);

	//Componente Ambient
    float3 ambientLight = intensity * lightColor * materialAmbientColor;

	//Componente Diffuse: N dot L
    float3 n_dot_l = dot(Nn, Ln);
    float3 diffuseLight = intensity * lightColor * materialDiffuseColor.rgb * max(0.0, n_dot_l); //Controlamos que no de negativo

	//Componente Specular: (N dot H)^exp
    float3 n_dot_h = dot(Nn, Hn);
    float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: (intensity * lightColor * materialSpecularColor * pow(max(0.0, n_dot_h), materialSpecularExp));

	/* Color final: modular (Emissive + Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
	   El color Alpha sale del diffuse material */
    float4 finalColor = float4(saturate(materialEmissiveColor + ambientLight + diffuseLight) * texelColor + specularLight, materialDiffuseColor.a);

    return finalColor;
}

technique Iluminate
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_DiffuseMap();
        PixelShader = compile ps_3_0 ps_DiffuseMap();
    }
}

technique Freeze
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 Freeze_vs();
        PixelShader = compile ps_3_0 Freeze_ps();
    }
}

technique Deform
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 Deformation_vs();
        PixelShader = compile ps_3_0 Deformation_ps();
    }
}


