/**
 * ISOFlow Advance Search & Filter Engine
 * Reusable client-side multi-criteria search and filter utility for tables and list cards.
 */

class AdvanceSearch {
    constructor(config) {
        this.containerId = config.containerId; // ID of table tbody or card container
        this.searchInputId = config.searchInputId; // ID of search text input
        this.filterSelects = config.filterSelects || []; // Array of { id: string, colIndex?: number, dataAttr?: string }
        this.counterId = config.counterId; // ID of element showing count
        this.resetBtnId = config.resetBtnId; // ID of reset button
        this.itemSelector = config.itemSelector || 'tr'; // 'tr' for tables, '.card-enterprise' for cards
        this.emptyStateId = config.emptyStateId; // ID of empty state element

        this.init();
    }

    init() {
        this.container = document.getElementById(this.containerId);
        this.searchInput = document.getElementById(this.searchInputId);
        this.counter = this.counterId ? document.getElementById(this.counterId) : null;
        this.resetBtn = this.resetBtnId ? document.getElementById(this.resetBtnId) : null;

        if (!this.container) return;

        // Attach search input listener
        if (this.searchInput) {
            this.searchInput.addEventListener('input', () => this.applyFilter());
        }

        // Attach dropdown filter listeners
        this.filterSelects.forEach(f => {
            const el = document.getElementById(f.id);
            if (el) {
                el.addEventListener('change', () => this.applyFilter());
            }
        });

        // Attach reset button listener
        if (this.resetBtn) {
            this.resetBtn.addEventListener('click', () => this.resetFilters());
        }

        // Initial count
        this.updateCounter(this.getItems().length, this.getItems().length);
    }

    getItems() {
        if (!this.container) return [];
        return Array.from(this.container.querySelectorAll(this.itemSelector));
    }

    applyFilter() {
        const query = (this.searchInput?.value || '').toLowerCase().trim();
        const items = this.getItems();
        let visibleCount = 0;

        // Get filter criteria
        const activeFilters = this.filterSelects.map(f => {
            const el = document.getElementById(f.id);
            return {
                val: el ? el.value.toLowerCase().trim() : '',
                colIndex: f.colIndex,
                dataAttr: f.dataAttr
            };
        }).filter(f => f.val !== '' && f.val !== 'all');

        items.forEach(item => {
            let matchesSearch = true;
            let matchesDropdowns = true;

            // 1. Text Search
            if (query) {
                const text = item.textContent.toLowerCase();
                matchesSearch = text.includes(query);
            }

            // 2. Dropdown Filters
            if (matchesSearch && activeFilters.length > 0) {
                for (const filter of activeFilters) {
                    if (filter.colIndex !== undefined) {
                        const cells = item.querySelectorAll('td');
                        if (cells.length > filter.colIndex) {
                            const cellText = cells[filter.colIndex].textContent.toLowerCase().trim();
                            if (!cellText.includes(filter.val)) {
                                matchesDropdowns = false;
                                break;
                            }
                        }
                    } else if (filter.dataAttr) {
                        const attrVal = (item.getAttribute(filter.dataAttr) || '').toLowerCase().trim();
                        if (attrVal !== filter.val) {
                            matchesDropdowns = false;
                            break;
                        }
                    }
                }
            }

            const shouldShow = matchesSearch && matchesDropdowns;
            item.style.display = shouldShow ? '' : 'none';
            if (shouldShow) visibleCount++;
        });

        this.updateCounter(visibleCount, items.length);
        this.toggleEmptyState(visibleCount);
    }

    resetFilters() {
        if (this.searchInput) this.searchInput.value = '';
        this.filterSelects.forEach(f => {
            const el = document.getElementById(f.id);
            if (el) el.value = el.options[0]?.value || '';
        });
        this.applyFilter();
    }

    updateCounter(visible, total) {
        if (this.counter) {
            this.counter.innerHTML = `Showing <strong>${visible}</strong> of <strong>${total}</strong> record${total === 1 ? '' : 's'}`;
        }
    }

    toggleEmptyState(visibleCount) {
        if (this.emptyStateId) {
            const emptyEl = document.getElementById(this.emptyStateId);
            if (emptyEl) {
                emptyEl.style.display = visibleCount === 0 ? 'block' : 'none';
            }
        }
    }
}

window.AdvanceSearch = AdvanceSearch;
