extends Node2D

## Hyper Guard: afterimages of the creature's body, filled with its character's main color, trailing its movements.
## Every spawn_interval seconds the body's current pose is copied, left where it was and faded out while drifting
## a little behind the creature, so the afterimages show even when it stands still.
## Works with Spine bodies (vanilla characters, enemies) and AnimatedSprite2D / Sprite2D bodies (mod characters).
## Driven from C# (CreatureFxController.cs) through set_body(), set_style(), set_facing() and set_active().
## Afterimages are pooled and reused, so Spine skeletons aren't created every spawn.

const SHADER_PATH := "res://ShionTheBruiser/scenes/fx/afterimage.gdshader"
const SPINE_CLASS := "SpineSprite"
const SPINE_TRACKS := 4

@export var spawn_interval := 0.06
@export var lifetime := 0.4
@export var start_alpha := 0.5
## How far an afterimage drifts behind the creature over its lifetime
@export var drift := 36.0

var _body: Node2D
var _facing := 1.0
var _active := false
var _timer := 0.0
var _material := ShaderMaterial.new()
## { "node": Node2D, "age": float, "origin": Transform2D }; age < 0 = unused
var _ghosts: Array[Dictionary] = []


func _ready() -> void:
	# Not top_level: that would also draw the afterimages over the body. They stay where they were left when the
	# creature moves because their global transform is set every frame instead.
	_material.shader = load(SHADER_PATH)
	visible = false


func set_body(body: Node2D) -> void:
	if body == _body:
		return
	_body = body
	# The pooled afterimages were made for the previous body
	for ghost in _ghosts:
		ghost["node"].queue_free()
	_ghosts.clear()


func set_style(color: Color) -> void:
	_material.set_shader_parameter("tint", color)


## 1 if the creature faces right, -1 if left
func set_facing(facing: float) -> void:
	_facing = facing


func set_active(active: bool) -> void:
	if active == _active:
		return
	_active = active
	if active:
		visible = true
		_timer = 0.0


func _max_ghosts() -> int:
	return int(ceil(lifetime / spawn_interval)) + 1


func _process(delta: float) -> void:
	if _active:
		_timer -= delta
		if _timer <= 0.0:
			_timer += spawn_interval
			_snapshot()

	var any_visible := false
	for ghost in _ghosts:
		if ghost["age"] < 0.0:
			continue
		ghost["age"] += delta
		var t: float = ghost["age"] / lifetime
		var node: Node2D = ghost["node"]
		if t >= 1.0:
			ghost["age"] = -1.0
			node.visible = false
			continue
		any_visible = true
		var origin: Transform2D = ghost["origin"]
		var eased := 1.0 - (1.0 - t) * (1.0 - t)
		node.global_transform = origin.translated(Vector2(-_facing * drift * eased, 0.0))
		node.modulate.a = start_alpha * (1.0 - t)

	if not _active and not any_visible:
		visible = false


func _snapshot() -> void:
	if not is_instance_valid(_body) or not _body.is_visible_in_tree():
		return
	var ghost := _take_ghost()
	if ghost.is_empty():
		return
	var node: Node2D = ghost["node"]
	if not _copy_pose(node):
		return
	ghost["age"] = 0.0
	ghost["origin"] = _body.global_transform
	node.global_transform = _body.global_transform
	node.modulate.a = start_alpha
	node.visible = true


## An unused afterimage, a new one, or the oldest one when the pool is full
func _take_ghost() -> Dictionary:
	var oldest := {}
	for ghost in _ghosts:
		if ghost["age"] < 0.0:
			return ghost
		if oldest.is_empty() or ghost["age"] > oldest["age"]:
			oldest = ghost
	if _ghosts.size() >= _max_ghosts():
		return oldest
	var node := _make_ghost()
	if node == null:
		return {}
	add_child(node)
	var ghost := { "node": node, "age": -1.0, "origin": Transform2D.IDENTITY }
	_ghosts.append(ghost)
	return ghost


func _make_ghost() -> Node2D:
	if _body.get_class() == SPINE_CLASS:
		if not ClassDB.can_instantiate(SPINE_CLASS):
			return null
		var spine: Node2D = ClassDB.instantiate(SPINE_CLASS)
		spine.set("skeleton_data_res", _body.get("skeleton_data_res"))
		for property in ["normal_material", "additive_material", "multiply_material", "screen_material"]:
			spine.set(property, _material)
		spine.visible = false
		return spine
	if _body is AnimatedSprite2D or _body is Sprite2D:
		var sprite := Sprite2D.new()
		sprite.material = _material
		sprite.texture_filter = _body.texture_filter
		sprite.visible = false
		return sprite
	return null


func _copy_pose(node: Node2D) -> bool:
	if node is Sprite2D:
		return _copy_sprite(node)
	return _copy_spine(node)


func _copy_sprite(ghost: Sprite2D) -> bool:
	if _body is AnimatedSprite2D:
		var body := _body as AnimatedSprite2D
		if body.sprite_frames == null or not body.sprite_frames.has_animation(body.animation):
			return false
		ghost.texture = body.sprite_frames.get_frame_texture(body.animation, body.frame)
		ghost.centered = body.centered
		ghost.offset = body.offset
		ghost.flip_h = body.flip_h
		ghost.flip_v = body.flip_v
	elif _body is Sprite2D:
		var body := _body as Sprite2D
		ghost.texture = body.texture
		ghost.centered = body.centered
		ghost.offset = body.offset
		ghost.flip_h = body.flip_h
		ghost.flip_v = body.flip_v
		ghost.hframes = body.hframes
		ghost.vframes = body.vframes
		ghost.frame = body.frame
		ghost.region_enabled = body.region_enabled
		ghost.region_rect = body.region_rect
	return ghost.texture != null


## Replays the body's animation tracks at their current times on the afterimage, frozen there
func _copy_spine(ghost: Node2D) -> bool:
	var source = _body.call("get_animation_state")
	var target = ghost.call("get_animation_state")
	if source == null or target == null:
		return false

	var any_track := false
	for track in SPINE_TRACKS:
		var entry = source.call("get_current", track)
		var animation = entry.call("get_animation") if entry != null else null
		if animation == null:
			if target.has_method("clear_track"):
				target.call("clear_track", track)
			continue
		var loop: bool = entry.call("get_loop") if entry.has_method("get_loop") else true
		var copy = target.call("set_animation", animation.call("get_name"), loop, track)
		if copy == null:
			continue
		copy.call("set_mix_duration", 0.0)
		copy.call("set_track_time", entry.call("get_track_time"))
		any_track = true
	target.call("set_time_scale", 0.0)

	var source_skeleton = _body.call("get_skeleton")
	var target_skeleton = ghost.call("get_skeleton")
	if source_skeleton != null and target_skeleton != null:
		if source_skeleton.has_method("get_skin"):
			var skin = source_skeleton.call("get_skin")
			if skin != null and (not target_skeleton.has_method("get_skin") or target_skeleton.call("get_skin") != skin):
				target_skeleton.call("set_skin", skin)
				target_skeleton.call("set_slots_to_setup_pose")
		if source_skeleton.has_method("get_scale_x"):
			target_skeleton.call("set_scale_x", source_skeleton.call("get_scale_x"))
			target_skeleton.call("set_scale_y", source_skeleton.call("get_scale_y"))
	return any_track
