shader_type canvas_item;

// for the white flash
uniform float flash_strength : hint_range(0.0, 1.0) = 0.0;
uniform vec4 color : source_color;

// OUTLINE
uniform vec4 outline_color : source_color = vec4(1.0);
uniform bool outline_on = false;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment()
{
	// for the white flash
	vec4 tex_color = texture(TEXTURE, UV);
    // mix sprite color with white, controlled by flash_strength
    COLOR = mix(tex_color, vec4(color.r, color.g, color.b, tex_color.a), flash_strength);

	// White flash and OUTLINE happens at same time. outline should be rendered on top.
	if (outline_on){
		vec2 pixel_size = TEXTURE_PIXEL_SIZE;
		bool within = texture(TEXTURE, UV + pixel_size * vec2(1.0, 0.0)).a > 0.0;
		within = within || texture(TEXTURE, UV + pixel_size * vec2(1.0, 1.0)).a > 0.0;
		within = within || texture(TEXTURE, UV + pixel_size * vec2(1.0, -1.0)).a > 0.0;
		within = within || texture(TEXTURE, UV + pixel_size * vec2(0.0, -1.0)).a > 0.0;
		within = within || texture(TEXTURE, UV + pixel_size * vec2(0.0, 1.0)).a > 0.0;
		within = within || texture(TEXTURE, UV + pixel_size * vec2(-1.0, 1.0)).a > 0.0;
		within = within || texture(TEXTURE, UV + pixel_size * vec2(-1.0, -1.0)).a > 0.0;
		within = within || texture(TEXTURE, UV + pixel_size * vec2(-1.0, 0.0)).a > 0.0;
		bool outline = within && texture(TEXTURE, UV).a == 0.0;
		COLOR = float(!outline) * COLOR + float(outline) * outline_color;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
