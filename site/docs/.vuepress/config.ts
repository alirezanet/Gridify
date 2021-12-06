import { defineUserConfig } from 'vuepress-vite'
import type { DefaultThemeOptions } from 'vuepress-vite'
import type { ViteBundlerOptions } from '@vuepress/bundler-vite'

export default defineUserConfig<DefaultThemeOptions, ViteBundlerOptions>({
   lang: 'en-US',
   title: 'Gridify',
   description: 'A Modern Dynamic LINQ library for .NET',
   plugins: [
      [
         '@vuepress/plugin-search',
         {
            // exclude the homepage
            isSearchable: (page) => page.path !== '/',
          },
      ],
   ],
   themeConfig: {
      editLinks: true,
      editLinkText: 'Help us improve this page!',
      contributors: false,
      docsRepo: 'alirezanet/gridify',
      docsBranch: 'master',
      docsDir: 'site/docs',
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
           text: 'Guide',
           link: '/guide/',
         },
         {
            text: 'Contribution',
            link: '/contribution/',
         },
         {
            text: 'Author',
            link: 'https://github.com/alirezanet',
         },
         {
            text: 'GitHub',
            link: 'https://github.com/alirezanet/Gridify',
         },

       ],
   },
   dest: 'dist',
   port: 3000,


   head: [['link', { rel: 'icon', href: '/favicon.ico' }]],
})
