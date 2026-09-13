extends Sprite2D

# Route-B 伪动画：呼吸式待机（上下浮动 + 轻微缩放 + 极小幅摇摆）
# 暂不做攻击/受击 Spine 动画；后续若需，可在此脚本接收信号后暂停待机并播放一次性动作。

var _idle_tween: Tween

func _ready() -> void:
	_start_idle_breath()

func _start_idle_breath() -> void:
	if _idle_tween != null and _idle_tween.is_valid():
		_idle_tween.kill()

	var base_scale := scale
	var base_pos := position

	_idle_tween = create_tween()
	_idle_tween.set_loops()
	_idle_tween.set_trans(Tween.TRANS_SINE)
	_idle_tween.set_ease(Tween.EASE_IN_OUT)

	# 吸气/上浮：2 秒
	_idle_tween.tween_property(self, "scale", base_scale + Vector2(0.0, 0.04), 1.0)
	_idle_tween.parallel().tween_property(self, "position", base_pos + Vector2(0, 3), 1.0)
	_idle_tween.parallel().tween_property(self, "rotation", 0.02, 1.0)

	# 呼气/下沉：2 秒
	_idle_tween.tween_property(self, "scale", base_scale, 1.0)
	_idle_tween.parallel().tween_property(self, "position", base_pos, 1.0)
	_idle_tween.parallel().tween_property(self, "rotation", -0.02, 1.0)

func play_attack_reaction() -> void:
	# 占位：攻击时向前冲刺并回位；后续可扩展。
	pass

func play_hit_reaction() -> void:
	# 占位：受击时闪红/后退；后续可扩展。
	pass
