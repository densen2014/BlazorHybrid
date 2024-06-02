function invokeApi(method, callback, wrapper) {
    wrapper.invokeMethodAsync('GetStatus', 'running...');
    if (window._spidereye != null) {
        window._spidereye.invokeApi(method, null,
            e => {
                wrapper.invokeMethodAsync(callback, e);
                console.log('done!', e);
            });
        return "waiting...";
    } else {
        return "This environment is not supported!";
    }
};

// function over riding. Redirecting to Console with Firebug installed.
function alert(message) {
    console.info(message);
} 

var oldPrintFunction = window.print;

window.print = function () {
    console.log('Gonna do some special stuff');
    oldPrintFunction();
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