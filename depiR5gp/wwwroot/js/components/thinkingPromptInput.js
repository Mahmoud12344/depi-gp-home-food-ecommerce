/**
 * Framework-free Vanilla JS Thinking Prompt Input Component
 * Simulates streaming reasoning progress, timed step reveals, and accordion transitions.
 * Attached to DOM via Alpine x-init="initThinkingPrompt($el, options)"
 */

window.initThinkingPrompt = function (container, options = {}) {
  if (!container) return;

  let timerId = null;
  let activeStepIndex = 0;
  const steps = options.steps || [];

  const stepsContainer = container.querySelector('[data-thinking-steps]');
  const progressBadge = container.querySelector('[data-thinking-badge]');

  function revealNextStep() {
    if (!stepsContainer || activeStepIndex >= steps.length) {
      if (options.onComplete) options.onComplete();
      return;
    }

    const stepItem = stepsContainer.querySelector(`[data-step-index="${activeStepIndex}"]`);
    if (stepItem) {
      stepItem.classList.remove('opacity-40');
      stepItem.classList.add('opacity-100', 'text-[var(--color-primary)]');
      const icon = stepItem.querySelector('[data-step-icon]');
      if (icon) {
        icon.innerHTML = '<svg class="w-3.5 h-3.5 text-emerald-500 shrink-0"><use href="/icons/sprite.svg#check"></use></svg>';
      }
    }

    activeStepIndex++;
    if (progressBadge) {
      progressBadge.textContent = `${activeStepIndex}/${steps.length} steps complete`;
    }

    if (activeStepIndex < steps.length) {
      timerId = setTimeout(revealNextStep, options.stepDelay || 850);
    } else if (options.onComplete) {
      options.onComplete();
    }
  }

  function start() {
    activeStepIndex = 0;
    revealNextStep();
  }

  function stop() {
    if (timerId) clearTimeout(timerId);
  }

  if (options.autoStart) {
    start();
  }

  return {
    start,
    stop,
    destroy() {
      stop();
    },
  };
};

