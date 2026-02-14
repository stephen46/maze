#version 330 core

in vec2 uv;
out vec4 FragColor;

uniform vec2  pPos;
uniform float pAng;
uniform vec2  mSize;
uniform sampler2D mTex;
uniform sampler2D wallTex;

uniform vec3 fogColor;
uniform float fogStart;
uniform float fogEnd;

void main()
{
    // Build ray direction
    vec2 dir   = vec2(cos(pAng), sin(pAng));
    vec2 plane = vec2(-sin(pAng), cos(pAng)) * 0.66;

    float cameraX = uv.x;
    vec2 rayDir = dir + plane * cameraX;

    // Avoid divide-by-zero
    if (abs(rayDir.x) < 1e-6) rayDir.x = (rayDir.x < 0.0 ? -1e-6 : 1e-6);
    if (abs(rayDir.y) < 1e-6) rayDir.y = (rayDir.y < 0.0 ? -1e-6 : 1e-6);

    // DDA setup
    int mapX = int(floor(pPos.x));
    int mapY = int(floor(pPos.y));

    vec2 deltaDist = vec2(abs(1.0 / rayDir.x), abs(1.0 / rayDir.y));
    vec2 sideDist;
    ivec2 step;
    int side = 0;

    if (rayDir.x < 0.0)
    {
        step.x = -1;
        sideDist.x = (pPos.x - float(mapX)) * deltaDist.x;
    }
    else
    {
        step.x = 1;
        sideDist.x = (float(mapX + 1) - pPos.x) * deltaDist.x;
    }

    if (rayDir.y < 0.0)
    {
        step.y = -1;
        sideDist.y = (pPos.y - float(mapY)) * deltaDist.y;
    }
    else
    {
        step.y = 1;
        sideDist.y = (float(mapY + 1) - pPos.y) * deltaDist.y;
    }

    bool hit = false;
    float perpWallDist = 0.0;

    // DDA loop
    for (int i = 0; i < 256; i++)
    {
        if (sideDist.x < sideDist.y)
        {
            sideDist.x += deltaDist.x;
            mapX += step.x;
            side = 0;
        }
        else
        {
            sideDist.y += deltaDist.y;
            mapY += step.y;
            side = 1;
        }

        if (mapX < 0 || mapX >= int(mSize.x) || mapY < 0 || mapY >= int(mSize.y))
            break;

        float cell = texture(mTex, (vec2(mapX, mapY) + 0.5) / mSize).r;
        if (cell > 0.5)
        {
            hit = true;
            perpWallDist = (side == 0 ? sideDist.x - deltaDist.x : sideDist.y - deltaDist.y);
            break;
        }
    }

    // If no hit, draw sky/fog
    if (!hit)
    {
        FragColor = vec4(fogColor, 1.0);
        return;
    }

    // Compute wall height in NDC
    float lineHeight = 1.0 / perpWallDist;

    // Compute top/bottom of wall slice in NDC space
    float wallTop    = -lineHeight;
    float wallBottom =  lineHeight;

    // Ceiling
    if (uv.y > wallBottom)
    {
        FragColor = vec4(0.10, 0.10, 0.15, 1.0);
        return;
    }

    // Floor
    if (uv.y < wallTop)
    {
        FragColor = vec4(0.20, 0.20, 0.20, 1.0);
        return;
    }

    // Wall texture coordinate
    float wallX;
    if (side == 0)
        wallX = pPos.y + perpWallDist * rayDir.y;
    else
        wallX = pPos.x + perpWallDist * rayDir.x;
    wallX -= floor(wallX);

    float texY = (uv.y - wallTop) / (wallBottom - wallTop);
    vec2 texCoord = vec2(wallX, texY);

    vec4 texColor = texture(wallTex, texCoord);

    // Side shading
    float shade = (side == 1) ? 0.7 : 1.0;
    vec3 litColor = texColor.rgb * shade;

    // Fog
    float fogFactor = clamp((fogEnd - perpWallDist) / (fogEnd - fogStart), 0.0, 1.0);
    vec3 finalColor = mix(fogColor, litColor, fogFactor);

    FragColor = vec4(finalColor, 1.0);
}
