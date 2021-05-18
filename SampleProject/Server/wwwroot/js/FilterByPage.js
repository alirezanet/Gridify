const pageSizeInput = document.getElementById('pageSize');
const pageInput = document.getElementById('page');
const searchBtn = document.getElementById('searchBtn');
const clearBtn = document.getElementById('clearBtn');
const tableBody = document.getElementById('tableBody');
const paginationBox=document.getElementById('pagination')
const proxy = "http://localhost:5000"
let queryParam = document.getElementById('queryParam')
let pagination
let count;
let query;

// Fetiching No search Data
query = proxy + '/api/Gridify'
fetch(query)
    .then(response => response.json())
    .then(data => {
        data.items.forEach((q, index) => {
            const headerOne = document.createElement('tr');
            headerOne.innerHTML = `
            <th scope="row">${ index+1 } </th>
            <th scope="row">${ q.firstName } </th>
            <th scope="row">${ q.lastName } </th>
            <th scope="row">${ q.age } </th>
            <th scope="row">${ q.phoneNumber } </th>
            <th scope="row">${ q.address } </th>
            `
            tableBody.append(headerOne)
        });
    })

//Search Data with params
async  function search() {
    query = proxy + '/api/Gridify'

      let pQuery = "";

         if (pageSizeInput.value && pageInput.value == "") {
            pageInput.value = 1;
         }
         if (pageInput.value && pageSizeInput.value == "") {
            pQuery = `?Page=${pageInput.value}`;
         }
         if (pageSizeInput.value != "") {
            pQuery = `?Page=${pageInput.value}&PageSize=${pageSizeInput.value}`;
         }

         query =  proxy+`/api/Gridify${pQuery}`;
    queryParam.innerHTML = 'query: ' + query;

    tableBody.innerHTML = ''
  await  fetch(query)
        .then(response => response.json())
        .then(data => {
            data.items.forEach((q, index) => {
                const headerOne = document.createElement('tr');
                headerOne.innerHTML = `
                <th scope="row">${ index+1 } </th>
                <th scope="row">${ q.firstName } </th>
                <th scope="row">${ q.lastName } </th>
                <th scope="row">${ q.age } </th>
                <th scope="row">${ q.phoneNumber } </th>
                <th scope="row">${ q.address } </th>
                `
                tableBody.append(headerOne)
            });
            count=data.totalItems
        })
        pageMaker()
}


// No Search param
function noSearchParam() {
    query = proxy + '/api/Gridify'
    queryParam.innerHTML = 'query: ' + query;
    tableBody.innerHTML = ''
    fetch(query)
        .then(response => response.json())
        .then(data => {
            data.items.forEach((q, index) => {
                const headerOne = document.createElement('tr');
                headerOne.innerHTML = `
            <th scope="row">${ index+1 } </th>
            <th scope="row">${ q.firstName } </th>
            <th scope="row">${ q.lastName } </th>
            <th scope="row">${ q.age } </th>
            <th scope="row">${ q.phoneNumber } </th>
            <th scope="row">${ q.address } </th>
            `
                tableBody.append(headerOne)



            });
        })

}

 //pagination method
 function pageMaker() {
    if (pageSizeInput.value > 0) {
       pagination = [];
       pageNum = Math.ceil(count / pageSizeInput.value);
       paginationBox.innerHTML=``
       for (let i = 1; i <= pageNum; i++) {
          pagination.push(i);
          const pageNums = document.createElement('span');
          pageNums.setAttribute('id','num-'+i)
          pageNums.innerHTML=`${i}`
          paginationBox.append(pageNums)
        document.getElementById('num-'+i).addEventListener('click',()=>{
            pageInput.value =i;
            search()
        })
       }
      
      
    }
 }
 // change the page by click from dome
 function pageChanger(page) {
    pageInput.value = page;
    console.log(pageInput.value);
    search()
 }

// Clear Input values    
function clear() {
    pageSizeInput.value = '';
    pageInput.value = '';
    paginationBox.innerHTML=``
    noSearchParam();
}
queryParam.innerHTML = 'query: ' + query;


// Event Listener For btn's
clearBtn.addEventListener('click', clear)
searchBtn.addEventListener('click', search)