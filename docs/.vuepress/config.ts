import { defineUserConfig } from 'vuepress-vite'
import type { DefaultThemeOptions } from 'vuepress-vite'
import type { ViteBundlerOptions } from '@vuepress/bundler-vite'



export default defineUserConfig<DefaultThemeOptions, ViteBundlerOptions>({
   lang: 'en-US',
   title: 'Gridify',
   description: 'A Modern Dynamic LINQ library for .NET',
   bundler: '@vuepress/bundler-vite',
   plugins: [
      [
         '@vuepress/plugin-search',
         {
            // exclude v1 route
            isSearchable: (page) => !page.path.match(/^\/v1\/*.*$/),
         },
      ],
   ],
   themeConfig: {
      editLinks: true,
      editLinkText: 'Help us improve this page!',
      contributors: false,
      docsRepo: 'alirezanet/gridify',
      docsBranch: 'master',
      docsDir: '/docs',
      repo: 'alirezanet/Gridify',
      sidebar: {
         '/guide/': [
            {
               text: 'Guide',
               link: '/guide/',
               children: [
                  '/guide/README.md',
                  '/guide/getting-started.md',
                  '/guide/extensions.md',
                  '/guide/filtering.md',
                  '/guide/ordering.md',
                  '/guide/gridifyMapper.md',
                  '/guide/gridifyQuery.md',
                  '/guide/queryBuilder.md',
                  '/guide/compile.md',
                  '/guide/entity-framework.md',
                  '/guide/autoMapper.md',
               ],
            },
         ],
         '/contribution/': [
            {
               text: 'Contribution',
               link: '/contribution/',
               children: [
                  '/contribution/README.md',
               ]
            }
         ]
      },
      navbar: [
         {
            text: 'Welcome',
            link: '/',
         },
         {
            text: 'Guide',
            link: '/guide/',
         },
         {
            text: 'Contribution',
            link: '/contribution/',
         },
         {
            text: 'Version',
            children: [
               {
                  text: 'v2',
                  link: '/',
                  activeMatch: '^((?!\/v1).)*$',
               },
               {
                  text: 'v1',
                  link: '/v1/',
                  // this item will be active when current route path starts with /foo/
                  // regular expression is supported
                  activeMatch: '^\/v1\/*.*$',
               },
            ],
         },
         {
            text: 'Author',
            link: 'https://github.com/alirezanet',
         },
      ],
   },
   port: 3000,
   base: '/Gridify/',

   head: [['link', { rel: 'icon', href: '/favicon.ico' }]],
})
