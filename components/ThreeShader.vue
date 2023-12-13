<template>
	<div
		class="c-three-shader"
		:style="{
			'--width': widthActual && `${widthActual}px`,
			'--height': heightActual && `${heightActual}px`,
		}"
	>
		<div
			v-if="$slots.fallback && !isLoaded"
			class="c-three-shader__fallback"
		>
			<slot name="fallback"></slot>
		</div>

		<canvas
			v-show="isLoaded"
			:style="{ opacity: +isLoaded }"
			class="w-full h-full"
			ref="canvas"
		>
			{{ $attrs.alt ?? '' }}
		</canvas>
	</div>
</template>

<script>
import * as THREE from 'three';
import { EffectComposer } from 'three/addons/postprocessing/EffectComposer.js';
import { RenderPass } from 'three/addons/postprocessing/RenderPass.js';
import { ShaderPass } from 'three/addons/postprocessing/ShaderPass.js';

const defaultVertexShader = `
    varying vec2 v_uv;

    void main() {
        v_uv = uv;
        vec4 local = vec4(position, 1.0);
        gl_Position = projectionMatrix * modelViewMatrix * local;
    }
`;

const defaultFragmentShader = `
    uniform vec2 u_resolution;
    varying vec2 v_uv;

    void main() {
        gl_FragColor = vec4(vec2(v_uv), 0.0, 1.0);
    }
`;

export default {
	name: 'ThreeShader',
	inheritAttrs: false,

	props: {
		/**
		 * Vertex shader, provided as raw text.
		 * Will default to '~/assets/shaders/default.vs'
		 */
		vertexShader: {
			type: String,
			required: false,
		},

		/**
		 * Fragment shader, provided as raw text.
		 * Will default to '~/assets/shaders/default.fs'
		 */
		fragmentShader: {
			type: String,
			required: false,
		},

		/**
		 * Custom post processing passes.
		 * Array of objects containing `fragmentShader`, `vertexShader` and `uniforms`.
		 */
		postProcessingShaders: {
			type: Array,
			default: () => [],
		},

		/**
		 * Extra uniforms to be passed to shaders.
		 * Does per default include `u_time` and `u_resolution`,
		 * as well as `u_texture` if one has been specified.
		 */
		appendedUniforms: {
			type: Object,
			default: () => ({}),
		},

		/**
		 * Configuration parsed to WebGLRenderer. See documentation:
		 * https://threejs.org/docs/#api/en/renderers/WebGLRenderer
		 */
		rendererConfiguration: {
			type: Object,
			default: () => ({
				precision: 'lowp',
				antialias: false,
			}),
		},

		/**
		 * Path to texture.
		 * Will not default to anything.
		 */
		texture: {
			type: String,
			required: false,
		},

		/**
		 * Pixel-ratio will default to `window.devicePixelRatio`.
		 * Note however that this has no cap, pass a capped
		 * value if needed.
		 */
		pixelRatio: {
			type: Number,
			required: false,
		},

		/**
		 * Default canvas width, which overwrites texture width.
		 */
		width: {
			type: Number,
			required: false,
		},

		/**
		 * Default canvas height, which overwrites texture height.
		 */
		height: {
			type: Number,
			required: false,
		},

		/**
		 * AutoRender automatically stops the rendering process
		 * when the canvas is outside the viewport.
		 */
		autoRender: {
			type: Boolean,
			default: true,
		},
	},

	emits: ['loaded'],

	data() {
		this.camera = null;
		this.scene = null;
		this.composer = null;
		this.renderer = null;

		this.renderPass = null;
		this.postProcessingPasses = [];
		this.fxaaPass = null;

		return {
			uniforms: {
				u_time: { value: 0 },
				u_pixelRatio: { value: 1 },
				u_canvasResolution: { value: new THREE.Vector2() },
				u_textureResolution: { value: new THREE.Vector2() },
				...this.appendedUniforms,
			},

			clock: new THREE.Clock(),
			observer: null,

			widthActual: 0,
			heightActual: 0,
			textureConfiguration: null,
			textureObject: null,

			isRendering: false,
			isLoaded: false,
		};
	},

	watch: {
		appendedUniforms: {
			deep: true,
			handler(uniforms) {
				Object.assign(this.uniforms, uniforms);
			},
		},

		uniforms: {
			deep: true,
			handler(uniforms) {
				Object.entries(uniforms).forEach(([key, { value }]) => {
					this.postProcessingPasses.forEach((pass, n) => {
						this.postProcessingPasses[n].uniforms[key] = value;
					});
				});
			},
		},

		postProcessingShaders: {
			deep: true,
			handler(shaders) {
				shaders.forEach(({ uniforms }, n) => {
					if (uniforms) {
						Object.entries(uniforms).forEach(([key, { value }]) => {
							if (this.postProcessingPasses[n]?.uniforms?.[key]) {
								this.postProcessingPasses[n].uniforms[
									key
								].value = value;
							}
						});
					}
				});
			},
		},

		width(value) {
			if (this.camera && this.renderer) {
				this.uniforms.u_canvasResolution.value.x = Math.max(value, 128);
				this.camera.aspect =
					Math.max(value, 128) / Math.max(this.height, 128);
				this.renderer.setSize(
					Math.max(value, 128),
					Math.max(this.height, 128)
				);
			}
		},

		height(value) {
			if (this.camera && this.renderer) {
				this.uniforms.u_canvasResolution.value.y = Math.max(value, 128);
				this.camera.aspect =
					Math.max(this.width, 128) / Math.max(value, 128);
				this.renderer.setSize(
					Math.max(this.width, 128),
					Math.max(value, 128)
				);
			}
		},

		isRendering(value) {
			value && this.render();
		},
	},

	async mounted() {
		await this.initialise();

		if (this.autoRender) {
			this.observer = new IntersectionObserver(this.onIntersection);
			this.observer.observe(this.$el);
		}
	},

	beforeUnmount() {
		this.intersectionObserver?.disconnect();
		this.isRendering = false;
		this.isLoaded = false;
	},

	methods: {
		async initialise() {
			const texConfig = this.texture && (await this.fetchTexture());
			this.widthActual = this.width ?? texConfig?.width ?? 256;
			this.heightActual = this.height ?? texConfig?.height ?? 256;

			let width = Math.max(this.widthActual, 128);
			let height = Math.max(this.heightActual, 128);

			const vertexShader = this.vertexShader ?? defaultVertexShader;
			const fragmentShader = this.fragmentShader ?? defaultFragmentShader;
			const pixelRatio = this.pixelRatio ?? window.devicePixelRatio;

			this.scene = new THREE.Scene();
			this.camera = new THREE.Camera();
			this.camera.position.z = 1;

			this.scene.add(
				new THREE.Mesh(
					new THREE.PlaneGeometry(2, 2),
					new THREE.ShaderMaterial({
						uniforms: this.uniforms,
						vertexShader,
						fragmentShader,
					})
				)
			);

			this.renderer = new THREE.WebGLRenderer({
				...this.rendererConfiguration,
				canvas: this.$refs?.canvas,
			});

			this.renderer.setSize(width, height);
			this.renderer.setPixelRatio(pixelRatio);

			this.composer = new EffectComposer(this.renderer);
			this.renderPass = new RenderPass(this.scene, this.camera);
			this.composer.addPass(this.renderPass);

			this.postProcessingShaders.forEach(
				({ fragmentShader, vertexShader, uniforms }) => {
					const pass = new ShaderPass({
						fragmentShader: fragmentShader ?? defaultFragmentShader,
						vertexShader: vertexShader ?? defaultVertexShader,
						uniforms: {
							...this.uniforms,
							...uniforms,
							tDiffuse: { value: null },
						},
					});

					this.postProcessingPasses.push(pass);
					this.composer.addPass(pass);
				}
			);

			this.uniforms.u_canvasResolution.value.x = width;
			this.uniforms.u_canvasResolution.value.y = height;

			this.uniforms.u_textureResolution.value.x = texConfig?.width;
			this.uniforms.u_textureResolution.value.y = texConfig?.height;
			this.uniforms.u_pixelRatio.value = pixelRatio;

			this.isLoaded = true;
			this.isRendering = true;
			this.$emit('loaded');
		},

		render() {
			if (this.isRendering) {
				window.requestAnimationFrame(this.render);
				this.uniforms['u_time'].value = this.clock.getElapsedTime();
				this.composer.render();
			}
		},

		async fetchTexture() {
			return new Promise((resolve) => {
				new THREE.TextureLoader().load(this.texture, (texture) => {
					this.uniforms['u_texture'] = {
						type: 'sampler2D',
						value: texture,
					};

					resolve({
						width: texture.image?.width,
						height: texture.image?.height,
					});
				});
			});
		},

		onIntersection([{ isIntersecting }]) {
			this.isRendering = this.isLoaded && isIntersecting;
		},
	},
};
</script>

<style lang="postcss">
:where(.c-three-shader) {
	width: var(--width);
	height: var(--height);
}

:where(.c-three-shader canvas) {
	image-rendering: -moz-crisp-edges;
	image-rendering: -webkit-crisp-edges;
	image-rendering: pixelated;
}

:where(.c-three-shader__fallback) {
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
}
</style>
