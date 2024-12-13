function getElementRect(element) {
    return element.getBoundingClientRect();
}

function toggleButtonColor(buttonId) {
    // Deselect all buttons
    const allButtons = document.querySelectorAll('.ts-basic-button');
    allButtons.forEach(button => {
        button.classList.remove('clicked');
    });

    // Select the clicked button
    const button = document.getElementById(buttonId);
    if (button) {
        button.classList.add('clicked');
    }
}

window.getMouseX = function () {
    return new Promise(resolve => {
        document.addEventListener('click', function (event) {
            resolve(event.clientX);
        }, { once: true });
    });
};

window.getMouseY = function () {
    return new Promise(resolve => {
        document.addEventListener('click', function (event) {
            resolve(event.clientY);
        }, { once: true });
    });
};

function requestNotificationPermission() {
    if ("Notification" in window) {
        Notification.requestPermission().then(permission => {
            if (permission === "granted") {
                console.log("Notification permission granted.");
            } else {
                console.log("Notification permission denied.");
            }
        });
    } else {
        console.log("Notifications not supported in this browser.");
    }
}

function showNotification(taskTitle, taskDueTime) {
    if (Notification.permission === "granted") {
        new Notification("Task Due", {
            body: `${taskTitle} is due at ${taskDueTime}.`,
            icon: "/path/to/icon.png" // Optional
        });
    } else {
        console.log("Notification permission not granted.");
    }
}

function scheduleTaskNotification(taskTitle, dueDateTime) {
    const now = new Date();
    const dueTime = new Date(dueDateTime);

    const delay = dueTime - now;

    if (delay > 0) {
        setTimeout(() => {
            showNotification(taskTitle, dueTime.toLocaleTimeString());
        }, delay);
    } else {
        console.log("Due time is in the past. Cannot schedule notification.");
    }
}