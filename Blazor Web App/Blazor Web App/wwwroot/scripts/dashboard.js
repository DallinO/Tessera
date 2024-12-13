//document.addEventListener('DOMContentLoaded', function () {
//    const container = document.getElementById('tsDash');

//    // Initialize SortableJS on the parent container (the grid or list)
//    new Sortable(container, {
//        animation: 150, // Smooth animation while dragging
//        handle: '.ts-dash-section-title', // Dragging only allowed by the title part
//        onEnd(evt) {
//            // Get the moved section and its new position
//            const movedSection = evt.item; // The dragged section element
//            const newIndex = evt.newIndex; // The new index of the dragged item

//            // Rearranging the elements in the DOM
//            const allSections = Array.from(container.children); // All section elements
//            container.innerHTML = ''; // Clear the container
//            allSections.splice(newIndex, 0, movedSection); // Re-insert the moved section at the new index

//            // Rebuild the DOM with the new order
//            allSections.forEach(section => container.appendChild(section));
//        }
//    });
//});
