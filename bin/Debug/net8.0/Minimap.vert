#version 330 core

layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aOffset;
layout(location = 2) in vec3 aColor;
layout(location = 3) in float aVisited;

out vec3 vColor;
out vec2 vRotatedPos;
out float vVisited;

uniform float scale;
uniform vec2 screenSize;
uniform vec2 playerPos;
uniform vec2 mapCenter;

uniform vec2 rotRow0;
uniform vec2 rotRow1;

void main()
{
    vec2 rel = aOffset - playerPos;

    // Apply rotation (no flipping)
    vec2 rotatedTile = vec2(
        dot(rel, rotRow0),
        dot(rel, rotRow1)
    );

    vRotatedPos = rotatedTile;
    vVisited = aVisited;

    vec2 rotatedCorner = vec2(
        dot(aPos * scale, rotRow0),
        dot(aPos * scale, rotRow1)
    );

    vec2 pixel = mapCenter + rotatedTile * scale + rotatedCorner;

    vec2 ndc = (pixel / screenSize) * 2.0 - 1.0;
    gl_Position = vec4(ndc, 0.0, 1.0);

    vColor = aColor;
}
