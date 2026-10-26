/**
 * HomeChef Global Alpine Store
 * Replaces AppContext.tsx and provides hash routing, client state, and persistence.
 */

document.addEventListener('alpine:init', () => {
  Alpine.store('app', {
    // Navigation & Routing
    currentRoute: window.location.hash || '#home',

    // User & Identity
    role: localStorage.getItem('hc_role') || 'customer',
    language: localStorage.getItem('hc_lang') || 'en',
    theme: localStorage.getItem('hc_theme') || 'light',

    // Cart State
    cart: (() => {
      try {
        const saved = localStorage.getItem('hc_cart');
        if (saved) return JSON.parse(saved);
      } catch (e) {}
      return {
        chefId: 'chef-1',
        chefName: "Amina's Heritage Kitchen",
        items: [
          {
            id: 'item-demo-1',
            dishId: 'dish-1',
            quantity: 1,
            selectedOptions: [
              { groupName: 'Duck Portion', valueName: 'Duck Thigh & Drumstick (ورك بط)', priceModifier: 0 }
            ],
            note: 'Extra crispy duck skin please',
            unitPrice: 240,
            lineTotal: 240,
          }
        ],
        subtotal: 240,
      };
    })(),

    cartConflictPending: null,

    // Favorites / Wishlist
    favorites: (() => {
      try {
        const saved = localStorage.getItem('hc_favorites');
        if (saved) return JSON.parse(saved);
      } catch (e) {}
      return ['dish-1', 'chef-1'];
    })(),

    // Modals & Drawers
    loginModalOpen: false,
    loginModalTab: 'login',
    filterDrawerOpen: false,
    quickViewDish: null,
    optionsModalDish: null,
    notificationsOpen: false,
    chatModalOpen: false,
    contactSupportOpen: false,
    forgotPasswordOpen: false,

    // Search & Filter State
    searchQuery: '',
    selectedCuisine: 'all',
    selectedArea: 'all',
    minPrice: '',
    maxPrice: '',

    // Toasts
    toasts: [],

    init() {
      // Listen for URL hash changes
      window.addEventListener('hashchange', () => {
        this.currentRoute = window.location.hash || '#home';
        window.scrollTo({ top: 0, behavior: 'smooth' });
      });

      // Synchronize HTML attributes on init
      document.documentElement.lang = this.language;
      document.documentElement.dir = this.language === 'ar' ? 'rtl' : 'ltr';
      document.documentElement.setAttribute('data-theme', this.theme);
      if (this.theme === 'dark') {
        document.documentElement.classList.add('dark');
      } else {
        document.documentElement.classList.remove('dark');
      }
    },

    navigate(route) {
      window.location.hash = route;
      this.currentRoute = route;
      window.scrollTo({ top: 0, behavior: 'smooth' });
    },

    setRole(newRole) {
      this.role = newRole;
      localStorage.setItem('hc_role', newRole);
      this.showToast('info', 'Switched role to ' + newRole.toUpperCase());
    },

    setLanguage(lang) {
      this.language = lang;
      localStorage.setItem('hc_lang', lang);
      document.documentElement.lang = lang;
      document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
    },

    toggleTheme() {
      this.theme = this.theme === 'light' ? 'dark' : 'light';
      localStorage.setItem('hc_theme', this.theme);
      document.documentElement.setAttribute('data-theme', this.theme);
      if (this.theme === 'dark') {
        document.documentElement.classList.add('dark');
      } else {
        document.documentElement.classList.remove('dark');
      }
    },

    addToCart(dish, quantity = 1, selectedOptions = [], note = '') {
      if (!dish.isAvailable) {
        this.showToast('error', 'This dish is currently unavailable.');
        return false;
      }

      // Check chef conflict
      if (this.cart.chefId && this.cart.items.length > 0 && this.cart.chefId !== dish.chefId) {
        this.cartConflictPending = { dish, quantity, selectedOptions, note };
        return false;
      }

      const optionsExtra = (selectedOptions || []).reduce((acc, opt) => acc + (opt.priceModifier || 0), 0);
      const unitPrice = dish.price + optionsExtra;
      const lineTotal = unitPrice * quantity;

      const optionsKey = JSON.stringify(selectedOptions || []);
      const existingIndex = this.cart.items.findIndex(
        (item) =>
          item.dishId === dish.id &&
          JSON.stringify(item.selectedOptions || []) === optionsKey &&
          (item.note || '').trim() === (note || '').trim()
      );

      let updatedItems = [...this.cart.items];
      if (existingIndex >= 0) {
        const existing = updatedItems[existingIndex];
        const newQty = existing.quantity + quantity;
        updatedItems[existingIndex] = {
          ...existing,
          quantity: newQty,
          lineTotal: existing.unitPrice * newQty,
        };
      } else {
        const newItem = {
          id: 'cart-' + Date.now() + '-' + Math.random().toString(36).substring(2, 7),
          dishId: dish.id,
          dish: dish,
          quantity: quantity,
          selectedOptions: selectedOptions,
          note: note,
          unitPrice: unitPrice,
          lineTotal: lineTotal,
        };
        updatedItems.push(newItem);
      }

      const newSubtotal = updatedItems.reduce((sum, item) => sum + item.lineTotal, 0);
      this.cart = {
        chefId: dish.chefId,
        chefName: dish.kitchenName,
        items: updatedItems,
        subtotal: newSubtotal,
      };

      localStorage.setItem('hc_cart', JSON.stringify(this.cart));
      this.showToast('success', (dish.name || 'Dish') + ' added to cart!');
      return true;
    },

    resolveCartConflict(action) {
      if (action === 'start_new' && this.cartConflictPending) {
        const { dish, quantity, selectedOptions, note } = this.cartConflictPending;
        const optionsExtra = (selectedOptions || []).reduce((acc, opt) => acc + (opt.priceModifier || 0), 0);
        const unitPrice = dish.price + optionsExtra;
        const lineTotal = unitPrice * quantity;

        const newItem = {
          id: 'cart-' + Date.now(),
          dishId: dish.id,
          dish: dish,
          quantity: quantity,
          selectedOptions: selectedOptions,
          note: note,
          unitPrice: unitPrice,
          lineTotal: lineTotal,
        };

        this.cart = {
          chefId: dish.chefId,
          chefName: dish.kitchenName,
          items: [newItem],
          subtotal: lineTotal,
        };

        localStorage.setItem('hc_cart', JSON.stringify(this.cart));
        this.showToast('info', 'Cart cleared and updated with dishes from ' + dish.kitchenName);
      }
      this.cartConflictPending = null;
    },

    updateCartQty(itemId, newQty) {
      if (newQty <= 0) {
        this.removeCartItem(itemId);
        return;
      }
      const updated = this.cart.items.map((item) => {
        if (item.id === itemId) {
          return {
            ...item,
            quantity: newQty,
            lineTotal: item.unitPrice * newQty,
          };
        }
        return item;
      });

      const newSubtotal = updated.reduce((sum, item) => sum + item.lineTotal, 0);
      this.cart = {
        ...this.cart,
        items: updated,
        subtotal: newSubtotal,
      };
      localStorage.setItem('hc_cart', JSON.stringify(this.cart));
    },

    // Undo cart removal (SCRUM-191)
    lastRemovedItem: null,

    removeCartItem(itemId) {
      const removed = this.cart.items.find((item) => item.id === itemId);
      if (removed) {
        this.lastRemovedItem = { ...removed };
      }
      const updated = this.cart.items.filter((item) => item.id !== itemId);
      if (updated.length === 0) {
        this.clearCart();
        this.showToast('info', 'Cart is now empty.');
      } else {
        const newSubtotal = updated.reduce((sum, item) => sum + item.lineTotal, 0);
        this.cart = {
          ...this.cart,
          items: updated,
          subtotal: newSubtotal,
        };
        localStorage.setItem('hc_cart', JSON.stringify(this.cart));
        this.showToast('info', 'Item removed from cart. You can undo this action.');
      }
    },

    undoRemoveItem() {
      if (!this.lastRemovedItem) return;
      const restored = { ...this.lastRemovedItem };
      const updated = [...this.cart.items, restored];
      const newSubtotal = updated.reduce((sum, item) => sum + item.lineTotal, 0);
      this.cart = {
        chefId: this.cart.chefId || restored.dish?.chefId || 'chef-1',
        chefName: this.cart.chefName || restored.dish?.kitchenName || "Amina's Heritage Kitchen",
        items: updated,
        subtotal: newSubtotal,
      };
      localStorage.setItem('hc_cart', JSON.stringify(this.cart));
      this.lastRemovedItem = null;
      this.showToast('success', 'Item restored to your cart!');
    },

    clearCart() {
      this.cart = {
        chefId: null,
        chefName: null,
        items: [],
        subtotal: 0,
      };
      localStorage.setItem('hc_cart', JSON.stringify(this.cart));
    },

    toggleFavorite(id) {
      if (this.role === 'guest') {
        this.loginModalOpen = true;
        this.showToast('info', 'Please sign in to save items to your favorites.');
        return;
      }
      if (this.favorites.includes(id)) {
        this.favorites = this.favorites.filter((x) => x !== id);
        this.showToast('info', 'Removed from favorites');
      } else {
        this.favorites.push(id);
        this.showToast('success', 'Saved to your favorites!');
      }
      localStorage.setItem('hc_favorites', JSON.stringify(this.favorites));
    },

    isFavorite(id) {
      return this.favorites.includes(id);
    },

    showToast(type, message, title = '') {
      const id = 'toast-' + Date.now() + '-' + Math.random().toString(36).substring(2, 6);
      this.toasts.push({ id, type, message, title });
      setTimeout(() => {
        this.dismissToast(id);
      }, 4000);
    },

    dismissToast(id) {
      this.toasts = this.toasts.filter((t) => t.id !== id);
    },

    clearAllFilters() {
      this.selectedCuisine = 'all';
      this.selectedArea = 'all';
      this.minPrice = '';
      this.maxPrice = '';
    },

    resetAllMockData() {
      localStorage.removeItem('hc_cart');
      localStorage.removeItem('hc_favorites');
      localStorage.removeItem('hc_role');
      localStorage.removeItem('hc_theme');
      localStorage.removeItem('hc_lang');
      window.location.reload();
    },
  });
});
