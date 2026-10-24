/**
 * Framework-free Vanilla JS Vertical Draggable Image Stack
 * Uses Pointer Events, wheel navigation, and direct DOM style transform writes.
 * Attached to DOM via Alpine x-init="initVerticalStack($el, options)"
 */

window.initVerticalStack = function (container, options = {}) {
  if (!container) return;

  const cardsContainer = container.querySelector('[data-stack-cards]');
  if (!cardsContainer) return;

  const cardEls = Array.from(cardsContainer.querySelectorAll('[data-stack-card]'));
  if (cardEls.length === 0) return;

  const total = cardEls.length;
  let currentIndex = options.currentIndex || 0;
  let startY = 0;
  let isDragging = false;
  let lastNavTime = 0;
  const cooldown = 350;

  const isVertical = options.aspectRatio === 'vertical';
  const stepY = options.stepOffset || (isVertical ? 140 : 115);
  const step2Y = Math.round(stepY * 1.82);

  function getCardStyle(index) {
    let diff = index - currentIndex;
    if (diff > total / 2) diff -= total;
    if (diff < -total / 2) diff += total;

    if (diff === 0) {
      return { y: 0, scale: 1, opacity: 1, zIndex: 5, rotateX: 0, visible: true };
    } else if (diff === -1) {
      return { y: -stepY, scale: 0.86, opacity: 0.65, zIndex: 4, rotateX: 7, visible: true };
    } else if (diff === -2) {
      return { y: -step2Y, scale: 0.74, opacity: 0.35, zIndex: 3, rotateX: 13, visible: true };
    } else if (diff === 1) {
      return { y: stepY, scale: 0.86, opacity: 0.65, zIndex: 4, rotateX: -7, visible: true };
    } else if (diff === 2) {
      return { y: step2Y, scale: 0.74, opacity: 0.35, zIndex: 3, rotateX: -13, visible: true };
    } else {
      return {
        y: diff > 0 ? (isVertical ? 360 : 300) : (isVertical ? -360 : -300),
        scale: 0.62,
        opacity: 0,
        zIndex: 0,
        rotateX: diff > 0 ? -16 : 16,
        visible: false,
      };
    }
  }

  function renderStack(animate = true) {
    cardEls.forEach((card, i) => {
      const s = getCardStyle(i);
      card.style.transition = animate ? 'transform 0.4s cubic-bezier(0.22, 1, 0.36, 1), opacity 0.4s ease' : 'none';
      card.style.transform = `translate3d(0, ${s.y}px, 0) scale(${s.scale}) rotateX(${s.rotateX}deg)`;
      card.style.opacity = s.opacity;
      card.style.zIndex = s.zIndex;
      card.style.visibility = s.visible ? 'visible' : 'hidden';
      card.style.pointerEvents = s.zIndex === 5 ? 'auto' : 'none';
    });

    if (options.onIndexChange) {
      options.onIndexChange(currentIndex);
    }
  }

  function navigate(dir) {
    const now = Date.now();
    if (now - lastNavTime < cooldown) return;
    lastNavTime = now;

    if (dir > 0) {
      currentIndex = currentIndex === total - 1 ? 0 : currentIndex + 1;
    } else {
      currentIndex = currentIndex === 0 ? total - 1 : currentIndex - 1;
    }
    renderStack(true);
  }

  function onPointerDown(e) {
    isDragging = true;
    startY = e.clientY;
    container.setPointerCapture(e.pointerId);
  }

  function onPointerUp(e) {
    if (!isDragging) return;
    isDragging = false;
    container.releasePointerCapture(e.pointerId);

    const deltaY = e.clientY - startY;
    const threshold = 40;
    if (deltaY < -threshold) {
      navigate(1);
    } else if (deltaY > threshold) {
      navigate(-1);
    }
  }

  function onWheel(e) {
    if (Math.abs(e.deltaY) > 25) {
      if (e.deltaY > 0) {
        navigate(1);
      } else {
        navigate(-1);
      }
    }
  }

  // Prev / Next button bindings
  const prevBtn = container.querySelector('[data-stack-prev]');
  const nextBtn = container.querySelector('[data-stack-next]');

  if (prevBtn) prevBtn.addEventListener('click', () => navigate(-1));
  if (nextBtn) nextBtn.addEventListener('click', () => navigate(1));

  container.addEventListener('pointerdown', onPointerDown);
  container.addEventListener('pointerup', onPointerUp);
  container.addEventListener('pointercancel', onPointerUp);
  container.addEventListener('wheel', onWheel, { passive: true });

  renderStack(false);

  return {
    destroy() {
      container.removeEventListener('pointerdown', onPointerDown);
      container.removeEventListener('pointerup', onPointerUp);
      container.removeEventListener('pointercancel', onPointerUp);
      container.removeEventListener('wheel', onWheel);
    },
    next: () => navigate(1),
    prev: () => navigate(-1),
  };
};

