shader_type canvas_item;

// Eleventh Hour / Extreme Speed: the affected creature's body in black and white.
// Set on the creature's body by CreatureLook.cs (replaces its material while active).

uniform float amount : hint_range(0.0, 1.0) = 1.0;

void fragment() {
	vec4 c = COLOR;
	float gray = dot(c.rgb, vec3(0.299, 0.587, 0.114));
	// A bit more contrast, so it reads as an old film rather than a washed out picture
	gray = clamp((gray - 0.5) * 1.15 + 0.5, 0.0, 1.0);
	COLOR = vec4(mix(c.rgb, vec3(gray), amount), c.a);
}
