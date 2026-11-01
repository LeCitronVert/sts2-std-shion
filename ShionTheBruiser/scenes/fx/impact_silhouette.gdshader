shader_type canvas_item;

// Energy Bomb's impact frame: bodies drawn white with a black outline, over a white screen.
// Set on every creature's body by CreatureLook.cs for a split second.

uniform vec4 line_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform vec4 fill_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
// Outline width, in texture pixels
uniform float width = 4.0;

void fragment() {
	float alpha = texture(TEXTURE, UV).a;
	vec2 step_size = TEXTURE_PIXEL_SIZE * width;
	float min_alpha = alpha;
	for (int i = 0; i < 8; i++) {
		float angle = float(i) * 0.785398;
		min_alpha = min(min_alpha, texture(TEXTURE, UV + vec2(cos(angle), sin(angle)) * step_size).a);
	}
	if (alpha < 0.5) {
		COLOR = vec4(0.0);
	} else if (min_alpha < 0.5) {
		COLOR = line_color;
	} else {
		COLOR = fill_color;
	}
}
