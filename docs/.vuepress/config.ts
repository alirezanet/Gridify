import { defineUserConfig } from 'vuepress-vite'
import type { DefaultThemeOptions } from 'vuepress-vite'
import type { ViteBundlerOptions } from '@vuepress/bundler-vite'
import { plugin, themeConfig, head } from './configs'

export default defineUserConfig<DefaultThemeOptions, ViteBundlerOptions>({
   lang: 'en-US',
   title: 'Gridify',
   description: 'A Modern Dynamic LINQ library for .NET',
   bundler: '@vuepress/bundler-vite',
   plugins: plugin,
   themeConfig: themeConfig,
   port: 3000,
   base: '/Gridify/',
   head: head,
})
