#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


float4x4 WorldViewProjection;
float4x4 World;
float Time;


//Defino la textura

uniform texture ModelTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};


struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinate : TEXCOORD0;
	float4 LocalPosition : TEXCOORD1;
};


VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = mul(input.Position, WorldViewProjection);
	output.LocalPosition = input.Position;
	return output;
	
	return output;
}

float3 random3(float3 c)
{
    float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
    float3 r;
    r.z = frac(512.0 * j);
    j *= .125;
    r.x = frac(512.0 * j);
    j *= .125;
    r.y = frac(512.0 * j);
    return r;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{	 
	float4 color = float4(0,0,0.8,1); 
	float3 position = input.LocalPosition.xyz;
	float ring1 = 2*(frac(Time*0.7));
	float ring2 = 2*(frac(Time*0.7))-2;
	float ring3 = 2*(frac(Time*0.7))-4;
	bool isRing = abs(position.y-ring1) <0.05f || abs(position.y-ring2) <0.05f || abs(position.y-ring3) <0.05f ;
	float3 random = random3(input.LocalPosition.xyz*Time);
	
	
	float factor = smoothstep(3,-4, position.y);
	color *=factor;
	if(isRing)
	{
		color = float4(0,0,1,0);
		color.a = smoothstep(1,-2,position.y);
	}	

    return color;
}


technique Default
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

//Funciones básicas de HLSL

// step(float x, float y): 		DEVUELVE 0 SI x<y. CASO CONTRARIO DEVUELVE 1
// saturate	(float x): 		UTILIZADA PARA RESTRINGIR (clamp) UN RANGO ENTRE [0,1]. RETORNA 0 SI x<0. RETORNA 1 SI x>1
// clamp (float value, float min, float max):	IGUAL QUE saturate PERO RESTRINGE value AL RANGO [min,max]
//  lerp (x, y, s):  	RETORNA UN VALOR QUE ESTA LINEALMENTE INTERPOLADO ENTRE x E y SEGUN s. Se puede pensar como que x es el start point e y es el end point. s es un factor 
						//que va de [0,1]. 0 indica el start point, y 1 el end point. Cualquier valor en medio, sera un pto ubicado entre el start point y el endpoint, sobre la recta
						//que los une
//  frac(float x): 	RETORNA LA PARTE FRACCIONARIA DE x
// length: 	RETORNA LA LONGITUD DEL VECTOR
//  distance: RETORNA LA DISTANCIA ENTRE DOS VECTORES
//   ceil(x) y floor(x):	 Retorna el entero mas pequeño que es mayor o igual a x. Retorna el entero mas grande que es menor igual a x. AKA: REDONDEA PARA ARRIBA O ABAJO
//					floor(3.2) devuelve 3.
//					floor(3.8) devuelve 3.


//  pow (X,Y)
//  clip(float x): DESCARTA EL FRAGMENTO O VERTICE SI x ES MENOR A 0; O SI LA CONDICION DEVUELVE FALSE.
// discard : DESCARTA EL FRAGMENTO O VERTICE 
// dot: producto escalar.


//PARA PASAR DE UN RANGO A OTRO : valor_transformado = (valor_original - min_original) * (max_nuevo - min_nuevo) / (max_original - min_original) + min_nuevo
//PARA PASAR DE COORDENADAS [-1,1] VISTA PROYECCION A COORDENADAS DE PANTALLA [0-VIEWPORT.HEIGHT/WIDHT] : 
// screen.x = viewProjPosition.x * (Width/2) + (Width/2)