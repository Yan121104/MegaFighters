extends Area2D


# ==========================================
# PORTAL DESTINO
# ==========================================

@export var otro_portal: Area2D


# ==========================================
# ENTRADA AL PORTAL
# ==========================================

func _on_body_entered(body: Node2D) -> void:

	# ------------------------------------------
	# Comprobar que sea el jugador
	# ------------------------------------------

	if not body.is_in_group("Player"):
		return


	# ------------------------------------------
	# Comprobar que exista el portal destino
	# ------------------------------------------

	if otro_portal == null:
		push_warning(
			name + ": No se ha asignado otro_portal."
		)
		return


	# ------------------------------------------
	# EVITAR LOOP INSTANTÁNEO
	# ------------------------------------------
	#
	# Si el jugador acaba de aparecer en este
	# portal, no volverá inmediatamente al portal
	# anterior.
	#
	# Esto NO es un cooldown.
	# El jugador podrá volver a usar este portal
	# después de salir de él.
	# ------------------------------------------

	if body.has_meta("portal_actual"):

		if body.get_meta("portal_actual") == self:
			return


	# ==========================================
	# OBTENER SPAWN DEL PORTAL DESTINO
	# ==========================================

	var spawn_point := otro_portal.get_node_or_null("SpawnPoint") as Node2D

	if spawn_point == null:
		push_warning(
			otro_portal.name + ": No existe SpawnPoint."
		)
		return


	# ==========================================
	# GUARDAR VELOCIDAD ACTUAL
	# ==========================================

	var velocidad := Vector2.ZERO

	if body is CharacterBody2D:
		velocidad = body.velocity


	# ==========================================
	# TELETRANSPORTAR
	# ==========================================

	body.global_position = spawn_point.global_position


	# ==========================================
	# INVERTIR DIRECCIÓN
	# ==========================================
	#
	# Si entra →
	# sale ←
	#
	# Si entra ↑
	# sale ↓
	#
	# La magnitud de la velocidad se conserva.
	# ==========================================

	if body is CharacterBody2D:

		body.velocity = -velocidad


	# ==========================================
	# INVERTIR ORIENTACIÓN DEL SPRITE
	# ==========================================

	var sprite := body.get_node_or_null(
		"AnimatedSprite2D"
	) as AnimatedSprite2D

	if sprite != null:

		sprite.flip_h = not sprite.flip_h


	# ==========================================
	# MARCAR PORTAL ACTUAL
	# ==========================================
	#
	# El jugador acaba de aparecer en otro_portal.
	# Por lo tanto, ese portal no debe devolverlo
	# inmediatamente.
	# ==========================================

	body.set_meta(
		"portal_actual",
		otro_portal
	)


# ==========================================
# SALIDA DEL PORTAL
# ==========================================

func _on_body_exited(body: Node2D) -> void:

	if not body.is_in_group("Player"):
		return


	# ==========================================
	# LIBERAR EL PORTAL
	# ==========================================
	#
	# Una vez que el jugador salió del portal,
	# puede volver a entrar normalmente.
	# ==========================================

	if body.has_meta("portal_actual"):

		if body.get_meta("portal_actual") == self:

			body.remove_meta("portal_actual")
