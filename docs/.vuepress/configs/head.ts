import type { HeadConfig } from 'vuepress-vite';

export const head: HeadConfig[] = [
   // favicon
   ['link', { rel: 'icon', href: 'favicon.ico', type: "image/x-icon" }],
   ['link', { rel: 'shortcut icon', href: 'favicon.ico', type: "image/x-icon" }],

   // social media image
   ['meta', { property: 'og:image', content: 'https://alirezanet.github.io/Gridify/social-logo.png' }],
   ['meta', { property: 'og:image:type', content: 'image/png' }],
   ['meta', { property: 'og:image:width', content: '1280' }],
   ['meta', { property: 'og:image:height', content: '640' }],
   ['meta', { property: 'og:image:title', content: 'Gridify' }],
]
