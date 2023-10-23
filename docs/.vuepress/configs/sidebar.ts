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
            '/guide/autoMapper.md',
         ]
      }
   ],
   '/guide/elasticsearch/': [
      {
         text: 'Introduction',
         children: [
            '/guide/elasticsearch/README.md',
            '/guide/elasticsearch/getting-started.md',
            '/guide/elasticsearch/extensions.md',
            '/guide/elasticsearch/queryBuilder.md',
         ],
      },
      {
         text: 'Configuration',
         children: [
            '/guide/elasticsearch/gridifyQuery.md',
            '/guide/elasticsearch/gridifyMapper.md',
            '/guide/elasticsearch/gridifyGlobalConfiguration.md',
         ]
      },
      {
         text: 'Syntax',
         children: [
            '/guide/elasticsearch/filtering.md',
            '/guide/elasticsearch/ordering.md',
         ]
      },
      {
         text: 'Advanced',
         children: [
            '/guide/elasticsearch/dependency-injection.md',
            '/guide/elasticsearch/elasticsearch.md',
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
