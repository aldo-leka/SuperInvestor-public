import * as highlighting from './highlighting';
import * as notesLayout from './notesLayout';
import * as resizing from './resizing';
import * as autoExpand from './autoExpand'

// Expose the functions you want to call from Blazor
window.highlighting = highlighting;
window.notesLayout = notesLayout;
window.resizing = resizing;
window.autoExpand = autoExpand;

window.showToast = function (duration) {
    var toastElement = document.getElementById('liveToast');
    var toast = new bootstrap.Toast(toastElement, {
        delay: duration
    });
    toast.show();
}

window.isSmallScreen = function () {
    return window.innerWidth < 992;
}

window.getWindowHeight = function () {
    return window.innerHeight;
}

window.addResizeListener = function (dotNetHelper) {
    window.addEventListener('resize', function () {
        dotNetHelper.invokeMethodAsync('OnWindowResize');
    });
}

window.removeResizeListener = function (dotNetHelper) {
    window.removeEventListener('resize', function () {
        dotNetHelper.invokeMethodAsync('OnWindowResize');
    });
}

// Store original font sizes
let originalFontSizes = [];

window.toggleFilingFontSize = function (isLargeFont) {
    const contentDiv = document.querySelector('#content');
    if (contentDiv) {
        const scaleFactor = isLargeFont ? 1.5 : 1;
        contentDiv.style.setProperty('--font-scale', scaleFactor);
        
        const elements = contentDiv.querySelectorAll('[style*="font-size"]');
        elements.forEach((element, index) => {
            if (originalFontSizes.length <= index) {
                // Store original font size if not already stored
                originalFontSizes.push(element.style.fontSize);
            }
            element.style.fontSize = `calc(${scaleFactor} * ${originalFontSizes[index]})`;
        });
    }
    // Save the preference to localStorage
    localStorage.setItem('filingLargeFontSize', isLargeFont);
};

window.getFilingFontSizePreference = function () {
    return localStorage.getItem('filingLargeFontSize') === 'true';
};

window.applyFilingFontSize = function () {
    const isLargeFont = window.getFilingFontSizePreference();
    if (isLargeFont) {
        window.toggleFilingFontSize(true);
    } else {
        // Reset to original size
        const contentDiv = document.querySelector('#content');
        if (contentDiv) {
            contentDiv.style.removeProperty('--font-scale');
            const elements = contentDiv.querySelectorAll('[style*="font-size"]');
            elements.forEach((element, index) => {
                if (index < originalFontSizes.length) {
                    element.style.fontSize = originalFontSizes[index];
                }
            });
        }
    }
    return isLargeFont;
};

// Function to initialize original font sizes
window.initializeFilingFontSizes = function () {
    const contentDiv = document.querySelector('#content');
    if (contentDiv) {
        const elements = contentDiv.querySelectorAll('[style*="font-size"]');
        originalFontSizes = Array.from(elements).map(element => element.style.fontSize);
    }
};