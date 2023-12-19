varying vec2 v_uv;
uniform vec2 u_canvasResolution;
uniform float u_pixelRatio;
uniform float u_time;

vec3 random3(vec3 c) {
    float j = 4096.0*sin(dot(c,vec3(17.0, 59.4, 15.0)));
    vec3 r;
    r.z = fract(512.0*j);
    j *= .125;
    r.x = fract(512.0*j);
    j *= .125;
    r.y = fract(512.0*j);
    return r-0.5;
}

float random2(in vec2 st) {
    return fract(sin(dot(st.xy,vec2(12.9898,78.233))) * 43758.5453123);
}


/**
    Simplex noise function made by smarter people than me.
    https://thebookofshaders.com/edit.php#11/2d-snoise-clear.frag
*/
const float F3 =  0.3333333;
const float G3 =  0.1666667;

float simplex3d(vec3 p) {
    vec3 s = floor(p + dot(p, vec3(F3)));
    vec3 x = p - s + dot(s, vec3(G3));
    
    vec3 e = step(vec3(0.0), x - x.yzx);
    vec3 i1 = e*(1.0 - e.zxy);
    vec3 i2 = 1.0 - e.zxy*(1.0 - e);
        
    vec3 x1 = x - i1 + G3;
    vec3 x2 = x - i2 + 2.0*G3;
    vec3 x3 = x - 1.0 + 3.0*G3;
    
    vec4 w, d;
    
    w.x = dot(x, x);
    w.y = dot(x1, x1);
    w.z = dot(x2, x2);
    w.w = dot(x3, x3);
    
    w = max(0.6 - w, 0.0);
    
    d.x = dot(random3(s), x);
    d.y = dot(random3(s + i1), x1);
    d.z = dot(random3(s + i2), x2);
    d.w = dot(random3(s + 1.0), x3);
    
    w *= w;
    w *= w;
    d *= w;
    
    return dot(d, vec4(52.0));
}

void main() {
    vec2 coords = (gl_FragCoord.xy / u_pixelRatio) * 0.03;

    float noise = 
        simplex3d(vec3(coords * .03, u_time * 0.1 * 1.0)) * 1.0 +
        simplex3d(vec3(coords * .1, u_time * 0.05 * 2.0)) * .2 +
        simplex3d(vec3(coords * .3, u_time * 0.1 * 3.0)) * .15 +
        simplex3d(vec3(coords * .6, u_time * 0.1 * 4.0)) * .075;


    float dx = abs(v_uv.x - .5) * 2.0;
    float dy = abs(v_uv.y - .5) * 2.0;
    float d = sqrt(dx * dx + dy * dy);

    float cutoff = round((noise + .1) * 50.0 * (1.0 - min(d * d * d * d * d * d * d * d, 1.0)));
    if (cutoff > 0.5) { gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0); }
    else if (d < 1.0) { gl_FragColor = vec4(1.0, 1.0, 1.0, 1.0); }
}