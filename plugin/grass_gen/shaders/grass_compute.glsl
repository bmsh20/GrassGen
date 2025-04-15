#[compute]
#version 450

layout(local_size_x = 64, local_size_y = 1, local_size_z = 1) in;

// output buffer for transforms
layout(set = 0, binding = 0, std430) buffer TransformBuffer {
    mat4 transforms[];
};

// grass param unibuff
layout(set = 0, binding = 1, std140) uniform GrassParams {
    float area_size;
    float min_scale;
    float max_scale;
};

// hash for randomness because no hash function in glsl
uint hash(uint x) {
    x ^= x >> 16;
    x *= 0x7feb352d;
    x ^= x >> 15;
    x *= 0x846ca68b;
    x ^= x >> 16;
    return x;
}

float rand(uint id) {
    return float(hash(id)) / 4294967295.0;
}

// random magic numbers everywhere just so its passable
// looks ok to me
void main() {
    uint id = gl_GlobalInvocationID.x;
    
    // simple random position
    float x = (rand(id * 3) * 2.0 - 1.0) * area_size;
    float z = (rand(id * 3 + 1) * 2.0 - 1.0) * area_size;
    
    // simple random rotation and scale
    float angle = rand(id * 3 + 2) * 6;
    float scale = min_scale + rand(id * 3 + 7) * (max_scale - min_scale);
    
    // we build the transform here with (identity with position and simple y-rotation)
    float c = cos(angle) * scale;
    float s = sin(angle) * scale;
    
    // identity matrix with rotation and position
    transforms[id] = mat4(
        vec4(c, 0.0, -s, 0.0),
        vec4(0.0, scale, 0.0, 0.0),
        vec4(s, 0.0, c, 0.0),
        vec4(x, 0.0, z, 1.0)
    );
}