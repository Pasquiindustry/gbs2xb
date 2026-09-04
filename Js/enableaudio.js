// Check if the emulator exists (emulator is provided by Binjgb) and its audio engine is enabled
if (typeof emulator !== 'undefined' && emulator.audio) {

    // Forse the audio output to start
    emulator.audio.startPlayback();

    // Tell GB2XB that the playback is now running
    true;
}
else {
    // Tell GB2XB that the playback is still loading
    false;
}