varying vec2 v_uv;

uniform sampler2D u_texture;
uniform vec2 u_resolution;
uniform float u_time;

void main() {
    vec2 st = v_uv;
    st.x += sin(st.y * 10.0 + u_time) * .05;
    st.y += sin(st.x * 10.0 + u_time) * .05;
    gl_FragColor = texture2D(u_texture, st);
}