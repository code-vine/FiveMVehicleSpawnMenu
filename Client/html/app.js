let allVehicleData = {};
let searchTimer;

const tabContainerEl = document.getElementById("tab-container");
const contentContainerEl = document.getElementById("tab-content-container");
const searchInput = document.getElementById("vehicle-search");

// Debounced search handler
searchInput.addEventListener("input", () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => {
        const query = searchInput.value.trim().toLowerCase();
        renderCategories(allVehicleData, query);
    }, 150); // Fast but smooth
});

function renderCategories(vehicleData, query = "") {
    tabContainerEl.innerHTML = "";
    contentContainerEl.innerHTML = "";

    const tabContainer = document.createElement("ul");
    tabContainer.className = "nav nav-tabs";

    const contentContainer = document.createElement("div");
    contentContainer.className = "tab-content";

    Object.entries(vehicleData).forEach(([category, vehicles], index) => {
        // Filter vehicles if query is active
        const filteredVehicles = query
            ? vehicles.filter(v =>
                (v.name && v.name.toLowerCase().includes(query)) ||
                (v.displayName && v.displayName.toLowerCase().includes(query)) ||
                (v.category && v.category.replace(/\s+/g, "").toLowerCase().includes(query.replace(/\s+/g, "")))

            )
            : vehicles;

        if (filteredVehicles.length === 0) return;

        const tabId = `tab-${category.toLowerCase().replace(/[^a-z0-9]+/g, "-")}`;
        const active = index === 0 ? "active" : "";

        // Create tab
        const tab = document.createElement("li");
        tab.className = "nav-item";
        tab.innerHTML = `
            <a class="nav-link ${active}" id="${tabId}-tab" data-bs-toggle="tab"
               href="#${tabId}" role="tab" aria-controls="${tabId}" aria-selected="${active === "active"}" tabindex="0">
                ${toTitleCase(category)}
            </a>
        `;
        tabContainer.appendChild(tab);

        // Create pane
        const tabPane = document.createElement("div");
        tabPane.className = `tab-pane fade ${active ? "show active" : ""}`;
        tabPane.id = tabId;
        tabPane.setAttribute("role", "tabpanel");
        tabPane.setAttribute("aria-labelledby", `${tabId}-tab`);

        const fragment = document.createDocumentFragment();
        filteredVehicles.forEach(vehicle => {
            // Defensive: skip if missing required fields
            if (!vehicle.name || !vehicle.image) return;

            const row = document.createElement("div");
            row.className = "vehicle-entry row align-items-center mb-2";
            row.innerHTML = `
                <div class="vehicle-entry">
                    <h5 class="vehicle-name">${vehicle.displayName || vehicle.name}</h5>
                    <img src="images/${vehicle.image}" alt="${vehicle.displayName || vehicle.name}" class="img-fluid" loading="lazy">
                    <button class="btn btn-sm" onclick="spawnVehicle('${vehicle.name}')">
                        Spawn
                    </button>
                </div>
            `;

            fragment.appendChild(row);
        });

        tabPane.appendChild(fragment);
        contentContainer.appendChild(tabPane);
    });

    tabContainerEl.appendChild(tabContainer);
    contentContainerEl.appendChild(contentContainer);
}

function toTitleCase(str) {
    return str
        .toLowerCase()
        .split(" ")
        .map(word => word.charAt(0).toUpperCase() + word.slice(1))
        .join(" ");
}


function spawnVehicle(model) {
    fetch(`https://${GetParentResourceName()}/spawn_vehicle`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ model })
    });
}

function closeMenu() {
    if (document.body.classList.contains("fade-out") || document.body.style.display === "none") return;

    document.body.classList.add("fade-out");

    // Notify backend first
    fetch(`https://${GetParentResourceName()}/close_menu`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({})
    }).then(() => {
        setTimeout(() => {
            document.body.style.display = "none";
            document.body.classList.remove("fade-out");
        }, 300);
    });
}

window.addEventListener("keydown", (event) => {
    if (event.key === "Escape" || event.key === "F3") {
        closeMenu();
    }
});

window.addEventListener("message", (event) => {
    if (event.data.type === "show") {
        if (event.data.vehicles) {
            allVehicleData = event.data.vehicles;
            renderCategories(allVehicleData);
            document.body.style.display = "block";
            document.body.classList.add("fade-in");

            setTimeout(() => {
                document.body.classList.remove("fade-in");
            }, 300);

            document.body.classList.remove("fade-out");
            document.body.style.opacity = "1";
        } else {
            closeMenu();
        }
    }
});

console.log("Vehicle menu loaded");