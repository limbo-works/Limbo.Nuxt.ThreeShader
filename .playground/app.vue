<template>
	<div class="c-app">
		<div class="c-app__navigation">
			<NuxtLink to="/exampleDefault">Default Example</NuxtLink>
			<NuxtLink to="/exampleTexture">Texture Example</NuxtLink>
			<NuxtLink to="/exampleBloodMoon">Blood Moon Example</NuxtLink>
		</div>

		<main>
			<NuxtPage />
		</main>
	</div>
</template>

<style lang="postcss">
.c-app__navigation {
	display: flex;
	position: fixed;

	top: 32px;
	left: 50%;
	transform: translateX(-50%);
	column-gap: 32px;

	& a {
		color: #000;
		font-family: 'Inter', sans-serif;
		text-transform: uppercase;
		text-decoration: none;
	}

	& a:hover {
		text-decoration: underline;
	}
}

main {
	padding: 96px;
}
</style>
