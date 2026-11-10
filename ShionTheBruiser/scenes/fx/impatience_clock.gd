extends Control

## Growing Impatience indicator, right of the energy counter: a clock drawn in code, then the number of cards
## the player can still play this turn. Driven from C# (ImpatienceIndicatorPatch in Strife/ImpatienceIndicator.cs)
## through set_count(). The clock's hand sweeps as the turn runs out; it turns red on the last 2 cards.

@export var clock_size := 40.0
@export var face_color := Color(0.98, 0.94, 0.84)
@export var rim_color := Color(0.16, 0.12, 0.09)
@export var hand_color := Color(0.16, 0.12, 0.09)
@export var warning_color := Color(0.85, 0.12, 0.1)

@onready var _label: Label = $Label

var _left := -1
var _limit := 1
var _warning := false


func set_count(left: int, limit: int) -> void:
	left = max(left, 0)
	limit = max(limit, 1)
	if left == _left and limit == _limit:
		return
	var had_value := _left >= 0
	_left = left
	_limit = limit
	_warning = left <= 2
	_label.text = str(left)
	_label.add_theme_color_override("font_color", warning_color if _warning else Color(1.0, 0.96, 0.86))
	queue_redraw()
	if had_value:
		# Small pop whenever the value changes
		_label.pivot_offset = _label.size * 0.5
		_label.scale = Vector2.ONE * 1.3
		create_tween().tween_property(_label, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


func _draw() -> void:
	var radius := clock_size * 0.5
	var center := Vector2(radius, size.y * 0.5)
	var rim := warning_color if _warning else rim_color

	# Face, rim and the 12 ticks
	draw_circle(center, radius, rim)
	draw_circle(center, radius - 3.0, face_color)
	for i in 12:
		var tick := TAU * i / 12.0
		var dir := Vector2(sin(tick), -cos(tick))
		var inner := radius - (8.0 if i % 3 == 0 else 6.0)
		draw_line(center + dir * inner, center + dir * (radius - 4.0), rim, 2.0 if i % 3 == 0 else 1.0, true)

	# Minute hand: a full turn means the turn is used up. Hour hand stays at 12 for a simple, readable face.
	var used := 1.0 - float(max(_left, 0)) / float(_limit)
	var sweep := TAU * used
	var minute := Vector2(sin(sweep), -cos(sweep)) * (radius - 7.0)
	draw_line(center, center + Vector2(0.0, -(radius - 12.0)), hand_color, 3.0, true)
	draw_line(center, center + minute, warning_color if _warning else hand_color, 2.0, true)
	draw_circle(center, 2.5, rim)
