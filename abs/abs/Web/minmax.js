let req = new XMLHttpRequest();
req.open('GET', '/login/weight');

req.onload = req => {
    let data = JSON.parse(req.target.response);
    let min = 9999999999;
    let max = 0;
    for (let i = 0; i < data.value.length; i++) {
        let val = data.value[i];
        if (val > max) max = val;
        if (val < min) min = val;
    }
    document.getElementById('min').innerText = min;
    document.getElementById('max').innerText = max;
};

req.send();