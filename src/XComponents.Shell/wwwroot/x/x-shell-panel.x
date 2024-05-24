<script type="module">
    import XElement from 'x-element';
    import { config } from 'x-shell';

    // class
    export default XElement.define('x-shell-panel', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                logo: config.logo,
                expanded: false,
            };
        }

        //props
        static get observedAttributes() { return []; }

        //methods
        connectedCallback() {
            super.connectedCallback();
            this.style.visibility = "hidden";
            this.addEventListener("transitionend", () => this.transitionEnd());
        }
        toggle() {
            if (this.state.expanded) {
                this.collapse();
            } else {
                this.expand();
            }
        }
        expand() {
            this.style.visibility = "visible";
            this.state.expanded = true;
            this.classList.add("expanded");
        }
        collapse() {
            this.state.expanded = false;
            this.classList.remove("expanded");
        }
        transitionEnd() {
            if (!this.state.expanded) {
                this.style.visibility = "hidden";
            }
        }
    });
</script>

<style>
    :host {
        --width: 22.8em;
        padding: 1.1em;
        z-index: 10;
        display: block;
        box-sizing: border-box;
        width: var(--width);
        position: fixed;
        top: 0em;
        bottom: 0;
        border: var(--popup-border);
        background: var(--popup-background);
        box-shadow: var(--popup-shadow);
        border-radius: 0 var(--popup-border-radius) var(--popup-border-radius) 0;
        left: calc(var(--width) * -1);
        transition: var(--transition-all);
    }

    :host(.expanded) {
        transform: translate(var(--width));
    }

    :host(.right) {
        border-radius: var(--popup-border-radius) 0 0 var(--popup-border-radius);
        left: unset;
        right: calc(var(--width) * -1);
    }

    :host(.right.expanded) {
        transform: translate(calc(var(--width) * -1));
    }

    :host(.right) div.header > div .logo {
        display: none;
    }

    :host > div.header {
        display: flex;
        height: 2.3em;
        margin-bottom: 1.16em;
    }

    :host > div.header{
        flex: 1;
        display: flex;
    }

    button.close {
        background: none;
        border: none;
        color: gray;
        cursor: pointer;
        font-size: var(--text-small);
        align-items: center;
        justify-content: center;
        display: flex;
        border-radius: var(--menuitem-border-radius);
        cursor: pointer;
        aspect-ratio: 1 / 1;
        position:absolute; top:1em; right:1em
    }
    button.close:hover {
        background: var(--menuitem-background-hover);
    }
    button.close:active {
        background: var(--menuitem-background-active);
    }
</style>

<template>
    <div class="header">
        <slot name="header"></slot>
    </div>

    <button class="close" x-on:click="this.collapse()">
        <x-icon icon="x-close"></x-icon>
    </button>

    <slot name="body">
    </slot>
</template>