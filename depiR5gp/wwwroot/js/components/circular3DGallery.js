/**
 * Framework-free Vanilla JS Circular 3D Drum Gallery
 * Uses Pointer Events, direct DOM style transform writes, and requestAnimationFrame.
 * Attached to DOM via Alpine x-init="initGallery3D($el, options)"
 */

window.initGallery3D = function (container, options = {}) {
  if (!container) return;

  const ring = container.querySelector('[data-gallery-ring]');
  if (!ring) return;

  const cards = Array.from(ring.querySelectorAll('[data-gallery-card]'));
  if (cards.length === 0) return;

  const cardCount = cards.length;
  const anglePerCard = 360 / Math.max(1, cardCount);

  let initialIndex = options.initialIndex || 0;
  let targetAngle = -initialIndex * anglePerCard;
  let currentAngle = targetAngle;
  let isDragging = false;
  let startX = 0;
  let startAngle = 0;
  let ringRadius = 580;
  let rafId = null;

  function updateRadius() {
    const width = container.clientWidth;
    if (width < 640) {
      ringRadius = Math.max(260, Math.round(width * 0.55));
    } else if (width < 1024) {
      ringRadius = Math.max(420, Math.round(width * 0.52));
    } else {
      ringRadius = Math.min(720, Math.max(520, Math.round(width * 0.46)));
    }
  }

  function renderTransforms() {
    ring.style.transform = `rotateY(${currentAngle}deg)`;

    cards.forEach((el, i) => {
      const baseAngle = i * anglePerCard;
      let totalAngle = (baseAngle + currentAngle) % 360;
      if (totalAngle > 180) totalAngle -= 360;
      if (totalAngle < -180) totalAngle += 360;

      const absAngle = Math.abs(totalAngle);
      el.style.transform = `rotateY(${baseAngle}deg) translateZ(${ringRadius}px)`;

      if (absAngle < 85) {
        const factor = 1 - absAngle / 85;
        const scale = 0.84 + factor * 0.16;
        const opacity = 0.48 + factor * 0.52;
        const blur = (1 - factor) * 3.5;
        const brightness = 0.68 + factor * 0.32;

        el.style.opacity = opacity.toFixed(2);
        el.style.filter = `blur(${blur.toFixed(1)}px) brightness(${brightness.toFixed(2)}) scale(${scale.toFixed(3)})`;
        el.style.pointerEvents = 'auto';
        el.style.visibility = 'visible';
      } else if (absAngle < 125) {
        el.style.opacity = '0.12';
        el.style.filter = 'blur(6px) brightness(0.4) scale(0.78)';
        el.style.pointerEvents = 'none';
        el.style.visibility = 'visible';
      } else {
        el.style.opacity = '0';
        el.style.filter = 'blur(8px)';
        el.style.pointerEvents = 'none';
        el.style.visibility = 'hidden';
      }
    });
  }

  function animationLoop() {
    const diff = targetAngle - currentAngle;
    if (Math.abs(diff) > 0.05) {
      currentAngle += diff * 0.12;
      renderTransforms();
    } else if (currentAngle !== targetAngle) {
      currentAngle = targetAngle;
      renderTransforms();
    }
    rafId = requestAnimationFrame(animationLoop);
  }

  function onPointerDown(e) {
    isDragging = true;
    startX = e.clientX;
    startAngle = currentAngle;
    container.setPointerCapture(e.pointerId);
    container.style.cursor = 'grabbing';
  }

  function onPointerMove(e) {
    if (!isDragging) return;
    const deltaX = e.clientX - startX;
    const sensitivity = 0.32;
    currentAngle = startAngle + deltaX * sensitivity;
    targetAngle = currentAngle;
    renderTransforms();
  }

  function onPointerUp(e) {
    if (!isDragging) return;
    isDragging = false;
    container.releasePointerCapture(e.pointerId);
    container.style.cursor = '';

    // Snap to nearest card
    const normalized = -currentAngle / anglePerCard;
    const nearestIndex = Math.round(normalized);
    targetAngle = -nearestIndex * anglePerCard;

    if (options.onIndexChange) {
      const activeIdx = ((nearestIndex % cardCount) + cardCount) % cardCount;
      options.onIndexChange(activeIdx);
    }
  }

  // Prev / Next button bindings
  const prevBtn = container.querySelector('[data-gallery-prev]');
  const nextBtn = container.querySelector('[data-gallery-next]');

  function goNext() {
    targetAngle -= anglePerCard;
  }

  function goPrev() {
    targetAngle += anglePerCard;
  }

  if (prevBtn) prevBtn.addEventListener('click', goPrev);
  if (nextBtn) nextBtn.addEventListener('click', goNext);

  window.addEventListener('resize', updateRadius);
  container.addEventListener('pointerdown', onPointerDown);
  container.addEventListener('pointermove', onPointerMove);
  container.addEventListener('pointerup', onPointerUp);
  container.addEventListener('pointercancel', onPointerUp);

  updateRadius();
  renderTransforms();
  rafId = requestAnimationFrame(animationLoop);

  return {
    destroy() {
      cancelAnimationFrame(rafId);
      window.removeEventListener('resize', updateRadius);
      container.removeEventListener('pointerdown', onPointerDown);
      container.removeEventListener('pointermove', onPointerMove);
      container.removeEventListener('pointerup', onPointerUp);
      container.removeEventListener('pointercancel', onPointerUp);
      if (prevBtn) prevBtn.removeEventListener('click', goPrev);
      if (nextBtn) nextBtn.removeEventListener('click', goNext);
    },
    next: goNext,
    prev: goPrev,
  };
};

