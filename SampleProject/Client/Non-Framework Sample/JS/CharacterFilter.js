const firstNameInput = document.getElementById('firstName');
const lastNameInput = document.getElementById('lastName');
const searchBtn = document.getElementById('searchBtn');
const clearBtn = document.getElementById('clearBtn');
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

//Search Data with params
function search() {
    query = proxy + '/api/Gridify'
    if (firstNameInput.value && !lastNameInput.value) {
        query = proxy + `/api/Gridify/?Filter=firstName=*${firstNameInput.value}`;
    }
    if (!firstNameInput.value && lastNameInput.value) {
        query = proxy + `/api/Gridify/?Filter=lastName==${lastNameInput.value}`;
    }
    if (firstNameInput.value && lastNameInput.value) {
        query = proxy + `/api/Gridify/?Filter=firstName=*${firstNameInput.value},lastName==${lastNameInput.value}`;
    }
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
clearBtn.addEventListener('click', clear)
searchBtn.addEventListener('click', search)