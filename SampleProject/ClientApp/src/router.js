import Vue from "vue";
import VueRouter from "vue-router";
import Home from "./Pages/Home";
import PageFilter from "./Pages/PageFilter";
import CharacterFilter from "./Pages/CharacterFilter";
import Ordering from "./Pages/Ordering";
import NumericFilter from "./Pages/NumericFilter";
import ComplexFilter from "./Pages/ComplexFilter"
import CustomMapping from "./Pages/CustomMapping"
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
      path: "/CharacterFilter",
      component: CharacterFilter
   },
   {
      path: "/Ordering",
      component: Ordering
   },
   {
      path: "/NumericFilter",
      component: NumericFilter
   },
   {
      path: "/ComplexFilter",
      component: ComplexFilter
   },
   {
      path: "/CustomMapping",
      component: CustomMapping
   }
];
