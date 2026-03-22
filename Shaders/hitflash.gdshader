shader_type canvas_item;

uniform vec4 base_tint : source_color = vec4(1.0);
uniform float hit_flash : hint_range(0.0, 1.0) = 0.0;

void fragment()
{
    vec4 tex = texture(TEXTURE, UV);
    vec4 colored = tex * base_tint;

    vec3 final_rgb = mix(colored.rgb, vec3(1.0), hit_flash);

    COLOR = vec4(final_rgb, tex.a * colored.a);
}