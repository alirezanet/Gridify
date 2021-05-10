import Vue from "vue";
import VueRouter from "vue-router";
import Home from "./Pages/Home";
import PageFilter from "./Pages/PageFilter";
import SimpleFilter from "./Pages/SimpleFilter";
import Ordering from "./Pages/Ordering";
Vue.use(VueRouter);

export const Routes = [
   // history: createWebHistory(),
   {
      path: "/",
      component: Home
   },
   {
      path: "/FilterByPage",
      component: PageFilter
   },
   {
      path: "/SimpleFilter",
      component: SimpleFilter
   },
   {
      path: "/Ordering",
      component: Ordering
   }
];
