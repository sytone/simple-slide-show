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

/// <summary>
/// Pixelate: Pixelated fade effect using CSS filter animation
/// Config options:
///   - Duration: transition duration in seconds (default: 1.0)
///   - MaxPixelSize: maximum pixel size at peak of transition (default: 20)
/// </summary>
Transitions.register('pixelate', function(currentImg, nextImg, config, onComplete) {
    console.log('[Transition:pixelate] Received config:', JSON.stringify(config));
    const duration = config.Duration || config.duration || 1.0;
    const maxPixelSize = config.MaxPixelSize || config.maxPixelSize || 20;
    const durationMs = duration * 1000;
    const halfDuration = durationMs / 2;
    
    console.log('[Transition:pixelate] Starting pixelate transition with duration:', duration, 'maxPixelSize:', maxPixelSize);
    
    // Set initial state
    currentImg.style.transition = 'none';
    nextImg.style.transition = 'none';
    currentImg.style.filter = 'blur(0px)';
    nextImg.style.filter = 'blur(0px)';
    nextImg.style.opacity = '0';
    
    // Force reflow
    void nextImg.offsetWidth;
    
    // Create pixelation effect using a custom animation approach
    let startTime = null;
    
    function animate(timestamp) {
        if (!startTime) startTime = timestamp;
        const elapsed = timestamp - startTime;
        const progress = Math.min(elapsed / durationMs, 1);
        
        if (progress < 0.5) {
            // First half: pixelate and fade out current image
            const halfProgress = progress * 2; // 0 to 1 over first half
            const pixelSize = Math.floor(halfProgress * maxPixelSize);
            const blur = halfProgress * (maxPixelSize / 2);
            currentImg.style.filter = `blur(${blur}px)`;
            currentImg.style.opacity = 1 - halfProgress;
        } else {
            // Second half: de-pixelate and fade in next image
            const halfProgress = (progress - 0.5) * 2; // 0 to 1 over second half
            const pixelSize = Math.floor((1 - halfProgress) * maxPixelSize);
            const blur = (1 - halfProgress) * (maxPixelSize / 2);
            currentImg.style.opacity = '0';
            currentImg.style.filter = 'blur(0px)';
            nextImg.style.opacity = halfProgress;
            nextImg.style.filter = `blur(${blur}px)`;
        }
        
        if (progress < 1) {
            requestAnimationFrame(animate);
        } else {
            // Clean up
            currentImg.style.filter = 'none';
            nextImg.style.filter = 'none';
            nextImg.style.opacity = '1';
            console.log('[Transition:pixelate] Pixelate complete, calling onComplete');
            if (onComplete) {
                onComplete();
            }
        }
    }
    
    requestAnimationFrame(animate);
});

console.log('[Transitions.js] Pixelate transition registered. Registry keys:', window.transitionRegistry ? Object.keys(window.transitionRegistry) : 'undefined');

console.log('[Transitions.js] Fade transition registered. Registry:', window.transitionRegistry);
