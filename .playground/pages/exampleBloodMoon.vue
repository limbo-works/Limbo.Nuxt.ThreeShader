<template>
	<div class="p-example-fullscreen">
		<ThreeShader
			class="p-example-fullscreen"
			:fragment-shader="shader"
			:pixel-ratio="0.2"
			v-bind="{ width, height }"
		/>

		Yooo
	</div>
</template>

<script setup>
import shader from '~/assets/shaders/pixelated.fs?raw';

const width = ref(0);
const height = ref(0);

onMounted(() => (window.addEventListener('resize', onResize), onResize()));
onBeforeUnmount(() => window.removeEventListener('resize', onResize));

function onResize() {
	width.value = window.innerHeight * 0.75;
	height.value = window.innerHeight * 0.75;
}
</script>

<style lang="postcss">
.p-example-fullscreen > * {
	position: fixed;
	top: 50%;
	left: 50%;
	translate: -50% -50%;

	width: 75dvh;
	height: 75dvh;
	z-index: -1;
}
</style>
