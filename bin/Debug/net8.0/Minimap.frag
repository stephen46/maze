#version 330 core

in vec3 vColor;
in vec2 vRotatedPos;
in float vVisited;

out vec4 FragColor;

uniform float fogStart;
uniform float fogEnd;
uniform vec3 fogColor;

void main()
{
    // If the tile has been visited at least once, show its real color
    if (vVisited > 0.5)
    {
        FragColor = vec4(vColor, 1.0);
        return;
    }

    // Tile has NOT been visited — show pure fog color (no blending)
    FragColor = vec4(fogColor, 1.0);
}
