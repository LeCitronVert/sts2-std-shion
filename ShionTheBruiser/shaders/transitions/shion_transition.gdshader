shader_type canvas_item;

// Shion's character-select → run transition (CharacterModel.CharacterSelectTransitionPath).
// NTransition.FadeOut tweens `threshold` 0 → 1 linearly over 0.8s; at 1 the screen must be black.
// Two big punch impacts (left, then right), a short beat, then a dragon claw rips the screen away right → left.
// Note: NTransition draws on an oversized rect, so everything is positioned with SCREEN_UV, not UV.

uniform sampler2D screen_tex : hint_screen_texture, filter_linear;
uniform float threshold : hint_range(0.0, 1.0) = 0.0;

uniform float punch1_time = 0.0;
uniform float punch2_time = 0.15;
uniform float punch_duration = 0.2;
uniform float claw_start = 0.36;
uniform float claw_end = 0.92;
uniform vec4 aura_color : source_color = vec4(0.573, 0.204, 0.922, 1.0);

float hash21(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

float vnoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), u.x),
			mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), u.x), u.y);
}

vec2 to_uv(vec2 p, float aspect) {
	return vec2(p.x / aspect + 0.5, p.y + 0.5);
}

// One punch: shockwave + flash + manga impact frame + radial cracks + shake.
void punch(vec2 p, vec2 center, float start, float push_dir, float seed,
		inout vec2 disp, inout float flash, inout float invert, inout float crack, inout vec2 shake) {
	float lt = (threshold - start) / punch_duration;
	if (lt < 0.0) {
		return;
	}
	float e = clamp(lt, 0.0, 1.0);
	vec2 d = p - center;
	float r = length(d);
	vec2 dir = d / (r + 1e-4);
	float a = atan(d.y, d.x);

	// Shockwave ring that bends the screen outward as it travels
	float ring_r = 0.04 + 0.95 * (1.0 - pow(1.0 - e, 3.0));
	float ring = exp(-pow((r - ring_r) / 0.04, 2.0)) * (1.0 - e);
	disp -= dir * ring * 0.045;
	// Crater: the point of impact gets punched in
	disp += dir * 0.05 * exp(-r * r * 30.0) * pow(1.0 - e, 2.0);

	// Flash core, ring glow and starburst rays
	float rays = smoothstep(0.62, 0.9, vnoise(vec2(a * 7.0 + seed * 17.0, seed)));
	rays *= smoothstep(ring_r * 1.3, ring_r * 0.2, r);
	flash += exp(-r * r * 25.0) * pow(1.0 - e, 2.0) * 1.4 + ring * 0.9 + rays * (1.0 - e) * 0.9;

	// Impact frame: whole screen inverts for a frame or two
	if (lt < 0.13) {
		invert = 1.0;
	}

	// Radial cracks that stay after the hit, with the shards slightly knocked out of place
	float crack_len = 0.36 * smoothstep(0.0, 0.35, lt) * (0.55 + 0.45 * vnoise(vec2(a * 3.0, seed * 5.0)));
	float in_crack = smoothstep(crack_len, crack_len * 0.7, r) * smoothstep(0.015, 0.05, r);
	float line_v = abs(vnoise(vec2(a * 4.5 + seed * 11.0, r * 4.0)) - 0.5);
	crack = max(crack, smoothstep(0.035, 0.0, line_v) * in_crack);
	float shard = floor((a + PI) / (2.0 * PI) * 9.0 + vnoise(vec2(r * 6.0, seed)) * 1.5);
	vec2 shard_off = vec2(hash21(vec2(shard, seed)), hash21(vec2(seed, shard))) - 0.5;
	disp += shard_off * 0.012 * in_crack * smoothstep(0.0, 0.3, lt);

	// Screen shake, knocked away from the fist
	float s = 0.025 * sin(lt * 42.0) * pow(1.0 - e, 2.0) * step(lt, 1.0);
	shake += vec2(push_dir * s, 0.4 * s * cos(lt * 57.0));
}

void fragment() {
	if (threshold <= 0.0) {
		COLOR = vec4(0.0);
	} else {
		float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
		vec2 p = vec2((SCREEN_UV.x - 0.5) * aspect, SCREEN_UV.y - 0.5);

		vec2 disp = vec2(0.0);
		vec2 shake = vec2(0.0);
		float flash = 0.0;
		float invert = 0.0;
		float crack = 0.0;
		punch(p, vec2(-0.27 * aspect, 0.03), punch1_time, 1.0, 1.0, disp, flash, invert, crack, shake);
		punch(p, vec2(0.27 * aspect, -0.05), punch2_time, -1.0, 2.0, disp, flash, invert, crack, shake);

		// Dragon claw: three talons sweep right → left, then the gashes rip open until nothing is left
		float c = clamp((threshold - claw_start) / (claw_end - claw_start), 0.0, 1.0);
		float swipe = 1.0 - pow(1.0 - smoothstep(0.0, 0.42, c), 2.0);
		float widen = smoothstep(0.22, 1.0, c);
		float tip = mix(aspect * 0.5 + 0.2, -aspect * 0.5 - 0.9, swipe);
		float torn = 0.0;
		float edge_glow = 0.0;
		float hot = 0.0;
		if (c > 0.0) {
			for (int i = 0; i < 3; i++) {
				float fi = float(i);
				float yc = (fi - 1.0) * 0.3 - 0.2 * p.x + 0.07 * sin(p.x * 1.7 + fi * 1.3);
				float dy = p.y - yc;
				float along = p.x - tip;
				float taper = sqrt(clamp(along / 0.45, 0.0, 1.0));
				float jag = 1.0 + 0.3 * (vnoise(vec2(p.x * 4.0, fi * 7.0)) - 0.5) + 0.08 * (vnoise(vec2(p.x * 26.0, fi * 3.0)) - 0.5);
				float hw = (0.008 + 0.36 * widen * widen) * taper * jag;
				float ad = abs(dy);
				torn = max(torn, smoothstep(hw + 0.002, hw - 0.002, ad) * step(0.0, along));
				edge_glow = max(edge_glow, smoothstep(hw + 0.02, hw, ad) * smoothstep(hw - 0.004, hw, ad) * step(0.0, along));
				// Hot slash streak right behind the talon tip
				hot = max(hot, exp(-max(along, 0.0) * 5.0) * step(0.0, along) * exp(-pow(dy / 0.012, 2.0)) * (1.0 - widen));
				// Flaps of screen peel away from each gash
				disp.y -= sign(dy) * 0.035 * widen * exp(-max(ad - hw, 0.0) / 0.05) * step(0.0, along);
			}
		}

		vec3 col = texture(screen_tex, to_uv(p + shake + disp, aspect)).rgb;
		col = mix(col, vec3(1.0) - col, invert);
		col = mix(col, vec3(0.03, 0.0, 0.06), crack * 0.9);
		col += crack * aura_color.rgb * 0.25;
		col += flash * mix(vec3(1.0), aura_color.rgb, 0.35);
		col = mix(col, vec3(0.0), torn);
		col += edge_glow * (1.0 - torn) * aura_color.rgb * 1.6 * (1.0 - widen * 0.6);
		col += hot * mix(vec3(1.0), aura_color.rgb, 0.3) * 1.5;
		col = mix(col, vec3(0.0), smoothstep(0.94, 1.0, threshold));
		COLOR = vec4(col, 1.0);
	}
}
