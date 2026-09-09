extends Node2D

# Lista de escenas de armas disponibles para aparecer aleatoriamente
@export var armas_escenas: Array[PackedScene] = [
	preload("res://scenes/armas/basuca.tscn"),
	preload("res://scenes/armas/desert.tscn"),
	preload("res://scenes/armas/escopeta.tscn"),
	preload("res://scenes/armas/hacha.tscn"),
	preload("res://scenes/armas/m4a1.tscn"),
	preload("res://scenes/armas/machete.tscn"),
	preload("res://scenes/armas/miniusi.tscn"),
	preload("res://scenes/armas/sable.tscn"),
	preload("res://scenes/armas/snyper.tscn"),
	preload("res://scenes/armas/usp.tscn")
	
]

# Tiempo en segundos antes de que desaparezcan las armas
@export var tiempo_respawn: float = 15.0

# Cantidad de armas que deben aparecer a la vez
@export var cantidad_activa: int = 3

var puntos_spawn: Array = []
var armas_actuales: Array = []
var temporizador: Timer

func _ready() -> void:
	# Recolectar todos los Marker2D hijos como puntos de spawn válidos
	for hijo in get_children():
		if hijo is Marker2D:
			puntos_spawn.append(hijo)
	
	if puntos_spawn.size() < cantidad_activa:
		push_warning("No hay suficientes Marker2D para la cantidad de armas requerida.")
	
	# Configurar el temporizador para el ciclo de aparición
	temporizador = Timer.new()
	temporizador.wait_time = tiempo_respawn
	temporizador.one_shot = false
	temporizador.autostart = true
	temporizador.timeout.connect(_on_timer_timeout)
	add_child(temporizador)
	
	# Generar el primer lote de armas inmediatamente
	_spawnear_armas()

func _spawnear_armas() -> void:
	# Limpiar armas anteriores si quedan en escena
	_limpiar_armas()
	
	if puntos_spawn.is_empty() or armas_escenas.is_empty():
		return
	
	# Mezclar los puntos de spawn aleatoriamente
	var puntos_mezclados = puntos_spawn.duplicate()
	puntos_mezclados.shuffle()
	
	# Tomar únicamente el número definido (ej. 3)
	var seleccion_puntos = puntos_mezclados.slice(0, min(cantidad_activa, puntos_mezclados.size()))
	
	for punto in seleccion_puntos:
		# Escoger un arma aleatoria de la lista
		var arma_aleatoria_escena = armas_escenas.pick_random() as PackedScene
		if arma_aleatoria_escena:
			var instancia_arma = arma_aleatoria_escena.instantiate()
			
			# Colocarla en la posición exacta del Marker2D
			instancia_arma.global_position = punto.global_position
			
			# Añadirla a la escena principal (o como hija del nivel)
			get_tree().current_scene.add_child.call_deferred(instancia_arma)
			armas_actuales.append(instancia_arma)

func _limpiar_armas() -> void:
	for arma in armas_actuales:
		if is_instance_valid(arma):
			arma.queue_free()
	armas_actuales.clear()

func _on_timer_timeout() -> void:
	_spawnear_armas()
