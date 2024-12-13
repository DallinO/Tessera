//let idleTime = 0;
//const maxIdleTime = 10; // Max idle time in seconds (e.g., 10 seconds for testing)

//function resetIdleTimer() {
//    idleTime = 0; // Reset the idle timer on user interaction
//    window.DotNet.invokeMethodAsync('Tessera.Web', 'ResetInactivityTimer');
//}

//// Increment idle time every second
//setInterval(() => {
//    idleTime++;
//    if (idleTime > maxIdleTime) {
//        // Inactivity detected, call Blazor method to handle inactivity
//        window.DotNet.invokeMethodAsync('Tessera.Web', 'OnUserInactive');
//    }
//}, 1000);

//// Add event listeners for user interaction
//function addEventListeners() {
//    window.addEventListener('mousemove', resetIdleTimer);
//    window.addEventListener('keypress', resetIdleTimer);
//    window.addEventListener('click', resetIdleTimer);
//}

//function removeEventListeners() {
//    window.removeEventListener('mousemove', resetIdleTimer);
//    window.removeEventListener('keypress', resetIdleTimer);
//    window.removeEventListener('click', resetIdleTimer);
//}

//// Expose functions to Blazor
//window.inactivityFunctions = {
//    addEventListeners,
//    removeEventListeners
//};
