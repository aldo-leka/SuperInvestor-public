import rangy from 'rangy';
import 'rangy/lib/rangy-serializer';
import Mark from 'mark.js/dist/mark.es6.min.js';
import debounce from 'lodash/debounce';

let content;
let mouseUpListener = null;
let selectionIndices = null;
let marker;

// Global variable to store highlight data
let storedHighlights = [];

export function initialize(dotNetHelper) {
    rangy.init();

    content = document.getElementById("content");
    marker = new Mark(content);
    
    initializeSelectionHandling(dotNetHelper);
    initializeHighlightPositioning(content);

    const contentCard = document.getElementById('content-card');
    const contentElement = contentCard.querySelector('#content');

    // Handle mobile selection
    contentElement.addEventListener('touchend', (event) => {
        const selection = window.getSelection();
        if (!selection.isCollapsed) {
            handleContentSelection(dotNetHelper);
        }
    });

    document.querySelector('#content').addEventListener('click', function(e) {
        if (e.target.classList.contains('highlighted-note')) {
            const noteId = e.target.getAttribute('data-note-id');
            
            // Scroll the highlight to the top of the screen
            e.target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            
            // Add a small delay to ensure the scroll completes before opening the menu
            setTimeout(() => {
                if (window.innerWidth < 1015) {
                    dotNetHelper.invokeMethodAsync('OpenMobileNoteMenu', noteId);
                }
            }, 300); // 300ms delay, adjust if needed
        }
    });
}

export function initializeSelectionHandling(dotNetHelper) {
    const contentCard = document.getElementById('content-card');
    const tickerAutocomplete = document.getElementById('ticker-autocomplete');

    mouseUpListener = (event) => {
        const notesArea = document.getElementById('notes-area');
        const addNoteCard = document.querySelector('.add-note-card');
        
        if (contentCard.contains(event.target)) {
            handleContentSelection(dotNetHelper);
        } else if (addNoteCard && isClickInsideElement(event, addNoteCard)) {
            highlightSelection();
        } else if ((notesArea && !isClickInsideElement(event, notesArea)) && !isClickInsideElement(event, tickerAutocomplete)) {
            clearSelectionAndNotify(dotNetHelper);
        }
    };

    document.addEventListener('mouseup', mouseUpListener);

    // Add touch event handling for mobile
    contentCard.addEventListener('touchend', (event) => {
        const selection = window.getSelection();
        if (!selection.isCollapsed) {
            const range = selection.getRangeAt(0);
            const preSelectionRange = range.cloneRange();
            preSelectionRange.selectNodeContents(content);
            preSelectionRange.setEnd(range.startContainer, range.startOffset);
            const start = preSelectionRange.toString().length;
            const selectedText = range.toString();
            dotNetHelper.invokeMethodAsync('MobileTextSelected', selectedText, start, start + selectedText.length);
        }
    });
}

function isClickInsideElement(event, element) {
    return element.contains(event.target);
}

function clearSelectionAndNotify(dotNetHelper) {
    window.getSelection().removeAllRanges();
    selectionIndices = null;
    dotNetHelper.invokeMethodAsync('TextUnselected');
}

export function handleContentSelection(dotNetHelper) {
    const selection = window.getSelection();
    if (!selection.isCollapsed) {
        const range = selection.getRangeAt(0);
        const preSelectionRange = range.cloneRange();
        preSelectionRange.selectNodeContents(content);
        preSelectionRange.setEnd(range.startContainer, range.startOffset);
        const start = preSelectionRange.toString().length;
        selectionIndices = { startIndex: start, endIndex: start + range.toString().length };
        const selectedText = range.toString();
        dotNetHelper.invokeMethodAsync('TextSelected', selectedText, start, start + selectedText.length);
    }
}

export function highlightSelection() {
    if (selectionIndices) {
        marker.markRanges([{
            start: selectionIndices.startIndex,
            length: selectionIndices.endIndex - selectionIndices.startIndex
        }], {
            className: 'selected-text',
            acrossElements: true,
            element: 'span'
        });

        selectionIndices = null;
    }
}

export function highlightNote(noteShortId, startIndex, endIndex) {
    // Store only the indices
    storedHighlights.push({ noteShortId, startIndex, endIndex });

    // Apply the highlight
    marker.markRanges([{
        start: startIndex,
        length: endIndex - startIndex
    }], {
        className: 'highlighted-note',
        acrossElements: true,
        element: 'span',
        each: (element) => {
            element.setAttribute('data-note-id', noteShortId);
        }
    });
}

export function clearNoteHighlight(noteShortId) {
    // Remove the highlight from storedHighlights
    storedHighlights = storedHighlights.filter(highlight => highlight.noteShortId !== noteShortId);

    // Clear all highlights
    marker.unmark();

    // Reapply all highlights except the removed one
    storedHighlights.forEach(highlight => {
        marker.markRanges([{
            start: highlight.startIndex,
            length: highlight.endIndex - highlight.startIndex
        }], {
            className: 'highlighted-note',
            acrossElements: true,
            element: 'span',
            each: (element) => {
                element.setAttribute('data-note-id', highlight.noteShortId);
            }
        });
    });
}

export function applyHoverHighlight(noteShortId, startIndex, endIndex) {
    marker.markRanges([{
        start: startIndex,
        length: endIndex - startIndex
    }], {
        className: 'hover-highlighted-note',
        acrossElements: true,
        element: 'span',
        each: (element) => {
            element.setAttribute('data-note-id', noteShortId);
        }
    });
}

export function clearHoverHighlight(noteShortId) {
    marker.unmark({
        className: 'hover-highlighted-note',
        element: 'span',
        filter: (node) => node.getAttribute('data-note-id') === noteShortId
    });
}

export function clearSelectedTextHighlight() {
    marker.unmark({
        className: 'selected-text'
    });
}

export function scrollToNote(noteShortId, scrollPosition) {
    const noteElement = document.querySelector(`[data-note-id="${noteShortId}"]`);
    if (noteElement) {
        noteElement.scrollIntoView({ behavior: 'smooth', block: scrollPosition });
    }
}

let highlightPositionCache = new Map();
let contentResizeObserver;

function initializeHighlightPositioning(contentElement) {
    contentResizeObserver = new ResizeObserver(debounce(() => {
        clearHighlightPositionCache();
    }, 250));
    
    contentResizeObserver.observe(contentElement);
}

export function getHighlightPosition(startIndex, endIndex) {
    const cacheKey = `${startIndex}-${endIndex}`;
    if (highlightPositionCache.has(cacheKey)) {
        return highlightPositionCache.get(cacheKey);
    }
    
    const range = document.createRange();
    try {
        const startNode = findNodeAtIndex(content, startIndex);
        const endNode = findNodeAtIndex(content, endIndex);
        
        if (startNode && endNode) {
            range.setStart(startNode.node, startNode.offset);
            range.setEnd(endNode.node, endNode.offset);
            const rect = range.getBoundingClientRect();
            const contentRect = content.getBoundingClientRect();
            
            // Get the scroll positions of both content and notes areas
            const contentScrollTop = content.scrollTop;
            const notesContainer = document.getElementById('notes-container');
            const notesScrollTop = notesContainer ? notesContainer.scrollTop : 0;
            
            // Calculate the position relative to the content area
            const position = rect.top - contentRect.top + contentScrollTop;
            
            // Adjust the position based on the notes area scroll
            const adjustedPosition = position - notesScrollTop;
            
            highlightPositionCache.set(cacheKey, adjustedPosition);
            return adjustedPosition;
        }
    } catch (error) {
        console.error("Error calculating highlight position:", error);
    } finally {
        range.detach();
    }
    
    return 0;
}

export const debouncedGetHighlightPosition = getHighlightPosition; // debounce(getHighlightPosition, 500);

export function getCurrentSelectionPosition() {
    if (selectionIndices) {
        return debouncedGetHighlightPosition(selectionIndices.startIndex, selectionIndices.endIndex);
    }
    return 0;
}

function clearHighlightPositionCache() {
    highlightPositionCache.clear();
}

function cleanupHighlightPositioning() {
    if (contentResizeObserver) {
        contentResizeObserver.disconnect();
    }
}

function findNodeAtIndex(root, targetIndex) {
    let currentIndex = 0;
    
    function traverse(node) {
        if (node.nodeType === Node.TEXT_NODE) {
            if (currentIndex + node.length > targetIndex) {
                return { node: node, offset: targetIndex - currentIndex };
            }
            currentIndex += node.length;
        } else {
            for (let child of node.childNodes) {
                const result = traverse(child);
                if (result) return result;
            }
        }
        return null;
    }
    
    return traverse(root);
}

export function dispose() {
    if (mouseUpListener) {
        document.removeEventListener('mouseup', mouseUpListener);
        mouseUpListener = null;
    }

    cleanupHighlightPositioning();
    clearAllHighlights();
}

// Add this function to clear all highlights (if needed)
function clearAllHighlights() {
    storedHighlights = [];
    marker.unmark();
}