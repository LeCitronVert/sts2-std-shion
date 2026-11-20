extends Node2D

## Small clock next to the energy cost of cards on Cooldown, showing the number of turns left inside it.
## Added and shown/hidden from C# (CooldownClockPatch in Visuals/CooldownLookPatch.cs) through set_turns().

@export var radius := 15.0
@export var face_color := Color(0.98, 0.94, 0.84)
@export var rim_color := Color(0.75, 0.14, 0.1)
@export var tick_color := Color(0.16, 0.12, 0.09)
@export var text_color := Color(0.16, 0.12, 0.09)
@export var font_size := 18

var _turns := 0


func set_turns(turns: int) -> void:
	turns = max(turns, 0)
	if turns == _turns:
		return
	var had_value := _turns > 0
	_turns = turns
	queue_redraw()
	if had_value:
		# Small pop whenever a turn ticks off
		scale = Vector2.ONE * 1.3
		create_tween().tween_property(self, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


func _draw() -> void:
	# Shadow, rim, face
	draw_circle(Vector2(1.0, 1.5), radius + 1.0, Color(0.0, 0.0, 0.0, 0.35))
	draw_circle(Vector2.ZERO, radius, rim_color)
	draw_circle(Vector2.ZERO, radius - 2.5, face_color)

	# Bell knobs on top, like an alarm clock
	for side in [-1.0, 1.0]:
		var dir := Vector2(side * 0.62, -0.8).normalized()
		draw_circle(dir * (radius + 1.0), radius * 0.22, rim_color)

	# Quarter ticks
	for i in 4:
		var a := TAU * i / 4.0
		var dir := Vector2(sin(a), -cos(a))
		draw_line(dir * (radius - 6.0), dir * (radius - 3.5), tick_color, 1.5, true)

	# Turns left, centered
	var font := ThemeDB.fallback_font
	var text := str(_turns)
	var text_size := font.get_string_size(text, HORIZONTAL_ALIGNMENT_LEFT, -1, font_size)
	var pos := Vector2(-text_size.x * 0.5, font.get_ascent(font_size) * 0.5 - font.get_descent(font_size) * 0.25)
	draw_string(font, pos, text, HORIZONTAL_ALIGNMENT_LEFT, -1, font_size, text_color)
