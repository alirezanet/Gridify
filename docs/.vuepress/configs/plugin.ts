import type { PluginOptions, PluginConfig } from 'vuepress-vite';
import { path } from '@vuepress/utils'

export const plugin: PluginConfig<PluginOptions>[] = [
   [
      '@vuepress/plugin-search',
      {
         // exclude v1 route
         isSearchable: (page) => !page.path.match(/^\/v1\/*.*$/),
      },
   ],
   [
      '@vuepress/plugin-google-analytics',
      {
        id: 'G-G5Z9S1WECF',
      },
   ],
   // [
   //    '@vuepress/plugin-register-components',
   //    {
   //       componentsDir: path.resolve(__dirname, '../components'),
   //    },
   // ]
]
