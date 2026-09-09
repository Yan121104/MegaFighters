class_name Reiniciador
extends RefCounted

var _cback: Callable

func _init(fn: Callable) -> void:
	_cback = fn

func aplicar() -> void:
	_cback.call()
