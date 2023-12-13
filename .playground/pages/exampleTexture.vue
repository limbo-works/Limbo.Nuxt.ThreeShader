<template>
	<ThreeShader
		texture="/placeholder-1378x774.jpg"
		:fragment-shader="shader"
		:width="1378"
		:height="774"
	/>
</template>

<script setup>
import shader from '~/assets/shaders/distorted.fs?raw';
</script>
