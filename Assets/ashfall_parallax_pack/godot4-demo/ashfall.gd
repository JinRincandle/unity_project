extends ParallaxBackground

# ---------------------------------------------------------------------------
# Ashfall Parallax Pack — demo driver
#
# DESIGN MEMORY:
#   Node type is ParallaxBackground (a CanvasLayer). Its children are
#   ParallaxLayer nodes, ordered back-to-front in the scene tree:
#     01_sky / 02_mid / 03_front
#   Each holds exactly one Sprite2D whose texture is the seamless strip.
#
#   ONE scroll value drives everything. It is multiplied per-layer by that
#   layer's motion_scale, and THAT is what produces depth. Back layers get
#   a SMALLER motion_scale, so they crawl. Do not move layers individually.
#
#   DRIVE scroll_base_offset, NOT scroll_offset. When an active Camera2D
#   exists, ParallaxBackground recomputes scroll_offset from the camera
#   transform every frame and any hand-written value is discarded.
#   scroll_base_offset is additive on top of that and is never overwritten.
#
#   WRAP PERIOD — the subtle one. Each layer wraps its texture every
#   motion_mirroring.x pixels of ITS OWN travel, and its travel is
#   _scroll_x * motion_scale. So resetting _scroll_x is only invisible if
#   EVERY layer has moved a whole number of texture widths at that instant.
#   Wrapping at the texture width alone snaps the slow layers mid-strip and
#   reads as a hitch every few seconds. The period below is therefore
#   mirror_width / smallest_scale, and the scales must divide 1.0 evenly.
#   Defaults 0.25 / 0.5 / 1.0 satisfy this. If you change them, keep them
#   as clean fractions or the warning below will fire.
#
#   motion_mirroring is set from the texture width at runtime rather than
#   typed by hand, so swapping a strip for a different-width one never
#   desyncs the wrap. Y stays 0: these strips tile sideways only.
#
#   Sprites MUST be centered = false at position zero, or the wrap seam
#   lands mid-screen instead of off-frame.
#
#   Strips are 960x540. The project viewport must match that exactly or
#   the art cannot cover the frame. Scale up via window override, never by
#   scaling the sprites — non-integer sprite scale destroys the pixel grid.
# ---------------------------------------------------------------------------

## Back-to-front. Must match the child order under this node.
@export var layer_names: PackedStringArray = ["01_sky", "02_mid", "03_front"]

## Depth factor per layer, back-to-front. Multiplies the shared scroll.
## Must be clean fractions of 1.0 (0.25, 0.5, 1.0) — see WRAP PERIOD above.
@export var motion_scales: PackedFloat32Array = [0.25, 0.5, 1.0]

## Vertical placement per layer, back-to-front. 0 = flush to frame top.
@export var layer_offsets_y: PackedFloat32Array = [0.0, 0.0, 0.0]

## Front-plane drift in px/sec. Negative = world slides left, feels like
## the camera is travelling right.
@export var speed: float = -70.0

## Master multiplier. Set 0.0 to freeze the scene for screenshots.
@export_range(0.0, 4.0, 0.05) var time_scale: float = 1.0

var _layers: Array[ParallaxLayer] = []
var _scroll_x: float = 0.0
var _wrap_period: float = 0.0


func _ready() -> void:
	_collect_layers()
	_configure_layers()
	_compute_wrap_period()


func _process(delta: float) -> void:
	_scroll_x += speed * time_scale * delta

	# Wrap only at the shared period, where every layer sits on a seam.
	if _wrap_period > 0.0:
		_scroll_x = fmod(_scroll_x, _wrap_period)

	scroll_base_offset = Vector2(_scroll_x, 0.0)


func _collect_layers() -> void:
	_layers.clear()
	for layer_name in layer_names:
		var node := get_node_or_null(NodePath(layer_name))
		if node == null:
			push_warning("Parallax layer not found: %s" % layer_name)
			continue
		if node is ParallaxLayer:
			_layers.append(node)
		else:
			push_warning("Node %s is not a ParallaxLayer." % layer_name)


func _configure_layers() -> void:
	for i in _layers.size():
		var layer: ParallaxLayer = _layers[i]
		var sprite := _find_sprite(layer)

		if sprite != null and sprite.texture != null:
			# Seam control: sprite anchored top-left at the layer origin.
			sprite.centered = false
			sprite.position = Vector2.ZERO
			sprite.scale = Vector2.ONE

			var tex_size: Vector2 = sprite.texture.get_size()
			# X only. Y = 0 means "do not wrap vertically."
			layer.motion_mirroring = Vector2(tex_size.x, 0.0)
		else:
			push_warning("No Sprite2D with a texture under %s." % layer.name)

		layer.motion_scale = Vector2(_at(motion_scales, i, 1.0), 1.0)
		layer.motion_offset = Vector2(0.0, _at(layer_offsets_y, i, 0.0))


func _compute_wrap_period() -> void:
	# Period = widest strip / slowest layer. At that many pixels of base
	# scroll, the slowest layer has moved exactly one strip width and every
	# faster layer has moved a whole multiple of one — provided the scales
	# divide evenly. Verify that and warn loudly if they do not.
	var widest: float = 0.0
	var slowest: float = INF
	for layer in _layers:
		widest = maxf(widest, layer.motion_mirroring.x)
		if layer.motion_scale.x > 0.0:
			slowest = minf(slowest, layer.motion_scale.x)

	if widest <= 0.0 or is_inf(slowest):
		_wrap_period = 0.0
		return

	_wrap_period = widest / slowest

	for layer in _layers:
		var mirror: float = layer.motion_mirroring.x
		if mirror <= 0.0:
			continue
		var travel: float = _wrap_period * layer.motion_scale.x
		var remainder: float = fmod(travel, mirror)
		if not (is_zero_approx(remainder) or is_equal_approx(remainder, mirror)):
			push_warning(
				"Layer %s will visibly snap on wrap: motion_scale %f does not divide evenly. Use clean fractions."
				% [layer.name, layer.motion_scale.x]
			)


func _widest_mirror() -> float:
	var widest: float = 0.0
	for layer in _layers:
		widest = maxf(widest, layer.motion_mirroring.x)
	return widest


func _find_sprite(layer: Node) -> Sprite2D:
	for child in layer.get_children():
		if child is Sprite2D:
			return child
	return null


func _at(arr: PackedFloat32Array, idx: int, fallback: float) -> float:
	if idx < arr.size():
		return arr[idx]
	return fallback
