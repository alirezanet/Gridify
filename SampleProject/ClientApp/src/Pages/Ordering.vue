<template>
   <div>
      <div class="main">
         <div class="container-fluid">
            <form class="mt-3 ">
               <h3 class="lable-age">Sort by Age</h3>
               <div class="row ml-2">
                  <div class="form-check col-md-4">
                     <input
                        class="form-check-input "
                        type="radio"
                        v-model="sort"
                        value="ac"
                     />
                     <label class="form-check-label">
                        Ascending
                     </label>
                  </div>
                  <div class="form-check col-md-4">
                     <input
                        class="form-check-input "
                        type="radio"
                        v-model="sort"
                        value="dc"
                     />
                     <label class="form-check-label">
                        Descending
                     </label>
                  </div>
                  <div class="form-check col-md-4">
                     <input
                        class="form-check-input "
                        type="radio"
                        v-model="sort"
                        value="none"
                        checked
                     />
                     <label class="form-check-label">
                        None Sort
                     </label>
                  </div>
               </div>
               <div class="mt-2">
                  <button
                     type="submit"
                     class="btn btn-primary"
                     @click.prevent="getData()"
                  >
                     Search
                  </button>
               </div>
            </form>

            <div class="query mt-5">
               <p class="ml-3 query-text">Query: {{ query }}</p>
            </div>
            <div class="table-responsive" v-if="items.length">
               <table class="table table-bordered mt-5">
                  <thead>
                     <tr>
                        <th scope="col">#</th>
                        <th scope="col">First Name</th>
                        <th scope="col">Last Name</th>
                        <th scope="col">Age</th>
                        <th scope="col">Phone Number</th>

                        <th scope="col">Address</th>
                     </tr>
                  </thead>
                  <tbody>
                     <tr v-for="(item, index) in items" :key="item.id">
                        <th scope="row">
                           {{ index + 1 }}
                        </th>
                        <td>{{ item.firstName }}</td>
                        <td>{{ item.lastName }}</td>
                        <td>{{ item.age }}</td>
                        <td>{{ item.phoneNumber }}</td>
                        <td>{{ item.address }}</td>
                     </tr>
                  </tbody>
               </table>
            </div>
            <h2 v-else class="text-muted mx-auto">No Data</h2>
         </div>
      </div>
   </div>
</template>

<script>
import axios from "axios";
export default {
   data() {
      return {
         items: [],
         count: null,
         query: `/api/Gridify/Ordering`,
         sort: "none"
      };
   },
   methods: {
      getData() {
         this.query = `/api/Gridify/Ordering`;
         if (this.sort === "ac") {
            this.query = `/api/Gridify/Ordering?SortBy=age`;
         }
         if (this.sort === "dc") {
            this.query = `/api/Gridify/Ordering?SortBy=age&isAsc=false`;
         }
         if (this.sort === "none") {
            this.query = `/api/Gridify/Ordering`;
         }
         axios.get(this.query).then(res => {
            this.items = res.data;
         });
      }
   },
   mounted() {
      this.getData();
   }
};
</script>

<style scoped>
.lable-age {
   font-size: 18px !important;
   margin-left: 0.5rem;
}
</style>
