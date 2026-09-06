extends AnimatableBody2D

enum Direccion {
	HORIZONTAL,
	VERTICAL
}

@export_category("Movimiento")
@export var direccion: Direccion = Direccion.HORIZONTAL
@export var distancia: float = 200.0
@export var velocidad: float = 100.0

var posicion_inicial: Vector2
var posicion_objetivo: Vector2
var moviendo_hacia_objetivo := true
var velocidad_lineal_calculada: Vector2 = Vector2.ZERO


func _ready() -> void:
	add_to_group("plataforma_movil")
	sync_to_physics = true

	posicion_inicial = global_position

	if direccion == Direccion.HORIZONTAL:
		posicion_objetivo = posicion_inicial + Vector2(distancia, 0.0)
	else:
		posicion_objetivo = posicion_inicial + Vector2(0.0, distancia)


func _physics_process(delta: float) -> void:
	var objetivo := posicion_objetivo if moviendo_hacia_objetivo else posicion_inicial
	var anterior_pos := global_position

	global_position = global_position.move_toward(objetivo, velocidad * delta)

	if delta > 0.0:
		velocidad_lineal_calculada = (global_position - anterior_pos) / delta

	if global_position.distance_to(objetivo) < 0.5:
		global_position = objetivo
		moviendo_hacia_objetivo = not moviendo_hacia_objetivo
