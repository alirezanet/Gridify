<template>
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
                     <label>LastName</label>
                     <input
                        type="text"
                        class="form-control"
                        placeholder="Last Name Equal (FieldName==Value)"
                        v-model="lastName"
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
         query: `/api/Gridify`,
         firstName: "",
         lastName: ""
      };
   },
   methods: {
      getData() {
         let fNameQuery;
         let lNameQuery;
         if (this.firstName) {
            fNameQuery = `firstName=*${this.firstName}`;
         }
         if (this.lastName) {
            lNameQuery = `lastName==${this.lastName}`;
         }
         this.query = `/api/Gridify`;

         if (fNameQuery && !lNameQuery) {
            this.query = `/api/Gridify/?Filter=${fNameQuery}`;
         }
         if (!fNameQuery && lNameQuery) {
            this.query = `/api/Gridify/?Filter=${lNameQuery}`;
         }
         if (fNameQuery && lNameQuery) {
            this.query = `/api/Gridify/?Filter=${fNameQuery},${lNameQuery}`;
         }

         axios.get(this.query).then(res => {
            this.items = res.data.items;
         });
      },
      clear() {
         this.lastName = "";
         this.firstName = "";
         this.getData();
      }
   },
   mounted() {
      this.getData();
   }
};
</script>

<style scoped></style>
