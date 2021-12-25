#version 140

in vec3 coord;
in vec3 normal;

uniform mat4 proj4d;
uniform mat4 view4d;
uniform mat4 view4dnormals;

out vec3 fragNormal;
out vec3 fragCoord;

void main()
{
    fragNormal = normalize((view4dnormals * vec4(normal, 1.0)).xyz);
    fragCoord = (view4d * vec4(coord, 1.0)).xyz;
    gl_Position = (proj4d * (view4d * vec4(coord, 1.0)));
}