// ============================================
// Global Site Scripts
// ============================================

// Anti-Forgery Token
function getAntiForgeryToken() {
    var token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
    );

    return token ? token.value : null;
}


// ============================================
// Favorites
// ============================================

async function toggleFavorite(button) {
    if (!button || button.dataset.busy === 'true') return;

    var dishId = button.getAttribute('data-favorite-dish-id');
    var token = getAntiForgeryToken();

    if (!dishId || !token) {
        console.error('Missing dish ID or Anti-Forgery Token');
        return;
    }

    button.dataset.busy = 'true';
    button.disabled = true;

    try {
        var response = await fetch(
            '/Dishes/ToggleFavorite?id=' + encodeURIComponent(dishId),
            {
                method: 'POST',
                body: new URLSearchParams({
                    __RequestVerificationToken: token
                }),
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
                }
            }
        );

        if (
            response.redirected ||
            response.url.includes('/Account/Login')
        ) {
            window.location.href =
                '/Account/Login?returnUrl=' +
                encodeURIComponent(window.location.pathname);

            return;
        }

        var data = await response.json();

        if (!response.ok || !data.success) {
            if (data.loginRequired) {
                window.location.href =
                    '/Account/Login?returnUrl=' +
                    encodeURIComponent(window.location.pathname);

                return;
            }

            throw new Error(
                data.message || 'Unable to update favorites.'
            );
        }

        // Update every button representing the same dish.
        var matchingButtons = document.querySelectorAll(
            '[data-favorite-dish-id="' +
            CSS.escape(String(dishId)) +
            '"]'
        );

        matchingButtons.forEach(function (favoriteButton) {
            var active = data.isFavorite;

            favoriteButton.dataset.favoriteActive =
                active ? 'true' : 'false';

            favoriteButton.setAttribute(
                'aria-pressed',
                String(active)
            );

            favoriteButton.setAttribute(
                'aria-label',
                active
                    ? 'Remove from favorites'
                    : 'Add to favorites'
            );

            var icon = favoriteButton.querySelector('svg');

            if (icon) {
                icon.classList.toggle('fill-rose-500', active);
                icon.classList.toggle('text-rose-500', active);
                icon.classList.toggle('fill-none', !active);
            }

            favoriteButton.classList.toggle('active', active);

            // Update Alpine state if available.
            var scope = favoriteButton.closest('[x-data]');

            if (scope && window.Alpine) {
                try {
                    var alpineData = window.Alpine.$data(scope);

                    if (alpineData && 'favorited' in alpineData) {
                        alpineData.favorited = active;
                    }
                } catch (error) {
                    console.warn(
                        'Could not update Alpine state:',
                        error
                    );
                }
            }
        });

        // Update header favorites counter immediately.
        if (typeof data.count !== 'undefined') {
            window.updateFavoritesCount(data.count);
        } else {
            window.refreshFavoritesCount();
        }

        console.log('Favorite updated successfully:', data);

    } catch (error) {
        console.error('Favorite error:', error);

    } finally {
        delete button.dataset.busy;
        button.disabled = false;
    }
}


// Support both function names used by existing Razor views.
window.toggleFavoriteDish = toggleFavorite;
window.toggleDishFavorite = toggleFavorite;


// ============================================
// Main Site Scripts
// ============================================

(function () {

    var dishCatalog = [];


    // ========================================
    // Normalize Dish Payload
    // ========================================

    function normalizeDishPayload(dish) {
        if (!dish) return dish;

        var match =
            typeof dish.id === 'string' &&
                dish.id.indexOf('dish-') === 0

                ? dishCatalog.find(function (item) {
                    return item.name === dish.name;
                })

                : dishCatalog.find(function (item) {
                    return String(item.id) === String(dish.id);
                });

        if (
            !match &&
            typeof dish.id === 'string' &&
            dish.id.indexOf('dish-') === 0
        ) {
            match = dishCatalog[
                parseInt(dish.id.substring(5), 10) - 1
            ];
        }

        if (!match) return dish;

        return Object.assign({}, dish, {
            id: match.id,
            price: match.price,
            image: match.image || dish.image,
            isAvailable: match.isAvailable,
            name: match.name || dish.name
        });
    }


    // ========================================
    // Load Dish Catalog
    // ========================================

    function loadDishCatalog() {

        return fetch('/Dishes/Catalog')

            .then(function (response) {
                return response.ok
                    ? response.json()
                    : [];
            })

            .then(function (catalog) {

                dishCatalog = catalog || [];

                // Restore favorite states from database.
                document
                    .querySelectorAll('[data-favorite-dish-id]')
                    .forEach(function (button) {

                        var dish = dishCatalog.find(function (item) {
                            return String(item.id) ===
                                String(button.dataset.favoriteDishId);
                        });

                        if (!dish) return;

                        var active = Boolean(dish.isFavorite);

                        button.dataset.favoriteActive =
                            active ? 'true' : 'false';

                        button.setAttribute(
                            'aria-pressed',
                            String(active)
                        );

                        var icon = button.querySelector('svg');

                        if (icon) {
                            icon.classList.toggle(
                                'fill-rose-500',
                                active
                            );

                            icon.classList.toggle(
                                'text-rose-500',
                                active
                            );

                            icon.classList.toggle(
                                'fill-none',
                                !active
                            );
                        }
                    });


                // Normalize cart dish IDs.
                document
                    .querySelectorAll(
                        'form[action="/Cart/Add"] [name="dishId"]'
                    )
                    .forEach(function (input) {

                        var form = input.closest('form');

                        var match = normalizeDishPayload({
                            id: input.value,
                            name: form &&
                                form.getAttribute('data-dish-name')
                        });

                        if (match && match.id) {
                            input.value = match.id;
                        }
                    });


                // Fix dish details links.
                document
                    .querySelectorAll(
                        '[onclick*="/Dishes/Details/"]'
                    )
                    .forEach(function (link) {

                        var cardText = link.textContent || '';

                        var dish = dishCatalog.find(function (item) {
                            return cardText.indexOf(item.name) >= 0;
                        });

                        if (dish) {
                            link.onclick = function () {
                                window.location.href =
                                    '/Dishes/Details/' + dish.id;
                            };
                        }
                    });

            })

            .catch(function (error) {
                console.error('Dish catalog error:', error);
                dishCatalog = [];
            });
    }


    // ========================================
    // Cart Counter
    // ========================================

    function updateCartCount(count) {

        document
            .querySelectorAll('[data-cart-count]')
            .forEach(function (badge) {

                badge.textContent = count;
                badge.hidden = count < 1;
            });
    }


    function refreshCartCount() {

        fetch('/Cart/Count', {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })

            .then(function (response) {
                return response.ok
                    ? response.json()
                    : null;
            })

            .then(function (data) {
                if (data) {
                    updateCartCount(data.count);
                }
            })

            .catch(function (error) {
                console.error('Cart count error:', error);
            });
    }


    // ========================================
    // Favorites Counter
    // ========================================

    function updateFavoritesCount(count) {

        var badge = document.getElementById('favorites-count');

        if (!badge) return;

        var total = Math.max(
            0,
            Number(count) || 0
        );

        badge.textContent = total;
        badge.hidden = total === 0;
    }


    function refreshFavoritesCount() {

        fetch('/Dishes/FavoritesCount', {
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })

            .then(function (response) {

                if (!response.ok) {
                    throw new Error(
                        'Failed to load favorites count.'
                    );
                }

                return response.json();
            })

            .then(function (data) {
                updateFavoritesCount(data.count);
            })

            .catch(function (error) {
                console.error(
                    'Favorites count error:',
                    error
                );
            });
    }


    // IMPORTANT:
    // Expose these functions globally because toggleFavorite()
    // is outside this IIFE.
    window.updateFavoritesCount = updateFavoritesCount;
    window.refreshFavoritesCount = refreshFavoritesCount;


    // ========================================
    // DOM Content Loaded
    // ========================================

    document.addEventListener('DOMContentLoaded', function () {

        // Load catalog and refresh counters.
        loadDishCatalog().then(function () {

            refreshCartCount();
            refreshFavoritesCount();

        });


        // ====================================
        // Quick View
        // ====================================

        document.addEventListener(
            'open-quick-view',
            function (event) {

                if (event.detail) {
                    event.detail = normalizeDishPayload(
                        event.detail
                    );
                }
            }
        );


        // ====================================
        // Options Modal
        // ====================================

        document.addEventListener(
            'open-options-modal',
            function (event) {

                if (event.detail) {
                    event.detail = normalizeDishPayload(
                        event.detail
                    );
                }
            }
        );


        // ====================================
        // Add To Cart
        // ====================================

        document.addEventListener('submit', function (event) {

            var form = event.target.closest(
                'form[action="/Cart/Add"]'
            );

            if (!form) return;

            event.preventDefault();

            var dishInput = form.querySelector(
                '[name="dishId"]'
            );

            if (
                dishInput &&
                typeof dishInput.value === 'string' &&
                dishInput.value.indexOf('dish-') === 0
            ) {

                var catalogDish = dishCatalog.find(function (item) {
                    return item.name ===
                        form.getAttribute('data-dish-name');
                });

                if (catalogDish) {

                    dishInput.value = catalogDish.id;

                } else {

                    var prototypeIndex = parseInt(
                        dishInput.value.substring(5),
                        10
                    ) - 1;

                    if (dishCatalog[prototypeIndex]) {
                        dishInput.value =
                            dishCatalog[prototypeIndex].id;
                    }
                }
            }


            var button = form.querySelector(
                'button[type="submit"]'
            );

            if (button) {
                button.disabled = true;
            }


            fetch(form.action, {
                method: 'POST',
                body: new FormData(form),
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })

                .then(function (response) {

                    return response.json().then(function (data) {

                        if (!response.ok) {
                            throw new Error(
                                data.message ||
                                'Unable to add item.'
                            );
                        }

                        return data;
                    });
                })

                .then(function (data) {

                    updateCartCount(data.count);

                    window.dispatchEvent(
                        new CustomEvent('cart-added', {
                            detail: data
                        })
                    );
                })

                .catch(function (error) {

                    window.dispatchEvent(
                        new CustomEvent('cart-error', {
                            detail: error.message
                        })
                    );
                })

                .finally(function () {

                    if (button) {
                        button.disabled = false;
                    }
                });
        });


        // ====================================
        // Cart Quantity Controls
        // ====================================

        document.addEventListener('click', function (event) {

            var button = event.target.closest(
                '[data-quantity-step]'
            );

            if (!button) return;

            var form = button.closest(
                '[data-cart-quantity-form]'
            );

            if (!form || form.dataset.busy === 'true') {
                return;
            }

            var input = form.querySelector(
                '[name="quantity"]'
            );

            var itemId = form.querySelector(
                '[name="itemId"]'
            );

            var token = form.querySelector(
                '[name="__RequestVerificationToken"]'
            );

            if (!input || !itemId || !token) return;

            var current = parseInt(input.value, 10) || 1;

            var next = Math.max(
                1,
                Math.min(
                    100,
                    current + parseInt(
                        button.dataset.quantityStep,
                        10
                    )
                )
            );

            if (next === current) return;


            form.dataset.busy = 'true';

            form.querySelectorAll('button').forEach(
                function (control) {
                    control.disabled = true;
                }
            );

            input.value = next;


            fetch(form.action, {
                method: 'POST',

                body: new URLSearchParams({
                    itemId: itemId.value,
                    quantity: String(next),
                    __RequestVerificationToken: token.value
                }),

                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type':
                        'application/x-www-form-urlencoded; charset=UTF-8'
                }
            })

                .then(function (response) {

                    return response.json().then(function (data) {

                        if (!response.ok) {
                            throw new Error(
                                data.message ||
                                'Unable to update quantity.'
                            );
                        }

                        return data;
                    });
                })

                .then(function (data) {

                    input.value = data.quantity;

                    var lineTotal = document.querySelector(
                        '[data-cart-line-total="' +
                        data.itemId +
                        '"]'
                    );

                    var subtotal = document.querySelector(
                        '[data-cart-subtotal]'
                    );


                    if (lineTotal) {
                        lineTotal.textContent =
                            Number(data.lineTotal).toFixed(2) +
                            ' EGP';
                    }

                    if (subtotal) {
                        subtotal.textContent =
                            Number(data.subtotal).toFixed(2) +
                            ' EGP';
                    }

                    updateCartCount(data.count);
                })

                .catch(function (error) {

                    input.value = current;

                    window.dispatchEvent(
                        new CustomEvent('cart-error', {
                            detail: error.message
                        })
                    );
                })

                .finally(function () {

                    delete form.dataset.busy;

                    form.querySelectorAll('button').forEach(
                        function (control) {
                            control.disabled = false;
                        }
                    );
                });
        });

    });

})();