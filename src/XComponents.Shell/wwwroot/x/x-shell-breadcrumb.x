<script type="module">
    import XElement from 'x-element';
    import XShell, { config } from 'x-shell';

    // class
    export default XElement.define('x-shell-breadcrumb', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                url: null,
                menuitems: []
            };
        }

        //props
        static get observedAttributes() { return ["url"]; }

        //methods
        attributeChangedCallback(name, oldValue, newValue) {
            if (name === "url") {
                this.state.url = newValue;
                this.state.menuitems = XShell.getSelectedMenuItems(this.state.url);
            }
        }
    });
</script>

<style>
    :host {
        display: flex;
        justify-content: center
    }
    x-menuitem + x-menuitem::before {
        content: "/";
        display: block;
        align-self: center;
        margin-top:-.2em;
        margin-left:.1em; margin-right:.1em;
        color:var(--text-light);
    }
</style>
                   
<template>

    <x-menuitem x-for="menuitem in state.menuitems"
                x-key="key"
                x-attr="menuitem"
                class="breacrumb"
                x-attr:selected="menuitem == state.menuitems[state.menuitems.length-1]">
    </x-menuitem>

</template>