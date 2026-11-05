shader_type canvas_item;

// Seels' character-select → run transition (CharacterModel.CharacterSelectTransitionPath).
// NTransition.FadeOut tweens `threshold` 0 → 1 linearly over 0.8s; at 1 the screen must be black.
// A black mask over the current screen: slash after slash from every direction lays black strokes on top of each
// other, each one bleeding a few black droplets that run down, until nothing is left.
// Note: NTransition draws on an oversized rect, so everything is positioned with SCREEN_UV, not UV.

uniform float threshold : hint_range(0.0, 1.0) = 0.0;

uniform float first_slash = 0.02;
uniform float slash_interval = 0.06;
// Time for the blade to cross the screen, then for the cut to open to its full width
uniform float slash_draw_time = 0.04;
uniform float slash_open_time = 0.07;
// How long a droplet takes to run its full length
uniform float drip_time = 0.35;
uniform vec4 mask_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform vec4 blade_color : source_color = vec4(1.0, 0.96, 0.96, 1.0);

const int SLASHES = 12;
const int DROPS_PER_SLASH = 3;

float hash11(float p) {
	p = fract(p * 0.1031);
	p *= p + 33.33;
	p *= p + p;
	return fract(p);
}

float sd_capsule(vec2 p, vec2 a, vec2 b, float r) {
	vec2 pa = p - a;
	vec2 ba = b - a;
	float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
	return length(pa - ba * h) - r;
}

float smin(float a, float b, float k) {
	float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
	return mix(b, a, h) - k * h * (1.0 - h);
}

void fragment() {
	if (threshold <= 0.0) {
		COLOR = vec4(0.0);
	} else {
		float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
		// y down, 1 unit = screen height
		vec2 p = vec2((SCREEN_UV.x - 0.5) * aspect, SCREEN_UV.y - 0.5);
		float px = SCREEN_PIXEL_SIZE.y * 1.5;

		float d = 1e5;
		float blade = 0.0;
		for (int i = 0; i < SLASHES; i++) {
			float fi = float(i);
			float ts = first_slash + fi * slash_interval;
			float age = threshold - ts;
			if (age <= 0.0) {
				continue;
			}
			// Golden-ratio spread so consecutive slashes come from very different directions
			float ang = fract(fi * 0.618 + 0.13) * 3.14159265 + (hash11(fi * 3.7) - 0.5) * 0.35;
			vec2 t = vec2(cos(ang), sin(ang));
			vec2 n = vec2(-t.y, t.x);
			// Keep n pointing down the screen, so drops hang from the lower lip of the cut
			n *= n.y < 0.0 ? -1.0 : 1.0;
			vec2 c = vec2((hash11(fi * 1.9 + 0.3) - 0.5) * 0.75 * aspect, (hash11(fi * 5.3 + 1.1) - 0.5) * 0.7);
			float half_len = 1.3;
			// Later slashes cut wider, so the black piles up until it covers everything
			float width = mix(0.025, 0.4, pow(fi / float(SLASHES - 1), 1.4)) * (0.8 + 0.4 * hash11(fi * 8.3));
			float bend = (hash11(fi * 2.9) - 0.5) * 0.25;

			vec2 rel = p - c;
			float s = dot(rel, t);
			float sd = dot(rel, n) - bend * s * s;
			float head = mix(-half_len, half_len, clamp(age / slash_draw_time, 0.0, 1.0));
			float open = smoothstep(0.0, slash_open_time, age);
			// Crescent stroke: fattest in the middle, tapering to points at both ends
			float u = clamp(s / half_len, -1.0, 1.0);
			float hw = width * open * (1.0 - u * u);
			float stroke = max(abs(sd) - hw, s - head);
			d = min(d, stroke);

			// Blade light racing along the fresh cut
			float fade = 1.0 - clamp(age / 0.1, 0.0, 1.0);
			float streak = exp(-max(head - s, 0.0) * 2.5) * step(s, head);
			blade += exp(-pow(abs(sd) / 0.004, 2.0)) * streak * fade;

			// A few droplets bleed from the lower lip and run down the screen
			for (int k = 0; k < DROPS_PER_SLASH; k++) {
				float fk = float(k);
				float seed = fi * 7.0 + fk * 13.0;
				float ds = (hash11(seed + 0.5) - 0.5) * 1.3;
				float delay = slash_draw_time + 0.02 + hash11(seed + 1.7) * 0.08;
				float q = clamp((age - delay) / drip_time, 0.0, 1.0);
				if (q <= 0.0) {
					continue;
				}
				float uk = ds / half_len;
				float lip_hw = width * (1.0 - uk * uk);
				vec2 lip = c + t * ds + n * (lip_hw + bend * ds * ds);
				float r = 0.012 + 0.016 * hash11(seed + 3.1);
				float len = (0.08 + 0.35 * hash11(seed + 4.9)) * (1.0 - pow(1.0 - q, 2.0));
				// Thin trail down to a round drop at the bottom, swelling where it leaves the cut
				float trail = sd_capsule(p, lip, lip + vec2(0.0, len), r * 0.45);
				float drop = length(p - (lip + vec2(0.0, len))) - r;
				float drip = smin(trail, drop, r * 0.8);
				d = smin(d, drip, 0.02);
			}
		}

		float mask = smoothstep(px, -px, d);
		// The last moments wash whatever slipped through to black
		mask = max(mask, smoothstep(0.88, 0.97, threshold));
		float a = max(mask, clamp(blade, 0.0, 1.0));
		vec3 col = mix(mask_color.rgb, blade_color.rgb, clamp(blade, 0.0, 1.0));
		COLOR = vec4(col, a);
	}
}
