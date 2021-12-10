import { defineClientAppEnhance } from '@vuepress/client'
import { version } from './configs'

export default defineClientAppEnhance(({ app, router, siteData }) => {
   app.config.globalProperties.$version = version
})
