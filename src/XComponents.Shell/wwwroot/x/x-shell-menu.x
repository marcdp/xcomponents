<script type="module">
    import XElement from 'x-element';
    import XShell, { config } from 'x-shell';

    // class
    export default XElement.define('x-shell-menu', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                menu: "",
                menuitems: [],
                selected: [],
                url: null,
            };
        }

        //props
        static get observedAttributes() { return ["menu", "url"]; }

        //methods
        attributeChangedCallback(name, oldValue, newValue) {
            if (name === "menu") {
                this.state.menu = newValue;
                this.state.menuitems = config.menus[newValue];
            } else if (name === "url") {
                this.state.url = newValue;
                this.state.selected = XShell.getSelectedMenuItems(newValue);
            }
        }
    });
</script>
         
<style>
    :host(.horizontal) {
        display:flex;
    }
</style> 

<template>
    <x-menuitem x-for="menuitem in state.menuitems"
                x-key="key"
                x-attr="menuitem"
                x-attr:selected="state.selected.indexOf(menuitem) != -1">
    </x-menuitem>
</template>
