var resizeListener = null;

export function initialize(dotNetHelper) {
    resizeListener = function () {
        dotNetHelper.invokeMethodAsync('OnWindowResize');
    };

    window.addEventListener('resize', resizeListener);
    dotNetHelper.invokeMethodAsync('OnWindowResize');
}

export function dispose() {
    if (resizeListener) {
        window.removeEventListener('resize', resizeListener);
        resizeListener = null;
    }
}