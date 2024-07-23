const sessionUrl = "/smart-bff/session";

async function getSession() {
    const req = new Request(sessionUrl, {
        headers: new Headers({
            'X-CSRF': '1'
        })
    });
    return await fetch(req);
}

async function getSessionAccessToken() {
    const resp = await getSession()
    const json = await resp.json();
    return json.accessToken;
}

async function getSessionBearerAccessToken() {
    return "Bearer " + (await getSessionAccessToken());
}

window.bff = {
    getSession: getSession,
    getSessionAccessToken: getSessionAccessToken,
    getSessionBearerAccessToken: getSessionBearerAccessToken
}

function addModalRow(body, key, value) {
    const c1 = document.createElement("td");
    c1.innerText = key;
    const c2 = document.createElement("td");
    c2.innerText = value;
    const r = document.createElement("tr");
    r.appendChild(c1);
    r.appendChild(c2);
    
    body.appendChild(r);
}

window.addEventListener("load", loadSession);

async function loadSession() {

    const username = document.querySelector("#username");

    try {
        const sessionResponse = await getSession();
        if (sessionResponse.ok) {
            document.querySelector(".logged-in").classList.remove("hide");
            let sessionDetails = await sessionResponse.json();
            
            username.innerText = sessionDetails.name;

        } else if (sessionResponse.status === 401) {
            document.querySelector(".not-logged-in").classList.remove("hide");
            username.innerText = "(N/A)";
            
        }
    } catch (e) {
        console.log("error checking user status", e);
    }

}

const { decode, verify, sign } = jwtJsDecode;

document.querySelector(".show_session").addEventListener("click", async () => {
    try {
        const sessionResponse = await getSession();
        if (sessionResponse.ok) {
            let sessionDetails = await sessionResponse.json();

            let name = document.getElementById("modalName");
            name.innerText = sessionDetails.name;

            const accessTokenJson = decode(sessionDetails.accessToken);
             
            let accessToken = document.getElementById("modalAccessToken");
            accessToken.innerText = JSON.stringify(accessTokenJson.payload, null, 4);

            let identityToken = document.getElementById("modalIdToken");
            
            if (sessionDetails.idToken) {
                const idTokenJson = decode(sessionDetails.idToken);
                identityToken.innerText = JSON.stringify(idTokenJson.payload, null, 4);
            } else {
                identityToken.innerText = "N/A";
            }
                                    
            const modal = document.getElementById("sessionModal");
            const myModal = new bootstrap.Modal(modal);
            myModal.toggle();

        } else if (sessionResponse.status === 401) {
            window.location = "/";
        }

    } catch (e) {
        console.log("error checking user session", e);
    }

});
document.getElementById('logoutSmileLink').addEventListener('click', async _ => {
    const x = await getSessionAccessToken();

    const resp = await fetch("https://wsl.wiredclone.com/auth/logout?cb=none&revoke=token&revoke=token_refresh", {
        method: "POST",
        credentials: "include",
        headers: { authorization:  x }
    });
    
    window.location = "/smart-bff/logout";
});

document.getElementById('api-test').addEventListener('click', async _ => {
    const resp = await fetch("/api/test", {
        method: "GET",
        headers: new Headers({
            'X-CSRF': '1'
        })
    });
    
    var json = await resp.json();

    document.getElementById("apiValue").innerText = JSON.stringify(json, null, 4);
    
    const modal = document.getElementById("apiModal");
    const myModal = new bootstrap.Modal(modal);
    myModal.toggle();
});