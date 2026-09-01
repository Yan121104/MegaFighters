extends CharacterBody2D


# ==========================================
# VELOCIDADES
# ==========================================

const VELOCIDAD_TROTE := 250.0
const VELOCIDAD_CARRERA := 450.0
const VELOCIDAD_AGACHADO := 150.0


# ==========================================
# SALTO
# ==========================================

const FUERZA_SALTO := -480.0
const GRAVEDAD := 1200.0


# ==========================================
# ROLL DIVE
# ==========================================

const VELOCIDAD_ROLL_DIVE := 200.0
const FUERZA_ROLL_DIVE := -150.0


# ==========================================
# SPRITE
# ==========================================

@onready var sprite: AnimatedSprite2D = $AnimatedSprite2D


# ==========================================
# ESTADO DEL ROLL
# ==========================================

var haciendo_roll_dive := false
var animacion_roll_terminada := false
var direccion_roll := 1.0


# ==========================================
# ESTADO DEL ATAQUE
# ==========================================

var atacando := false

# Indica que X fue soltada
var ataque_soltado := false


func _ready() -> void:

	sprite.play("standing")

	sprite.animation_finished.connect(_on_animation_finished)


func _physics_process(delta: float) -> void:
	print("PLAYER PHYSICS: ", global_position)
	# ==========================================
	# GRAVEDAD
	# ==========================================

	if not is_on_floor():
		velocity.y += GRAVEDAD * delta


	# ==========================================
	# DIRECCIÓN
	# ==========================================

	var direccion := Input.get_axis("ui_left", "ui_right")


	# A / D
	if Input.is_key_pressed(KEY_A):
		direccion = -1

	elif Input.is_key_pressed(KEY_D):
		direccion = 1


	# ==========================================
	# ROLL DIVE
	# ==========================================

	if haciendo_roll_dive:

		velocity.x = move_toward(
			velocity.x,
			direccion_roll * VELOCIDAD_ROLL_DIVE,
			VELOCIDAD_ROLL_DIVE * 3.0 * delta
		)

		move_and_slide()


		if is_on_floor():

			if animacion_roll_terminada:
				finalizar_roll_dive()

		return


	# ==========================================
	# AGACHARSE
	# ==========================================

	var agachado := Input.is_key_pressed(KEY_DOWN)


	# ==========================================
	# ATAQUE
	# ==========================================

	if atacando:

		# --------------------------------------
		# 1. ACCIONES QUE INTERRUMPEN ATAQUE
		# --------------------------------------

		# SALTO
		if Input.is_action_just_pressed("ui_up") and is_on_floor():

			interrumpir_ataque()

			velocity.y = FUERZA_SALTO

			sprite.play("jump")

			move_and_slide()

			return


		# AGACHARSE
		if agachado:

			interrumpir_ataque()

			velocity.x = move_toward(
				velocity.x,
				direccion * VELOCIDAD_AGACHADO,
				VELOCIDAD_AGACHADO * 8.0 * delta
			)

			sprite.play("ducking")

			if direccion != 0:
				sprite.flip_h = direccion < 0

			move_and_slide()

			return


		# --------------------------------------
		# MOVIMIENTO
		# --------------------------------------

		if direccion != 0:

			interrumpir_ataque()

			if Input.is_key_pressed(KEY_Z):

				velocity.x = move_toward(
					velocity.x,
					direccion * VELOCIDAD_CARRERA,
					VELOCIDAD_CARRERA * 8.0 * delta
				)

				sprite.play("run")

			else:

				velocity.x = move_toward(
					velocity.x,
					direccion * VELOCIDAD_TROTE,
					VELOCIDAD_TROTE * 8.0 * delta
				)

				sprite.play("trot")


			sprite.flip_h = direccion < 0

			move_and_slide()

			return


		# --------------------------------------
		# SOLTAR X
		# --------------------------------------

		if not Input.is_key_pressed(KEY_X):

			# Si se soltó X, detener ataque
			interrumpir_ataque()

			sprite.play("standing")

			velocity.x = 0

			move_and_slide()

			return


		# --------------------------------------
		# X SIGUE PRESIONADA
		# --------------------------------------

		velocity.x = 0

		# Si la animación terminó pero X
		# sigue presionada, volver a reproducirla.
		if sprite.animation != "attack":

			sprite.play("attack")

		move_and_slide()

		return


	# ==========================================
	# INICIAR ROLL DIVE
	# ==========================================

	if (
		agachado
		and Input.is_action_just_pressed("ui_up")
		and is_on_floor()
	):

		iniciar_roll_dive(direccion)

		move_and_slide()

		return


	# ==========================================
	# INICIAR ATAQUE
	# ==========================================

	if Input.is_key_pressed(KEY_X) and is_on_floor():

		iniciar_ataque()

		move_and_slide()

		return


	# ==========================================
	# MOVIMIENTO
	# ==========================================

	if agachado:

		velocity.x = move_toward(
			velocity.x,
			direccion * VELOCIDAD_AGACHADO,
			VELOCIDAD_AGACHADO * 8.0 * delta
		)

	elif Input.is_key_pressed(KEY_Z):

		velocity.x = move_toward(
			velocity.x,
			direccion * VELOCIDAD_CARRERA,
			VELOCIDAD_CARRERA * 8.0 * delta
		)

	else:

		velocity.x = move_toward(
			velocity.x,
			direccion * VELOCIDAD_TROTE,
			VELOCIDAD_TROTE * 8.0 * delta
		)


	# ==========================================
	# SALTO NORMAL
	# ==========================================

	if (
		Input.is_action_just_pressed("ui_up")
		and is_on_floor()
		and not agachado
	):

		velocity.y = FUERZA_SALTO


	# ==========================================
	# ANIMACIONES
	# ==========================================

	if not is_on_floor():

		sprite.play("jump")

	elif agachado:

		sprite.play("ducking")

	elif direccion != 0:

		if Input.is_key_pressed(KEY_Z):
			sprite.play("run")
		else:
			sprite.play("trot")

	else:

		sprite.play("standing")


	# ==========================================
	# GIRAR
	# ==========================================

	if direccion != 0:
		sprite.flip_h = direccion < 0


	# ==========================================
	# MOVER
	# ==========================================

	move_and_slide()


# =========================================================
# INICIAR ATAQUE
# =========================================================

func iniciar_ataque() -> void:

	atacando = true
	ataque_soltado = false

	velocity.x = 0

	sprite.play("attack")


# =========================================================
# INTERRUMPIR ATAQUE
# =========================================================

func interrumpir_ataque() -> void:

	atacando = false
	ataque_soltado = false

	sprite.stop()


# =========================================================
# INICIAR ROLL DIVE
# =========================================================

func iniciar_roll_dive(direccion: float) -> void:

	haciendo_roll_dive = true
	animacion_roll_terminada = false


	if direccion != 0:

		direccion_roll = direccion

	else:

		if sprite.flip_h:
			direccion_roll = -1
		else:
			direccion_roll = 1


	velocity.x = direccion_roll * VELOCIDAD_ROLL_DIVE
	velocity.y = FUERZA_ROLL_DIVE

	sprite.flip_h = direccion_roll < 0

	sprite.play("roll_dive")


# =========================================================
# ANIMACIÓN TERMINADA
# =========================================================

func _on_animation_finished() -> void:

	# ------------------------------------------
	# ROLL DIVE
	# ------------------------------------------

	if sprite.animation == "roll_dive":

		animacion_roll_terminada = true

		sprite.stop()


	# ------------------------------------------
	# ATAQUE
	# ------------------------------------------

	elif sprite.animation == "attack":

		# --------------------------------------
		# X SIGUE PRESIONADA
		# --------------------------------------

		if Input.is_key_pressed(KEY_X):

			# Repetir ataque
			sprite.play("attack")

		# --------------------------------------
		# X FUE SOLTADA
		# --------------------------------------

		else:

			atacando = false

			velocity.x = 0

			sprite.play("standing")


# =========================================================
# FINALIZAR ROLL DIVE
# =========================================================

func finalizar_roll_dive() -> void:

	haciendo_roll_dive = false
	animacion_roll_terminada = false

	velocity.x = 0

	sprite.play("standing")
