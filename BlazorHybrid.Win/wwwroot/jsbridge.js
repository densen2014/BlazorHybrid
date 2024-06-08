function alert(message) {
    console.info(message);

    if (bridge != null) {
        //调用C#方法
        bridge.Alert(message);
    }
} 

var oldPrintFunction = window.print;
var oldConsoleErrorFunction = console.error;

window.print = function (e) {
    console.log('Gonna do some special stuff');
    if (bridge != null) {
        //调用C#方法
        bridge.PrintDemo('打印对象示例: '+e);
    } else {
        oldPrintFunction();
    }
};
console.error = function (e) {
    if (bridge != null) {
        //调用C#方法
        bridge.Alert(e);
    } else {
        oldConsoleErrorFunction(e);
    }
};

function beforePrint() {
    console.log('Do something special before print');
}

function afterPrint() {
    console.log('Do something after print');
}

if (window.matchMedia) {
    window.matchMedia('print').addListener(function (mql) {
        if (mql.matches) {
            beforePrint();
        }
        else {
            afterPrint();
        }
    });
}

// For IE, does not attach in browsers that do not support these events
window.addEventListener('beforeprint', beforePrint, false);
window.addEventListener('afterprint', afterPrint, false);