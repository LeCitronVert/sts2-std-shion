extends Control

## Emerald wheel (Super Form, Act 4): a ring of sections drawn in code, like Sonic Superstars' power wheel.
## Section 0 is at the top, the others follow clockwise. Driven from C# (EmeraldWheelUi in Strife/EmeraldWheel.cs):
## setup() gives each section its texture and whether it can be picked; the wheel reports the hovered section
## (hovered, -1 for none), the picked one (chosen) and when it should close (closed). C# frees it.
## Mouse: hover a section, click to pick it; click outside the ring or right click to close.
## Gamepad / keyboard: point the left stick at a section (or step with the D-pad / arrows), accept to pick it,
## cancel to close.

signal hovered(index: int)
signal chosen(index: int)
signal closed

@export var outer_radius := 290.0
@export var inner_radius := 165.0
@export var icon_size := 104.0
@export var backdrop_color := Color(0.0, 0.0, 0.0, 0.45)
@export var ring_color := Color(0.04, 0.06, 0.1, 0.88)
@export var separator_color := Color(0.75, 0.78, 0.85, 0.55)
@export var highlight_color := Color(1.0, 0.82, 0.1, 1.0)
@export var blocked_highlight_color := Color(0.45, 0.45, 0.5, 0.9)
@export var inner_line_color := Color(0.85, 0.88, 0.95, 0.6)
@export var stick_deadzone := 0.5

const ARC_POINTS := 24

var _textures: Array = []
var _selectable: Array = []
var _hovered := -1
var _appear := 0.0


func setup(textures: Array, selectable: Array) -> void:
	_textures = textures
	_selectable = selectable
	queue_redraw()


func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	focus_mode = Control.FOCUS_ALL
	grab_focus()
	var tween := create_tween()
	tween.tween_property(self, "_appear", 1.0, 0.15).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	tween.parallel().tween_method(func(_v): queue_redraw(), 0.0, 1.0, 0.15)


func _count() -> int:
	return max(_textures.size(), 1)


func _center() -> Vector2:
	return size * 0.5


func _section_angle(index: int) -> float:
	# Center angle of a section: 0 at the top, clockwise
	return -PI * 0.5 + TAU * float(index) / float(_count())


func _section_at_angle(angle: float) -> int:
	var step := TAU / float(_count())
	var from_top := fposmod(angle + PI * 0.5 + step * 0.5, TAU)
	return int(floor(from_top / step)) % _count()


func _set_hovered(index: int) -> void:
	if index == _hovered:
		return
	_hovered = index
	queue_redraw()
	hovered.emit(index)


func _pick() -> void:
	if _hovered >= 0 and _hovered < _selectable.size() and _selectable[_hovered]:
		chosen.emit(_hovered)


func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		var offset: Vector2 = event.position - _center()
		var radius := offset.length()
		if radius >= inner_radius and radius <= outer_radius:
			_set_hovered(_section_at_angle(offset.angle()))
		else:
			_set_hovered(-1)
		accept_event()
	elif event is InputEventMouseButton and event.pressed:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if _hovered >= 0:
				_pick()
			else:
				closed.emit()
		elif event.button_index == MOUSE_BUTTON_RIGHT:
			closed.emit()
		accept_event()


func _input(event: InputEvent) -> void:
	# Gamepad and keyboard work wherever the mouse is
	if event is InputEventJoypadMotion:
		var stick := Vector2(Input.get_joy_axis(event.device, JOY_AXIS_LEFT_X), Input.get_joy_axis(event.device, JOY_AXIS_LEFT_Y))
		if stick.length() >= stick_deadzone:
			_set_hovered(_section_at_angle(stick.angle()))
		get_viewport().set_input_as_handled()
		return
	if not event.is_pressed() or event.is_echo():
		return
	# D-pad / arrows: left and right step around the wheel, up jumps to the top, down to the bottom
	var target := -1
	if event.is_action("ui_left"):
		target = 0 if _hovered < 0 else posmod(_hovered - 1, _count())
	elif event.is_action("ui_right"):
		target = 0 if _hovered < 0 else posmod(_hovered + 1, _count())
	elif event.is_action("ui_up"):
		target = 0
	elif event.is_action("ui_down"):
		target = _count() / 2
	if target >= 0:
		_set_hovered(target)
		get_viewport().set_input_as_handled()
	elif event.is_action("ui_accept") or event.is_action("ui_select") or event.is_action("ui_confirm"):
		_pick()
		get_viewport().set_input_as_handled()
	elif event.is_action("ui_cancel") or event.is_action("mega_back") or event.is_action("mega_pause_and_back"):
		closed.emit()
		get_viewport().set_input_as_handled()


func _arc(radius: float, from_angle: float, to_angle: float) -> PackedVector2Array:
	var points := PackedVector2Array()
	for i in ARC_POINTS + 1:
		var angle := lerpf(from_angle, to_angle, float(i) / ARC_POINTS)
		points.append(_center() + Vector2.from_angle(angle) * radius)
	return points


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, size), Color(backdrop_color, backdrop_color.a * _appear))
	var grow := lerpf(0.85, 1.0, _appear)
	var outer := outer_radius * grow
	var inner := inner_radius * grow
	var center := _center()
	var step := TAU / float(_count())

	# Ring
	for i in _count():
		var from_angle := _section_angle(i) - step * 0.5
		var to_angle := _section_angle(i) + step * 0.5
		var color: Color = ring_color
		if i == _hovered:
			var can_pick: bool = i < _selectable.size() and _selectable[i]
			color = highlight_color if can_pick else blocked_highlight_color
		var outer_arc := _arc(outer, from_angle, to_angle)
		var inner_arc := _arc(inner, to_angle, from_angle)
		var polygon := outer_arc + inner_arc
		draw_colored_polygon(polygon, Color(color, color.a * _appear))

	# Separators and inner circle
	for i in _count():
		var angle := _section_angle(i) - step * 0.5
		draw_line(center + Vector2.from_angle(angle) * inner, center + Vector2.from_angle(angle) * outer,
			Color(separator_color, separator_color.a * _appear), 3.0, true)
	draw_arc(center, inner, 0.0, TAU, 96, Color(inner_line_color, inner_line_color.a * _appear), 4.0, true)

	# Icons
	var icon_radius := (outer + inner) * 0.5
	var size_now := icon_size * grow
	for i in _textures.size():
		var texture: Texture2D = _textures[i]
		if texture == null:
			continue
		var pos := center + Vector2.from_angle(_section_angle(i)) * icon_radius - Vector2(size_now, size_now) * 0.5
		draw_texture_rect(texture, Rect2(pos, Vector2(size_now, size_now)), false, Color(1, 1, 1, _appear))
