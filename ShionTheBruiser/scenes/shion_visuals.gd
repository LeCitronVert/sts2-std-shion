extends AnimatedSprite2D

func _ready() -> void:
	animation_finished.connect(_on_animation_finished)

func _on_animation_finished() -> void:
	if animation != &"idle" and animation != &"merchant_idle" and animation != &"die":
		# Multi-hit attacks speed up the animation so each hit gets a full loop (see AnimationRestartPatch.cs)
		speed_scale = 1.0
		play(&"idle")
