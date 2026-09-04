// Restore the original navigator.getGamepads
if (window.originalNavigatorGetGamepadFunction) {
    Object.defineProperty(navigator, 'getGamepads', { value: window.originalNavigatorGetGamepadFunction, configurable: true });
}

// Remove the events from all event listeners
if (window.blockInputFunction) {
    window.removeEventListener('keydown', window.blockInputFunction, true);
    window.removeEventListener('keyup', window.blockInputFunction, true);
    window.removeEventListener('keypress', window.blockInputFunction, true);
    window.removeEventListener('pointerdown', window.blockInputFunction, true);
    window.removeEventListener('touchstart', window.blockInputFunction, true);
}