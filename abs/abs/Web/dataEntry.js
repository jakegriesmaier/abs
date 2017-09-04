class dataEntry {
    constructor(parentElement, url, sizeX, sizeY) {
        this.ele = document.getElementById('dataEntry');

        this.canvas = document.createElement('canvas');
        this.canvas.width = sizeX;
        this.canvas.height = sizeY;
        this.ctx = this.canvas.getContext('2d');
        parentElement.appendChild(this.canvas);
        
        this.sizeX = sizeX;
        this.sizeY = sizeY;

        this.x0 = 0;
        this.x1 = 100;
        this.y0 = 0;
        this.y1 = 200;

        this.dates = [];
        this.values = [];

        this.url = url;

        console.log(this.canvas);
    }

    drawLine(startX, startY, endX, endY) {
        let self = this;
        let transformX = function (x) {
            return (x - self.x0) / (self.x1 - self.x0) * self.sizeX;
        };
        let transformY = function (y) {
            return (y - self.y0) / (self.y1 - self.y0) * self.sizeY;
        };

        this.ctx.beginPath();
        this.ctx.moveTo(transformX(startX), transformY(startY));
        this.ctx.lineTo(transformX(endX), transformY(endY));
        this.ctx.stroke();
    }

    drawGraph(values) {
        this.ctx.clearRect(0, 0, this.sizeX, this.sizeY);

        for (let i = 0; i < values.length - 1; i++) {
            let val = values[i];

            this.drawLine(i, values[i], (i + 1), values[i + 1]);
        }

        this.ctx.fillText("x=" + this.x0, 0, )
    }

    drawData() {
        let self = this;
        let req = new XMLHttpRequest();
        req.open("GET", this.url);
        req.onload = function (res) {
            let rawValues = JSON.parse(res.target.response);
            let values = rawValues.value;
            self.drawGraph(values);
        };
        req.send();
    }
}

let ent = new dataEntry(document.getElementById("dataEntry"), "/userRecordings/weight", 500, 250);

ent.drawData();