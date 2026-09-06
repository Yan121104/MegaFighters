extends Area2D
class_name Escalera


# =========================================================
# CONFIGURACIÓN
# =========================================================

@export var permitir_subir: bool = true
@export var permitir_bajar: bool = true


# =========================================================
# ESTADO
# =========================================================

var jugador: CharacterBody2D = null


# =========================================================
# READY
# =========================================================

func _ready() -> void:

	# Agregar esta escalera al grupo.
	add_to_group("escalera")

	# Detectar entrada y salida del jugador.
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)


# =========================================================
# JUGADOR ENTRA EN LA ESCALERA
# =========================================================

func _on_body_entered(body: Node2D) -> void:

	# Solo nos interesa CharacterBody2D.
	if not body is CharacterBody2D:
		return

	# Comprobar que sea nuestro Player.
	if body.has_method("entrar_en_escalera"):

		jugador = body

		body.entrar_en_escalera(self)


# =========================================================
# JUGADOR SALE DE LA ESCALERA
# =========================================================

func _on_body_exited(body: Node2D) -> void:

	if body != jugador:
		return

	if body.has_method("salir_de_escalera"):

		body.salir_de_escalera(self)

	jugador = null
