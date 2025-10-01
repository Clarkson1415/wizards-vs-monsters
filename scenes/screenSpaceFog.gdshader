shader_type canvas_item;

// Define a 'varying' to pass the world position from vertex to fragment
// This is the key to solving the 'GLOBAL_POSITION' error.
varying vec2 world_pos;

// Uniforms for customization
uniform float fog_speed : hint_range(0.0, 1.0) = 0.1;
uniform float fog_density : hint_range(0.0, 5.0) = 1.0;
uniform vec4 fog_color : source_color = vec4(0.8, 0.8, 0.8, 0.8);
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D noise_texture;
uniform float fog_scale : hint_range(0.001, 1000) = 0.001;

// --- Noise Function (You'll need a good one) ---
// Note: For simplicity, this example uses a placeholder hash function.
// For a proper look, replace this with a Simplex or FBM noise function.
vec2 random2(vec2 p) {
    p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3)));
    return fract(sin(p) * 43758.5453);
}

float noise(vec2 coord) {
    // Replace with a proper noise implementation
    return random2(floor(coord * 0.1)).x;
}
// ------------------------------------------------

void vertex() {
    // Calculate the fragment's world position and pass it via the varying
    // 'VERTEX' holds the local coordinate, 'MODEL_MATRIX' transforms it to global space.
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment()
{
	// The world_pos is already in global units, now we scale it
    vec2 scaled_coord = world_pos * fog_scale * fog_density;

    // Add time offset and sample the noise texture repeatedly (tiling)
    vec2 noise_uv = scaled_coord + TIME * fog_speed;

    float noise_value = texture(noise_texture, fract(noise_uv)).r;

    // Smooth out the noise effect (optional but recommended)
    // The power function makes the fog patches thicker
    noise_value = pow(noise_value, 2.0);

    vec4 final_fog = fog_color;
    // Set the final alpha based on the noise value
    final_fog.a *= noise_value;

    COLOR = mix(texture(SCREEN_TEXTURE, SCREEN_UV), final_fog, final_fog.a);
}