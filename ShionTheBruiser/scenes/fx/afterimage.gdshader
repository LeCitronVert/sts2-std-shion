shader_type canvas_item;

// Hyper Guard's afterimages (afterimage.gd): the body's shape, filled with one color.
// Keeps the vertex alpha, so Spine slot transparency and the afterimage's fade (modulate) still apply.

uniform vec4 tint : source_color = vec4(1.0);

void fragment() {
	float alpha = texture(TEXTURE, UV).a;
	COLOR = vec4(tint.rgb, alpha * COLOR.a * tint.a);
}
