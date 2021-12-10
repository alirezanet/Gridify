import { defineUserConfig } from 'vuepress-vite'
import type { DefaultThemeOptions } from 'vuepress-vite'
import type { ViteBundlerOptions } from '@vuepress/bundler-vite'
import { navbar, sidebar , plugin } from './configs'


export default defineUserConfig<DefaultThemeOptions, ViteBundlerOptions>({
   lang: 'en-US',
   title: 'Gridify',
   description: 'A Modern Dynamic LINQ library for .NET',
   bundler: '@vuepress/bundler-vite',
   plugins: plugin,
   themeConfig: {
      logo: '/gridify-logo.svg',
      logoDark: '/gridify-logo.svg',
      editLinks: true,
      editLinkText: 'Help us improve this page!',
      contributors: false,
      docsRepo: 'alirezanet/gridify',
      docsBranch: 'master',
      docsDir: '/docs',
      repo: 'alirezanet/Gridify',
      sidebar: sidebar,
      navbar: navbar,
   },
   port: 3000,
   base: '/Gridify/',

   head: [['link', { rel: 'icon', href: '/favicon.ico' }]],
})
