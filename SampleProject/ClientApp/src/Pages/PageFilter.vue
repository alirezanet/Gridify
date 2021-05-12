<template>
   <div>
      <div class="main">
         <div class="container-fluid">
            <form class="mt-3">
               <div class="row">
                  <div class="form-group col-md-6">
                     <label>Page Size</label>
                     <input
                        type="number"
                        class="form-control"
                        placeholder="Enter page Size"
                        v-model="pageSize"
                     />
                  </div>
                  <div class="form-group col-md-6">
                     <label>Page</label>
                     <input
                        type="number"
                        class="form-control"
                        placeholder="Enter page"
                        v-model="page"
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
               <div class="pagination">
                  <span
                     v-for="page in pagination"
                     :key="page"
                     @click.prevent="pageChanger(page)"
                     >{{ page }}</span
                  >
               </div>
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
         pageSize: "",
         page: "",
         pagination: []
      };
   },
   methods: {
      async getData() {
         let pQuery = "";

         if (this.pageSize && this.page == "") {
            this.page = 0;
         }
         if (this.page && this.pageSize == "") {
            pQuery = `?Page=${this.page}`;
         }
         if (this.pageSize != "") {
            pQuery = `?Page=${this.page}&PageSize=${this.pageSize}`;
         }

         this.query = `/api/Gridify${pQuery}`;

         await axios.get(this.query).then(res => {
            this.items = res.data.items;
            this.count = res.data.totalItems;
         });
         this.pageMaker();
      },
      clear() {
         this.pageSize = "";
         this.page = "";
         this.getData();
      },
      pageMaker() {
         if (this.pageSize > 0) {
            this.pagination = [];
            this.pageNum = Math.ceil(this.count / this.pageSize);

            for (let i = 1; i <= this.pageNum; i++) {
               this.pagination.push(i);
            }
         }
         console.log(this.pagination);
      },
      pageChanger(page) {
         this.page = page;
         this.getData();
      }
   },
   mounted() {
      this.getData();
   }
};
</script>

<style scoped>
.pagination {
   display: block;
   align-items: center;
   text-align: center;
}
.pagination span {
   margin: 0.5rem;
   color: blue;
   font-size: 17px;
   cursor: pointer;
}
.pagination span:hover {
   border-bottom: 1px solid blue;
}
</style>
