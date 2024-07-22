const todoUrl = "https://wsl.wiredclone.com/fhir/Task";

const todos = document.getElementById("todos");

document.getElementById("createNewButton").addEventListener("click", createTodo);
const name = document.getElementById("name");
const date = document.getElementById("date");

window.addEventListener("load", showTodos);

async function createTodo() {
    error();

    const token = await window.bff.getSessionBearerAccessToken();    
    
    let request = new Request(todoUrl, {
        method: "POST",        
        headers: {
            "content-type": "application/json",
            "authorization": token
        },
        body: JSON.stringify({
            resourceType: "Task",
            status: "draft",
            code: {
                text: name.value
            },
            authoredOn: date.value,
            owner: {
                reference: "RelatedPerson/1"
            }
        })
    });

    let result = await fetch(request);
    if (result.ok) {
        var item = await result.json();
        addRow(item);
    }
    else {
        error(result.status)
    }
}

async function showTodos() {
    const token = await window.bff.getSessionBearerAccessToken();

    let result = await fetch(new Request(todoUrl, {
        headers: {
            "authorization": token,
        },
    }));

    if (result.ok) {
        let data = await result.json();
        
        if (data.entry)
            data.entry.forEach(item => addRow(item.resource));
    }
    else if (result.status !== 401) {
        // 401 == not logged in
        error(result.status)
    }
}

async function deleteTodo(id) {
    error();

    const token = await window.bff.getSessionBearerAccessToken();

    let request = new Request(todoUrl + "/" + id, {
        headers: {
            //'x-csrf': '1'
            "authorization": token
        },
        method: "DELETE"
    });

    let result = await fetch(request);
    if (result.ok) {
        deleteRow(id);
    }
    else {
        error(result.status)
    }
}


/////// UI helpers

function error(msg) {
    let alert = document.querySelector(".alert");
    let alertMsg = document.querySelector("#errText");

    if (msg) {
        alert.classList.remove("hide");
        alertMsg.innerText = msg;
    }
    else {
        alert.classList.add("hide");
        alertMsg.innerText = '';
    }
}


function addRow(item) {
    let row = document.createElement("tr");
    row.dataset.id = item.id;
    todos.appendChild(row);

    function addCell(row, text) {
        
        let cell = document.createElement("td");
        row.appendChild(cell);
        cell.innerText = text;
    }

    function addDeleteButton(row, id) {
        let cell = document.createElement("td");
        row.appendChild(cell);
        let btn = document.createElement("button");
        btn.classList.add("btn");
        btn.classList.add("btn-danger");
        cell.appendChild(btn);
        btn.textContent = "delete";
        btn.addEventListener("click", async () => await deleteTodo(id));
    }

    addDeleteButton(row, item.id);
    addCell(row, item.id);
    addCell(row, item.authoredOn ?? "N/A");
    addCell(row, item.code?.text ?? "N/A");
    addCell(row, item.owner?.reference ?? "N/A");
}


async function deleteRow(id) {
    let row = todos.querySelector(`tr[data-id='${id}']`);
    if (row) {
        todos.removeChild(row);
    }
}


