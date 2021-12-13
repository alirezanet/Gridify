import type { DefaultThemeOptions } from 'vuepress-vite'
import { sidebar, navbar } from '.'

export const themeConfig: DefaultThemeOptions = {
   logo: '/gridify-light-logo.svg',
   logoDark: '/gridify-dark-logo.svg',
   editLinks: true,
   editLinkText: 'Help us improve this page!',
   contributors: false,
   docsRepo: 'alirezanet/gridify',
   docsBranch: 'master',
   docsDir: '/docs',
   repo: 'alirezanet/Gridify',
   sidebar: sidebar,
   navbar: navbar,
}
