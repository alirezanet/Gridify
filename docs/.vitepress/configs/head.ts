import type { HeadConfig } from 'vitepress';

export const head: HeadConfig[] = [
   // favicon
   ['link', { rel: 'icon', href: '/Gridify/favicon.ico', type: "image/x-icon" }],
   ['link', { rel: 'shortcut icon', href: '/Gridify/favicon.ico', type: "image/x-icon" }],

   // social media image
   ['meta', { property: 'og:image', content: 'https://alirezanet.github.io/Gridify/social-logo.png' }],
   ['meta', { property: 'og:image:type', content: 'image/png' }],
   ['meta', { property: 'og:image:width', content: '1280' }],
   ['meta', { property: 'og:image:height', content: '640' }],
   ['meta', { property: 'og:title', content: 'Gridify' }],
   ['meta', { property: 'og:type', content: 'website' }],
   ['meta', { property: 'og:url', content: 'https://alirezanet.github.io/Gridify/' }],
   ['meta', { property: 'og:description', content: 'A powerful dynamic LINQ library for .NET' }],
   ['meta', { property: 'twitter:card', content: 'summary_large_image' }],

   // google analytics
   [ 'script', { async: true, src: 'https://www.googletagmanager.com/gtag/js?id=G-G5Z9S1WECF', }, ], 
   [ 'script', {}, "window.dataLayer = window.dataLayer || [];\nfunction gtag(){dataLayer.push(arguments);}\ngtag('js', new Date());\ngtag('config', 'G-G5Z9S1WECF');", ],

]
