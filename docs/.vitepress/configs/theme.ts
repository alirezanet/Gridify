import { DefaultTheme } from 'vitepress'
import { navbar } from './navbar'
import { sidebar } from './sidebar'

export const theme: DefaultTheme.Config = {
  logo: {
    light: '/gridify-light-logo2.svg',
    dark: '/gridify-dark-logo2.svg'
  },
  siteTitle: '',
  sidebar: sidebar,
  nav: navbar,
  search: {
    provider: 'local',
  },
  outline: {
    level: [2, 3],
  },
  footer: {
    copyright:
      'Copyright © 2021-present <a target="_blank" href="https://github.com/alirezanet">AliReza Sabouri</a>, Released under <a target="_blank" href="https://github.com/alirezanet/Gridify/blob/master/LICENSE">MIT License</a>',
  },
  socialLinks: [
    { icon: 'github', link: 'https://github.com/alirezanet/Gridify' },
    { icon: "discord", link: "https://discord.gg/Q6HvFGPm" },
  ],
  editLink: {
    pattern: 'https://github.com/alirezanet/Gridify/edit/master/docs/pages/:path',
  },
  // lastUpdated: LastUpdatedOptions
}
