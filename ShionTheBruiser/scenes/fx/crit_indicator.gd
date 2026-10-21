extends Control

## Crit Hit Rate indicator shown above the energy counter.
## Driven from C# (CritIndicatorPatch.cs) through set_rate().
## The digits stay still at 0%, bob around 50% and shake rapidly around 100%.

const DIGIT_PATH := "res://ShionTheBruiser/images/charui/shion/crit/100_typo_num01_%d.png"

@export var icon_size := 40.0
@export var digit_size := 46.0
## Horizontal distance between two digits (the glyphs are narrower than their 46px canvas)
@export var digit_spacing := 30.0
@export var max_bob_amplitude := 5.0
@export var max_shake_amplitude := 4.0
## Shake jitter refresh rate, in times per second
@export var shake_rate := 30.0

@onready var _icon: TextureRect = $Icon
@onready var _digits: Control = $Digits

## Horizontal center the indicator stays aligned to, in the parent's coordinates
var anchor_x := 0.0

var _digit_textures: Array[Texture2D] = []
var _rate := -1
var _time := 0.0
var _shake_timer := 0.0
var _shake_offsets: Array[Vector2] = []


func _ready() -> void:
	for i in 10:
		_digit_textures.append(load(DIGIT_PATH % i))
	_icon.size = Vector2(icon_size, icon_size)
	_icon.position = Vector2(0.0, (digit_size - icon_size) * 0.5)
	set_rate(0)


func set_rate(rate: int) -> void:
	rate = max(rate, 0)
	if rate == _rate:
		return
	var had_value := _rate >= 0
	_rate = rate
	_rebuild_digits()
	if had_value:
		# Small pop whenever the value changes
		_digits.scale = Vector2.ONE * 1.25
		create_tween().tween_property(_digits, "scale", Vector2.ONE, 0.2).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


func _rebuild_digits() -> void:
	for child in _digits.get_children():
		_digits.remove_child(child)
		child.queue_free()
	_shake_offsets.clear()

	var text := str(_rate)
	for i in text.length():
		var digit := TextureRect.new()
		digit.texture = _digit_textures[int(text[i])]
		digit.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		digit.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		digit.size = Vector2(digit_size, digit_size)
		digit.mouse_filter = Control.MOUSE_FILTER_IGNORE
		digit.set_meta("base_position", Vector2(i * digit_spacing, 0.0))
		digit.position = digit.get_meta("base_position")
		_digits.add_child(digit)
		_shake_offsets.append(Vector2.ZERO)

	var digits_width := (text.length() - 1) * digit_spacing + digit_size
	_digits.position = Vector2(icon_size, 0.0)
	_digits.pivot_offset = Vector2(digits_width, digit_size) * 0.5
	size = Vector2(icon_size + digits_width, digit_size)
	position.x = anchor_x - size.x * 0.5


func _process(delta: float) -> void:
	if _rate <= 0:
		return

	_time += delta
	var t := clampf(_rate / 100.0, 0.0, 1.0)
	var bob_amplitude := max_bob_amplitude * smoothstep(0.0, 0.5, t)
	var bob_speed := 2.0 + 4.0 * t
	var shake_amplitude := max_shake_amplitude * smoothstep(0.5, 1.0, t)

	_shake_timer -= delta
	var refresh_shake := _shake_timer <= 0.0
	if refresh_shake:
		_shake_timer = 1.0 / shake_rate

	var digits := _digits.get_children()
	for i in digits.size():
		var digit := digits[i] as TextureRect
		if refresh_shake:
			_shake_offsets[i] = Vector2(randf_range(-1.0, 1.0), randf_range(-1.0, 1.0)) * shake_amplitude
		var bob := Vector2(0.0, sin(_time * bob_speed + i * 0.9) * bob_amplitude)
		digit.position = digit.get_meta("base_position") + bob + _shake_offsets[i]
