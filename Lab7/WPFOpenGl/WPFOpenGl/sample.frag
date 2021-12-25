#version 140

in vec3 fragNormal;
in vec3 fragCoord;

uniform vec3 fragColor;
uniform vec3 lightPosition;
uniform vec3 lightKA;
uniform vec3 lightKD;
uniform vec3 lightKS;
uniform vec3 lightIA;
uniform vec3 lightID;
uniform float p, md, mk;
uniform mat4 proj4d;
uniform mat4 view4d;
uniform float time;

const float pi = 3.14159265359f;

void main()
{
    vec3 result = vec3(0.0);
    vec3 realLightPosition = (view4d * vec4(lightPosition, 1.0)).xyz;
    vec3 Iamb = lightIA * lightKA;

    vec3 L = realLightPosition - fragCoord;
    float cos_alpha = dot(normalize(fragNormal), normalize(L));

   // vec3 Idiff = lightID * lightKD * max(cos_alpha, 0.0);
    vec3 f = vec3(
        sin(time + asin(lightKD.r)), 
        sin(time + asin(lightKD.g)), 
        sin(time + asin(lightKD.b))
    );
    vec3 Idiff = lightID * (f) * max(cos_alpha, 0.0);

    vec3 R = reflect(L, fragNormal);
    vec3 S = normalize(vec3(0.0, 0.0, -1.0));

    float cos_tetha = dot(normalize(R), S);
    
    if (cos_alpha <= 0.0) cos_tetha = 0.0;

    vec3 Ispec = lightID * lightKS * pow(max(cos_tetha, 0.0), p);

    result = Iamb + (Idiff + Ispec) / (md * length(L) + mk);

    gl_FragColor = vec4(fragColor * result, 1);
}