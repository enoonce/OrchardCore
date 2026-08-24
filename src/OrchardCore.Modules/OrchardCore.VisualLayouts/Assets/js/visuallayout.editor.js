(function () {
    'use strict';

    var BLOCK_TYPES = {
        Text: [{ name: 'text', label: 'Text', tag: 'input' }],
        Html: [{ name: 'html', label: 'HTML', tag: 'textarea' }],
        Widget: [{ name: 'contentItemId', label: 'Content item ID', tag: 'input' }],
        Shape: [{ name: 'name', label: 'Shape name', tag: 'input' }]
    };

    var COLUMN_PRESETS = [
        { width: 12, label: '100%' },
        { width: 9, label: '75%' },
        { width: 8, label: '66%' },
        { width: 6, label: '50%' },
        { width: 4, label: '33%' },
        { width: 3, label: '25%' }
    ];

    function Editor(rootElement, stateElement) {
        this.root = rootElement;
        this.stateElement = stateElement;
        this.state = null;
        this.dragging = null;

        this.bind();
        this.load();
    }

    Editor.prototype.load = function () {
        try {
            this.state = JSON.parse(this.stateElement.value || '{}');
        } catch (e) {
            this.state = {};
        }

        if (!Array.isArray(this.state.rows)) {
            this.state.rows = [];
        }

        this.render();
    };

    Editor.prototype.save = function () {
        this.stateElement.value = JSON.stringify(this.state);
        this.stateElement.dispatchEvent(new Event('change', { bubbles: true }));
    };

    Editor.prototype.bind = function () {
        var self = this;

        this.root.addEventListener('click', function (e) {
            var actionTarget = e.target.closest('[data-vl-action]');
            if (!actionTarget) {
                return;
            }

            e.preventDefault();

            var action = actionTarget.getAttribute('data-vl-action');
            var rowEl = actionTarget.closest('[data-vl-level=row]');
            var colEl = actionTarget.closest('[data-vl-level=column]');
            var blockEl = actionTarget.closest('[data-vl-level=block]');
            var r = rowEl ? parseInt(rowEl.getAttribute('data-r'), 10) : -1;
            var c = colEl ? parseInt(colEl.getAttribute('data-c'), 10) : -1;
            var b = blockEl ? parseInt(blockEl.getAttribute('data-b'), 10) : -1;
            var row = self.state.rows[r];
            var column = row && row.columns[c];
            var block = column && column.blocks[b];

            switch (action) {
                case 'add-row':
                    self.state.rows.push({ cssClasses: '', columns: [] });
                    break;
                case 'remove-row':
                    self.state.rows.splice(r, 1);
                    break;
                case 'move-row-up':
                    if (r > 0) {
                        self.state.rows.splice(r - 1, 0, self.state.rows.splice(r, 1)[0]);
                    }
                    break;
                case 'move-row-down':
                    if (r < self.state.rows.length - 1) {
                        self.state.rows.splice(r + 1, 0, self.state.rows.splice(r, 1)[0]);
                    }
                    break;
                case 'add-column': {
                    var width = parseInt(actionTarget.getAttribute('data-width'), 10);
                    row.columns.push({ widthLg: width, cssClasses: '', blocks: [] });
                    break;
                }
                case 'remove-column':
                    row.columns.splice(c, 1);
                    break;
                case 'add-block': {
                    var menu = actionTarget.parentElement.querySelector('.vl-block-menu');
                    if (menu) {
                        menu.classList.toggle('d-none');
                    }
                    return;
                }
                case 'create-block': {
                    var type = actionTarget.getAttribute('data-type');
                    var properties = {};
                    var defs = BLOCK_TYPES[type] || [];
                    for (var i = 0; i < defs.length; i++) {
                        properties[defs[i].name] = '';
                    }
                    column.blocks.push({ type: type, properties: properties });
                    break;
                }
                case 'remove-block':
                    column.blocks.splice(b, 1);
                    break;
                default:
                    return;
            }

            self.render();
            self.save();
        });

        this.root.addEventListener('change', function (e) {
            var target = e.target;
            if (!target.hasAttribute('data-prop')) {
                return;
            }

            var colEl = target.closest('[data-vl-level=column]');
            var blockEl = target.closest('[data-vl-level=block]');
            var r = parseInt(target.closest('[data-vl-level]').getAttribute('data-r'), 10);
            var c = parseInt(colEl.getAttribute('data-c'), 10);
            var prop = target.getAttribute('data-prop');

            if (blockEl) {
                var b = parseInt(blockEl.getAttribute('data-b'), 10);
                self.state.rows[r].columns[c].blocks[b].properties[prop] = target.value;
            } else {
                if (prop === 'widthLg') {
                    self.state.rows[r].columns[c].widthLg = parseInt(target.value, 10) || 12;
                } else {
                    self.state.rows[r].columns[c][prop] = target.value;
                }
            }

            self.save();
        });

        this.root.addEventListener('dragstart', function (e) {
            var level = e.target.closest('[data-vl-level]');
            if (!level || !level.hasAttribute('draggable')) {
                return;
            }

            var payload = {
                t: level.getAttribute('data-vl-level'),
                r: parseInt(level.getAttribute('data-r'), 10),
                c: level.hasAttribute('data-c') ? parseInt(level.getAttribute('data-c'), 10) : -1,
                b: level.hasAttribute('data-b') ? parseInt(level.getAttribute('data-b'), 10) : -1
            };

            self.dragging = payload;
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/plain', JSON.stringify(payload));
            level.classList.add('vl-dragging');
        });

        this.root.addEventListener('dragend', function () {
            self.dragging = null;
            self.root.querySelectorAll('.vl-dragging, .vl-drag-over').forEach(function (el) {
                el.classList.remove('vl-dragging');
                el.classList.remove('vl-drag-over');
            });
        });

        this.root.addEventListener('dragover', function (e) {
            var info = self.resolveDrop(e);
            if (!info) {
                return;
            }

            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';

            self.root.querySelectorAll('.vl-drag-over').forEach(function (el) { el.classList.remove('vl-drag-over'); });
            (info.item || info.zone).classList.add('vl-drag-over');
        });

        this.root.addEventListener('drop', function (e) {
            var info = self.resolveDrop(e);
            if (!info) {
                return;
            }

            e.preventDefault();

            self.move(self.dragging, info.dst);
            self.render();
            self.save();
        });
    };

    Editor.prototype.resolveDrop = function (e) {
        var zone = e.target.closest('[data-vl-drop]');
        if (!zone || !this.dragging) {
            return null;
        }

        var type = this.dragging.t;

        if (zone.getAttribute('data-vl-drop').indexOf(type) === -1) {
            return null;
        }

        var dst = {
            t: type,
            r: parseInt(zone.getAttribute('data-r'), 10),
            c: zone.hasAttribute('data-c') ? parseInt(zone.getAttribute('data-c'), 10) : -1,
            b: -1,
            before: false
        };

        var item = e.target.closest('[data-vl-item="' + type + '"]');

        if (item) {
            if (type !== 'row' && item.hasAttribute('data-c')) {
                dst.c = parseInt(item.getAttribute('data-c'), 10);
            }

            if (item.hasAttribute('data-b')) {
                dst.b = parseInt(item.getAttribute('data-b'), 10);
            }

            // Insert before the hovered item when the pointer is on its upper (or left)
            // half, after it otherwise.
            var rect = item.getBoundingClientRect();
            var horizontal = type === 'column';
            var position = horizontal ? e.clientX : e.clientY;
            var middle = horizontal ? rect.left + rect.width / 2 : rect.top + rect.height / 2;

            dst.before = position < middle;
        }

        if (isNaN(dst.r) || dst.r < 0) {
            dst.r = -1;
        }

        if (isNaN(dst.c) || dst.c < 0) {
            dst.c = -1;
        }

        if (isNaN(dst.b) || dst.b < 0) {
            dst.b = -1;
        }

        return { zone: zone, item: item, dst: dst };
    };

    Editor.prototype.move = function (src, dst) {
        var removed;

        if (src.t === 'row') {
            var rows = this.state.rows;

            if (!rows[src.r]) {
                return;
            }

            var insertAt = dst.before ? dst.r : dst.r + 1;

            if (insertAt < 0) {
                return;
            }

            removed = rows.splice(src.r, 1)[0];

            if (src.r < insertAt) {
                insertAt--;
            }

            rows.splice(insertAt, 0, removed);
        } else if (src.t === 'column') {
            var sourceRow = this.state.rows[src.r];
            var destinationRow = this.state.rows[dst.r];

            if (!sourceRow || !destinationRow || !sourceRow.columns[src.c]) {
                return;
            }

            var columnInsertAt = dst.c >= 0
                ? (dst.before ? dst.c : dst.c + 1)
                : destinationRow.columns.length;

            var sameRow = src.r === dst.r;

            removed = sourceRow.columns.splice(src.c, 1)[0];

            if (sameRow && src.c < columnInsertAt) {
                columnInsertAt--;
            }

            destinationRow.columns.splice(columnInsertAt, 0, removed);
        } else if (src.t === 'block') {
            var sourceColumn = this.state.rows[src.r] && this.state.rows[src.r].columns[src.c];
            var destinationColumn = this.state.rows[dst.r] && this.state.rows[dst.r].columns[dst.c];

            if (!sourceColumn || !destinationColumn || !sourceColumn.blocks[src.b]) {
                return;
            }

            var blockInsertAt = dst.b >= 0
                ? (dst.before ? dst.b : dst.b + 1)
                : destinationColumn.blocks.length;

            var sameColumn = src.r === dst.r && src.c === dst.c;

            removed = sourceColumn.blocks.splice(src.b, 1)[0];

            if (sameColumn && src.b < blockInsertAt) {
                blockInsertAt--;
            }

            destinationColumn.blocks.splice(blockInsertAt, 0, removed);
        }
    };

    Editor.prototype.render = function () {
        var self = this;
        var canvas = this.root.querySelector('[data-vl-canvas]');
        canvas.textContent = '';

        if (this.state.rows.length === 0) {
            var empty = document.createElement('div');
            empty.className = 'alert alert-info mb-0';
            empty.textContent = this.root.getAttribute('data-empty-text');
            canvas.appendChild(empty);
        }

        this.state.rows.forEach(function (row, r) {
            canvas.appendChild(self.renderRow(row, r));
        });
    };

    Editor.prototype.renderRow = function (row, r) {
        var self = this;
        var rowEl = document.createElement('div');
        rowEl.className = 'card mb-3 vl-row';
        rowEl.setAttribute('data-vl-level', 'row');
        rowEl.setAttribute('data-vl-item', 'row');
        rowEl.setAttribute('data-vl-drop', 'row');
        rowEl.setAttribute('data-r', r);
        rowEl.setAttribute('draggable', 'true');

        var header = document.createElement('div');
        header.className = 'card-header py-2 d-flex align-items-center gap-2';

        var grip = document.createElement('i');
        grip.className = 'fa-solid fa-grip-vertical text-muted';
        grip.title = this.root.getAttribute('data-drag-text');
        header.appendChild(grip);

        var title = document.createElement('strong');
        title.textContent = this.root.getAttribute('data-row-text');
        title.className = 'me-auto';
        header.appendChild(title);

        var presetsWrap = document.createElement('div');
        presetsWrap.className = 'dropdown';
        var addColBtn = document.createElement('button');
        addColBtn.type = 'button';
        addColBtn.className = 'btn btn-sm btn-secondary';
        addColBtn.setAttribute('data-bs-toggle', 'dropdown');
        addColBtn.setAttribute('data-vl-action', 'add-column-toggle');
        addColBtn.textContent = this.root.getAttribute('data-add-column-text');
        presetsWrap.appendChild(addColBtn);

        var menu = document.createElement('ul');
        menu.className = 'dropdown-menu dropdown-menu-end';
        COLUMN_PRESETS.forEach(function (preset) {
            var li = document.createElement('li');
            var link = document.createElement('a');
            link.className = 'dropdown-item';
            link.href = '#';
            link.setAttribute('data-vl-action', 'add-column');
            link.setAttribute('data-width', preset.width);
            link.textContent = preset.label;
            li.appendChild(link);
            menu.appendChild(li);
        });
        presetsWrap.appendChild(menu);
        header.appendChild(presetsWrap);

        header.appendChild(this.iconButton('arrow-up', 'move-row-up'));
        header.appendChild(this.iconButton('arrow-down', 'move-row-down'));
        header.appendChild(this.iconButton('trash', 'remove-row', 'btn-outline-danger'));
        rowEl.appendChild(header);

        var body = document.createElement('div');
        body.className = 'card-body vl-columns d-flex flex-wrap gap-2';
        body.setAttribute('data-vl-drop', 'column');
        body.setAttribute('data-r', r);

        if (row.columns.length === 0) {
            var hint = document.createElement('div');
            hint.className = 'text-muted small p-2 w-100';
            hint.textContent = this.root.getAttribute('data-empty-columns-text');
            body.appendChild(hint);
        }

        row.columns.forEach(function (column, c) {
            body.appendChild(self.renderColumn(column, r, c));
        });

        rowEl.appendChild(body);

        return rowEl;
    };

    Editor.prototype.renderColumn = function (column, r, c) {
        var self = this;
        var colEl = document.createElement('div');
        colEl.className = 'card vl-column bg-light';
        colEl.style.width = Math.round((column.widthLg / 12) * 100) + '%';
        colEl.setAttribute('data-vl-level', 'column');
        colEl.setAttribute('data-vl-item', 'column');
        colEl.setAttribute('data-vl-drop', 'column');
        colEl.setAttribute('data-r', r);
        colEl.setAttribute('data-c', c);
        colEl.setAttribute('draggable', 'true');

        var header = document.createElement('div');
        header.className = 'card-header py-1 px-2 d-flex align-items-center gap-2';

        var grip = document.createElement('i');
        grip.className = 'fa-solid fa-grip-vertical text-muted';
        header.appendChild(grip);

        var select = document.createElement('select');
        select.className = 'form-select form-select-sm w-auto';
        select.setAttribute('data-prop', 'widthLg');
        for (var w = 1; w <= 12; w++) {
            var option = document.createElement('option');
            option.value = w;
            option.textContent = w + '/12';
            if (w === column.widthLg) {
                option.selected = true;
            }
            select.appendChild(option);
        }
        header.appendChild(select);

        var spacer = document.createElement('span');
        spacer.className = 'me-auto';
        header.appendChild(spacer);

        header.appendChild(this.iconButton('trash', 'remove-column', 'btn-outline-danger btn-sm'));
        colEl.appendChild(header);

        var blocksZone = document.createElement('div');
        blocksZone.className = 'card-body p-2 vl-blocks d-flex flex-column gap-2';
        blocksZone.setAttribute('data-vl-drop', 'block');
        blocksZone.setAttribute('data-r', r);
        blocksZone.setAttribute('data-c', c);

        if (column.blocks.length === 0) {
            var hint = document.createElement('div');
            hint.className = 'text-muted small text-center border border-dashed rounded p-2';
            hint.textContent = this.root.getAttribute('data-empty-blocks-text');
            blocksZone.appendChild(hint);
        }

        column.blocks.forEach(function (block, b) {
            blocksZone.appendChild(self.renderBlock(block, r, c, b));
        });

        var addBlockWrap = document.createElement('div');
        addBlockWrap.className = 'dropdown';

        var addBlockBtn = document.createElement('button');
        addBlockBtn.type = 'button';
        addBlockBtn.className = 'btn btn-sm btn-outline-primary w-100';
        addBlockBtn.setAttribute('data-bs-toggle', 'dropdown');
        addBlockBtn.setAttribute('data-vl-action', 'add-block');
        addBlockBtn.textContent = this.root.getAttribute('data-add-block-text');
        addBlockWrap.appendChild(addBlockBtn);

        var blockMenu = document.createElement('ul');
        blockMenu.className = 'dropdown-menu vl-block-menu d-none';
        ['Text', 'Html', 'Widget', 'Shape'].forEach(function (type) {
            var li = document.createElement('li');
            var link = document.createElement('a');
            link.className = 'dropdown-item';
            link.href = '#';
            link.setAttribute('data-vl-action', 'create-block');
            link.setAttribute('data-type', type);
            link.textContent = type;
            li.appendChild(link);
            blockMenu.appendChild(li);
        });
        addBlockWrap.appendChild(blockMenu);
        blocksZone.appendChild(addBlockWrap);

        colEl.appendChild(blocksZone);

        return colEl;
    };

    Editor.prototype.renderBlock = function (block, r, c, b) {
        var self = this;
        var blockEl = document.createElement('div');
        blockEl.className = 'card vl-block';
        blockEl.setAttribute('data-vl-level', 'block');
        blockEl.setAttribute('data-vl-item', 'block');
        blockEl.setAttribute('data-r', r);
        blockEl.setAttribute('data-c', c);
        blockEl.setAttribute('data-b', b);
        blockEl.setAttribute('draggable', 'true');

        var header = document.createElement('div');
        header.className = 'card-header py-1 px-2 d-flex align-items-center gap-2';

        var grip = document.createElement('i');
        grip.className = 'fa-solid fa-grip-vertical text-muted';
        header.appendChild(grip);

        var badge = document.createElement('span');
        badge.className = 'badge text-bg-secondary';
        badge.textContent = block.type;
        header.appendChild(badge);

        var spacer = document.createElement('span');
        spacer.className = 'me-auto';
        header.appendChild(spacer);

        header.appendChild(this.iconButton('trash', 'remove-block', 'btn-outline-danger btn-sm'));
        blockEl.appendChild(header);

        var body = document.createElement('div');
        body.className = 'card-body p-2';

        (BLOCK_TYPES[block.type] || []).forEach(function (def) {
            var label = document.createElement('label');
            label.className = 'form-label small mb-1';
            label.textContent = def.label;
            body.appendChild(label);

            var input = document.createElement(def.tag === 'textarea' ? 'textarea' : 'input');
            if (def.tag !== 'textarea') {
                input.type = 'text';
            } else {
                input.rows = 3;
            }
            input.className = 'form-control form-control-sm mb-2';
            input.setAttribute('data-prop', def.name);
            input.value = block.properties[def.name] || '';
            body.appendChild(input);
        });

        blockEl.appendChild(body);

        return blockEl;
    };

    Editor.prototype.iconButton = function (icon, action, extraClass) {
        var button = document.createElement('button');
        button.type = 'button';
        button.className = 'btn btn-sm btn-outline-secondary ' + (extraClass || '');
        button.setAttribute('data-vl-action', action);

        var iconEl = document.createElement('i');
        iconEl.className = 'fa-solid fa-' + icon;
        button.appendChild(iconEl);

        return button;
    };

    function boot() {
        var rootElement = document.getElementById('VisualLayoutEditor');
        var stateElement = document.getElementById('State');

        if (!rootElement || !stateElement) {
            if (window.console && console.warn) {
                console.warn('Visual Layouts: the editor markup (#VisualLayoutEditor / #State) was not found on this page.');
            }

            return;
        }

        new Editor(rootElement, stateElement);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }
})();
