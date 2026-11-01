extends Node2D

## A honeycomb barrier in front of a creature (Scintilla-like), while it has Defensive (pale, thin)
## or Mighty Guard (azure, thicker). Driven from C# (CreatureFxController.cs) through
## set_style(), set_area() and set_active().
## The cells are laid out on a panel wrapped around a vertical cylinder: cells near its sides are squashed,
## so the barrier looks curved. Cells pop in from the center outwards, shimmer while active and
## scatter outwards when it goes away.

@export var cell_radius := 20.0
## Half width of the panel on screen, relative to its height
@export var bulge := 0.2
@export var appear_time := 0.35
@export var disappear_time := 0.35

var _color := Color(0.88, 0.85, 1.0)
## Edge width; Mighty Guard also adds an inner layer
var _thickness := 2.5
var _layers := 1
## Panel center (in front of the creature) and size, local to this node
var _center := Vector2(80.0, -150.0)
var _half_width := 60.0
var _half_height := 160.0

var _active := false
var _visibility := 0.0  # 0 = gone, 1 = fully shown
var _time := 0.0
var _cells: Array[Dictionary] = []


func _ready() -> void:
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	material = additive
	visible = false


func set_style(color: Color, thickness: float, layers: int) -> void:
	_color = color
	_thickness = thickness
	_layers = layers


## area: the creature's bounds (local), facing: 1 if the creature faces right, -1 if left
func set_area(area: Rect2, facing: float) -> void:
	_half_height = area.size.y * 0.55
	_half_width = _half_height * 2.0 * bulge
	var front := area.end.x if facing > 0.0 else area.position.x
	_center = Vector2(front - facing * area.size.x * 0.1, area.get_center().y)
	_build_cells()


func set_active(active: bool) -> void:
	if active == _active:
		return
	_active = active
	if active:
		visible = true


func _build_cells() -> void:
	_cells.clear()
	var row_h := cell_radius * 1.5
	var col_w := cell_radius * sqrt(3.0)
	# Panel coordinates: u across (wraps around the cylinder), v up/down, both in [-_half_height, _half_height]
	var rows := int(ceil(_half_height / row_h))
	var cols := int(ceil(_half_height / col_w)) + 1
	for r in range(-rows, rows + 1):
		for c in range(-cols, cols + 1):
			var u := c * col_w + (col_w * 0.5 if posmod(r, 2) == 1 else 0.0)
			var v := r * row_h
			var ex := u / (_half_height * 0.85)
			var ey := v / _half_height
			var dist := sqrt(ex * ex + ey * ey)
			if dist > 1.0:
				continue
			_cells.append({
				"u": u,
				"v": v,
				"dist": dist,
				"phase": randf() * TAU,
				"scatter": Vector2.from_angle(atan2(v, u) + randf_range(-0.4, 0.4)) * randf_range(40.0, 110.0),
			})


## Panel point -> local position, wrapping the panel around a vertical cylinder
func _project(u: float, v: float) -> Vector2:
	var angle := clampf(u / (_half_height * 0.85), -1.0, 1.0) * PI * 0.45
	return _center + Vector2(sin(angle) * _half_width, v)


func _process(delta: float) -> void:
	if not visible:
		return
	_time += delta
	if _active:
		_visibility = minf(_visibility + delta / appear_time, 1.0)
	else:
		_visibility = maxf(_visibility - delta / disappear_time, 0.0)
		if _visibility <= 0.0:
			visible = false
			return
	queue_redraw()


func _draw() -> void:
	for layer in _layers:
		# Inner layers are slightly smaller and offset backwards
		var shrink := 1.0 - layer * 0.12
		for cell in _cells:
			var reveal: float
			if _active:
				# Pops in from the center outwards
				reveal = clampf((_visibility * 1.6 - cell["dist"]) / 0.6, 0.0, 1.0)
			else:
				reveal = _visibility
			if reveal <= 0.0:
				continue
			var offset: Vector2 = Vector2.ZERO if _active else cell["scatter"] * (1.0 - _visibility)
			var shimmer := 0.65 + 0.35 * sin(_time * 3.0 + cell["phase"])
			var rim := 0.5 + 0.5 * float(cell["dist"])  # brighter towards the edges, like a bubble
			_draw_cell(cell["u"], cell["v"], shrink, reveal, shimmer * rim, offset)


func _draw_cell(u: float, v: float, shrink: float, reveal: float, brightness: float, offset: Vector2) -> void:
	var points := PackedVector2Array()
	var r := cell_radius * 0.92 * reveal
	for i in 6:
		var a := PI / 6.0 + i * PI / 3.0
		points.append(_project((u + cos(a) * r) * shrink, (v + sin(a) * r) * shrink) + offset)
	var fill := _color
	fill.a = 0.12 * brightness * reveal
	draw_colored_polygon(points, fill)
	points.append(points[0])
	var edge := _color
	edge.a = 0.85 * brightness * reveal
	draw_polyline(points, edge, _thickness, true)
