// Save the original navigator.getGamepads function provided by the browser
window.originalNavigatorGetGamepadFunction = window.originalNavigatorGetGamepadFunction || navigator.getGamepads;

// Override the navigator.getGamepads function. This is the best way I found to stop any gamepad input from everywhere in a WebView
Object.defineProperty(navigator, 'getGamepads', { value: () => [], configurable: true });

// Create the function that, if attached to some triggers, stops their execution
window.blockInputFunction = window.blockInputFunction || function (eventArgs) {
    eventArgs.stopImmediatePropagation();
    eventArgs.preventDefault();
}

// Add the event to all the event listeners
window.addEventListener('keydown', window.blockInputFunction, true);
window.addEventListener('keyup', window.blockInputFunction, true);
window.addEventListener('keypress', window.blockInputFunction, true);
window.addEventListener('pointerdown', window.blockInputFunction, true);
window.addEventListener('touchstart', window.blockInputFunction, true);