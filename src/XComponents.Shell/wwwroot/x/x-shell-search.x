<script type="module">
    import XElement from 'x-element';
    import { config } from 'x-shell';

    // class
    export default XElement.define('x-shell-search', class extends XElement {

        //vars
        _closing = false;

        // ctor
        constructor() {
            super();
            this.state = {
            };
        }

        //props
        static get observedAttributes() { return []; }

        //methods
        attributeChangedCallback(name, oldValue, newValue) {
        }
        onLoad() {
            this.refs.input.focus();
        }
        focusout(event) {
            if (event.relatedTarget !== event.currentTarget && !event.currentTarget.contains(event.relatedTarget)) {
                this.close();
            }
        }
        ok() {
        }
        close() {
            if (!this._closing) {
                this._closing = true;
                this.dispatchEvent(new CustomEvent('close'));
            }
        }
    });
</script>


<style>
    :host {
        display: block;
    }
    :host::before {
        content: "";
        position: fixed;
        left: 0;
        top: 0;
        right: 0;
        bottom: 0;
        background: #8c959f33;
        z-index:10;
    }

    .container {
        border: var(--popup-border);
        box-shadow: var(--popup-shadow);
        border-radius: var(--popup-border-radius);
        background: var(--popup-background);
        margin-top: -1em;
        z-index: 10;
        position: relative
    }

    .container .header {
        padding: 1em;
        display:flex;
        align-items:center;
    }
    .container .header x-icon {
        position: absolute;
        margin-left:.65em;
    }
    .container .header input {
        width: 100%;
        box-sizing: border-box;
        display: block;
        outline: none;
        border: .17em #0969DA solid;
        border-radius: var(--menuitem-border-radius);
        color: var(--input-color);
        padding: var(--input-padding);
        padding-left: 2.5em;
        box-sizing: border-box;
    }
    .results {
        padding: .75em;
    }
    .results + .results {
        border-top: var(--popup-separator-border);
    }    
</style>

<template>
    <div class="container" x-on:focusout="focusout">

        <div class="header">
            <x-icon icon="x-search"></x-icon>
            <input ref="input" type="text" x-on:keydown.escape="close" />
        </div>

    </div>
</template>







