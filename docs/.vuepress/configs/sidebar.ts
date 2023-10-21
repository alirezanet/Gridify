import type { SidebarConfig } from '@vuepress/theme-default'

export const sidebar: SidebarConfig = {
   '/guide/': [
      {
         text: 'Introduction',
         children: [
            '/guide/README.md',
            '/guide/getting-started.md',
            '/guide/extensions.md',
            '/guide/queryBuilder.md',
         ],
      },
      {
         text: 'Configuration',
         children: [
            '/guide/gridifyQuery.md',
            '/guide/gridifyMapper.md',
            '/guide/gridifyGlobalConfiguration.md',
         ]
      },
      {
         text: 'Syntax',
         children: [
            '/guide/filtering.md',
            '/guide/ordering.md',
         ]
      },
      {
         text: 'Advanced',
         children: [
            '/guide/dependency-injection.md',
            '/guide/compile.md',
            '/guide/entity-framework.md',
            '/guide/elasticsearch.md',
            '/guide/autoMapper.md',
         ]
      }
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
