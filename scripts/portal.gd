extends Area2D

@export var otro_portal: Area2D


func _on_body_entered(body: Node2D) -> void:
	# Permitir paso de Player o Enemy
	var es_viajero := body.is_in_group("player") or body.is_in_group("Player") or body.is_in_group("enemy")
	if not es_viajero:
		return

	if otro_portal == null:
		push_warning(name + ": No se ha asignado otro_portal.")
		return

	# Evitar bucle infinito si acaba de salir en este portal
	if body.has_meta("portal_actual"):
		if body.get_meta("portal_actual") == self:
			return

	var spawn_point := otro_portal.get_node_or_null("SpawnPoint") as Node2D
	if spawn_point == null:
		push_warning(otro_portal.name + ": No existe SpawnPoint.")
		return

	var nueva_velocidad := Vector2.ZERO
	if body is CharacterBody2D:
		nueva_velocidad = -body.velocity

	# Marcar portal de destino
	body.set_meta("portal_actual", otro_portal)

	# Delegar teletransporte e inversión de vista tanto al Player como al Enemy
	if body.has_method("aplicar_efecto_portal"):
		body.aplicar_efecto_portal(spawn_point.global_position, nueva_velocidad)
	else:
		body.global_position = spawn_point.global_position
		if body is CharacterBody2D:
			body.velocity = nueva_velocidad


func _on_body_exited(body: Node2D) -> void:
	var es_viajero := body.is_in_group("player") or body.is_in_group("Player") or body.is_in_group("enemy")
	if not es_viajero:
		return

	if body.has_meta("portal_actual"):
		if body.get_meta("portal_actual") == self:
			body.remove_meta("portal_actual")
