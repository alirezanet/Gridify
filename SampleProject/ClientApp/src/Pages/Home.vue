<template>
   <div class="container-fluid">
      <div class="paragraph">
         <h2>Welcome To Gridify</h2>
         <p>
            Easy and optimized way to apply Filtering, Sorting and pagination
            using text-based data. The best use case of this library is Asp-net
            APIs. when you need to get some string base filtering conditions to
            filter data or sort it by a field name or apply pagination concepts
            to your lists and return a pageable, data grid ready information,
            from any repository or database.
         </p>
         <hr />

         <h2>
            Installation
         </h2>
         <p>
            Install the Gridify NuGet Package.
            <a href="https://www.nuget.org/packages/Gridify/" class="d-block">
               Package Manager Console
            </a>
         </p>
         <h5 class="mt-1">
            Package Manager Console
         </h5>
         <div class="code-border mt-0">
            Install-Package Gridify
         </div>

         <h5 class="mt-3">
            .NET Core CLI
         </h5>
         <div class="code-border mt-0">
            dotnet add package Gridify
         </div>
         <hr />
         <div class="links">
            <i class="fa fa-github mr-2"></i>
            <a href="https://github.com/Alirezanet/Gridify">Github Link </a>
         </div>
      </div>
   </div>
</template>

<script>
export default {};
</script>

<style scoped>
.container-fluid {
   background-color: beige;
   padding: 1rem;
   height: 100vh;
}
.paragraph {
   margin-left: 220px;
}
.code-border {
   box-shadow: rgba(0, 0, 0, 0.05) 0px 6px 24px 0px,
      rgba(0, 0, 0, 0.08) 0px 0px 0px 1px;
   padding: 0.5rem;
   width: 250px;
   color: brown;
}
</style>
