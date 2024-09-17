import { debouncedGetHighlightPosition, getCurrentSelectionPosition } from './highlighting.js';

export const updateNotesLayout = () => {
    const notesArea = document.getElementById('notes-area');

    if (!notesArea) {
        return;
    }

    const notes = Array.from(notesArea.querySelectorAll('.note-card'));
    const addNoteButton = document.getElementById('add-note-button');
    const addNoteCard = notesArea.querySelector('.add-note-card');
    
    const elements = [
        ...(addNoteCard ? [addNoteCard] : []),
        ...notes,
        ...(addNoteButton ? [addNoteButton] : [])
    ];

    let fixedElements = [];
    let noteElements = [];
    let fixedElementsBottom = 0;

    // First pass: determine positions of fixed elements
    elements.forEach(element => {
        if (element === addNoteCard || element === addNoteButton) {
            const selectionPosition = getCurrentSelectionPosition();
            element.style.top = `${selectionPosition}px`;
            const elementHeight = element.offsetHeight;
            fixedElements.push({ element, top: selectionPosition, height: elementHeight });
            fixedElementsBottom = Math.max(fixedElementsBottom, selectionPosition + elementHeight);
        } else {
            noteElements.push(element);
        }
    });

    // Sort noteElements by their data-start-index
    noteElements.sort((a, b) => {
        const aStartIndex = parseInt(a.getAttribute('data-start-index'));
        const aEndIndex = parseInt(a.getAttribute('data-end-index'));
        const bStartIndex = parseInt(b.getAttribute('data-start-index'));
        const bEndIndex = parseInt(b.getAttribute('data-end-index'));

        const aPosition = debouncedGetHighlightPosition(aStartIndex, aEndIndex);
        const bPosition = debouncedGetHighlightPosition(bStartIndex, bEndIndex);

        return aPosition - bPosition;
    });

    // Second pass: adjust note positions
    let currentTop = 0;
    noteElements.forEach(element => {
        const startIndex = parseInt(element.getAttribute('data-start-index'));
        const endIndex = parseInt(element.getAttribute('data-end-index'));
        const highlightPosition = debouncedGetHighlightPosition(startIndex, endIndex);
        const elementHeight = element.offsetHeight;

        // Check for overlap with fixed elements and previous notes
        let adjustedTop = Math.max(highlightPosition, currentTop);
        fixedElements.forEach(fixedElement => {
            if (adjustedTop < fixedElement.top + fixedElement.height &&
                adjustedTop + elementHeight > fixedElement.top) {
                adjustedTop = fixedElement.top + fixedElement.height + 10; // 10px gap
            }
        });

        // Set the new top position
        element.style.top = `${adjustedTop}px`;
        currentTop = adjustedTop + elementHeight + 10; // 10px gap
    });

    // Ensure fixed elements are on top
    fixedElements.forEach(({ element }) => {
        element.style.zIndex = '1000';
    });
}

export const calculateInitialNoteLength = (noteShortId) => {
    const noteCard = document.querySelector(`[data-note-card-id="${noteShortId}"]`);
    if (!noteCard) {
        console.warn(`Note card with id ${noteShortId} not found`);
        return 168; // fallback to default value
    }

    const cardWidth = noteCard.offsetWidth;
    const tempElement = document.createElement('span');
    tempElement.style.visibility = 'hidden';
    tempElement.style.whiteSpace = 'nowrap';
    tempElement.style.position = 'absolute';
    tempElement.style.font = window.getComputedStyle(noteCard).font;
    noteCard.appendChild(tempElement);

    let text = '';
    let totalLength = 0;
    const averageCharsPerLine = Math.floor(cardWidth / 8); // Assuming average char width of 8px

    // 5 lines of visible text
    for (let i = 0; i < 5; i++) {
        text += 'X'.repeat(averageCharsPerLine);
        tempElement.textContent = text;
        totalLength += averageCharsPerLine;
    }

    noteCard.removeChild(tempElement);
    return totalLength;
};

export function updateMobileNoteMenuScrollState() {
    const menu = document.querySelector('.mobile-note-menu');
    if (menu) {
        menu.classList.toggle('scrollable', menu.scrollHeight > menu.clientHeight);
    }
}

// Call this function whenever the mobile note menu content changes
// For example, after loading notes or adding/removing a note