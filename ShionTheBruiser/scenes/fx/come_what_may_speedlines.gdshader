shader_type canvas_item;

// "Running" speedlines: cuts the radial lines of the texture into dashes that rush outward
// from the burst's origin. Each thin angular sector gets its own speed and phase,
// so neighbouring lines never move in sync.

// Burst origin on the texture (UV)
uniform vec2 origin = vec2(0.5, 0.838);
// Texture width / height, so distances are measured in a round space
uniform float aspect = 1.519;
// Number of angular sectors with their own dash speed/phase
uniform float rays = 220.0;
// Dashes per unit of distance from the origin
uniform float density = 3.5;
// Dash speed, in units of distance per second
uniform float speed = 2.8;
// 0 = plain texture, 1 = fully dashed
uniform float strength = 0.85;
// Radius around the origin kept solid, so the glow at the center doesn't flicker
uniform float core_radius = 0.12;
// Changed on each frame swap, so every frame gets new dashes
uniform float seed = 0.0;

float hash(float n) {
	return fract(sin(n * 127.1 + seed * 311.7) * 43758.5453);
}

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	vec2 p = (UV - origin) * vec2(aspect, 1.0);
	float d = length(p);
	float a = atan(p.y, p.x) / TAU + 0.5;
	float ray = floor(a * rays);
	float r1 = hash(ray);
	float r2 = hash(ray + 17.0);

	float band = fract(d * density * (0.7 + 0.6 * r2) - TIME * speed * (0.6 + 0.8 * r1) + r1);
	float dash = smoothstep(0.0, 0.12, band) * (1.0 - smoothstep(0.4, 0.75, band));
	float core = 1.0 - smoothstep(core_radius * 0.5, core_radius * 2.0, d);
	float mask = max(dash, core);

	COLOR = vec4(tex.rgb, tex.a * mix(1.0, mask, strength)) * COLOR;
}
