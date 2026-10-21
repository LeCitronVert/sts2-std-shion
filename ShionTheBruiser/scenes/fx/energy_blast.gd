extends Node2D

## Energy blast thrown by Energy Attacks. Spawned from C# (EnergyBlast.cs), which calls launch()
## and waits for the same duration before dealing the damage.

const SHEETS := {
	false: {
		"path": "res://ShionTheBruiser/images/charui/shion/energy/normal.png",
		"blast": [Rect2(2, 0, 222, 128), Rect2(1, 124, 223, 109), Rect2(2, 229, 214, 119)],
		"burst": Rect2(5, 346, 111, 131),
		"orb": Rect2(115, 348, 62, 60),
		"star": Rect2(188, 352, 49, 51),
	},
	true: {
		"path": "res://ShionTheBruiser/images/charui/shion/energy/spirit_burst.png",
		"blast": [Rect2(0, 0, 230, 131), Rect2(258, 0, 222, 132), Rect2(0, 126, 224, 113)],
		"burst": Rect2(256, 135, 133, 114),
		"orb": Rect2(389, 197, 64, 62),
		"star": Rect2(403, 139, 44, 46),
	},
}

@export var blast_scale := 0.8
@export var blast_fps := 18.0
## Where the blast's head is on its texture (fraction of its width from the left)
@export var head_position := 0.85

var _sheet: Dictionary
var _texture: Texture2D


func launch(from: Vector2, to: Vector2, spirit_burst: bool, duration: float) -> void:
	_sheet = SHEETS[spirit_burst]
	_texture = load(_sheet["path"])
	global_position = from
	var direction := (to - from).angle()

	# Muzzle flash at Shion's hands
	_flash(_sheet["orb"], from, 0.9, 0.15)

	var blast := AnimatedSprite2D.new()
	var frames := SpriteFrames.new()
	frames.set_animation_speed(&"default", blast_fps)
	frames.set_animation_loop(&"default", true)
	for region in _sheet["blast"]:
		frames.add_frame(&"default", _atlas(region))
	blast.sprite_frames = frames
	blast.material = _additive()
	blast.rotation = direction
	blast.scale = Vector2.ONE * blast_scale
	var width: float = (_sheet["blast"][0] as Rect2).size.x
	blast.offset = Vector2(width * (0.5 - head_position), 0.0)
	blast.play(&"default")
	add_child(blast)

	var tween := create_tween()
	tween.tween_property(blast, "global_position", to, max(duration, 0.01)).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
	tween.tween_callback(func() -> void:
		blast.queue_free()
		_flash(_sheet["burst"], to, 1.1, 0.3)
		_flash(_sheet["star"], to, 1.6, 0.25)
	)
	tween.tween_interval(0.35)
	tween.tween_callback(queue_free)


func _flash(region: Rect2, at: Vector2, flash_scale: float, duration: float) -> void:
	var sprite := Sprite2D.new()
	sprite.texture = _atlas(region)
	sprite.material = _additive()
	add_child(sprite)
	sprite.global_position = at
	sprite.rotation = randf_range(-0.3, 0.3)
	sprite.scale = Vector2.ONE * flash_scale * 0.6
	var tween := sprite.create_tween().set_parallel(true)
	tween.tween_property(sprite, "scale", Vector2.ONE * flash_scale, duration).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	tween.tween_property(sprite, "modulate:a", 0.0, duration).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	tween.chain().tween_callback(sprite.queue_free)


func _atlas(region: Rect2) -> AtlasTexture:
	var atlas := AtlasTexture.new()
	atlas.atlas = _texture
	atlas.region = region
	return atlas


func _additive() -> CanvasItemMaterial:
	var material := CanvasItemMaterial.new()
	material.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	return material
