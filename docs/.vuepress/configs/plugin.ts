import type { PluginOptions, PluginConfig } from 'vuepress-vite';
import { path } from '@vuepress/utils'

export const plugin: PluginConfig<PluginOptions>[] = [
   [
      '@vuepress/plugin-search',
      {
         // exclude v1 route
         isSearchable: (page) => !page.path.match(/^\/v1\/*.*$/),
      },
   ]
   // [
   //    '@vuepress/plugin-register-components',
   //    {
   //       componentsDir: path.resolve(__dirname, '../components'),
   //    },
   // ]
]
