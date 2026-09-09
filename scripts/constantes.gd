class_name Constantes

enum Estado {
	# ========================================
	# JUGADOR
	# ========================================
	NORMAL,
	ROLL_DIVE,
	ATAQUE,
	ESCALANDO,
	# ========================================
	# ENEMIGO
	# ========================================
	PATRULLANDO,
	ESPERA_PATRULLA,
	PERSEGUIR,
	ANTICIPANDO_ATAQUE,
	ATACANDO,
	RECUPERACION,
	INTERCEPTANDO_PLATAFORMA,
	EN_PLATAFORMA,
	# ========================================
	# COMPARTIDO
	# ========================================
	MUERTO,
}
