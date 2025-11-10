shader_type canvas_item; // LINE 1: Must be first for file recognition

render_mode blend_mix;

// --- Uniforms passed by the C# Script ---
uniform vec2 FRAME_UV_START = vec2(0.0, 0.0);
uniform vec2 FRAME_UV_SIZE = vec2(1.0, 1.0);
uniform vec2 TEXEL_SIZE = vec2(0.0, 0.0);
uniform sampler2D sprite_Texture : source_color;
// ----------------------------------------

uniform vec2 uv_Add = vec2(0.0, 0.0);
uniform float uv_Mul = 1.2;
uniform vec4 modulate : source_color; // NOTE: This is likely for a separate sprite effect, not the shadow color
uniform float x_buffer = 2.0;
uniform float y_buffer = 1.0;
uniform float time = 11.0;
uniform float N_dot = 32.0;
uniform vec4 SHADOW_COLOR : source_color = vec4(0.0, 0.0, 0.0, 0.5); // Black with 50% opacity

void vertex() {
	VERTEX.x *= x_buffer;
	VERTEX.y *= y_buffer;
}

void fragment() {
	// Declaring and calculating formerly 'varying' variables locally
	vec2 input;
	float intensity;

	input.x = 3.0 * cos((time - 18.0) * 3.14159265359 / 12.0); // Use M_PI for better precision
	input.y = 2.0 * (sin((time - 18.0) * 3.14159265359 / 12.0) + 1.0);
	intensity = clamp(cos((time - 12.0) * 3.14159265359 / 12.0), 0.0, 1.0);

	// --- 1. Calculate the base Sheet UV ---
	// Maps the local UV (0-1 inside ColorRect) to the frame's actual coordinates on the sheet.
	vec2 sheet_uv_Origin = FRAME_UV_START + UV * FRAME_UV_SIZE;

	// Use the raw local UV (0-1) for distortion math
	vec2 uv_Local = UV;

	// Distortion Setup (Normalization logic from original shader)
	vec2 uv_Distortion_Base = uv_Local * 2.0 - 1.0;
	uv_Distortion_Base.x *= x_buffer;
	uv_Distortion_Base.y *= y_buffer;
	uv_Distortion_Base = (uv_Distortion_Base + 1.0) / 2.0;

	// Translation/Distortion
	vec2 uv_Trans_Local = uv_Distortion_Base;
	uv_Trans_Local.x += 0.15 * input.x;
	uv_Trans_Local.y -= 0.1 * input.y;

	// --- 2. Sampling for Distortion Value ---
	vec2 sheet_uv_Trans = FRAME_UV_START + uv_Trans_Local * FRAME_UV_SIZE;

	// Clamp the UV to prevent sampling outside the frame's region
	sheet_uv_Trans = clamp(sheet_uv_Trans, FRAME_UV_START, FRAME_UV_START + FRAME_UV_SIZE - TEXEL_SIZE);

	float val = texture(sprite_Texture, sheet_uv_Trans * uv_Mul + uv_Add).r;

	// Final Distorted Local UV (used for shadow position)
	vec2 uv_Final_Local = uv_Distortion_Base * val + uv_Trans_Local * (1.0 - val);

	// --- 3. Final Shadow Sample UV ---
	vec2 sheet_uv_Shadow = FRAME_UV_START + uv_Final_Local * FRAME_UV_SIZE;

	// Clamp the shadow UV
	sheet_uv_Shadow = clamp(sheet_uv_Shadow, FRAME_UV_START, FRAME_UV_START + FRAME_UV_SIZE - TEXEL_SIZE);

	// Pixelization/Dithering Step (using N_dot)
	vec2 pixel_size_on_sheet = TEXEL_SIZE * (1.0 / N_dot);
	sheet_uv_Shadow.x = floor(sheet_uv_Shadow.x / pixel_size_on_sheet.x) * pixel_size_on_sheet.x;
	sheet_uv_Shadow.y = floor(sheet_uv_Shadow.y / pixel_size_on_sheet.y) * pixel_size_on_sheet.y;

	// --- 4. Final Color ---

	// Sample the alpha from the sprite's texture using the distorted UV
	float shadow_alpha = texture(sprite_Texture, sheet_uv_Shadow).a;

	// Create the shadow color: Use the SHADOW_COLOR's RGB and its alpha multiplied by the sampled texture alpha and intensity/val.
	vec4 shadow = vec4(
        SHADOW_COLOR.rgb,
        shadow_alpha * SHADOW_COLOR.a * intensity * val );

	// Get the color and alpha for the original sprite using the base (undistorted) UV
	// Clamp the base UV one last time for safety
	sheet_uv_Origin = clamp(sheet_uv_Origin, FRAME_UV_START, FRAME_UV_START + FRAME_UV_SIZE - TEXEL_SIZE);
	vec4 col = texture(sprite_Texture, sheet_uv_Origin);

	// Mix the shadow and color (only display shadow where the sprite is opaque)
	// NOTE: For a typical shadow, you usually don't mix it with the original sprite's color ('col').
	// Since this script is attached to a ColorRect (presumably the shadow itself), the final COLOR should just be the 'shadow' variable.
	// If this shader is on the shadow object, the last line should be:
	COLOR = shadow;

	// If you intend to draw the shadow AND the sprite on the same object, the mix is kept (but this is generally wrong for a shadow):
	// COLOR = mix(shadow, col, col.a);
}