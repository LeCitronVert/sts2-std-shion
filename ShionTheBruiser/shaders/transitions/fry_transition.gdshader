shader_type canvas_item;

// Fry's character-select → run transition (CharacterModel.CharacterSelectTransitionPath).
// NTransition.FadeOut tweens `threshold` 0 → 1 linearly over 0.8s; at 1 the screen must be black.
// The pitch-black shadow of a hand closes in, grabs the screen and shoves it back, crumples it like a sheet of paper
// in its fist, then pulls away with it.
// Note: NTransition draws on an oversized rect, so everything is positioned with SCREEN_UV, not UV.

uniform sampler2D screen_tex : hint_screen_texture, filter_linear;
uniform float threshold : hint_range(0.0, 1.0) = 0.0;

uniform float approach_end = 0.22;
uniform float grasp_end = 0.36;
uniform float crumple_end = 0.7;
uniform float retreat_end = 0.94;
uniform vec4 backdrop_color : source_color = vec4(0.8, 0.0, 0.0, 1.0);

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

mat2 rot(float a) {
	float c = cos(a);
	float s = sin(a);
	return mat2(vec2(c, s), vec2(-s, c));
}

float sd_capsule(vec2 p, vec2 a, vec2 b, float r) {
	vec2 pa = p - a;
	vec2 ba = b - a;
	float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
	return length(pa - ba * h) - r;
}

float sd_round_box(vec2 p, vec2 b, float r) {
	vec2 q = abs(p) - b + r;
	return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
}

float smin(float a, float b, float k) {
	float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
	return mix(b, a, h) - k * h * (1.0 - h);
}

// Hand silhouette, palm toward the screen, arm coming from below. y is up, ~1 unit = screen height.
// curl 0 = fingers spread, 1 = closed fist.
float hand_sdf(vec2 q, float curl) {
	float d = sd_round_box(q, vec2(0.15, 0.15), 0.07);
	d = smin(d, sd_capsule(q, vec2(0.0, -0.12), vec2(0.04, -2.5), 0.115), 0.06);
	for (int i = 0; i < 4; i++) {
		float fi = float(i);
		vec2 base = vec2(-0.105 + fi * 0.07, 0.12);
		float len = 0.26 - 0.035 * abs(fi - 1.3);
		float spread = (fi - 1.5) * 0.13 * (1.0 - curl);
		vec2 dir = vec2(sin(spread), cos(spread));
		// Curling folds the fingers toward the screen, so from the front they shorten into knuckles
		float l = len * (1.0 - 0.75 * curl);
		float r = 0.034 - 0.003 * abs(fi - 1.5) + 0.01 * curl;
		d = smin(d, sd_capsule(q, base, base + dir * l, r), 0.02);
	}
	float ta = mix(2.3, 1.2, curl);
	vec2 tb = vec2(-0.13, -0.06);
	d = smin(d, sd_capsule(q, tb, tb + vec2(cos(ta), sin(ta)) * mix(0.2, 0.17, curl), 0.042), 0.04);
	return d;
}

// Voronoi: x = distance to the nearest cell edge, yz = cell id
vec3 voronoi(vec2 x) {
	vec2 n = floor(x);
	vec2 f = fract(x);
	vec2 mg = vec2(0.0);
	vec2 mr = vec2(0.0);
	float md = 8.0;
	for (int j = -1; j <= 1; j++) {
		for (int i = -1; i <= 1; i++) {
			vec2 g = vec2(float(i), float(j));
			vec2 r = g + hash22(n + g) - f;
			float d = dot(r, r);
			if (d < md) {
				md = d;
				mr = r;
				mg = g;
			}
		}
	}
	md = 8.0;
	for (int j = -2; j <= 2; j++) {
		for (int i = -2; i <= 2; i++) {
			vec2 g = mg + vec2(float(i), float(j));
			vec2 r = g + hash22(n + g) - f;
			if (dot(mr - r, mr - r) > 0.00001) {
				md = min(md, dot(0.5 * (mr + r), normalize(r - mr)));
			}
		}
	}
	return vec3(md, n + mg);
}

void fragment() {
	if (threshold <= 0.0) {
		COLOR = vec4(0.0);
	} else {
		float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
		// y up from here on
		vec2 p = vec2((SCREEN_UV.x - 0.5) * aspect, 0.5 - SCREEN_UV.y);
		float t = threshold;

		float approach = smoothstep(0.0, approach_end, t);
		float grasp = smoothstep(approach_end, grasp_end, t);
		float crumple = smoothstep(grasp_end, crumple_end, t);
		float retreat = smoothstep(crumple_end, retreat_end, t);

		// --- Hand: a blurry, faint shadow far from the surface that sharpens and darkens as it lands
		float hand_scale = mix(2.3, 1.0, approach) * mix(1.0, 0.8, retreat);
		vec2 hand_pos = mix(vec2(0.25, -0.35), vec2(0.0, -0.02), approach);
		vec2 retreat_off = vec2(0.08, -1.5) * retreat * retreat;
		hand_pos += retreat_off;
		float hand_rot = mix(-0.35, -0.12, approach) + 0.1 * crumple;
		float curl = 0.45 * grasp + 0.55 * crumple;
		float blur = mix(0.14, 0.003, approach);
		float shadow_alpha = mix(0.3, 1.0, approach);

		// --- Sheet (the screen itself): shoved back on the grab, crumpled into the fist, carried off
		vec2 grip = vec2(0.0, 0.1);
		// Shove: overshoots back, then settles
		float shove = grasp * (1.0 + 0.6 * sin(grasp * 3.14159265) * (1.0 - grasp));
		float sheet_scale = mix(1.0, 0.82, shove) * mix(1.0, 0.13, pow(crumple, 1.4)) * mix(1.0, 0.8, retreat);
		float jolt = 0.012 * sin(grasp * 30.0) * (1.0 - grasp) * step(0.001, grasp);
		vec2 sheet_center = grip + retreat_off + vec2(jolt, -jolt * 0.5);
		vec2 q = (p - sheet_center) * rot(0.06 * shove - 0.25 * crumple) / sheet_scale + grip;

		// Crumple: facets shift and tilt, creases darken
		vec3 vor = voronoi(q * 3.2 + 7.0);
		vec2 cell = vor.yz;
		vec2 facet_off = (hash22(cell) - 0.5) * 0.14 * crumple;
		vec2 qw = q + facet_off;
		vec3 fnorm = normalize(vec3((hash22(cell + 13.0) - 0.5) * 2.2 * crumple, 1.0));
		float shade = mix(1.0, clamp(dot(fnorm, normalize(vec3(-0.4, 0.5, 0.77))) / 0.77, 0.2, 1.25), crumple);
		float crease = smoothstep(0.03, 0.0, vor.x) * crumple;

		// Sheet outline goes from the screen rectangle to a lumpy ball
		float box_d = sd_round_box(q - grip, vec2(aspect * 0.5, 0.5), 0.0);
		vec2 qc = q - grip;
		float lump = vnoise(vec2(atan(qc.y, qc.x) * 2.5 + 4.0, 1.0));
		// Sized so the ball's lumps peek out around the fist
		float ball_d = length(qc) - (1.45 + 0.5 * (lump - 0.5));
		float sheet_d = mix(box_d, ball_d, smoothstep(0.15, 0.85, crumple));
		float in_sheet = smoothstep(0.004, -0.004, sheet_d * sheet_scale);

		// The ball is bigger than the sheet in sheet space: squeeze it back in and mirror at the borders
		// so crumpled facets never sample off-screen (which would smear clamped edge pixels)
		vec2 qs = grip + (qw - grip) * mix(1.0, 0.6, smoothstep(0.15, 0.85, crumple));
		vec2 suv = vec2(qs.x / aspect + 0.5, 0.5 - qs.y);
		suv = 1.0 - abs(1.0 - mod(suv, 2.0));
		vec3 sheet = texture(screen_tex, suv).rgb;
		sheet *= shade;
		sheet *= 1.0 - crease * 0.5;
		// Paper edge catches a bit of light once it's pulled away from the frame
		sheet += smoothstep(-0.012, 0.0, sheet_d * sheet_scale) * 0.25 * grasp;

		// Backdrop behind the sheet: a dark ember glow so the black hand still reads, fading out as it leaves
		float vign = 1.0 - smoothstep(0.1, 1.1, length(p));
		vec3 backdrop = backdrop_color.rgb * 0.16 * vign * (1.0 - retreat);
		// Sheet's drop shadow on the backdrop
		backdrop *= 1.0 - 0.7 * smoothstep(0.08, 0.0, sheet_d * sheet_scale) * grasp;

		vec3 col = mix(backdrop, sheet, in_sheet);

		vec2 hq = (p - hand_pos) * rot(hand_rot) / hand_scale;
		float hd = hand_sdf(hq, curl) * hand_scale;
		float hand = (1.0 - smoothstep(-blur, blur, hd)) * shadow_alpha;
		// Soft contact shadow around the hand once it presses on the sheet
		hand = max(hand, (1.0 - smoothstep(0.0, 0.05, hd)) * 0.45 * approach * (1.0 - retreat));
		col = mix(col, vec3(0.0), hand);

		col = mix(col, vec3(0.0), smoothstep(0.94, 1.0, t));
		COLOR = vec4(col, 1.0);
	}
}
