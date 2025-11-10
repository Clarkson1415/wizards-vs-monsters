@tool
extends GaeaNodeResource
class_name GaeaNodeQuadPlacer
## Places 4 different [param materials] on every 2x2 group of cells in [param reference_data].
## If *all 4 cells* in the group exist, each cell in the block gets its respective material:
## - top_left in the top-left cell
## - top_right in the top-right cell
## - bottom_left in the bottom-left cell
## - bottom_right in the bottom-right cell
##
## The output [param map] will contain material placements for all 4 cells.

func _get_title() -> String:
	return "QuadPlacer"


func _get_description() -> String:
	return ("Places 4 materials on every 2x2 block of cells in [param reference_data]. " +
			"If all 4 cells exist, each cell gets its assigned material.")


func _get_arguments_list() -> Array[StringName]:
	return [
		&"reference_data",
		&"material_top_left",
		&"material_top_right",
		&"material_bottom_left",
		&"material_bottom_right"
	]


func _get_argument_type(arg_name: StringName) -> GaeaValue.Type:
	match arg_name:
		&"reference_data": return GaeaValue.Type.DATA
		&"material_top_left": return GaeaValue.Type.MATERIAL
		&"material_top_right": return GaeaValue.Type.MATERIAL
		&"material_bottom_left": return GaeaValue.Type.MATERIAL
		&"material_bottom_right": return GaeaValue.Type.MATERIAL
	return GaeaValue.Type.NULL


func _get_output_ports_list() -> Array[StringName]:
	return [&"map"]


func _get_output_port_type(_output_name: StringName) -> GaeaValue.Type:
	return GaeaValue.Type.MAP


func _get_required_arguments() -> Array[StringName]:
	return [
		&"reference_data",
		&"material_top_left",
		&"material_top_right",
		&"material_bottom_left",
		&"material_bottom_right"
	]


func _get_data(_output_port: StringName, area: AABB, graph: GaeaGraph) -> Dictionary[Vector3i, GaeaMaterial]:
	var grid_data: Dictionary = _get_arg(&"reference_data", area, graph)
	var rng := define_rng(graph)
	var grid: Dictionary[Vector3i, GaeaMaterial]

	# Load materials
	var material_top_left: GaeaMaterial = _get_arg(&"material_top_left", area, graph).prepare_sample(rng)
	var material_top_right: GaeaMaterial = _get_arg(&"material_top_right", area, graph).prepare_sample(rng)
	var material_bottom_left: GaeaMaterial = _get_arg(&"material_bottom_left", area, graph).prepare_sample(rng)
	var material_bottom_right: GaeaMaterial = _get_arg(&"material_bottom_right", area, graph).prepare_sample(rng)

	# Validate
	if not is_instance_valid(material_top_left): return grid
	if not is_instance_valid(material_top_right): return grid
	if not is_instance_valid(material_bottom_left): return grid
	if not is_instance_valid(material_bottom_right): return grid

	# Loop through grid, only checking top-left corners (non-overlapping)
	for x in _get_axis_range(Vector3i.AXIS_X, area):
		for y in _get_axis_range(Vector3i.AXIS_Y, area):
			for z in _get_axis_range(Vector3i.AXIS_Z, area):
				if (x % 2 != 0) or (y % 2 != 0):
					continue

				var origin: Vector3i = Vector3i(x, y, z)

				# Offsets for quad
				var top_left := origin
				var top_right := origin + Vector3i(1, 0, 0)
				var bottom_left := origin + Vector3i(0, 1, 0)
				var bottom_right := origin + Vector3i(1, 1, 0)

				# Check all four cells exist
				var all_present := (
					grid_data.has(top_left) and
					grid_data.has(top_right) and
					grid_data.has(bottom_left) and
					grid_data.has(bottom_right)
				)

				if not all_present:
					continue

				# Assign materials to each of the four cells
				grid[top_left] = material_top_left.execute_sample(rng, grid_data.get(top_left, 0.0))
				grid[top_right] = material_top_right.execute_sample(rng, grid_data.get(top_right, 0.0))
				grid[bottom_left] = material_bottom_left.execute_sample(rng, grid_data.get(bottom_left, 0.0))
				grid[bottom_right] = material_bottom_right.execute_sample(rng, grid_data.get(bottom_right, 0.0))

	return grid
