extends Node2D

## Seels' Blood: scarlet will-o'-wisps circling the creature.
## One wisp per Blood for the first 10; past 10, the wisps grow instead (bigger with more Blood).
## Driven from C# (CreatureFxController.cs) through set_area() and set_blood().
## Wisps on the far half of the orbit are drawn on a sibling node moved behind the creature's body.

const MAX_WISPS := 10
const CORE := Color(1.0, 0.55, 0.55)
const GLOW := Color(0.85, 0.04, 0.12)

@export var wisp_radius := 7.0
@export var orbit_speed := 0.7
@export var bob_amplitude := 8.0
@export var fade_time := 0.35
## Extra size per Blood above 10, and the most it can grow
@export var growth_per_blood := 0.06
@export var max_growth := 2.5
@export var trail_length := 5

var _center := Vector2(0.0, -150.0)
var _radius := Vector2(120.0, 35.0)
var _wisps: Array = []
var _time := 0.0
var _scale := 1.0
var _target_scale := 1.0
## Sibling drawn behind the creature's body, for the wisps on the far side of the orbit
var _back: Node2D


func _ready() -> void:
	_back = Node2D.new()
	_back.name = "BloodWispsBack"
	_back.draw.connect(func() -> void: _draw_wisps(_back, false))
	_attach_back.call_deferred()


func _attach_back() -> void:
	var parent := get_parent()
	if parent == null or not is_instance_valid(_back) or _back.get_parent() != null:
		return
	parent.add_child(_back)
	parent.move_child(_back, 0)
	_back.position = position
	_back.scale = scale


## Freed with this node (not on exit_tree, so moving the creature around the tree keeps it)
func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE and is_instance_valid(_back):
		_back.queue_free()


## area: the creature's bounds (local)
func set_area(area: Rect2) -> void:
	_center = area.get_center() + Vector2(0.0, area.size.y * 0.05)
	_radius = Vector2(area.size.x * 0.65, area.size.y * 0.1)


func set_blood(blood: int) -> void:
	var count := clampi(blood, 0, MAX_WISPS)
	_target_scale = minf(1.0 + maxf(blood - MAX_WISPS, 0) * growth_per_blood, max_growth)
	var alive := _wisps.filter(func(w: Dictionary) -> bool: return not w["leaving"])
	while alive.size() < count:
		var wisp := {
			"phase": 0.0,
			"bob": randf() * TAU,
			"height": randf_range(-0.6, 0.6),
			"flicker": randf() * TAU,
			"alpha": 0.0,
			"leaving": false,
		}
		_wisps.append(wisp)
		alive.append(wisp)
	while alive.size() > count:
		var leaving: Dictionary = alive.pop_back()
		leaving["leaving"] = true
	# Spread the remaining wisps evenly around the orbit
	for i in alive.size():
		alive[i]["target_phase"] = TAU * i / maxf(alive.size(), 1.0)
		if alive[i]["alpha"] == 0.0:
			alive[i]["phase"] = alive[i]["target_phase"]


func _process(delta: float) -> void:
	_time += delta
	_scale = lerpf(_scale, _target_scale, minf(delta * 3.0, 1.0))
	for wisp in _wisps:
		if wisp["leaving"]:
			wisp["alpha"] = maxf(wisp["alpha"] - delta / fade_time, 0.0)
		else:
			wisp["alpha"] = minf(wisp["alpha"] + delta / fade_time, 1.0)
			wisp["phase"] = lerp_angle(wisp["phase"], wisp["target_phase"], minf(delta * 3.0, 1.0))
	_wisps = _wisps.filter(func(w: Dictionary) -> bool: return not (w["leaving"] and w["alpha"] <= 0.0))
	visible = not _wisps.is_empty()
	if is_instance_valid(_back):
		_back.visible = visible
	if visible:
		queue_redraw()
		if is_instance_valid(_back):
			_back.queue_redraw()


func _wisp_position(wisp: Dictionary, time: float) -> Vector2:
	var angle: float = wisp["phase"] + time * orbit_speed
	var pos := _center + Vector2(cos(angle) * _radius.x, sin(angle) * _radius.y + wisp["height"] * _radius.y * 2.0)
	pos.y += sin(time * 1.9 + wisp["bob"]) * bob_amplitude
	return pos


func _draw() -> void:
	# Before the back layer is attached, draw everything here
	_draw_wisps(self, true, not (is_instance_valid(_back) and _back.is_inside_tree()))


## front: draws the near half of the orbit (on this node, in front of the body), else the far half (on _back)
func _draw_wisps(canvas: Node2D, front: bool, all := false) -> void:
	# Back wisps first, so the front ones are drawn over them
	var order := _wisps.duplicate()
	order.sort_custom(func(a: Dictionary, b: Dictionary) -> bool: return sin(a["phase"] + _time * orbit_speed) < sin(b["phase"] + _time * orbit_speed))
	for wisp in order:
		var depth := sin(wisp["phase"] + _time * orbit_speed)  # -1 = behind, 1 = in front
		if not all and (depth >= 0.0) != front:
			continue
		var size := wisp_radius * _scale * lerpf(0.7, 1.1, (depth + 1.0) * 0.5)
		var alpha: float = wisp["alpha"] * lerpf(0.5, 1.0, (depth + 1.0) * 0.5)
		var flicker := 0.85 + 0.15 * sin(_time * 9.0 + wisp["flicker"])
		# Fading trail behind the wisp
		for i in range(trail_length, 0, -1):
			var p := _wisp_position(wisp, _time - i * 0.045)
			var k := 1.0 - float(i) / (trail_length + 1)
			canvas.draw_circle(p, size * 0.75 * k, Color(GLOW, alpha * 0.35 * k))
		var pos := _wisp_position(wisp, _time)
		# Soft glow, flame, then bright core
		canvas.draw_circle(pos, size * 2.0 * flicker, Color(GLOW, alpha * 0.18))
		canvas.draw_circle(pos, size * 1.3 * flicker, Color(GLOW, alpha * 0.45))
		canvas.draw_circle(pos + Vector2(0.0, -size * 0.35), size * 0.9, Color(GLOW, alpha * 0.85))
		canvas.draw_circle(pos, size * 0.5, Color(CORE, alpha))
