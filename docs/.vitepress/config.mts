import { defineConfig } from "vitepress";
import { theme, head } from "./configs";

// https://vitepress.dev/reference/site-config
export default defineConfig({
   title: "Gridify",
   lang: "en-US",
   description: "A powerful dynamic LINQ library for .NET",
   base: "/Gridify/",
   themeConfig: theme,
   cleanUrls: true,
   srcDir: "./pages",
   outDir: "./dist",
   head: head,
   sitemap: {
      hostname: "https://alirezanet.github.io/Gridify/",
   }
});
