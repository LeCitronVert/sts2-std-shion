extends Node2D

## Slal's cartoon gags: something heavy falls from the sky onto the target's head.
##   "anvil": a black anvil lands with a CLONK, squashes the target, little stars spin, then it fades away.
##   "flower_pot": a terracotta pot with a flower lands, shatters into shards and dirt, the flower flips away.
## Both leave the target a bit flatter for the rest of the combat, more with every gag (see _squash_target).
## Spawned from C# (SlalVfx.PlayFallingGag) in the target's visuals, then play(kind, at, fall_height) is called.
## Purely visual (randf is fine here: nothing in the game state depends on it).

const OUTLINE := Color(0.08, 0.08, 0.1)
const ANVIL := Color(0.24, 0.25, 0.3)
const ANVIL_SHINE := Color(0.5, 0.52, 0.6)
const POT := Color(0.8, 0.4, 0.22)
const POT_DARK := Color(0.6, 0.27, 0.14)
const DIRT := Color(0.33, 0.21, 0.12)
const STEM := Color(0.25, 0.6, 0.25)
const PETAL := Color(1.0, 0.45, 0.7)
const PISTIL := Color(1.0, 0.85, 0.2)
const STAR := Color(1.0, 0.9, 0.3)

@export var fall_time := 0.42
@export var anvil_size := 70.0
@export var pot_size := 46.0

## Lasting flattening per gag (fraction of the target's height), the anvil's is 3x the flower pot's
const POT_FLATTEN := 0.04
const ANVIL_FLATTEN := POT_FLATTEN * 3.0
## The target never gets flatter than this (each gag adds less the closer it gets)
const FLATTEN_MAX := 0.55
## Metas on the target's body: how flat it is so far, its scale before any gag, the running squash tween
const FLATTEN_META := &"slal_gag_flatten"
const BASE_SCALE_META := &"slal_gag_base_scale"
const TWEEN_META := &"slal_gag_squash_tween"

var kind := "anvil"
var _impact_at := Vector2.ZERO
var _fall_height := 900.0
var _y := 0.0               # Offset above the impact point (fall_height -> 0)
var _landed := false
var _after := 0.0           # Time since impact
var _alpha := 1.0
var _squash := 1.0          # Squash of the falling object on impact
var _pieces: Array[Dictionary] = []
var _flower := {}


func _ready() -> void:
	set_process(false)


func play(gag_kind: String, at: Vector2, fall_height: float) -> void:
	kind = gag_kind
	# The head is lower on a target the previous gags flattened (the body's origin is at its feet)
	var body := _target_body()
	if body != null:
		var lowered := body.position.y + (at.y - body.position.y) * (1.0 - _flatten_of(body))
		# Still starts from the same spot above the screen
		fall_height += lowered - at.y
		at.y = lowered
	_impact_at = at
	_fall_height = fall_height
	_y = fall_height
	position = at
	set_process(true)
	queue_redraw()
	var tween := create_tween()
	# Accelerates like a real fall
	tween.tween_property(self, "_y", 0.0, fall_time).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	tween.tween_callback(_land)


func _land() -> void:
	_landed = true
	_squash_target()
	if kind == "anvil":
		_squash = 0.7
		create_tween().tween_property(self, "_squash", 1.0, 0.25).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)
		for i in 8:
			# Dust puffs on both sides
			var side := -1.0 if i % 2 == 0 else 1.0
			_pieces.append({
				"pos": Vector2(side * anvil_size * 0.4, 0.0),
				"velocity": Vector2(side * randf_range(80.0, 220.0), randf_range(-120.0, -30.0)),
				"size": randf_range(8.0, 16.0),
				"dust": true,
			})
		var tween := create_tween()
		tween.tween_interval(0.75)
		tween.tween_property(self, "_alpha", 0.0, 0.3)
		tween.tween_callback(queue_free)
	else:
		for i in 9:
			var dir := Vector2.from_angle(randf_range(PI * 1.05, TAU * 0.98))
			_pieces.append({
				"pos": dir * pot_size * 0.3,
				"velocity": dir * randf_range(200.0, 420.0) + Vector2(0.0, -120.0),
				"spin": randf_range(-14.0, 14.0),
				"rotation": randf() * TAU,
				"size": randf_range(9.0, 16.0),
				"shard": true,
			})
		for i in 12:
			var dir := Vector2.from_angle(randf_range(PI * 1.1, TAU * 0.95))
			_pieces.append({
				"pos": dir * pot_size * 0.2,
				"velocity": dir * randf_range(120.0, 320.0),
				"size": randf_range(3.0, 6.0),
				"dirt": true,
			})
		_flower = {"pos": Vector2(0.0, -pot_size), "velocity": Vector2(randf_range(-90.0, 90.0), -380.0), "rotation": 0.0, "spin": randf_range(-9.0, 9.0)}
		var tween := create_tween()
		tween.tween_interval(0.7)
		tween.tween_property(self, "_alpha", 0.0, 0.3)
		tween.tween_callback(queue_free)


## Squashes the creature the gag lands on (its visuals are our parent), then it springs back, but not all the way:
## each gag leaves the target a bit flatter for the rest of the combat, pressed down from where it landed.
## The flattening builds up with every gag (the anvil's 3x the flower pot's) and slows down near FLATTEN_MAX.
func _squash_target() -> void:
	var body := _target_body()
	if body == null:
		return
	var base := _base_scale(body)
	var before := _flatten_of(body)
	var after := before + (ANVIL_FLATTEN if kind == "anvil" else POT_FLATTEN) * (1.0 - before / FLATTEN_MAX)
	body.set_meta(FLATTEN_META, after)

	var strength := 0.72 if kind == "anvil" else 0.85
	var hold := 0.12 if kind == "anvil" else 0.04
	var squashed := _flat_scale(base, before) * Vector2((2.0 - strength) * 0.95, strength)
	var rest := _flat_scale(base, after)

	# A gag landing while the previous one's squash is still going takes over from it
	if body.has_meta(TWEEN_META):
		var previous: Tween = body.get_meta(TWEEN_META)
		if previous != null and previous.is_valid():
			previous.kill()
	var tween := body.create_tween()
	body.set_meta(TWEEN_META, tween)
	tween.tween_property(body, "scale", squashed, 0.06)
	tween.tween_interval(hold)
	tween.tween_property(body, "scale", rest, 0.4).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)

	# The gag stays on the head while it goes down and back up (the body's origin is at its feet)
	# (_impact_at is already on the head as flattened by the previous gags, see play())
	var feet := body.position.y
	var height := _impact_at.y - feet
	var current := _flat_scale(base, before).y
	var follow := create_tween()
	follow.tween_property(self, "position", Vector2(_impact_at.x, feet + height * squashed.y / current), 0.06)
	follow.tween_interval(hold)
	follow.tween_property(self, "position", Vector2(_impact_at.x, feet + height * rest.y / current), 0.4).set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)


func _target_body() -> Node2D:
	var target := get_parent() as Node2D
	if target == null:
		return null
	return target.call("GetCurrentBody") if target.has_method("GetCurrentBody") else target


## The body's scale before any gag flattened it (remembered on the first one).
func _base_scale(body: Node2D) -> Vector2:
	if not body.has_meta(BASE_SCALE_META):
		body.set_meta(BASE_SCALE_META, body.scale)
	return body.get_meta(BASE_SCALE_META)


func _flatten_of(body: Node2D) -> float:
	return body.get_meta(FLATTEN_META, 0.0)


## Shorter by "flatten", and a bit wider to keep some volume (keeps the sign of a flipped body).
func _flat_scale(base: Vector2, flatten: float) -> Vector2:
	return Vector2(base.x * (1.0 + flatten * 0.6), base.y * (1.0 - flatten))


func _process(delta: float) -> void:
	if _landed:
		_after += delta
		for p in _pieces:
			p["velocity"].y += (300.0 if p.has("dust") else 1100.0) * delta
			if p.has("dust"):
				p["velocity"] *= 1.0 - 3.0 * delta
				p["size"] += 14.0 * delta
			p["pos"] += p["velocity"] * delta
			if p.has("spin"):
				p["rotation"] += p["spin"] * delta
		if not _flower.is_empty():
			_flower["velocity"].y += 900.0 * delta
			_flower["pos"] += _flower["velocity"] * delta
			_flower["rotation"] += _flower["spin"] * delta
	queue_redraw()


func _draw() -> void:
	if kind == "anvil":
		_draw_anvil_scene()
	else:
		_draw_pot_scene()


# ---------------------------------------------------------------- Anvil

func _draw_anvil_scene() -> void:
	if not _landed:
		# Shadow growing on the head as it gets closer
		var k := 1.0 - _y / _fall_height
		draw_set_transform(Vector2(0.0, 4.0), 0.0, Vector2(1.0, 0.25))
		draw_circle(Vector2.ZERO, anvil_size * 0.6 * k, Color(0, 0, 0, 0.3 * k))
		draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
		_draw_anvil(Vector2(0.0, -_y), 1.0)
		# Speed lines above it
		for i in 3:
			var x := (i - 1) * anvil_size * 0.35
			draw_line(Vector2(x, -_y - anvil_size * 0.9), Vector2(x, -_y - anvil_size * 1.6), Color(1, 1, 1, 0.6), 3.0)
		return

	for p in _pieces:
		draw_circle(p["pos"], p["size"], Color(0.85, 0.8, 0.7, 0.55 * _alpha * clampf(1.0 - _after / 0.7, 0.0, 1.0)))
	_draw_anvil(Vector2.ZERO, _alpha)
	# Dizzy stars spinning above it
	if _after > 0.1:
		for i in 3:
			var a := _after * 7.0 + TAU * i / 3.0
			var c := Vector2(cos(a) * anvil_size * 0.55, -anvil_size * 0.85 + sin(a) * anvil_size * 0.12)
			_draw_star(c, 9.0, Color(STAR, _alpha))
	# CLONK flash
	if _after < 0.18:
		var k := _after / 0.18
		for i in 8:
			var dir := Vector2.from_angle(PI + PI * (i + 0.5) / 8.0)
			draw_line(dir * anvil_size * (0.6 + k * 0.6), dir * anvil_size * (0.9 + k * 0.9), Color(1, 1, 1, 1.0 - k), 4.0)


## Bottom center of the anvil at "base".
func _draw_anvil(base: Vector2, alpha: float) -> void:
	var s := anvil_size
	var squash := Vector2((2.0 - _squash), _squash)
	draw_set_transform(base, 0.0, squash)
	var body := PackedVector2Array([
		# Foot
		Vector2(-s * 0.45, 0.0), Vector2(s * 0.45, 0.0), Vector2(s * 0.38, -s * 0.12), Vector2(s * 0.2, -s * 0.18),
		# Waist
		Vector2(s * 0.16, -s * 0.4),
		# Face (right side, then the horn on the left)
		Vector2(s * 0.5, -s * 0.46), Vector2(s * 0.5, -s * 0.62), Vector2(-s * 0.3, -s * 0.62),
		Vector2(-s * 0.75, -s * 0.56), Vector2(-s * 0.32, -s * 0.46),
		Vector2(-s * 0.16, -s * 0.4),
		Vector2(-s * 0.2, -s * 0.18), Vector2(-s * 0.38, -s * 0.12),
	])
	var outline := PackedVector2Array()
	for v in body:
		outline.append(v)
	outline.append(body[0])
	draw_colored_polygon(body, Color(ANVIL, alpha))
	draw_polyline(outline, Color(OUTLINE, alpha), 3.0, true)
	draw_line(Vector2(-s * 0.25, -s * 0.58), Vector2(s * 0.42, -s * 0.58), Color(ANVIL_SHINE, alpha), 3.0)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_star(center: Vector2, r: float, color: Color) -> void:
	var points := PackedVector2Array()
	for i in 10:
		var radius := r if i % 2 == 0 else r * 0.45
		points.append(center + Vector2.from_angle(-PI / 2.0 + TAU * i / 10.0) * radius)
	draw_colored_polygon(points, color)


# ---------------------------------------------------------------- Flower pot

func _draw_pot_scene() -> void:
	if not _landed:
		var k := 1.0 - _y / _fall_height
		draw_set_transform(Vector2(0.0, 4.0), 0.0, Vector2(1.0, 0.25))
		draw_circle(Vector2.ZERO, pot_size * 0.55 * k, Color(0, 0, 0, 0.3 * k))
		draw_set_transform(Vector2(0.0, -_y), sin(_y * 0.01) * 0.25, Vector2.ONE)
		_draw_pot()
		_draw_flower(Vector2(0.0, -pot_size), 1.0)
		draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
		return

	for p in _pieces:
		if p.has("shard"):
			var tri := PackedVector2Array([Vector2(-1.0, -0.7), Vector2(1.0, -0.5), Vector2(0.2, 0.9)])
			for j in 3:
				tri[j] = p["pos"] + (tri[j] * p["size"]).rotated(p["rotation"])
			draw_colored_polygon(tri, Color(POT, _alpha))
		else:
			draw_circle(p["pos"], p["size"], Color(DIRT, _alpha))
	if not _flower.is_empty():
		draw_set_transform(_flower["pos"] + Vector2(0.0, pot_size), _flower["rotation"], Vector2.ONE)
		_draw_flower(Vector2(0.0, -pot_size), _alpha)
		draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
	# Crash flash
	if _after < 0.15:
		var k := _after / 0.15
		draw_circle(Vector2(0.0, -pot_size * 0.3), pot_size * (0.6 + k), Color(1, 1, 1, 0.5 * (1.0 - k)))


## Bottom center of the pot at the origin.
func _draw_pot() -> void:
	var s := pot_size
	var pot := PackedVector2Array([
		Vector2(-s * 0.32, 0.0), Vector2(s * 0.32, 0.0), Vector2(s * 0.45, -s * 0.72), Vector2(-s * 0.45, -s * 0.72),
	])
	draw_colored_polygon(pot, POT)
	draw_polyline(PackedVector2Array([pot[0], pot[1], pot[2], pot[3], pot[0]]), OUTLINE, 3.0, true)
	var rim := Rect2(-s * 0.55, -s * 0.92, s * 1.1, s * 0.22)
	draw_rect(rim, POT_DARK)
	draw_rect(rim, OUTLINE, false, 3.0)
	draw_rect(Rect2(-s * 0.48, -s * 0.98, s * 0.96, s * 0.08), DIRT)


## The stem starts at "base" (the dirt) and goes up.
func _draw_flower(base: Vector2, alpha: float) -> void:
	var s := pot_size
	var top := base + Vector2(s * 0.08, -s * 0.75)
	draw_line(base, top, Color(STEM, alpha), 4.0)
	draw_colored_polygon(PackedVector2Array([
		base + Vector2(0.0, -s * 0.3), base + Vector2(s * 0.35, -s * 0.5), base + Vector2(s * 0.05, -s * 0.42),
	]), Color(STEM, alpha))
	for i in 5:
		draw_circle(top + Vector2.from_angle(TAU * i / 5.0 - PI / 2.0) * s * 0.17, s * 0.14, Color(PETAL, alpha))
	draw_circle(top, s * 0.11, Color(PISTIL, alpha))
