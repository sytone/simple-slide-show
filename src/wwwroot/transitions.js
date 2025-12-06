/// <summary>
/// Transition registry for slideshow effects.
/// Each transition is a function that receives (currentImg, nextImg, config, onComplete)
/// - currentImg: the currently visible image element
/// - nextImg: the next image element (already loaded, currently hidden)
/// - config: object with transition-specific configuration
/// - onComplete: callback to invoke when transition completes
/// The transition should animate from current to next and call onComplete when done.
/// </summary>

console.log('[Transitions.js] Loading transitions.js file');

const Transitions = {
    /// <summary>
    /// Registers a new transition effect.
    /// </summary>
    /// <param name="name">Unique name for the transition</param>
    /// <param name="transitionFn">Function(currentImg, nextImg, config, onComplete) that executes the transition</param>
    register: function(name, transitionFn) {
        console.log('[Transitions.js] Registering transition:', name);
        if (!window.transitionRegistry) {
            console.log('[Transitions.js] Creating window.transitionRegistry');
            window.transitionRegistry = {};
        }
        window.transitionRegistry[name] = transitionFn;
        console.log('[Transitions.js] Registered. Current registry keys:', Object.keys(window.transitionRegistry));
    },

    /// <summary>
    /// Gets all registered transition names.
    /// </summary>
    getAvailable: function() {
        return window.transitionRegistry ? Object.keys(window.transitionRegistry) : [];
    }
};

// ============================================================================
// Built-in Transitions
// ============================================================================

/// <summary>
/// Fade: Cross-fade between current and next image
/// Config options:
///   - Duration: fade duration in seconds (default: 0.6)
/// </summary>
Transitions.register('fade', function(currentImg, nextImg, config, onComplete) {
    console.log('[Transition:fade] Received config:', JSON.stringify(config));
    const duration = config.Duration || config.duration || 0.6;
    const durationMs = duration * 1000;
    const durationCss = duration + 's';
    
    console.log('[Transition:fade] Starting fade transition with duration:', duration);
    
    // Both images are already loaded
    // Set initial state
    currentImg.style.transition = `opacity ${durationCss} ease-in-out`;
    nextImg.style.transition = `opacity ${durationCss} ease-in-out`;
    nextImg.style.opacity = '0';
    
    console.log('[Transition:fade] Initial state set, current opacity:', currentImg.style.opacity, 'next opacity:', nextImg.style.opacity);
    
    // Force reflow
    void nextImg.offsetWidth;
    
    // Trigger cross-fade
    requestAnimationFrame(() => {
        console.log('[Transition:fade] Triggering cross-fade animation');
        currentImg.style.opacity = '0';
        nextImg.style.opacity = '1';
    });
    
    if (onComplete) {
        setTimeout(() => {
            console.log('[Transition:fade] Fade complete, calling onComplete');
            onComplete();
        }, durationMs);
    }
});

console.log('[Transitions.js] Fade transition registered. Registry keys:', window.transitionRegistry ? Object.keys(window.transitionRegistry) : 'undefined');

console.log('[Transitions.js] Fade transition registered. Registry:', window.transitionRegistry);
