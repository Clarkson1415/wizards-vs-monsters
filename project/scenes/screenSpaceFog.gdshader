shader_type canvas_item;

// Define a 'varying' to pass the world position from vertex to fragment
varying vec2 world_pos;

uniform sampler2D noise_texture_large; // For the base layer
uniform sampler2D noise_texture_medium; // For the detail layer
uniform sampler2D noise_texture_small; // For the fine variance layer

// Uniforms for customization
uniform float fog_speed : hint_range(0.0, 1.0) = 0.1;
uniform float fog_density : hint_range(0.0, 5.0) = 1.0;
uniform float fog_scale : hint_range(0.0001, 0.1) = 0.005; // Base scale for largest features
uniform float fog_intensity : hint_range(0.0, 1.0) = 0.5; // Controls overall opacity
uniform vec4 fog_color : source_color = vec4(0.8, 0.8, 0.8, 0.8);

// Built-in for screen capture
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment()
{
    // --- Layer 1: Base Fog (Coarsest, Slowest) ---
    vec2 coord1 = world_pos * fog_scale * fog_density;
    vec2 uv1 = coord1 + TIME * fog_speed * 0.8;
    // Sample the unique large noise texture
    float noise1 = texture(noise_texture_large, fract(uv1)).r;

    // --- Layer 2: Detail Noise (Medium scale, Medium speed) ---
    // Scale is 2.5x the base layer's scale
    vec2 coord2 = world_pos * fog_scale * 2.5 * fog_density;
    vec2 uv2 = coord2 + TIME * fog_speed * 1.5 + vec2(100.0, 50.0);
    // Sample the unique medium noise texture
    float noise2 = texture(noise_texture_medium, fract(uv2)).r;

    // --- Layer 3: Tertiary Layer (Finest, Fastest, Different direction) ---
    // Scale is 4.0x the base layer's scale
    vec2 coord3 = world_pos * fog_scale * 4.0 * fog_density;
    vec2 uv3 = coord3 + TIME * fog_speed * 2.5 + vec2(-200.0, 150.0);
    // Sample the unique small noise texture
    float noise3 = texture(noise_texture_small, fract(uv3)).r;
    // Add all three layers together. You can assign weights here if you want:
    float weight1 = 0.8; // Coarse noise is less important here, let medium/small define the look
    float weight2 = 0.3;
    float weight3 = 0.8;

    // Add all weighted noise layers. The max possible value is sum of weights (2.3)
    float combined_noise = noise1 * weight1 + noise2 * weight2 + noise3 * weight3;

    // Normalize the result back to the 0.0 - 1.0 range
    // Max value is 2.3, so divide by 2.3 (sum of weights)
    combined_noise /= (weight1 + weight2 + weight3);

    // --- Final Pass ---
    // Increase contrast / thickness
    float final_noise_value = pow(combined_noise, 2.0 * (1.0 - fog_intensity));

    vec4 final_fog = fog_color;
    // Set the final alpha based on the combined noise value and intensity
    final_fog.a *= final_noise_value * fog_intensity;

    COLOR = mix(texture(SCREEN_TEXTURE, SCREEN_UV), final_fog, final_fog.a);
}