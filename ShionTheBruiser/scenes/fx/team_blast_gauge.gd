extends Control

## Team Blast gauge shown under the players.
## Driven from C# (TeamBlastGaugePatch.cs) through set_gauge().
## Filling: empty bar + cyan fill. Full: yellow "TEAM BLAST". Used this combat: orange "TEAM BLAST".

enum GaugeState { FILLING, READY, LOCKED }

const BAR_TEXTURE := preload("res://ShionTheBruiser/images/ui/team_blast/team_blast_bar.png")
const READY_TEXTURE := preload("res://ShionTheBruiser/images/ui/team_blast/team_blast_ready.png")
const LOCKED_TEXTURE := preload("res://ShionTheBruiser/images/ui/team_blast/team_blast_locked.png")

## Size of one sprite pixel on screen
@export var pixel_scale := 3.0
## Fill speed of the displayed bar, in full bars per second
@export var fill_speed := 1.5

# Fill area inside the bar sprite, in sprite pixels
const FILL_X := 7
const FILL_Y := 3
const FILL_WIDTH := 70
const FILL_HEIGHT := 4
# Rows 0 and 3 of the fill are darker than rows 1 and 2; each row has a darker edge pixel on both ends
const FILL_OUTER_EDGE := Color8(41, 152, 193)
const FILL_OUTER := Color8(0, 206, 255)
const FILL_INNER_EDGE := Color8(0, 206, 255)
const FILL_INNER := Color8(107, 231, 255)

var _state := GaugeState.FILLING
var _target_ratio := 0.0
var _shown_ratio := 0.0
var _time := 0.0
var _initialized := false


func _ready() -> void:
	texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	size = Vector2(BAR_TEXTURE.get_width(), BAR_TEXTURE.get_height()) * pixel_scale
	custom_minimum_size = size
	pivot_offset = size * 0.5


func set_gauge(value: int, max_value: int, state: int) -> void:
	var ratio := clampf(float(value) / float(max(max_value, 1)), 0.0, 1.0)
	if not _initialized:
		# No fill animation when the combat starts with a gauge carried over
		_initialized = true
		_shown_ratio = ratio
	if state != _state:
		_state = state
		# Small pop on every state change
		scale = Vector2.ONE * 1.25
		create_tween().tween_property(self, "scale", Vector2.ONE, 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	_target_ratio = ratio
	queue_redraw()


func _process(delta: float) -> void:
	_time += delta
	if _shown_ratio != _target_ratio:
		_shown_ratio = move_toward(_shown_ratio, _target_ratio, delta * fill_speed)
		queue_redraw()
	elif _state == GaugeState.READY:
		queue_redraw()


func _draw() -> void:
	var rect := Rect2(Vector2.ZERO, size)
	match _state:
		GaugeState.READY:
			# Gentle glow while waiting to be used
			var glow := 1.0 + 0.25 * (0.5 + 0.5 * sin(_time * 4.0))
			draw_texture_rect(READY_TEXTURE, rect, false, Color(glow, glow, glow))
		GaugeState.LOCKED:
			draw_texture_rect(LOCKED_TEXTURE, rect, false)
		_:
			draw_texture_rect(BAR_TEXTURE, rect, false)
			_draw_fill(int(round(_shown_ratio * FILL_WIDTH)))


func _draw_fill(width: int) -> void:
	if width <= 0:
		return
	for row in FILL_HEIGHT:
		var outer := row == 0 or row == FILL_HEIGHT - 1
		_draw_pixels(FILL_X, FILL_Y + row, width, FILL_OUTER if outer else FILL_INNER)
		var edge := FILL_OUTER_EDGE if outer else FILL_INNER_EDGE
		_draw_pixels(FILL_X, FILL_Y + row, 1, edge)
		_draw_pixels(FILL_X + width - 1, FILL_Y + row, 1, edge)


func _draw_pixels(x: int, y: int, width: int, color: Color) -> void:
	draw_rect(Rect2(Vector2(x, y) * pixel_scale, Vector2(width, 1) * pixel_scale), color)
