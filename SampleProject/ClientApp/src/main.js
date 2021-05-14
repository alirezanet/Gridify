import Vue from "vue";
import App from "./App.vue";
import VueRouter from "vue-router";

Vue.config.productionTip = false;
Vue.use(VueRouter);

import { Routes } from "./router";
export const router = new VueRouter({
   routes: Routes,
   mode: "history",
   scrollBehavior() {
      return {
         x: 0,
         y: 0
      };
   }
});

new Vue({
   router,
   render: h => h(App)
}).$mount("#app");
