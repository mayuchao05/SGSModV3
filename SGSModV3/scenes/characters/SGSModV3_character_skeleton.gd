extends Node2D

# 方案C：真·2D 骨骼动画（cutout）
# 用 Bone2D 父子链驱动拆件，Tweens 驱动待机动画 / 攻击 / 受击。
# 骨骼结构：Skeleton2D/Hip/Torso/{Head, ArmL, ArmR} + Hip/{LegL, LegR}

@onready var _hip: Bone2D = $Skeleton2D/Hip
@onready var _torso: Bone2D = $Skeleton2D/Hip/Torso
@onready var _head: Bone2D = $Skeleton2D/Hip/Torso/Head
@onready var _arm_l: Bone2D = $Skeleton2D/Hip/Torso/ArmL
@onready var _arm_r: Bone2D = $Skeleton2D/Hip/Torso/ArmR
@onready var _leg_l: Bone2D = $Skeleton2D/Hip/LegL
@onready var _leg_r: Bone2D = $Skeleton2D/Hip/LegR

var _idle_tween: Tween
var _action_tween: Tween
var _state: String = "idle"


func _ready() -> void:
	if _hip == null:
		push_error("SGSModV3CharacterSkeleton: 找不到 Skeleton2D/Hip，场景结构是否被改动？")
		return
	# 确保初始姿势归零
	_reset_pose()
	start_idle()


func _reset_pose() -> void:
	for b in [_hip, _torso, _head, _arm_l, _arm_r, _leg_l, _leg_r]:
		if b == null:
			continue
		b.rotation = 0.0
		b.position = b.position  # 保留位置，不重置


func start_idle() -> void:
	_state = "idle"
	_kill_action_tween()
	if _idle_tween != null and _idle_tween.is_valid():
		_idle_tween.kill()

	_idle_tween = create_tween()
	_idle_tween.set_loops()
	_idle_tween.set_trans(Tween.TRANS_SINE)
	_idle_tween.set_ease(Tween.EASE_IN_OUT)

	# 吸气/上浮 1 秒
	_idle_tween.tween_property(_torso, "rotation", 0.035, 1.0)
	_idle_tween.parallel().tween_property(_head, "rotation", 0.02, 1.0)
	_idle_tween.parallel().tween_property(_arm_l, "rotation", 0.04, 1.0)
	_idle_tween.parallel().tween_property(_arm_r, "rotation", -0.04, 1.0)
	_idle_tween.parallel().tween_property(_hip, "position:y", _hip.position.y - 2.0, 1.0)

	# 呼气/下沉 1 秒
	_idle_tween.tween_property(_torso, "rotation", 0.0, 1.0)
	_idle_tween.parallel().tween_property(_head, "rotation", 0.0, 1.0)
	_idle_tween.parallel().tween_property(_arm_l, "rotation", 0.0, 1.0)
	_idle_tween.parallel().tween_property(_arm_r, "rotation", 0.0, 1.0)
	_idle_tween.parallel().tween_property(_hip, "position:y", _hip.position.y, 1.0)


func play_attack() -> void:
	if _state == "attack":
		return
	_state = "attack"
	_kill_idle_tween()
	_kill_action_tween()

	_action_tween = create_tween()
	_action_tween.set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	# 前冲 + 挥剑（右臂下摆到前挥）
	_action_tween.tween_property(_hip, "rotation", -0.12, 0.10)
	_action_tween.parallel().tween_property(_torso, "rotation", -0.10, 0.10)
	_action_tween.parallel().tween_property(_arm_r, "rotation", 0.85, 0.12)
	_action_tween.parallel().tween_property(_arm_l, "rotation", 0.12, 0.10)

	# 回位
	_action_tween.tween_property(_hip, "rotation", 0.0, 0.35).set_ease(Tween.EASE_IN_OUT)
	_action_tween.parallel().tween_property(_torso, "rotation", 0.0, 0.35)
	_action_tween.parallel().tween_property(_arm_r, "rotation", 0.0, 0.35)
	_action_tween.parallel().tween_property(_arm_l, "rotation", 0.0, 0.35)
	_action_tween.tween_callback(start_idle)


func play_hit() -> void:
	if _state == "hit":
		return
	_state = "hit"
	_kill_idle_tween()
	_kill_action_tween()

	_action_tween = create_tween()
	_action_tween.set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	# 后仰受击
	_action_tween.tween_property(_hip, "rotation", 0.10, 0.08)
	_action_tween.parallel().tween_property(_torso, "rotation", 0.12, 0.08)
	_action_tween.parallel().tween_property(_head, "rotation", 0.08, 0.08)
	_action_tween.parallel().tween_property(_arm_r, "rotation", -0.15, 0.08)

	# 回位
	_action_tween.tween_property(_hip, "rotation", 0.0, 0.30).set_ease(Tween.EASE_IN_OUT)
	_action_tween.parallel().tween_property(_torso, "rotation", 0.0, 0.30)
	_action_tween.parallel().tween_property(_head, "rotation", 0.0, 0.30)
	_action_tween.parallel().tween_property(_arm_r, "rotation", 0.0, 0.30)
	_action_tween.tween_callback(start_idle)


func _kill_idle_tween() -> void:
	if _idle_tween != null and _idle_tween.is_valid():
		_idle_tween.kill()
	_idle_tween = null


func _kill_action_tween() -> void:
	if _action_tween != null and _action_tween.is_valid():
		_action_tween.kill()
	_action_tween = null
