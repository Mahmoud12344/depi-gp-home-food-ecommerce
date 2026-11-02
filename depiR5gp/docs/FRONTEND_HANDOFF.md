# Frontend MVC Restructure — Backend Handoff Guide (`FRONTEND_HANDOFF.md`)

## Overview

The frontend of **Beitouti / HomeChef** (`depiR5gp`) has been restructured from a client-side Alpine.js hash-routed Single-Page Application (formerly loaded all at once inside `Views/Home/Index.cshtml` and powered by `wwwroot/js/store.js`) into standard multi-page **ASP.NET Core MVC Razor views** (`Views/<Controller>/<Action>.cshtml`), while preserving 100% of `main`'s visual design, DOM structure, Tailwind/custom CSS classes, responsive behavior, and interactive modals.

### Key Architecture Guarantees
- **Zero client-side routing or global store**: `wwwroot/js/store.js`, hash routes (`#dishes`, `#checkout`, etc.), `$store.app.*` bindings, and inline client-side language ternary bindings (`$store.app.language === 'ar' ? ...`) have been completely removed.
- **Plain HTML navigation**: Every link uses standard HTML `<a href="/Controller/Action/id">`. No `asp-controller`, `asp-action`, `asp-route-*`, `Url.Action`, or `Html.ActionLink` tag helpers are used anywhere in the views.
- **Plain HTML forms**:
  - Filters, search, and sort on `/Dishes`, `/Chefs`, and `_Header.cshtml` use plain `<form method="get" action="/...">` submitting query parameters.
  - Mutations use standard HTML `<form method="post" action="/Controller/Action">` with explicit `name="..."` attributes on all inputs, selects, and textareas, plus `@Html.AntiForgeryToken()`.
- **Static sample markup with markers**: Because ViewModels are out of scope for this frontend pass, dynamic lists and data fields are unrolled from `main`'s exact dataset into static English HTML wrapped in `<!-- BEGIN SAMPLE: <description> -->` ... `<!-- END SAMPLE -->` blocks and annotated with `<!-- TODO(backend): ... -->` / `@* TODO(backend): ... *@` comments.
- **Prototype-only blocks clearly isolated**: Interactive prototype helpers from `main` (e.g., the Order Tracking status simulator bar, AI streaming animation, Login demo credential pills, Prototype Bar role simulator, Cart unavailable simulator, and Checkout Stripe test simulator) are wrapped in `@* PROTOTYPE-ONLY: BEGIN ... *@` ... `@* PROTOTYPE-ONLY: END *@` comments so backend developers can remove or replace them cleanly.
- **Scoped Alpine.js only**: Alpine.js is used strictly for local visual UI state (modals, filter drawers, grid/list view toggles, tabs, dropdowns, accordions, 3D image gallery stack, quantity steppers, star rating selectors, and dark/light theme toggle) via local `x-data`. No embedded data arrays are used for client-side filtering.

---

## 1. Action Needed: 5 New View Files Requiring Controller `[HttpGet]` Actions

In `main`, `_ChefDashboard.cshtml`, `_AdminDashboard.cshtml`, and `_SupportPortal.cshtml` contained additional client-side tabs/views that did not yet have `[HttpGet]` action methods in the scaffolded controllers. Per architecture guidelines, these have been extracted into **5 dedicated MVC view files** rather than in-page client-side tabs.

To make these 5 URLs live, add the following one-line `[HttpGet]` actions to their respective controllers:

| Action Needed URL | View File Created | Controller File | Method to Add | Screen Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET /ChefDashboard/History` | `Views/ChefDashboard/History.cshtml` | `Controllers/ChefDashboardController.cs` | `public IActionResult History() => View();` | **Chef Order History** (Tab 3 of `main` `_ChefDashboard.cshtml`): status filter (`GET /ChefDashboard/History?status=...`) and 4 historical orders (`#HC-2026-8840` Completed, `#HC-2026-8812` Completed, `#HC-2026-8790` Cancelled, `#HC-2026-8744` Rejected). |
| `GET /ChefDashboard/Reviews` | `Views/ChefDashboard/Reviews.cshtml` | `Controllers/ChefDashboardController.cs` | `public IActionResult Reviews() => View();` | **Chef Customer Reviews** (Tab 4 of `main` `_ChefDashboard.cshtml`): `4.9 / 5.0` average rating summary card and 3 verified customer review cards (`Youssef Nabil`, `Salma Fathy`, `Karim Gad`). |
| `GET /ChefDashboard/Wallet` | `Views/ChefDashboard/Wallet.cshtml` | `Controllers/ChefDashboardController.cs` | `public IActionResult Wallet() => View();` | **Chef Wallet & Ledger** (Tab 5 of `main` `_ChefDashboard.cshtml`): `4,820.00 EGP` available balance card with `POST /ChefDashboard/RequestPayout`, `850.50 EGP` 15% platform commission card, `EG89 0044 ...` IBAN payout destination card, and 4-row financial ledger table. |
| `GET /Admin/Reviews` | `Views/Admin/Reviews.cshtml` | `Controllers/AdminController.cs` | `public IActionResult Reviews() => View();` | **Admin Review Moderation (`SCRUM-552`)** (Tab 4 of `main` `_AdminDashboard.cshtml`): moderation filter bar (`All`, `Pending`, `Approved`, `Rejected`) and 4 customer review moderation cards (`rev-1`..`rev-4`) with `POST /Admin/ApproveReview` and `POST /Admin/RejectReview`. |
| `GET /Support/AgentDesk` | `Views/Support/AgentDesk.cshtml` | `Controllers/SupportController.cs` | `public IActionResult AgentDesk() => View();` | **Support Agent & Refunds Desk** (`main` `_SupportPortal.cshtml`): Support Cases Queue (`CASE-7701`, `CASE-7702`) with reply/status workspace (`POST /Support/AgentReplyCase`) + Refund Approval Queue (`REF-2026-104`, `REF-2026-098`) with `POST /Support/ApproveRefund` and Reject Refund modal (`POST /Support/RejectRefund`). |

---

## 2. Shared Card Partials Expected Inputs (`_DishCard.cshtml` & `_ChefCard.cshtml`)

Per Rule 3, `ViewData` dictionaries are **not** used to pass mock items into `_DishCard.cshtml` or `_ChefCard.cshtml`. Instead:
- Multi-item sample grids (`Views/Home/Index.cshtml`, `Views/Dishes/Index.cshtml`, `Views/Chefs/Index.cshtml`, `Views/Customer/Favorites.cshtml`) unroll full card markup per item inside `<!-- BEGIN SAMPLE -->` ... `<!-- END SAMPLE -->` blocks so all 6 dishes and 5 chefs display their exact `main` data without C# ViewModels.
- Once backend ViewModels are ready, replace the unrolled `<!-- BEGIN SAMPLE -->` blocks with a `@foreach` loop calling `@await Html.PartialAsync("~/Views/Shared/Partials/_DishCard.cshtml", dish)` or `@await Html.PartialAsync("~/Views/Shared/Partials/_ChefCard.cshtml", chef)`.

### 2.1 `Views/Shared/Partials/_DishCard.cshtml` — Expected Model Properties
When binding `_DishCard.cshtml` to `@model DishCardViewModel`, replace the sample constants inside `_DishCard.cshtml` with the following properties:

| Property | Type | Sample Value in `_DishCard.cshtml` | Usage in Markup |
| :--- | :--- | :--- | :--- |
| `Id` | `int` / `string` | `1` (`dish-1`) | Links `/Dishes/Details/@Model.Id`, hidden `<input name="dishId" value="@Model.Id">`, Quick View & Options modal payload. |
| `Name` | `string` | `"Baladi Duck Molokhia with Vermicelli Rice"` | Card `<h3>` title, image `alt`, Quick View & Options modal payload. |
| `Description` | `string` | `"Slow-simmered farm duck in rich garlic-coriander green molokhia, served with fluffy vermicelli rice."` | Two-line clamped description (`line-clamp-2 min-h-[2.5rem]`) and modal payload. |
| `ImageUrl` | `string` | `"https://images.unsplash.com/photo-1547592180-85f173990554?w=800&q=80"` | Card cover `<img>` (`aspect-4/3`). |
| `Price` | `decimal` | `240.00m` | Primary price (`@Model.Price.ToString("0.00") EGP`). |
| `OriginalPrice` | `decimal?` | `280.00m` | Strikethrough original price (`280.00 EGP`) and discount calculation (`(1 - Price / OriginalPrice) * 100`) for the `.discount-burst` 12-point starburst badge. |
| `Rating` | `double` | `4.9` | Top-right rating pill (`#star`). |
| `ReviewCount` | `int` | `64` | Review count in Quick View modal payload. |
| `PrepTimeMinutes` | `int` | `45` | Footer prep time badge (`~45m`). |
| `Calories` | `int?` | `680` | Quick View modal payload (`680 kcal`). |
| `ServingSize` | `string` | `"2 Persons"` | Quick View modal payload. |
| `Available` | `bool` | `true` | Bottom-left status badge (`Available` emerald pulsing dot vs. `Out of Stock` red dot) and `Add to Cart` disabled state. |
| `ChefId` | `int` / `string` | `1` (`chef-1`) | Link to `/Chefs/Details/@Model.ChefId`. |
| `ChefName` / `KitchenName` | `string` | `"Amina's Heritage Kitchen"` | Subtitle link above dish title. |
| `IsFavorite` | `bool` | `true` | Initial state for heart button (`POST /Customer/ToggleFavorite`). |
| `HasOptionGroups` | `bool` | `true` | Determines whether clicking the hover cart button opens `_OptionsDialog.cshtml` (`$dispatch('open-options-modal', ...)`) or submits directly to `POST /Cart/Add`. |

### 2.2 `Views/Shared/Partials/_ChefCard.cshtml` — Expected Model Properties
When binding `_ChefCard.cshtml` to `@model ChefCardViewModel`, replace the sample constants inside `_ChefCard.cshtml` with the following properties:

| Property | Type | Sample Value in `_ChefCard.cshtml` | Usage in Markup |
| :--- | :--- | :--- | :--- |
| `Id` | `int` / `string` | `1` (`chef-1`) | Links `/Chefs/Details/@Model.Id` and hidden `<input name="chefId" value="@Model.Id">` in `POST /Customer/ToggleFavorite`. |
| `Name` | `string` | `"Amina's Heritage Kitchen"` | Card `<h3>` kitchen title and avatar `alt`. |
| `ChefFullName` | `string` | `"Chef Amina Mansour"` | Subtitle below kitchen name (`by @Model.ChefFullName`). |
| `CoverUrl` | `string` | `"https://images.unsplash.com/photo-1556910103-1c02745aae4d?w=900&q=80"` | Banner cover image (`h-48`). |
| `AvatarUrl` | `string` | `"https://images.unsplash.com/photo-1583394838336-acd977736f90?w=300&q=80"` | Overlapping chef avatar (`w-16 h-16 rounded-2xl`). |
| `Verified` | `bool` | `true` | Top-left `Verified Kitchen` badge (`#shield-check`) and avatar verified badge. |
| `IsBusy` | `bool` | `false` | Top-left status badge (`Open Now` emerald dot vs. `Busy` amber dot). |
| `Rating` | `double` | `4.9` | Rating score next to `#star`. |
| `ReviewCount` | `int` | `142` | Review count (`(142)`). |
| `Neighborhood` | `string` | `"Maadi, Cairo"` | Location metadata (`#map-pin`). |
| `Specialty` | `string` | `"Traditional Egyptian"` | Primary specialty badge. |
| `MenuDishCount` | `int` | `4` | Dish count pill (`4 Dishes`). |
| `Bio` | `string` | `"30+ years crafting authentic Egyptian family recipes..."` | Clamped kitchen bio (`line-clamp-2 min-h-[2.5rem]`). |
| `Specialties` | `IReadOnlyList<string>` | `["Molokhia & Duck", "Stuffed Pigeon", "Mahshi"]` | Up to 3 specialty tag pills (`Specialties.Take(2)` + `+1` overflow pill). |
| `MinOrderEgp` | `decimal` | `150m` | Footer minimum order (`Min 150 EGP`). |
| `AvgPrepMinutes` | `int` | `45` | Footer average prep time (`~45m prep`). |
| `IsFavorite` | `bool` | `true` | Initial state for favorite heart button (`POST /Customer/ToggleFavorite`). |

---

## 3. `@* PROTOTYPE-ONLY *@` Blocks Inventory

Per Rule 6, all prototype-only UI simulation helpers from `main` are preserved visually and wrapped in `@* PROTOTYPE-ONLY: BEGIN ... *@` ... `@* PROTOTYPE-ONLY: END *@` comments so the backend team can easily locate and remove them when wiring real server state:

| File | Lines / Block Description | What to Do in Production |
| :--- | :--- | :--- |
| `Views/Shared/Partials/_PrototypeBar.cshtml` | Entire partial (`@* PROTOTYPE-ONLY: BEGIN ... *@` around the fixed bottom bar with Role Simulator pills `Guest`/`Customer`/`Chef`/`Admin` and 19-screen quick-jump links). | Remove `@await Html.PartialAsync("~/Views/Shared/Partials/_PrototypeBar.cshtml")` from `_Layout.cshtml` or gate behind `<environment include="Development">`. |
| `Views/Cart/Index.cshtml` | Prototype simulator bar at top of `/Cart` (`Simulate Item Unavailable` checkbox + `Preview Empty Cart` toggle). | Remove block and drive unavailable item warning and empty cart state from `Model.Items`. |
| `Views/Checkout/Index.cshtml` | Stripe Test Card simulation banner & test card defaults (`4242 •••• •••• 4242`, `Test Mode Enabled` badge). | Replace with real Stripe Elements / Paymob payment gateway integration. |
| `Views/Orders/Track.cshtml` | **Prototype Status Simulator Bar** (`Pending`, `Accepted`, `Preparing`, `Ready for Pickup`, `Completed`, `Cancelled`, `Rejected` buttons that mutate `currentStatus` client-side). | Remove simulator bar and set `currentStatus` (or server-rendered conditional blocks) directly from `@Model.Status`. |
| `Views/Account/Login.cshtml` | **Quick Demo Auto-Fill Pills** (`Customer: tamer.zaki@example.com`, `Home Chef: amina.chef@example.com`, `Admin: karim.admin@homechef.eg`). | Remove demo credential pills before production deployment. |
| `Views/Chefs/Onboarding.cshtml` | **Instant Demo Approval** card in right sidebar (`Preview Approved State` button). | Remove demo preview button so chef applications await admin approval. |
| `Views/AIAssistant/Index.cshtml` | Client-side typewriter & 5-step reasoning simulation (`submitPrompt` timer in `x-data`). | Replace client-side timer simulation with real streaming/JSON response from `POST /AIAssistant/Ask`. |

---

## 4. Screen Inventory (`Views/<Controller>/<Action>.cshtml`)

### 4.1 Layout & Shared Partials (`Views/Shared/` & Portal Navs)

| File | Source in `main` | Purpose |
| :--- | :--- | :--- |
| `Views/Shared/_Layout.cshtml` | `Views/Shared/_Layout.cshtml` | Master layout containing `<head>` assets, `_Sprite`, `_Header`, `@RenderBody()`, `_Footer`, `_OptionsDialog`, `_QuickViewModal`, `_ChatModal`, `_CartConflictDialog`, `_ToastContainer`, `_AIAssistantWidget`, and `_PrototypeBar`. |
| `Views/Shared/Partials/_Header.cshtml` | `Views/Shared/Partials/_Header.cshtml` | Sticky `h-20` top navbar with Delivery Location pill (`Maadi, Cairo`), `GET /Dishes` search input, Favorites & Chat icon badges, Logged-In user profile dropdown (`TODO(backend)` marker for `User.Identity.IsAuthenticated`), Sign In link, Cart Drawer button, and mobile menu drawer. |
| `Views/Shared/Partials/_Footer.cshtml` | `Views/Shared/Partials/_Footer.cshtml` | 4-column site footer with plain HTML navigation links and `POST /Home/Subscribe` newsletter form. |
| `Views/Shared/Partials/_PrototypeBar.cshtml` | `Views/Shared/Partials/_PrototypeBar.cshtml` | Fixed bottom developer/demo bar wrapped in `@* PROTOTYPE-ONLY *@` with Role Simulator pills, plain `<a href="/...">` links to all screens, and Dark/Light theme toggle. |
| `Views/Shared/Partials/_DishCard.cshtml` | `Views/Shared/Partials/_DishCard.cshtml` | Reusable single-item dish card partial with `.discount-burst` starburst badge, availability pill, hover actions (`POST /Customer/ToggleFavorite`, Options/Cart trigger, Quick View trigger), and `line-clamp-2` description. |
| `Views/Shared/Partials/_ChefCard.cshtml` | `Views/Shared/Partials/_ChefCard.cshtml` | Reusable single-item chef card partial with Verified/Open badges, overlapping avatar, specialty pills, dish count badge, and `View Kitchen Menu` button. |
| `Views/Shared/Partials/_FilterDrawer.cshtml` | `Views/Shared/Partials/_FilterDrawer.cshtml` | Slide-over advanced filter drawer for `/Dishes` and `/Chefs` submitting a plain `<form method="get">` with query parameters (`availableOnly`, `minRating`, `maxPrepTime`, `dietary`, `minPrice`, `maxPrice`). |
| `Views/Shared/Partials/_OptionsDialog.cshtml` | `Views/Shared/Partials/_OptionsDialog.cshtml` | Dish customization modal triggered via `@@open-options-modal.window`, with radio/checkbox option groups, special instructions, live total price calculation, and `POST /Cart/Add` form. |
| `Views/Shared/Partials/_QuickViewModal.cshtml` | `Views/Shared/Partials/_QuickViewModal.cshtml` | Dish quick-view modal triggered via `@@open-quick-view.window`, dynamically binding `$event.detail` dish data and submitting to `POST /Cart/Add` or opening `_OptionsDialog`. |
| `Views/Shared/Partials/_ChatModal.cshtml` | `Views/Shared/Partials/_ChatModal.cshtml` | Floating customer–chef chat modal triggered via `@@open-chat-modal.window`, submitting to `POST /Chat/SendMessage` and linking to `/Chat`. |
| `Views/Shared/Partials/_CartConflictDialog.cshtml` | `Views/Shared/Partials/_CartConflictDialog.cshtml` | Different-chef cart warning dialog triggered via `@@open-cart-conflict.window` or `TempData["CartConflict"]`, submitting to `POST /Cart/ClearAndAdd`. |
| `Views/Shared/Partials/_ToastContainer.cshtml` | `Views/Shared/Partials/_ToastContainer.cshtml` | Server-side `TempData` flash toast alerts + client-side `@@show-toast.window` notification listener. |
| `Views/Shared/Partials/_AIAssistantWidget.cshtml` | `Views/Shared/Partials/_AIAssistantWidget.cshtml` | Floating AI Food Concierge popup widget linking to `/AIAssistant` and `/Dishes/Details/{id}`. |
| `Views/Shared/Partials/_HeroSection7.cshtml` | `Views/Shared/Partials/_HeroSection7.cshtml` | Home page hero section (`hero7-grid`) linking to `/Dishes` and `/Chefs`. |
| `Views/Shared/Partials/_HoverRevealCards.cshtml` | `Views/Shared/Partials/_HoverRevealCards.cshtml` | Home page 6-card interactive cuisine category grid (`hover-reveal-grid`) linking to `/Dishes?category=...`. |
| `Views/Shared/Partials/_VerticalImageStack.cshtml` | `Views/Shared/Partials/_VerticalImageStack.cshtml` | Home page Featured Chef spotlight with 5-card interactive vertical image stack (`window.initVerticalStack`) linking to `/Chefs/Details/1` and `/Chefs`. |
| `Views/Shared/Partials/_ThinkingPromptInput.cshtml` | `Views/Shared/Partials/_ThinkingPromptInput.cshtml` | Home page AI meal discovery prompt (`window.initThinkingPrompt`) with `Enable step-by-step thinking` checkbox, inline streaming preview, and link to `/AIAssistant`. |
| `Views/Shared/Partials/_CoverflowCarousel.cshtml` | `Views/Shared/Partials/_CoverflowCarousel.cshtml` | Home page 3D circular coverflow gallery (`window.initGallery3D`) with reactive 6-slide caption switcher (`selected === 0..5`) linking to `/Dishes/Details/1..6`. |
| `Views/Customer/Partials/_CustomerNav.cshtml` | `Views/Customer/Partials/_CustomerProfile.cshtml` | Customer portal header card (`Tamer Zaki`, `450 pts`, `Edit Profile` button) + navigation tabs (`/Customer/Profile`, `/Customer/MyOrders`, `/Customer/Favorites`, `/Customer/Points`, `/Support`). |
| `Views/ChefDashboard/Partials/_ChefNav.cshtml` | `Views/Chefs/Partials/_ChefDashboard.cshtml` | Chef portal header banner (`Amina's Heritage Kitchen`, `POST /ChefDashboard/ToggleAvailability` switch, `+ Add New Dish` button) + navigation tabs (`/ChefDashboard`, `/ChefDashboard/Orders`, `/MenuManagement`, `/ChefDashboard/History`, `/ChefDashboard/Reviews`, `/ChefDashboard/Wallet`, `/ChefDashboard/Profile`, `/ChefDashboard/WorkingHours`, `/ChefDashboard/Documents`). |
| `Views/Admin/Partials/_AdminNav.cshtml` | `Views/Admin/Partials/_AdminDashboard.cshtml` | Admin portal header banner + 7 navigation tabs (`/Admin`, `/Admin/Chefs`, `/Admin/Orders`, `/Admin/Users`, `/Admin/Reviews`, `/Admin/Payouts`, `/Admin/Settings`). |

### 4.2 Controller Views (40 Views: 35 Scaffolded + 5 New Action-Needed Views)

| View File | GET URL | `main` Source Partial | Screen Description |
| :--- | :--- | :--- | :--- |
| `Views/Home/Index.cshtml` | `/` or `/Home/Index` | `_HomePageV2.cshtml` | Landing page with `_HeroSection7`, `_HoverRevealCards`, `_VerticalImageStack`, `_ThinkingPromptInput`, `_CoverflowCarousel`, 4 unrolled Featured Dish cards (`dish-1`..`dish-4`), 3 How-It-Works steps, and Chef Onboarding CTA banner. |
| `Views/Home/Privacy.cshtml` | `/Home/Privacy` | `Views/Home/Privacy.cshtml` | Privacy policy page. |
| `Views/Dishes/Index.cshtml` | `/Dishes` | `_BrowseDishes.cshtml` | Browse Dishes catalog with sticky horizontal `GET /Dishes` filter bar, `_FilterDrawer`, Grid/List toggle (`viewMode`), 6 unrolled Grid View dish cards (`dish-1`..`dish-6`), and 6 unrolled `.dish-list-container` / `.dish-list-item` List View rows. |
| `Views/Dishes/Details.cshtml` | `/Dishes/Details/{id}` | `_DishDetail.cshtml` | Single dish detail page with `.dish-detail-grid`, `.dish-gallery-frame-690` / `.dish-photo-card-420-520` 3D vertical stacked image gallery + zoom lightbox, `.discount-burst` badge, reactive customization options + `POST /Cart/Add` form, 3 inline accordions, and 3 verified reviews. |
| `Views/Chefs/Index.cshtml` | `/Chefs` | `_BrowseChefs.cshtml` | Browse Chefs directory with sticky horizontal `GET /Chefs` filter bar, `_FilterDrawer`, Grid/List toggle (`viewMode`), 5 unrolled Grid View chef cards (`chef-1`..`chef-5`), and 5 unrolled `.chef-list-container` / `.chef-list-item` List View rows. |
| `Views/Chefs/Details.cshtml` | `/Chefs/Details/{id}` | `_PublicChef.cshtml` | Public chef storefront (`Amina's Heritage Kitchen`) with cover banner, `-mt-24` overlapping profile card, 4-stat grid, Kitchen Schedule modal, category/search filter bar, 4 horizontal dish cards (`Customize` / `Add`), and 3-column verified reviews grid. |
| `Views/Chefs/Onboarding.cshtml` | `/Chefs/Onboarding` | `_ChefOnboarding.cshtml` | 3-step Home Chef application wizard (`lg:col-span-8`, `POST /Chefs/Onboarding`) + `Why Cook with HomeChef?` benefits sidebar (`lg:col-span-4`). |
| `Views/Cart/Index.cshtml` | `/Cart` | `_CartPage.cshtml` | Shopping cart with `Amina's Heritage Kitchen` banner, 2 sample items (`Baladi Duck Molokhia` 240 EGP + `Authentic Baladi Feteer Meshaltet` 160 EGP = 400 EGP subtotal), quantity/remove POST forms, visual promo (`HOME20`/`WELCOME50`) & loyalty points (`-25 EGP`) preview, and Checkout CTA. |
| `Views/Checkout/Index.cshtml` | `/Checkout` | `_CheckoutPage.cshtml` | Checkout page (`POST /Checkout`) with `Amina's Heritage Kitchen` pickup location card, 8-slot radio grid (`15:00–15:30` disabled `Full`), Cash vs. Card payment selector (with Stripe test card inputs), loyalty points toggle, and `415.00 EGP` summary. |
| `Views/Orders/Confirmation.cshtml` | `/Orders/Confirmation/{id}` | `_OrderConfirmation.cshtml` | Post-checkout receipt card (`max-w-3xl`) for Order `#HC-2026-9041`, Pickup Code `4829`, `Pending Chef Acceptance` pulsing status bar, and `415.00 EGP` item summary. |
| `Views/Orders/Track.cshtml` | `/Orders/Track/{id}` | `_OrderTracking.cshtml` + `_CancelOrderDialog.cshtml` + `_RequestRefundModal.cshtml` | Live order tracking (`#HC-2026-9041`, code `4829`), `@* PROTOTYPE-ONLY *@` Status Simulator Bar, 5-step progress tracker, Cancel Order modal (`POST /Orders/Cancel`), and Request Refund modal with photo proof dropzone (`POST /Orders/RequestRefund`). |
| `Views/Reviews/Create.cshtml` | `/Reviews/Create?orderId={id}` | `_OrderReviewModal.cshtml` | Order rating & review form (`POST /Reviews/Create`) for `#HC-2026-9041` (`Amina's Heritage Kitchen`) with interactive 5-star selector, live rating word labels (`Terrible`..`Exceptional!`), and 500-char counter. |
| `Views/Customer/Profile.cshtml` | `/Customer/Profile` | `_CustomerProfile.cshtml` + `_EditCustomerProfileModal.cshtml` | Customer personal details form (`POST /Customer/Profile`), 2 saved addresses (`Home — Maadi`, `Office — Smart Village`), and password change form (`POST /Customer/ChangePassword`). |
| `Views/Customer/MyOrders.cshtml` | `/Customer/MyOrders` | `_CustomerProfile.cshtml` (Tab 1) | Customer orders list with status filter pills and `main`'s 3 orders (`#HC-2026-9041` Preparing 240 EGP, `#HC-2026-8890` Completed 310 EGP, `#HC-2026-8512` Cancelled 180 EGP). |
| `Views/Customer/Favorites.cshtml` | `/Customer/Favorites` | `_CustomerProfile.cshtml` (Tab 2) | Customer saved favorites with Dishes (3) / Kitchens (2) tab switcher, 3 unrolled favorite dish cards (`dish-1`, `dish-2`, `dish-4`), and 2 unrolled favorite chef cards (`chef-1`, `chef-2`). |
| `Views/Customer/Points.cshtml` | `/Customer/Points` | `_CustomerProfile.cshtml` (Tab 3) | Customer loyalty points card (`450` points = `112.50 EGP` value, 50 pts to Gold Tier), How Points Work cards, Redeemable Rewards (`POST /Customer/RedeemPoints`), and 3-row Points Activity Ledger. |
| `Views/ChefDashboard/Index.cshtml` | `/ChefDashboard` | `_ChefDashboard.cshtml` | Chef portal overview with `main`'s 4 KPI cards (`Pending Acceptance: 1`, `In Preparation: 1`, `Ready for Pickup: 0`, `Net Wallet Earnings: 4,820.00 EGP`), active incoming orders queue (`#HC-2026-9041`, `#HC-2026-9042`), and quick links. |
| `Views/ChefDashboard/Orders.cshtml` | `/ChefDashboard/Orders` | `_ChefDashboard.cshtml` (Tab 1) + `_RejectOrderDialog.cshtml` | Chef live orders queue (`#HC-2026-9041` Tamer Zaki 240 EGP Pending, `#HC-2026-9042` Nour El-Din 310 EGP Preparing) with Accept/Advance POST forms and Reject Order modal (`POST /ChefDashboard/RejectOrder` with `x-text="rejectOrderNumber"`). |
| `Views/ChefDashboard/History.cshtml` | `/ChefDashboard/History` *(Action Needed)* | `_ChefDashboard.cshtml` (Tab 3) | Chef Order History table with status filter (`GET /ChefDashboard/History`) and 4 historical orders (`#HC-2026-8840`, `#HC-2026-8812`, `#HC-2026-8790`, `#HC-2026-8744`). |
| `Views/ChefDashboard/Reviews.cshtml` | `/ChefDashboard/Reviews` *(Action Needed)* | `_ChefDashboard.cshtml` (Tab 4) | Chef Customer Reviews page with `4.9 / 5.0` summary header and 3 verified customer review cards. |
| `Views/ChefDashboard/Wallet.cshtml` | `/ChefDashboard/Wallet` *(Action Needed)* | `_ChefDashboard.cshtml` (Tab 5) | Chef Wallet & Ledger page with `4,820.00 EGP` balance (`POST /ChefDashboard/RequestPayout`), `850.50 EGP` commission card, IBAN card, and 4-row financial ledger table. |
| `Views/ChefDashboard/Profile.cshtml` | `/ChefDashboard/Profile` | `_EditChefProfileModal.cshtml` | Chef kitchen profile settings form (`POST /ChefDashboard/Profile`) with kitchen name, specialty, neighborhood, prep time, min order, bio, and payout IBAN. |
| `Views/ChefDashboard/WorkingHours.cshtml` | `/ChefDashboard/WorkingHours` | `_ChefDashboard.cshtml` | Chef weekly operating schedule (`POST /ChefDashboard/WorkingHours`) for Saturday–Friday and vacation pause toggle. |
| `Views/ChefDashboard/Documents.cshtml` | `/ChefDashboard/Documents` | `_ChefOnboarding.cshtml` | Chef verification & hygiene compliance documents status and renewal upload form (`POST /ChefDashboard/UploadDocument`). |
| `Views/MenuManagement/Index.cshtml` | `/MenuManagement` | `_ChefDashboard.cshtml` (Tab 2) + `_DeleteDishConfirmDialog.cshtml` | Chef Menu Catalog (`4` dishes: `Baladi Duck Molokhia` 240/280 EGP, `Braised Oxtail Clay Pot` 275/310 EGP, `Pigeon Stuffed with Freekeh` 295 EGP, `Authentic Baladi Feteer Meshaltet` 160 EGP) with availability toggles and Delete Dish modal (`POST /MenuManagement/Delete`). |
| `Views/MenuManagement/Create.cshtml` | `/MenuManagement/Create` | `_AddDishModal.cshtml` | Add New Dish form (`POST /MenuManagement/Create`) with pricing, category, prep time, dietary tags, and inline Customization Option Groups builder. |
| `Views/MenuManagement/Edit.cshtml` | `/MenuManagement/Edit/{id}` | `_EditDishModal.cshtml` | Edit Dish form (`POST /MenuManagement/Edit/1`) pre-populated with `Baladi Duck Molokhia with Vermicelli Rice` (`240` / `280 EGP`) and inline Customization Option Groups builder. |
| `Views/MenuManagement/ManageOptions.cshtml` | `/MenuManagement/ManageOptions?dishId={id}` | `_AddDishModal.cshtml` / `_EditDishModal.cshtml` | Dish customization option groups manager (`POST /MenuManagement/AddOptionGroup`, `POST /MenuManagement/DeleteOptionGroup`) with dynamic Alpine option row builder. |
| `Views/Admin/Index.cshtml` | `/Admin` | `_AdminDashboard.cshtml` | Admin executive overview with `main`'s 4 KPI cards (`48,920.00 EGP` GMV, `7,338.00 EGP` 15% Commission, `3,240.00 EGP` Stripe Held Funds, `4 Verified` / `1 Pending` Kitchens) and quick overview tables. |
| `Views/Admin/Chefs.cshtml` | `/Admin/Chefs` | `_AdminDashboard.cshtml` (Tab 1) | Admin Chef Applications & Verification table (`5` kitchens including `Mama Nadia's Pastry` Pending) with reactive status filter tabs (`activeTab`), Document Inspection modal (`x-text="selectedChefName"`), and Approve/Reject/Suspend POST forms. |
| `Views/Admin/Orders.cshtml` | `/Admin/Orders` | `_AdminDashboard.cshtml` (Tab 2) + `_OrderSettlementModal.cshtml` | Admin Orders & Financial Settlement ledger (`4` orders `#HC-2026-9041`..`#HC-2026-8512`) with reactive status filter tabs and Order Settlement modal (`POST /Admin/SettleOrder`). |
| `Views/Admin/Users.cshtml` | `/Admin/Users` | `_AdminDashboard.cshtml` (Tab 3) | Admin User Accounts Management table (`5` users including customers & chefs) with role/search filter, Confirm Account Suspension modal (`POST /Admin/ToggleUserStatus`), and Points Adjustment modal (`POST /Admin/AdjustUserPoints`). |
| `Views/Admin/Reviews.cshtml` | `/Admin/Reviews` *(Action Needed)* | `_AdminDashboard.cshtml` (Tab 4 `SCRUM-552`) | Admin Review Moderation (`4` reviews `rev-1`..`rev-4`) with status filter pills and Approve/Reject POST forms. |
| `Views/Admin/Payouts.cshtml` | `/Admin/Payouts` | `_AdminPayoutModal.cshtml` + `_CashReconciliationModal.cshtml` | Admin Chef Payouts & Cash Commission Reconciliation page with `POST /Admin/ProcessPayout`, Recent Payout Disbursement Log table, and 5-column Chef Cash Collections Ledger table (`POST /Admin/ReconcileCash`). |
| `Views/Admin/Settings.cshtml` | `/Admin/Settings` | `_AIFaqManagement.cshtml` | Admin Platform Commission Settings (`POST /Admin/Settings`) and AI Assistant FAQ Knowledge Base manager (`faqSearch` filter, Add/Edit/Toggle/Delete FAQ forms). |
| `Views/Account/Login.cshtml` | `/Account/Login` | `_LoginModal.cshtml` | Sign-In page (`POST /Account/Login`) with `@* PROTOTYPE-ONLY *@` clickable demo credential pills (`Customer`, `Home Chef`, `Admin`). |
| `Views/Account/Register.cshtml` | `/Account/Register` | `_LoginModal.cshtml` (Register Tab) | Registration page (`POST /Account/Register`) with Customer / Home Chef role selector and live `passwordMismatch` check. |
| `Views/Account/ForgotPassword.cshtml` | `/Account/ForgotPassword` | `_ForgotPasswordModal.cshtml` (Step 1) | Password recovery request form (`POST /Account/ForgotPassword`). |
| `Views/Account/ResetPassword.cshtml` | `/Account/ResetPassword` | `_ForgotPasswordModal.cshtml` (Step 2) | Reset password form (`POST /Account/ResetPassword`) with 6-digit recovery code and live `passwordMismatch` validation. |
| `Views/Account/AccessDenied.cshtml` | `/Account/AccessDenied` | Scaffolded MVC View | 403 Access Denied screen with plain links to `/` and `/Account/Login`. |
| `Views/Support/Index.cshtml` | `/Support` | `_MySupportCases.cshtml` | Customer Support Cases inbox (`CASE-7701` In Progress, `CASE-7650` Resolved) with inline expandable message thread, reply form (`POST /Support/ReplyCase`), FAQ accordion, and link to `/Support/AgentDesk`. |
| `Views/Support/NewCase.cshtml` | `/Support/NewCase` | `_ContactSupportModal.cshtml` | Open New Support Case form (`POST /Support/NewCase`) with issue category pills, related order reference (`#HC-2026-9041`), subject, description, and photo proof upload. |
| `Views/Support/CaseDetails.cshtml` | `/Support/CaseDetails/{id}` | `_MySupportCases.cshtml` | Dedicated Support Case thread (`CASE-7701`) with right-aligned customer bubbles (`ms-auto bg-[var(--color-primary)] text-white`), left-aligned agent replies, reply form (`POST /Support/ReplyCase`), and Close Case form (`POST /Support/CloseCase`). |
| `Views/Support/AgentDesk.cshtml` | `/Support/AgentDesk` *(Action Needed)* | `_SupportPortal.cshtml` | Support Agent & Refunds Desk with Support Cases Queue (`CASE-7701`, `CASE-7702`), agent reply & status selector (`POST /Support/AgentReplyCase`), and Refund Approval Queue (`REF-2026-104`, `REF-2026-098`) with Approve/Reject modals. |
| `Views/Chat/Index.cshtml` | `/Chat` | `_ChatInboxPage.cshtml` | Unified 12-column Messages & Support Inbox with 3 threads (`Chef Amina Mansour` `#HC-2026-9041`, `Om Nabil` `#HC-2026-8890`, `HomeChef Support Desk` `CASE-7701`), reactive search filter, active conversation thread, and `POST /Chat/SendMessage` composer. |
| `Views/Chat/Conversation.cshtml` | `/Chat/Conversation/{id}` | `_ChatInboxPage.cshtml` | Conversation thread view (`/Chat/Conversation/1`) with mobile back button, 3-conversation sidebar, message history with `#check-check` read receipts, and `#paperclip` + `POST /Chat/SendMessage` composer. |
| `Views/AIAssistant/Index.cshtml` | `/AIAssistant` | `_AIAssistant.cshtml` | Full-page AI Food Concierge (`max-w-[1520px]`) with dark gradient header, `lg:col-span-8` prompt card (`POST /AIAssistant/Ask`), 2x2 preset cards, `Enable step-by-step thinking` checkbox, `@* PROTOTYPE-ONLY *@` reasoning/streaming preview, and `lg:col-span-4` 4 recommended dishes (`dish-1`, `dish-2`, `dish-3`, `dish-6`). |

---

## 5. Forms Inventory (`GET` & `POST`)

Every `method="post"` form includes `@Html.AntiForgeryToken()`.

| View File | Method | Action URL | Form Input `name` Attributes |
| :--- | :--- | :--- | :--- |
| `Views/Shared/Partials/_Header.cshtml` | `GET` | `/Dishes` | `search` |
| `Views/Shared/Partials/_Header.cshtml` | `POST` | `/Account/Logout` | *(AntiForgeryToken only)* |
| `Views/Shared/Partials/_Footer.cshtml` | `POST` | `/Home/Subscribe` | `email` |
| `Views/Shared/Partials/_DishCard.cshtml` | `POST` | `/Customer/ToggleFavorite` | `dishId` |
| `Views/Shared/Partials/_DishCard.cshtml` | `POST` | `/Cart/Add` | `dishId`, `quantity` |
| `Views/Shared/Partials/_ChefCard.cshtml` | `POST` | `/Customer/ToggleFavorite` | `chefId` |
| `Views/Shared/Partials/_FilterDrawer.cshtml` | `GET` | `/Dishes` or `/Chefs` | `availableOnly`, `minRating`, `maxPrepTime`, `dietary`, `minPrice`, `maxPrice` |
| `Views/Shared/Partials/_OptionsDialog.cshtml` | `POST` | `/Cart/Add` | `dishId`, `quantity`, `selectedOptions`, `specialInstructions` |
| `Views/Shared/Partials/_QuickViewModal.cshtml` | `POST` | `/Cart/Add` | `dishId`, `quantity` |
| `Views/Shared/Partials/_ChatModal.cshtml` | `POST` | `/Chat/SendMessage` | `conversationId`, `recipientName`, `orderNumber`, `message` |
| `Views/Shared/Partials/_CartConflictDialog.cshtml` | `POST` | `/Cart/ClearAndAdd` | `dishId`, `quantity` |
| `Views/Shared/Partials/_ThinkingPromptInput.cshtml` | `GET` | `/AIAssistant` | `q`, `think` |
| `Views/Dishes/Index.cshtml` | `GET` | `/Dishes` | `search`, `category`, `sort`, `availableOnly`, `minRating`, `maxPrepTime`, `dietary`, `minPrice`, `maxPrice` |
| `Views/Dishes/Index.cshtml` | `POST` | `/Cart/Add` | `dishId`, `quantity` |
| `Views/Dishes/Index.cshtml` | `POST` | `/Customer/ToggleFavorite` | `dishId` |
| `Views/Dishes/Details.cshtml` | `POST` | `/Cart/Add` | `dishId`, `quantity`, `spiceLevel`, `selectedOptionIds`, `specialInstructions` |
| `Views/Dishes/Details.cshtml` | `POST` | `/Customer/ToggleFavorite` | `dishId` |
| `Views/Chefs/Index.cshtml` | `GET` | `/Chefs` | `search`, `neighborhood`, `specialty`, `availableOnly`, `minRating` |
| `Views/Chefs/Index.cshtml` | `POST` | `/Customer/ToggleFavorite` | `chefId` |
| `Views/Chefs/Details.cshtml` | `GET` | `/Chefs/Details/1` | `search`, `category` |
| `Views/Chefs/Details.cshtml` | `POST` | `/Customer/ToggleFavorite` | `chefId` |
| `Views/Chefs/Details.cshtml` | `POST` | `/Cart/Add` | `dishId`, `quantity` |
| `Views/Chefs/Onboarding.cshtml` | `POST` | `/Chefs/Onboarding` | `kitchenNameEn`, `kitchenNameAr`, `specialty`, `experienceYears`, `bioEn`, `bioAr`, `neighborhood`, `pickupAddress`, `prepTimeMinutes`, `minOrderEgp`, `nationalIdNumber`, `idDocumentFile`, `kitchenPhotoFile`, `hygieneAck`, `payoutMethod`, `bankName`, `iban`, `accountHolderName`, `agreeCommission` (`enctype="multipart/form-data"`) |
| `Views/Cart/Index.cshtml` | `POST` | `/Cart/UpdateQuantity` | `cartItemId`, `quantity` |
| `Views/Cart/Index.cshtml` | `POST` | `/Cart/RemoveItem` | `cartItemId` |
| `Views/Cart/Index.cshtml` | `POST` | `/Cart/Clear` | *(AntiForgeryToken only)* |
| `Views/Cart/Index.cshtml` | `POST` | `/Cart/ApplyPromo` | `promoCode` |
| `Views/Checkout/Index.cshtml` | `POST` | `/Checkout` | `pickupSlot`, `pickupNotes`, `paymentMethod`, `cardHolderName`, `cardNumber`, `cardExpiry`, `cardCvc`, `usePoints` |
| `Views/Orders/Track.cshtml` | `POST` | `/Orders/Cancel` | `orderId`, `reason`, `notes` |
| `Views/Orders/Track.cshtml` | `POST` | `/Orders/RequestRefund` | `orderId`, `reason`, `amount`, `description`, `proofPhoto` (`enctype="multipart/form-data"`) |
| `Views/Reviews/Create.cshtml` | `POST` | `/Reviews/Create` | `orderId`, `rating`, `comment` |
| `Views/Customer/Profile.cshtml` | `POST` | `/Customer/Profile` | `fullName`, `email`, `phone`, `neighborhood`, `pickupInstructions` |
| `Views/Customer/Profile.cshtml` | `POST` | `/Customer/ChangePassword` | `currentPassword`, `newPassword`, `confirmPassword` |
| `Views/Customer/MyOrders.cshtml` | `POST` | `/Cart/Reorder` | `orderId` |
| `Views/Customer/Favorites.cshtml` | `POST` | `/Customer/ToggleFavorite` | `dishId` or `chefId` |
| `Views/Customer/Favorites.cshtml` | `POST` | `/Cart/Add` | `dishId`, `quantity` |
| `Views/Customer/Points.cshtml` | `POST` | `/Customer/RedeemPoints` | `rewardId`, `pointsCost` |
| `Views/ChefDashboard/Partials/_ChefNav.cshtml` | `POST` | `/ChefDashboard/ToggleAvailability` | `isBusy` |
| `Views/ChefDashboard/Index.cshtml` | `POST` | `/ChefDashboard/AcceptOrder` | `orderId` |
| `Views/ChefDashboard/Index.cshtml` | `POST` | `/ChefDashboard/UpdateOrderStatus` | `orderId`, `newStatus` |
| `Views/ChefDashboard/Orders.cshtml` | `POST` | `/ChefDashboard/AcceptOrder` | `orderId` |
| `Views/ChefDashboard/Orders.cshtml` | `POST` | `/ChefDashboard/UpdateOrderStatus` | `orderId`, `newStatus` |
| `Views/ChefDashboard/Orders.cshtml` | `POST` | `/ChefDashboard/RejectOrder` | `orderId`, `reason`, `note` |
| `Views/ChefDashboard/History.cshtml` | `GET` | `/ChefDashboard/History` | `status` |
| `Views/ChefDashboard/Wallet.cshtml` | `POST` | `/ChefDashboard/RequestPayout` | `amount`, `iban` |
| `Views/ChefDashboard/Profile.cshtml` | `POST` | `/ChefDashboard/Profile` | `kitchenNameEn`, `kitchenNameAr`, `specialty`, `neighborhood`, `prepTime`, `minOrder`, `pickupAddress`, `bioEn`, `iban`, `instapayHandle` |
| `Views/ChefDashboard/WorkingHours.cshtml` | `POST` | `/ChefDashboard/WorkingHours` | `isPaused`, `pauseUntilDate`, `days[0..6].dayOfWeek`, `days[0..6].isOpen`, `days[0..6].openTime`, `days[0..6].closeTime`, `days[0..6].orderCutoffMinutes` |
| `Views/ChefDashboard/Documents.cshtml` | `POST` | `/ChefDashboard/UploadDocument` | `documentType`, `expiryDate`, `documentFile`, `notes` (`enctype="multipart/form-data"`) |
| `Views/MenuManagement/Index.cshtml` | `POST` | `/MenuManagement/ToggleAvailability` | `dishId` |
| `Views/MenuManagement/Index.cshtml` | `POST` | `/MenuManagement/Delete` | `id` |
| `Views/MenuManagement/Create.cshtml` | `POST` | `/MenuManagement/Create` | `nameEn`, `nameAr`, `price`, `originalPrice`, `category`, `prepTime`, `calories`, `servingSize`, `descriptionEn`, `ingredients`, `preparationTips`, `imageUrl`, `imageFile`, `tags`, `optionGroupsJson` (`enctype="multipart/form-data"`) |
| `Views/MenuManagement/Edit.cshtml` | `POST` | `/MenuManagement/Edit/1` | `id`, `nameEn`, `nameAr`, `price`, `originalPrice`, `category`, `prepTime`, `calories`, `servingSize`, `descriptionEn`, `ingredients`, `preparationTips`, `imageUrl`, `imageFile`, `tags`, `available`, `optionGroupsJson` (`enctype="multipart/form-data"`) |
| `Views/MenuManagement/ManageOptions.cshtml` | `POST` | `/MenuManagement/AddOptionGroup` | `dishId`, `groupName`, `isRequired`, `selectionType`, `optionNames[]`, `optionPrices[]` |
| `Views/MenuManagement/ManageOptions.cshtml` | `POST` | `/MenuManagement/DeleteOptionGroup` | `dishId`, `optionGroupId` |
| `Views/Admin/Chefs.cshtml` | `GET` | `/Admin/Chefs` | `status`, `search` |
| `Views/Admin/Chefs.cshtml` | `POST` | `/Admin/ApproveChef` | `chefId` |
| `Views/Admin/Chefs.cshtml` | `POST` | `/Admin/RejectChef` | `chefId`, `reason` |
| `Views/Admin/Chefs.cshtml` | `POST` | `/Admin/ToggleChefStatus` | `chefId`, `status` |
| `Views/Admin/Orders.cshtml` | `GET` | `/Admin/Orders` | `status`, `search` |
| `Views/Admin/Orders.cshtml` | `POST` | `/Admin/SettleOrder` | `orderId`, `resolutionType`, `refundAmount`, `chefPayoutAdjustment`, `adminNotes` |
| `Views/Admin/Users.cshtml` | `GET` | `/Admin/Users` | `role`, `search` |
| `Views/Admin/Users.cshtml` | `POST` | `/Admin/ToggleUserStatus` | `userId`, `status`, `reason` |
| `Views/Admin/Users.cshtml` | `POST` | `/Admin/AdjustUserPoints` | `userId`, `pointsDelta`, `reason` |
| `Views/Admin/Reviews.cshtml` | `GET` | `/Admin/Reviews` | `status` |
| `Views/Admin/Reviews.cshtml` | `POST` | `/Admin/ApproveReview` | `reviewId` |
| `Views/Admin/Reviews.cshtml` | `POST` | `/Admin/RejectReview` | `reviewId` |
| `Views/Admin/Payouts.cshtml` | `POST` | `/Admin/ProcessPayout` | `chefId`, `amount`, `payoutMethod`, `referenceNumber`, `notes` |
| `Views/Admin/Payouts.cshtml` | `POST` | `/Admin/ReconcileCash` | `chefId`, `collectedAmount`, `receiptReference`, `notes` |
| `Views/Admin/Settings.cshtml` | `POST` | `/Admin/Settings` | `platformCommissionPercent`, `serviceFeeFixed`, `minOrderAmount`, `loyaltyPointsPerHundredEgp`, `maintenanceMode`, `allowNewChefRegistrations` |
| `Views/Admin/Settings.cshtml` | `POST` | `/Admin/SaveFaqEntry` | `faqId`, `questionEn`, `questionAr`, `answerEn`, `answerAr`, `category`, `keywords`, `active` |
| `Views/Admin/Settings.cshtml` | `POST` | `/Admin/ToggleFaqStatus` | `faqId` |
| `Views/Admin/Settings.cshtml` | `POST` | `/Admin/DeleteFaqEntry` | `faqId` |
| `Views/Account/Login.cshtml` | `POST` | `/Account/Login` | `returnUrl`, `email`, `password`, `rememberMe` |
| `Views/Account/Register.cshtml` | `POST` | `/Account/Register` | `role`, `fullName`, `email`, `phone`, `neighborhood`, `password`, `confirmPassword`, `agreeTerms` |
| `Views/Account/ForgotPassword.cshtml` | `POST` | `/Account/ForgotPassword` | `email` |
| `Views/Account/ResetPassword.cshtml` | `POST` | `/Account/ResetPassword` | `email`, `resetCode`, `newPassword`, `confirmPassword` |
| `Views/Support/Index.cshtml` | `POST` | `/Support/ReplyCase` | `caseId`, `message` |
| `Views/Support/NewCase.cshtml` | `POST` | `/Support/NewCase` | `category`, `orderId`, `subject`, `description`, `attachmentFile` (`enctype="multipart/form-data"`) |
| `Views/Support/CaseDetails.cshtml` | `POST` | `/Support/ReplyCase` | `caseId`, `message`, `attachmentFile` (`enctype="multipart/form-data"`) |
| `Views/Support/CaseDetails.cshtml` | `POST` | `/Support/CloseCase` | `caseId` |
| `Views/Support/AgentDesk.cshtml` | `POST` | `/Support/AgentReplyCase` | `caseId`, `status`, `replyMessage` |
| `Views/Support/AgentDesk.cshtml` | `POST` | `/Support/ApproveRefund` | `refundId`, `orderId`, `amount` |
| `Views/Support/AgentDesk.cshtml` | `POST` | `/Support/RejectRefund` | `refundId`, `reason` |
| `Views/Chat/Index.cshtml` | `GET` | `/Chat` | `search` |
| `Views/Chat/Index.cshtml` | `POST` | `/Chat/SendMessage` | `conversationId`, `message` |
| `Views/Chat/Conversation.cshtml` | `POST` | `/Chat/SendMessage` | `conversationId`, `message` |
| `Views/AIAssistant/Index.cshtml` | `POST` | `/AIAssistant/Ask` | `prompt`, `thinkEnabled` |

---

## 6. Remaining Local Alpine.js Usage (`x-data`)

Alpine.js is no longer used for routing, global state, or client-side data array filtering. Every remaining `x-data` attribute is strictly local visual UI state:

| View / Partial | Local `x-data` State & Visual Behavior |
| :--- | :--- |
| `Views/Shared/Partials/_Header.cshtml` | `{ mobileMenuOpen: false, profileDropdownOpen: false, cartDrawerOpen: false, isDark: ... }` — mobile drawer, logged-in profile dropdown, slide-over cart preview drawer, and dark/light mode icon toggle. |
| `Views/Shared/Partials/_PrototypeBar.cshtml` | `{ currentRole: 'customer', isDark: ... }` — prototype role pill highlight and dark/light mode toggle (`@* PROTOTYPE-ONLY *@`). |
| `Views/Shared/Partials/_FilterDrawer.cshtml` | `{ open: false, availableOnly: false, minRating: 0, maxPrepTime: 0, dietary: [], minPrice: '', maxPrice: '' }` — slide-over filter drawer open/close and visual pill selection synced to hidden `<form method="get">` inputs. |
| `Views/Shared/Partials/_OptionsDialog.cshtml` | `{ open: false, dishTitle: '...', basePrice: 240, qty: 1, spice: 'Medium (Standard)', extras: [], notes: '' }` — customization option group selection and live price preview inside `POST /Cart/Add` form. |
| `Views/Shared/Partials/_QuickViewModal.cshtml` | `{ open: false, qty: 1, dish: { ... } }` — dish quick-view modal populated via `@@open-quick-view.window`. |
| `Views/Shared/Partials/_ChatModal.cshtml` | `{ open: false, recipientName: '...', orderNumber: '...' }` — floating chat modal populated via `@@open-chat-modal.window`. |
| `Views/Shared/Partials/_CartConflictDialog.cshtml` | `{ open: false, dishId: '1', dishName: '...', newChefName: '...' }` — different-chef cart warning dialog. |
| `Views/Shared/Partials/_CoverflowCarousel.cshtml` | `initGallery3D(...)` (`selected: 0..5`) — 3D coverflow carousel and reactive 6-slide caption switcher. |
| `Views/Shared/Partials/_ThinkingPromptInput.cshtml` | `initThinkingPrompt(...)` — home page AI prompt input with step-by-step thinking toggle. |
| `Views/Dishes/Index.cshtml` | `{ viewMode: 'grid' }` — Grid View vs. `.dish-list-container` List View toggle. |
| `Views/Dishes/Details.cshtml` | `{ activeImageIndex: 0, zoomOpen: false, quantity: 1, selectedSpice: 'Medium (Standard)', selectedExtras: [], specialNote: '', openAccordion: 'description', isFav: true, ... }` — 3D vertical stacked image gallery (`.dish-gallery-frame-690` / `.dish-photo-card-420-520`), fullscreen zoom lightbox, live customization price calculator, and 3 collapsible accordions. |
| `Views/Chefs/Index.cshtml` | `{ viewMode: 'grid' }` — Grid View vs. `.chef-list-container` List View toggle. |
| `Views/Chefs/Details.cshtml` | `{ selectedCategory: 'all', scheduleOpen: false, isFav: true }` — category pill filter, Kitchen Schedule modal, and favorite heart toggle. |
| `Views/Chefs/Onboarding.cshtml` | `{ step: 1, ... }` — 3-step chef onboarding wizard progression. |
| `Views/Cart/Index.cshtml` | `{ promoCode: '', appliedPromo: '', usePoints: false, ... }` — visual-only promo code (`HOME20`/`WELCOME50`) & loyalty points (`-25 EGP`) discount preview (`TODO(backend)`). |
| `Views/Checkout/Index.cshtml` | `{ selectedSlot: '14:00 - 14:30', paymentMethod: 'card', usePoints: false, ... }` — 8-slot pickup selector, Cash vs. Card fields toggle, and loyalty points toggle. |
| `Views/Orders/Track.cshtml` | `{ currentStatus: 'Preparing', cancelModalOpen: false, refundModalOpen: false }` — prototype status stepper preview (`@* PROTOTYPE-ONLY *@`), Cancel Order modal, and Request Refund modal. |
| `Views/Reviews/Create.cshtml` | `{ rating: 5, hoverRating: 0, comment: '', ratingLabels: [...] }` — interactive 5-star rating selector with live word labels. |
| `Views/Customer/Favorites.cshtml` | `{ favSubTab: 'dishes' }` — Saved Dishes (3) vs. Saved Kitchens (2) tab switcher. |
| `Views/ChefDashboard/Orders.cshtml` | `{ rejectModalOpen: false, rejectOrderId: '...', rejectOrderNumber: '...' }` — Reject Order confirmation modal. |
| `Views/MenuManagement/Index.cshtml` | `{ deleteModalOpen: false, deleteDishId: '1', deleteDishName: '...' }` — Delete Dish confirmation modal. |
| `Views/MenuManagement/Create.cshtml`, `Edit.cshtml`, `ManageOptions.cshtml` | `{ optionGroups: [...] }` — dynamic customization option group & choice row builder. |
| `Views/Admin/Chefs.cshtml`, `Orders.cshtml`, `Users.cshtml`, `Reviews.cshtml`, `Payouts.cshtml`, `Settings.cshtml` | Local modal toggles (`inspectModalOpen`, `settlementModalOpen`, `showSuspendModal`, `pointsModalOpen`, `payoutModalOpen`, `cashModalOpen`, `faqModalOpen`) and visual filter tabs (`activeTab`, `faqSearch`). |
| `Views/Support/Index.cshtml`, `AgentDesk.cshtml` | `{ activeCaseId: 'CASE-7701', openFaq: 1 }`, `{ activeTab: 'cases', rejectRefundModalOpen: false }` — expandable support case thread, FAQ accordion, and agent refund rejection modal. |
| `Views/AIAssistant/Index.cshtml` | `{ prompt: '', thinkEnabled: true, status: 'idle', ... }` — preset prompt cards and `@* PROTOTYPE-ONLY *@` thinking/streaming preview. |
