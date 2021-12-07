import type { SidebarConfig } from '@vuepress/theme-default'

export const sidebar: SidebarConfig = {
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
}
