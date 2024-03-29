﻿#version 330

in vec2 v_TexCoords;

uniform float xTexScale;
uniform float Glow;

uniform sampler2D noise;

layout(location = 0) out vec4 Color;

//# import SM_base_fragment_textureGamma
vec4 texture2DGamma(sampler2D s, vec2 P);

// bezier curve with 2 control points
// A is the starting point, B, C are the control points, D is the destination
// t from 0 ~ 1
// from: https://gist.github.com/yiwenl/bb87c2d7df8bc735dab017c808a381ab
vec3 bezier(vec3 A, vec3 B, vec3 C, vec3 D, float t) {
  vec3 E = mix(A, B, t);
  vec3 F = mix(B, C, t);
  vec3 G = mix(C, D, t);

  vec3 H = mix(E, F, t);
  vec3 I = mix(F, G, t);

  vec3 P = mix(H, I, t);

  return P;
}

void main() {
	const float size = .1;
	const float borderThickness = .1f;

	vec3 b = bezier(vec3(0,.5,0), vec3(.5, 0, 0), vec3(.5, 1, 0), vec3(1, .5, 0), mod(v_TexCoords.x, 1));
	float hit = abs(b.y - v_TexCoords.y);
	
	float bezierHit = hit < size  ? 1 : 0;
	float bezierBorder = (hit < size + .05 ? 1 : 0);
	float border = v_TexCoords.y < borderThickness || v_TexCoords.y > 1 - borderThickness || abs(v_TexCoords.x) < borderThickness || abs(v_TexCoords.x) > xTexScale - borderThickness ? 1 : 0;
	float mergedBorders = (bezierBorder - bezierHit) + border;

	float noise = texture2DGamma(noise, fract(v_TexCoords)).r * (1 - border) * (1 - bezierHit ) * (1 - bezierBorder);
	
	const vec3 GlowColor = vec3(1, .5, 0);
	vec3 result = (mergedBorders) * vec3(.01) + bezierHit * GlowColor * Glow + (1 - bezierHit) * vec3(.005) + noise * GlowColor * .01f * Glow;
	Color = vec4(result, 1);
}