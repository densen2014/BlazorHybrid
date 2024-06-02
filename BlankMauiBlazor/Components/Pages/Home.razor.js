export function invokeApi(method, callback, wrapper) {
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
 