extends Node2D

## Super Saiyan 2-style sparks that occasionally flash around a Berserk creature.
## Driven from C# (CreatureFxController.cs) through set_active() and set_area().

const SPARK_PATHS: Array[String] = [
	"res://ShionTheBruiser/images/charui/shion/sparks/battle_120000_16.png",
	"res://ShionTheBruiser/images/charui/shion/sparks/battle_120000_17.png",
	"res://ShionTheBruiser/images/charui/shion/sparks/battle_120000_18.png",
	"res://ShionTheBruiser/images/charui/shion/sparks/battle_120000_19.png",
	"res://ShionTheBruiser/images/charui/shion/sparks/battle_120000_20.png",
	"res://ShionTheBruiser/images/charui/shion/sparks/battle_120000_21.png",
	"res://ShionTheBruiser/images/charui/shion/sparks/battle_120000_22.png",
]

## Random delay between two spark bursts, in seconds
@export var min_interval := 0.25
@export var max_interval := 0.9
## Chance that a burst spawns a second spark at the same time
@export var double_spark_chance := 0.35
@export var min_scale := 0.45
@export var max_scale := 0.8
## How long a spark stays visible, in seconds
@export var spark_duration := 0.22

var _textures: Array[Texture2D] = []
var _active := false
var _area := Rect2(-100.0, -300.0, 200.0, 300.0)
var _timer := 0.0


func _ready() -> void:
	for path in SPARK_PATHS:
		_textures.append(load(path))


func set_active(active: bool) -> void:
	if active == _active:
		return
	_active = active
	_timer = randf_range(0.0, min_interval)


## Area (in this node's local coordinates) in which sparks can appear
func set_area(area: Rect2) -> void:
	_area = area


func _process(delta: float) -> void:
	if not _active:
		return
	_timer -= delta
	if _timer > 0.0:
		return
	_timer = randf_range(min_interval, max_interval)
	_spawn_spark()
	if randf() < double_spark_chance:
		_spawn_spark()


func _spawn_spark() -> void:
	var spark := Sprite2D.new()
	spark.texture = _textures.pick_random()
	spark.position = _area.position + Vector2(randf() * _area.size.x, randf() * _area.size.y)
	spark.rotation = randf_range(-PI, PI)
	spark.flip_h = randf() < 0.5
	spark.scale = Vector2.ONE * randf_range(min_scale, max_scale)
	var material := CanvasItemMaterial.new()
	material.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	spark.material = material
	add_child(spark)

	# Flicker on, then fade out
	var tween := spark.create_tween()
	spark.modulate.a = 1.0
	tween.tween_property(spark, "modulate:a", 0.3, spark_duration * 0.2)
	tween.tween_property(spark, "modulate:a", 1.0, spark_duration * 0.2)
	tween.tween_property(spark, "modulate:a", 0.0, spark_duration * 0.6)
	tween.tween_callback(spark.queue_free)
