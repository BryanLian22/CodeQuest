/* Purpose: Adds accessible mobile navigation toggles, role-aware menus and page-transition animation hooks. */
(function () {
    "use strict";

    // Menu state helpers keep the generated button and navigation ARIA state aligned.
    function closeMenu(navigation, button) {
        navigation.classList.remove("open");
        button.setAttribute("aria-expanded", "false");
        button.setAttribute("aria-label", "Open navigation");
    }

    function createMenuButton(header, navigation, index) {
        var button = header.querySelector(".menu-button");
        if (button) {
            return button;
        }

        if (!navigation.id) {
            navigation.id = "codequestNavigation" + index;
        }

        button = document.createElement("button");
        button.type = "button";
        button.className = "menu-button";
        button.setAttribute("aria-label", "Open navigation");
        button.setAttribute("aria-expanded", "false");
        button.setAttribute("aria-controls", navigation.id);
        button.innerHTML = "<span></span><span></span><span></span>";
        header.insertBefore(button, navigation);
        return button;
    }

    function addMobileAccountActions(header, navigation) {
        if (navigation.querySelector(".mobile-nav-actions")) {
            return;
        }

        var desktopActions = header.querySelector(".header-actions");
        var links = desktopActions ? desktopActions.querySelectorAll("a") : [];
        if (!links.length) {
            links = Array.prototype.filter.call(header.children, function (element) {
                return element.tagName === "A" && element.classList.contains("header-cta");
            });
        }

        if (!links.length) {
            return;
        }

        var mobileActions = document.createElement("div");
        mobileActions.className = "mobile-nav-actions";

        Array.prototype.forEach.call(links, function (link) {
            var clone = link.cloneNode(true);
            clone.removeAttribute("id");
            mobileActions.appendChild(clone);
        });

        navigation.appendChild(mobileActions);
    }

    // Bind keyboard, pointer and viewport behaviour once for each shared site header.
    function initialiseHeader(header, index) {
        var navigation = header.querySelector(".main-nav");
        if (!navigation) {
            return;
        }

        var button = createMenuButton(header, navigation, index);
        addMobileAccountActions(header, navigation);

        if (button.getAttribute("data-codequest-menu-bound") === "true") {
            return;
        }

        button.setAttribute("data-codequest-menu-bound", "true");
        button.addEventListener("click", function () {
            var isOpen = navigation.classList.toggle("open");
            button.setAttribute("aria-expanded", isOpen ? "true" : "false");
            button.setAttribute("aria-label", isOpen ? "Close navigation" : "Open navigation");
        });

        navigation.addEventListener("click", function (event) {
            var target = event.target;
            while (target && target !== navigation && target.tagName !== "A") {
                target = target.parentNode;
            }

            if (target && target.tagName === "A") {
                closeMenu(navigation, button);
            }
        });

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                closeMenu(navigation, button);
                button.focus();
            }
        });

        document.addEventListener("click", function (event) {
            if (!header.contains(event.target)) {
                closeMenu(navigation, button);
            }
        });

        window.addEventListener("resize", function () {
            if (window.innerWidth > 980) {
                closeMenu(navigation, button);
            }
        });
    }

    function initialiseNavigation() {
        Array.prototype.forEach.call(document.querySelectorAll(".site-header"), initialiseHeader);
    }

    // Run immediately for late-loaded scripts, or wait until the markup is available.
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initialiseNavigation);
    } else {
        initialiseNavigation();
    }
}());
