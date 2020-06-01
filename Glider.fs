#ifdef GL_FRAGMENT_PRECISION_HIGH
precision highp float;
#else
precision mediump float;
#endif

uniform vec2 resolution;
uniform float time;

const float period = 4.0;
const float cell_glow_sharpness = 12.0;
const float cell_glow_size = 8.0;
const float grid_scale = 4.0;
const float grid_thickness = 0.015;
const float cell_gap = 0.133;
const vec3 grid_color = vec3(0.0, 0.659, 0.729);
const vec3 cell_color = vec3(0.0, 0.627, 0.918);
const vec3 glow_color = vec3(0.0, 0.2, 1.0);

bool close_to_grid(vec2 tiled_coordinates, float distance_to_grid)
{
	bool is_on_vertical =
		tiled_coordinates.x > 1.0 - distance_to_grid ||
		tiled_coordinates.x < distance_to_grid;
	bool is_on_horizontal =
		tiled_coordinates.y > 1.0 - distance_to_grid ||
		tiled_coordinates.y < distance_to_grid;
	return is_on_vertical || is_on_horizontal;
}

bool is_in_cell(vec2 coordinates)
{
	return
		coordinates.x > cell_gap &&
		coordinates.x < 1.0 - cell_gap &&
		coordinates.y > cell_gap &&
		coordinates.y < 1.0 - cell_gap;
}

float get_distance_to_cell(vec2 coordinates, ivec2 cell_coordinates)
{
	vec2 cell_center = vec2(cell_coordinates) + 0.5;
	return distance(coordinates, cell_center);
}

float get_glow(float attenuation, float sharpness)
{
	return pow((1.0 - abs(attenuation)), sharpness);
}

float get_minimum(float a, float b, float c, float d, float e)
{
	if(a <= b && a <= c && a <= d && a <= e)
	{
		return a;
	}
	else if(b <= a && b <= c && b <= d && b <= e)
	{
		return b;
	}
	else if(c <= a && c <= b && c <= d && c <= e)
	{
		return c;
	}
	else if(d <= a && d <= b && d <= c && d <= e)
	{
		return d;
	}
	else
	{
		return e;
	}
}

void main(void)
{
	float loop_time = mod(time * 2.0, period);
	vec2 offset = vec2(loop_time / period);
	vec2 coordinates = grid_scale * gl_FragCoord.xy / min(resolution.x, resolution.y);
	coordinates -= loop_time / period;
	coordinates += 0.75;

	int stage = int(loop_time);
	ivec2 v03 = ivec2(0, 3);
	ivec2 v12 = ivec2(1, 2);
	ivec2 v13 = ivec2(1, 3);
	ivec2 v14 = ivec2(1, 4);
	ivec2 v22 = ivec2(2, 2);
	ivec2 v23 = ivec2(2, 3);
	ivec2 v24 = ivec2(2, 4);
	ivec2 v25 = ivec2(2, 5);
	ivec2 v33 = ivec2(3, 3);
	ivec2 v34 = ivec2(3, 4);
	float d12 = get_distance_to_cell(coordinates, v12);
	float d13 = get_distance_to_cell(coordinates, v13);
	float d14 = get_distance_to_cell(coordinates, v14);
	float d22 = get_distance_to_cell(coordinates, v22);
	float d23 = get_distance_to_cell(coordinates, v23);
	float d33 = get_distance_to_cell(coordinates, v33);
	float smallest_distance = 0.0;
	if(stage == 0)
	{
		float d25 = get_distance_to_cell(coordinates, v25);
		smallest_distance = get_minimum(d13, d14, d23, d25, d33);
	}
	else if(stage == 1)
	{
		float d34 = get_distance_to_cell(coordinates, v34);
		smallest_distance = get_minimum(d13, d14, d22, d23, d34);
	}
	else if(stage == 2)
	{
		smallest_distance = get_minimum(d13, d22, d33, d14, d12);
	}
	else if(stage == 3)
	{
		float d03 = get_distance_to_cell(coordinates, v03);
		float d24 = get_distance_to_cell(coordinates, v24);
		smallest_distance = get_minimum(d03, d12, d13, d22, d24);
	}
	if(smallest_distance < 2.0)
	{
		float glow = get_glow(smallest_distance / cell_glow_size, cell_glow_sharpness);
		gl_FragColor = vec4(glow_color * glow, 0.0);
	}

	vec2 tiled_coordinates = mod(coordinates, 1.0);
	if(close_to_grid(tiled_coordinates, grid_thickness))
	{
		gl_FragColor = vec4(grid_color, 1.0);
	}

	ivec2 cell_coordinates = ivec2(floor(coordinates));
	if(is_in_cell(tiled_coordinates))
	{
		bool b12 = cell_coordinates == v12;
		bool b13 = cell_coordinates == v13;
		bool b14 = cell_coordinates == v14;
		bool b22 = cell_coordinates == v22;
		bool b23 = cell_coordinates == v23;
		bool b33 = cell_coordinates == v33;
		bool on = false;

		if(stage == 0)
		{
			bool b25 = cell_coordinates == v25;
			on = b13 || b14 || b23 || b25 || b33;
		}
		else if (stage == 1)
		{
			bool b34 = cell_coordinates == v34;
			on = b13 || b14 || b22 || b23 || b34;
		}
		else if (stage == 2)
		{
			on = b13 || b22 || b33 || b14 || b12;
		}
		else if (stage == 3)
		{
			bool b03 = cell_coordinates == v03;
			bool b24 = cell_coordinates == v24;
			on = b03 || b12 || b13 || b22 || b24;
		}

		if(on)
		{
			gl_FragColor = vec4(cell_color, 1.0);
		}
	}
}
