<template>
   <!--  In complex filter the search method checks all parameters and if one theme would true it will show the results -->
   <div>
      <div class="main">
         <div class="container-fluid">
            <form class="mt-3">
               <div class="row">
                  <div class="form-group col-md-6">
                     <label>First Name</label>
                     <input
                        type="text"
                        class="form-control"
                        placeholder="First Name Contains (FieldName=*Value)"
                        v-model="firstName"
                     />
                  </div>
                  <div class="form-group col-md-6">
                     <label>Age Less Than</label>
                     <input
                        type="number"
                        class="form-control"
                        placeholder="Enter page (GreaterThanOrEqual	FieldName<=Value) "
                        v-model="ageLessThan"
                     />
                  </div>
               </div>
               <button
                  type="submit"
                  class="btn btn-primary"
                  @click.prevent="getData()"
               >
                  Search
               </button>
               <button
                  type="submit"
                  class="btn btn-danger ml-1"
                  @click.prevent="clear()"
               >
                  Clear
               </button>
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
                     <tr v-for="item in items" :key="item.id">
                        <th scope="row">
                           {{ item.id }}
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
         query: `/api/Gridify`,
         firstName: "",
         ageLessThan: ""
      };
   },
   methods: {
      // Get Data From Backend
      getData() {
         this.query = `/api/Gridify`;
         if (this.firstName && !this.ageLessThan) {
            this.query = `/api/Gridify?Filter=firstName=*${this.firstName}`;
         }
         if (!this.firstName && this.ageLessThan) {
            this.query = `/api/Gridify?Filter=age<=${this.ageLessThan}`;
         }
         if (this.firstName && this.ageLessThan) {
            this.query = `/api/Gridify?Filter= firstName=*${this.firstName}&age<=${this.ageLessThan}`;
         }
         // Call data from Get method by axios (third party library)
         axios.get(this.query).then(res => {
            this.items = res.data.items;
            this.count = res.data.totalItems;
         });
      },
      clear() {
         this.firstName = "";
         this.ageLessThan = "";
         this.getData();
      }
   },
   mounted() {
      this.getData();
   }
};
</script>

<style scoped></style>
