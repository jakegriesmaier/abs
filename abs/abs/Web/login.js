document.getElementById('login').onclick = () => {

    let username = document.getElementById('username').value;
    let password = document.getElementById('password').value;

    let req = new XMLHttpRequest();
    req.open('POST', '/login');
    req.onload = req => {
        document.cookie = "AuthID=" + req.target.response;
        location.reload();
    };
    req.send(username + ':' + password);
};

document.getElementById('logout').onclick = () => {
    document.cookie = "AuthID=";
    location.reload();
};