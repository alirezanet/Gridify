import type { DefaultTheme } from 'vitepress'

export const navbar: DefaultTheme.NavItem[] = [
   {
      text: 'Welcome',
      link: '/',
   },
   {
      text: 'Guide',
      link: '/guide/',
      activeMatch: '^\/guide\/*.*$'
      
   },
   {
      text: 'Examples',
      items: [
         {
            text: 'Using Gridify in API Controllers',
            link: '/example/api-controller',
            activeMatch: '/example/vuepress/',
         }
      ]
   },
   {
      text: 'Contribution',
      link: '/contribution/',
   },
   {
      text: 'Version',
      items: [
         {
            text: 'v2',
            link: '/',
            activeMatch: '^((?!\/v1).)*$',
         },
         {
            text: 'v1',
            link: '/v1/',
            activeMatch: '^\/v1\/*.*$',
         },
      ],
   },
]
