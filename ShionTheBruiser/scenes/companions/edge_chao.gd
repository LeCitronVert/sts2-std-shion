extends Node2D

## Edge the Chao (EdgeTheChao relic), gently floating next to Shion.
## Created from C# (CompanionVisuals.cs): set_mode() picks his look, proc() shows his "!" for a moment in combat.
## His ponpon (the ball above his head) becomes an emote: "!" on proc, "?" at the merchant, a heart at rest sites.

enum Mode { COMBAT, MERCHANT, REST }

## The sprites are pixel art, scaled up by a whole number to stay crisp
const PIXEL_SCALE := 3.0

const EXCLAMATION := preload("res://ShionTheBruiser/images/companions/edge_emote_exclamation.png")
const QUESTION := preload("res://ShionTheBruiser/images/companions/edge_emote_question.png")
const HEART := preload("res://ShionTheBruiser/images/companions/edge_emote_heart.png")

## Body texture, emote texture, and where the bottom of the ponpon was, from the body's center, in sprite pixels.
## The emote sits on that spot.
const LOOKS := {
	"combat": [preload("res://ShionTheBruiser/images/companions/edge_combat.png"), null, Vector2.ZERO],
	"proc": [preload("res://ShionTheBruiser/images/companions/edge_proc.png"), EXCLAMATION, Vector2(4.5, -12.0)],
	"merchant": [preload("res://ShionTheBruiser/images/companions/edge_merchant.png"), QUESTION, Vector2(4.5, -11.0)],
	"rest": [preload("res://ShionTheBruiser/images/companions/edge_rest.png"), HEART, Vector2(4.5, -11.0)],
}

@export var bob_height := 6.0
@export var bob_period := 2.6
@export var proc_time := 1.2

var _mode := Mode.COMBAT
var _time := 0.0
var _proc_left := 0.0
var _float: Node2D
var _body: Sprite2D
var _emote: Sprite2D


func _ready() -> void:
	texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	_float = Node2D.new()
	_float.scale = Vector2.ONE * PIXEL_SCALE
	add_child(_float)
	_body = Sprite2D.new()
	_float.add_child(_body)
	_emote = Sprite2D.new()
	_emote.centered = false
	_float.add_child(_emote)
	# Out of sync with other Edges (multiplayer)
	_time = randf() * bob_period
	_refresh()


func set_mode(mode: int) -> void:
	_mode = mode
	_proc_left = 0.0
	if is_node_ready():
		_refresh()


## Combat only: his fourth sprite with a "!" for proc_time seconds
func proc() -> void:
	if _mode != Mode.COMBAT:
		return
	var already := _proc_left > 0.0
	_proc_left = proc_time
	_refresh()
	if not already:
		_emote.scale = Vector2.ZERO
		var tween := create_tween()
		tween.tween_property(_emote, "scale", Vector2.ONE, 0.15).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


func _process(delta: float) -> void:
	_time += delta
	_float.position.y = sin(_time * TAU / bob_period) * bob_height
	if _proc_left > 0.0:
		_proc_left -= delta
		if _proc_left <= 0.0:
			_refresh()


func _refresh() -> void:
	var look: Array
	match _mode:
		Mode.MERCHANT:
			look = LOOKS["merchant"]
		Mode.REST:
			look = LOOKS["rest"]
		_:
			look = LOOKS["proc"] if _proc_left > 0.0 else LOOKS["combat"]
	# At the merchant he looks away from Shion
	var flip := _mode == Mode.MERCHANT
	_body.texture = look[0]
	_body.flip_h = flip
	var emote: Texture2D = look[1]
	_emote.visible = emote != null
	if emote == null:
		return
	_emote.texture = emote
	var anchor: Vector2 = look[2]
	if flip:
		anchor.x = -anchor.x
	# Bottom center of the emote on the ponpon's spot. Only the position is mirrored, so the "?" still reads right.
	var size := emote.get_size()
	_emote.offset = Vector2(-size.x * 0.5, -size.y)
	_emote.position = anchor
