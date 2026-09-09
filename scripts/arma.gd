extends Area2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	if body.is_in_group("Player"):
		# Lógica para sumar el arma al inventario o estadísticas del jugador
		# Ejemplo: body.equipar_arma()

		# Eliminar el objeto de la escena al ser recogido
		queue_free()
