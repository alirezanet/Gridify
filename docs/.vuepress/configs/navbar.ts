import type { NavbarConfig } from '@vuepress/theme-default'

export const navbar: NavbarConfig = [
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
]
