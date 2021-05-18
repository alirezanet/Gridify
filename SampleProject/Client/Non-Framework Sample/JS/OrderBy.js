
const ascendingBtn=document.getElementById('ascending')
const descendingBtn=document.getElementById('descending')
const noneSortBtn=document.getElementById('noneSort')
const tableBody = document.getElementById('tableBody');
const proxy = "http://localhost:5000"
let queryParam = document.getElementById('queryParam')
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

// Descending Search
function descendingSearch() {
    query = proxy + '/api/Gridify?SortBy=age'
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

// Ascending Sort
function ascendingSearch() {
    query = proxy + '/api/Gridify?SortBy=age&&IsSortAsc=true'
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

// Clear Input values    
function clear() {
    firstNameInput.value = '';
    lastNameInput.value = '';
    noSearchParam();
}
queryParam.innerHTML = 'query: ' + query;


// Event Listener For btn's
ascendingBtn.addEventListener('click',ascendingSearch)
descendingBtn.addEventListener('click',descendingSearch)
noneSortBtn.addEventListener('click',noSearchParam)