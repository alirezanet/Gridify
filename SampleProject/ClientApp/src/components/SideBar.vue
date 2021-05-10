<template>
   <div class="sidenav ">
      <router-link
         class="side-link mt-2"
         active-class="side-active"
         exact
         to="/"
      >
         <i class="fa fa-home"></i>
         Home</router-link
      >
      <router-link
         class="side-link"
         active-class="side-active"
         to="/FilterByPage"
      >
         <i class=" fa fa-sort-amount-asc"></i> Filter By Page</router-link
      >
      <router-link class="side-link" active-class="side-active" to="/Ordering">
         <i class=" fa fa-sort"></i> Order By</router-link
      >
      <router-link
         class="side-link"
         active-class="side-active"
         to="/SimpleFilter"
      >
         <i class=" fa fa-search"></i> Simple Filter</router-link
      >
      <router-link
         class="side-link"
         active-class="side-active"
         to="/AdvanceFilter"
      >
         <i class=" fa fa-search-plus"></i> Advance Filter</router-link
      >
   </div>
</template>

<script>
export default {};
</script>

<style scoped>
.side-active {
   color: #fff;
   border-bottom: 1px solid #fff;
}
.side-link {
   border-bottom: 1px solid #818181;
   font-size: 18px;
}
</style>
