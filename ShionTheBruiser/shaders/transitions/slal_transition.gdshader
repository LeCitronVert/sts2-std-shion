shader_type canvas_item;

// Slal's character-select → run transition (CharacterModel.CharacterSelectTransitionPath).
// NTransition.FadeOut tweens `threshold` 0 → 1 linearly over 0.8s; at 1 the screen must be black.
// A goopy azure slime cascade pours from the top, drags the screen content down with it and leaves darkness behind.
// Note: NTransition draws on an oversized rect, so everything is positioned with SCREEN_UV, not UV.

uniform sampler2D screen_tex : hint_screen_texture, filter_linear;
uniform float threshold : hint_range(0.0, 1.0) = 0.0;

uniform float flow_end = 0.94;
// Azure blue goop
uniform vec4 goo_color : source_color = vec4(0.0, 0.5, 1.0, 1.0);
uniform vec4 goo_deep_color : source_color = vec4(0.0, 0.13, 0.36, 1.0);
// How far (in screen heights) behind the front the goo has fully washed the screen away
uniform float goo_thickness = 0.38;

float hash11(float p) {
	p = fract(p * 0.1031);
	p *= p + 33.33;
	p *= p + p;
	return fract(p);
}

float hash21(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

vec2 hash22(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.xx + p3.yz) * p3.zy);
}

float vnoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), u.x),
			mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), u.x), u.y);
}

float fbm(vec2 p) {
	float v = 0.0;
	float a = 0.5;
	for (int i = 0; i < 4; i++) {
		v += a * vnoise(p);
		p *= 2.03;
		a *= 0.5;
	}
	return v;
}

// Drips hanging under the front: thick fingers of goo with a round, bulging tip, filleted into the front
float drip_len(float x, float c) {
	const float CW = 0.11;
	float ci = floor(x / CW);
	float best = 0.0;
	for (int k = -1; k <= 1; k++) {
		float id = ci + float(k);
		float h = hash11(id * 1.37 + 0.5);
		float h2 = hash11(id * 7.11 + 2.1);
		float center = (id + 0.5 + (h2 - 0.5) * 0.5) * CW;
		float r = CW * (0.2 + 0.18 * h2);
		float dx = abs(x - center);
		float len = (0.02 + 0.26 * h * h) * smoothstep(0.0, 0.5, c + h2 * 0.25);
		// Neck slightly thinner than the bulb, widening into a fillet where it meets the front
		float neck = len * smoothstep(r * 1.9, r * 0.75, dx);
		float bulb = len + sqrt(max(0.0, r * r - dx * dx)) * 0.9 - r * 0.3;
		best = max(best, max(neck, dx < r ? bulb : 0.0));
	}
	return best;
}

float goo_height(vec2 p) {
	return fbm(vec2(p.x * 5.0, p.y * 3.5 - TIME * 1.6));
}

void fragment() {
	if (threshold <= 0.0) {
		COLOR = vec4(0.0);
	} else {
		float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
		vec2 uv = SCREEN_UV;
		float x = uv.x * aspect;
		float c = clamp(threshold / flow_end, 0.0, 1.0);

		// Front edge (uv.y grows downward): wobbles, drips hang below it, the whole thing pours down
		float base = mix(-0.2, 1.0 + goo_thickness + 0.1, c * c * (3.0 - 2.0 * c) * 0.35 + c * 0.65);
		float sheet_front = base
				+ 0.025 * sin(x * 7.0 + TIME * 3.0)
				+ 0.015 * sin(x * 13.0 - TIME * 5.0);
		float front = sheet_front + drip_len(x, c);
		float g = front - uv.y; // distance inside the goo (> 0 means covered)
		// How long this spot has been under the main sheet of goo; drips alone don't wash the screen away
		float soak = max(sheet_front - uv.y, 0.0);

		vec3 col;
		if (g <= 0.0) {
			// Still dry: the goo's weight drags the content right under the front a little
			float pull = 0.03 * exp(g / 0.04);
			col = texture(screen_tex, vec2(uv.x, uv.y - pull)).rgb;
			col *= 1.0 - 0.45 * smoothstep(-0.035, 0.0, g);
		} else {
			// Goo surface normal for refraction and highlights
			vec2 gp = vec2(x, uv.y);
			float e = 0.004;
			float h0 = goo_height(gp);
			float hx = goo_height(gp + vec2(e, 0.0)) - h0;
			float hy = goo_height(gp + vec2(0.0, e)) - h0;
			vec3 n = normalize(vec3(-hx / e * 0.08, -hy / e * 0.08, 1.0));

			// Content gets washed downward and refracted through the slime
			float drag = min(g, 0.12) * 0.4 + soak * 0.4;
			vec2 suv = vec2(uv.x, uv.y - drag) + n.xy * 0.02;
			vec3 content = texture(screen_tex, suv).rgb;
			float depth = smoothstep(0.0, goo_thickness, soak);
			vec3 goo = mix(goo_color.rgb, goo_deep_color.rgb, depth);
			col = mix(content * goo * 1.3, goo, 0.35 + 0.45 * depth);
			col *= 1.0 - smoothstep(goo_thickness * 0.55, goo_thickness, soak); // washed away into the dark

			// Bubbles
			vec2 bp = vec2(x * 9.0, uv.y * 9.0 - TIME * 2.5);
			vec2 bi = floor(bp);
			vec2 bo = hash22(bi);
			float br = 0.12 + 0.18 * bo.y;
			float bd = length(fract(bp) - (0.25 + 0.5 * bo));
			float bubble = step(0.6, hash21(bi + 3.1)) * smoothstep(0.05, 0.0, abs(bd - br));
			col += bubble * goo_color.rgb * 0.5 * (1.0 - depth);

			// Specular glint and the shiny lip of the front
			float spec = pow(clamp(dot(n, normalize(vec3(-0.3, -0.4, 0.87))), 0.0, 1.0), 28.0);
			col += spec * 0.55 * (1.0 - depth * 0.8);
			float rim = smoothstep(0.025, 0.0, g);
			col = mix(col, mix(goo_color.rgb, vec3(1.0), 0.35), rim * 0.85);
		}

		col = mix(col, vec3(0.0), smoothstep(0.95, 1.0, threshold));
		COLOR = vec4(col, 1.0);
	}
}
