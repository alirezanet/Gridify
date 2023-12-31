import type { DefaultTheme } from 'vitepress'

export const sidebar: DefaultTheme.Sidebar = {
   '/guide/': [
      {
         text: 'Introduction',
         items: [
            { text: 'Introduction', link: '/guide/' },
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'Extensions', link: '/guide/extensions' },
            { text: 'QueryBuilder', link: '/guide/queryBuilder' },
         ],
      },
      {
         text: 'Syntax',
         items: [
            { text: 'Filtering', link: '/guide/filtering' },
            { text: 'Ordering', link: '/guide/ordering' },
         ]
      },
      {
         text: 'Configuration',
         items: [
            { text: 'GridifyQuery', link: '/guide/gridifyQuery' },
            { text: 'GridifyMapper', link: '/guide/gridifyMapper' },
            { text: 'Global Configuration', link: '/guide/gridifyGlobalConfiguration' },
         ]
      },
      {
         text: 'Extension Packages',
         items: [
            { text: 'EntityFramework', link: '/guide/extensions/entityframework' },
            { text: 'Elasticsearch', link: '/guide/extensions/elasticsearch' },
            { text: 'Gridify Client (JS/TS)', link: '/guide/extensions/gridify-client' },
         ]
      },
      {
         text: 'Advanced',
         items: [
            { text: 'Dependency injection', link: '/guide/dependency-injection' },
            { text: 'Compilation', link: '/guide/compile' },
            { text: 'AutoMapper', link: '/guide/autoMapper' },
         ]
      },

   ],

}
