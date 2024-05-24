<script type="module">
    import XElement from 'x-element';
    import XShell, { config } from 'x-shell';

    // class
    export default XElement.define('x-shell-menu-inner', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                menuitems: [],
                level: 0,
                url: null,
                selected: [],
                menuitemClass: null
            };
        }

        //props
        static get observedAttributes() { return ["level", "url", "menuitem-class"]; }

        //methods
        attributeChangedCallback(name, oldValue, newValue) {
            if (name === "level") {
                this.state.level = parseInt(newValue);
            } else if (name === "url") {
                this.state.url = newValue;
            } else if (name === "menuitem-class") {
                this.state.menuitemClass = newValue;
            }
            this.process();
        }
        process() {
            if (this.state.level == 0) {
            } else if (this.state.url == null) {
            } else {
                this.state.selected = XShell.getSelectedMenuItems(this.state.url);
                var menuitem = this.state.selected[this.state.level - 1];
                this.state.menuitems = (menuitem ? menuitem.children || [] : []);
            }
        }
    });
</script>
         
<style>
    :host { }
    :host(.horizontal){
        display:flex;
    }
    :host(.tabs) > div {
        display: flex;
        align-items: center;
        padding-left: 1.1em;
    }
    x-menuitem {
        margin-top: .2em;        
        margin-bottom: .51em;
    }
</style>

<template>
    <div>
        <x-menuitem x-for="menuitem in state.menuitems"
                    x-key="href"
                    x-attr="menuitem"
                    x-attr:selected="state.selected.indexOf(menuitem) != -1"
                    x-attr:class="state.menuitemClass">
        </x-menuitem>
    </div>

</template>
