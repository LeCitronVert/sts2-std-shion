extends Node2D

## Codependency's salmon pink aura: a soft glow behind the creature plus a light source shining on it.
## Driven from C# (CreatureFxController.cs) through set_area() and set_active().
## The light only lights bodies on its light layer (players' bodies, see CreatureFxController.AuraLightLayer).

const COLOR := Color8(255, 140, 130)
const LIGHT_LAYER := 1 << 19

@export var light_energy := 0.9
@export var pulse_speed := 2.0
@export var fade := 0.3

var _light: PointLight2D
var _glow: Sprite2D
var _active := false
var _strength := 0.0
var _time := 0.0


func _ready() -> void:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1, 1, 1, 1))
	gradient.set_color(1, Color(1, 1, 1, 0))
	var texture := GradientTexture2D.new()
	texture.gradient = gradient
	texture.fill = GradientTexture2D.FILL_RADIAL
	texture.fill_from = Vector2(0.5, 0.5)
	texture.fill_to = Vector2(1.0, 0.5)
	texture.width = 256
	texture.height = 256

	_glow = Sprite2D.new()
	_glow.texture = texture
	_glow.modulate = Color(COLOR, 0.0)
	_glow.light_mask = 0
	var additive := CanvasItemMaterial.new()
	additive.blend_mode = CanvasItemMaterial.BLEND_MODE_ADD
	_glow.material = additive
	add_child(_glow)

	_light = PointLight2D.new()
	_light.texture = texture
	_light.color = COLOR
	_light.energy = 0.0
	_light.blend_mode = Light2D.BLEND_MODE_ADD
	_light.range_item_cull_mask = LIGHT_LAYER
	add_child(_light)
	visible = false


## area: the creature's bounds (local)
func set_area(area: Rect2) -> void:
	position = area.get_center()
	_glow.scale = Vector2(area.size.x * 1.6, area.size.y * 1.3) / 256.0
	_light.texture_scale = area.size.y * 2.0 / 256.0


func set_active(active: bool) -> void:
	_active = active
	if active:
		visible = true


func _process(delta: float) -> void:
	if not visible:
		return
	_time += delta
	_strength = move_toward(_strength, 1.0 if _active else 0.0, delta / fade)
	if _strength <= 0.0 and not _active:
		visible = false
		return
	var pulse := 0.85 + 0.15 * sin(_time * pulse_speed)
	_glow.modulate.a = 0.45 * _strength * pulse
	_light.energy = light_energy * _strength * pulse
