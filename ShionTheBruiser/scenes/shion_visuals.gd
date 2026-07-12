extends AnimatedSprite2D

func _ready() -> void:
	animation_finished.connect(_on_animation_finished)

func _on_animation_finished() -> void:
	if animation != &"idle" and animation != &"merchant_idle" and animation != &"die":
		play(&"idle")

