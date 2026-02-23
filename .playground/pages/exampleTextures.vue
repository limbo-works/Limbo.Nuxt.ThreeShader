<template>
	<ThreeShader
		:textures="[
			'/placeholder2-5842x3895.jpg',
			'/placeholder3-5472x3648.jpg',
		]"
		:fragment-shader="shader"
		:width="1378"
		:height="774"
	/>
</template>

<script setup>
import shader from '~/assets/shaders/noise.fs?raw';
</script>
