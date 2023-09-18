#version 460
layout(local_size_x = 8, local_size_y = 8) in;

layout(binding=0) buffer input_texture_xy
{
    vec4 in_color_xy[];
};

layout(binding=1) buffer input_texture_zw
{
    vec4 in_color_zw[];
};


layout(binding=4) buffer output_texture
{
    vec4 out_color[];
};

uniform uvec2 u_resolution = uvec2(256, 256);


uint i(uvec2 xy)
{
    return xy.x + xy.y * u_resolution.x;
}

uvec2 clamp_bound(uvec2 xy, uvec4 bound)
{
    uvec2 remainder = uvec2(mod(xy - bound.xy, bound.zw));
    return bound.xy + remainder;
}

float xy_at(uvec2 xy, uvec4 bound)
{
    xy = clamp_bound(xy, bound);
    return (in_color_xy[i(xy)]).x / 255.0f;
}

float zw_at(uvec2 xy, uvec4 bound)
{
    xy = clamp_bound(xy, bound);
    return (in_color_zw[i(xy)]).x / 255.0f;
}

vec3 normal_at_xy(uvec2 xy, uvec4 bound)
{
    ivec3 of = ivec3(-1, 0, 1);

    float v21 = xy_at(xy + of.zy, bound);
    float v01 = xy_at(xy + of.xy, bound);
    float v12 = xy_at(xy + of.yz, bound);
    float v10 = xy_at(xy + of.yx, bound);

    vec3 vx = normalize(vec3(1.0f, 0.0f, v21 - v01));
    vec3 vy = normalize(vec3(0.0f, 1.0f, v12 - v10));

    return cross(vx, vy);
}

vec3 normal_at_zw(uvec2 xy, uvec4 bound)
{
    ivec3 of = ivec3(-1, 0, 1);

    float v21 = zw_at(xy + of.zy, bound);
    float v01 = zw_at(xy + of.xy, bound);
    float v12 = zw_at(xy + of.yz, bound);
    float v10 = zw_at(xy + of.yx, bound);

    vec3 vx = normalize(vec3(1.0f, 0.0f, v21 - v01));
    vec3 vy = normalize(vec3(0.0f, 1.0f, v12 - v10));

    return cross(vx, vy);
}

void main()
{
    uvec2 xy = gl_LocalInvocationID.xy + gl_WorkGroupID.xy * gl_WorkGroupSize.xy;

    uint i = xy.x + xy.y * u_resolution.x;

    uvec2 cellsize = u_resolution / 4;
    uvec4 bound = uvec4(
        uvec2(xy / cellsize) * cellsize,
        cellsize - uvec2(1, 1)
    );

    vec3 n_xy = normal_at_xy(xy, bound);
    vec3 n_zw = normal_at_zw(xy, bound);

    vec4 rgba = vec4(n_xy.xy, n_zw.xy) * 0.5f + 0.5f;
    out_color[i] = rgba * 255.0f;
}
