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
         to="/CharacterFilter"
      >
         <i class=" fa fa-search"></i> Character Filter</router-link
      >
      <router-link
         class="side-link"
         active-class="side-active"
         to="/NumericFilter"
      >
         <i class="fa fa-sort-numeric-desc" aria-hidden="true"></i>
         Numeric Filter</router-link
      >
      <router-link
         class="side-link"
         active-class="side-active"
         to="/ComplexFilter"
      >
         <i class="fa fa-search-plus" aria-hidden="true"></i>
         Complex Filter</router-link
      >
      <router-link
         class="side-link"
         active-class="side-active"
         to="/CustomMapping"
      >
         <i class="fa fa-cog" aria-hidden="true"></i>
         Custom Mapping</router-link
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
