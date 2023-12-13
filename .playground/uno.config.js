import { defineConfig } from 'unocss';
import { presetCore } from '@limbo-works/nuxt-core/assets/js/unocss/preset-core.js';
import { presetNoDefaultRem } from '@limbo-works/nuxt-core/assets/js/unocss/preset-no-default-rem.js';
import transformerVariantGroup from '@unocss/transformer-variant-group';
import transformerDirectives from '@unocss/transformer-directives';
import {
	makeThemeUtilities,
	makeRules,
} from '../components/ThemeConfiguration/helpers.uno.js';
import defaultConfig from './assets/js/theme-configuration.default.js';

export default defineConfig({
	presets: [presetCore(), presetNoDefaultRem()],
	transformers: [transformerVariantGroup(), transformerDirectives()],

	theme: { ...makeThemeUtilities(defaultConfig) },
	rules: [...makeRules(defaultConfig)],

	content: {
		pipeline: {
			exclude: [
				/* Something here is wreaking havoc with unoCSS */
				'**/ThemeConfiguration.vue',
			],
		},
	},
});
