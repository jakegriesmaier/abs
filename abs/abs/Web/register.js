document.getElementById("registerButton").onclick = function () {
    let username = document.getElementById("registerUsername").value;
    let password0 = document.getElementById("registerPassword0").value;
    let password1 = document.getElementById("registerPassword1").value;
    let email = document.getElementById("registerEmail").value;
    if (password0 === password1) {
        let req = new XMLHttpRequest();
        req.open("POST", "/registerTarget");
        req.send(JSON.stringify({ username: username, password: password0, email: email }));
    } else {
        document.getElementById("registerPassword1").style.backgroundColor = "#faa";
    }

    
};