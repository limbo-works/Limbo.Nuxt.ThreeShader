<template>
	<div class="p-example-fullscreen">
		<ThreeShader
			class="p-example-fullscreen"
			:fragment-shader="shader"
			:pixel-ratio="0.05"
			v-bind="{ width, height }"
		/>
	</div>
</template>

<script setup>
import shader from '~/assets/shaders/pixelated.fs?raw';

const width = ref(0);
const height = ref(0);

onMounted(() => (window.addEventListener('resize', onResize), onResize()));
onBeforeUnmount(() => window.removeEventListener('resize', onResize));

function onResize() {
	width.value = window.innerWidth;
	height.value = window.innerHeight;
}
</script>

<style lang="postcss">
.p-example-fullscreen > * {
	position: fixed;
	top: 0px;
	left: 0px;
	width: 100dvw;
	height: 100dvh;
	z-index: -1;
}
</style>
