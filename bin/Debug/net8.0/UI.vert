#version 330 core

layout(location = 0) in vec2 aPos;  // local-space position

uniform vec2 screenSize;   // window size in pixels
uniform vec2 offset;       // pixel offset for this draw
uniform vec2 rotRow0;      // rotation matrix row 0 (for arrow)
uniform vec2 rotRow1;      // rotation matrix row 1 (for arrow)
uniform float arrowScale;  // scale for local units (1 for border, tile size for arrow)

void main()
{
    // Local -> scaled
    vec2 scaled = aPos * arrowScale;

    // Apply rotation (identity for border)
    vec2 rotated = vec2(
        dot(scaled, rotRow0),
        dot(scaled, rotRow1)
    );

    // To pixel space
    vec2 pixel = offset + rotated;

    // To NDC
    vec2 ndc = (pixel / screenSize) * 2.0 - 1.0;

    gl_Position = vec4(ndc, 0.0, 1.0);
}
