<script type="module">
    import XElement from 'x-element';
    import { config } from 'x-shell';

    // class
    export default XElement.define('x-shell-header', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                title: config.manifest.name,
                logo: config.logo,
                menu: [],
                submenu: [],
                user: config.user,
                search: false,
                url: null
            };
        }

        //props
        static get observedAttributes() { return ["menu", "url"]; }

        //methods
        attributeChangedCallback(name, oldValue, newValue) {
            if (name === "menu") {
                this.state.menu = config.menus[newValue];
            } else {
                super.attributeChangedCallback(name, oldValue, newValue);
            }
        } 
        showSearch() {
            this.state.search = true;
        }
        closeSearch() {
            this.state.search = false;
        }

    });
</script>


<style>
    :host {
        display: block;
        background: var(--color-bg-dark);
        border-bottom: var(--panel-border);
        user-select: none;
    }

    .top {
        display: flex;
        height: 2.35em;
        box-sizing: content-box;
        padding: 1.15em;
        padding-bottom: 1em;
    }
    .top button.menu {
        border: var(--menuitem-border);
        background: var(--menuitem-background);
        border-radius: var(--menuitem-border-radius);
        color: var(--menuitem-color);
        cursor: pointer;
        display: flex;
        justify-content: center;
        align-items: center;
        width: 2.3em;
    }

    .top a.logo {
        width: 2.25em;
        display: block;
        margin-left: 0.4em;
        aspect-ratio: 1 / 1;
    }

    .top .center {
        flex: 1 !important;
        display: flex;
        margin-left: .65em;
    }

    .top .center span {
        display: flex;
        align-items: center
    }

    .top x-shell-search {
        flex: 1;
        margin-right: .75em;
    }

    .bottom {
        _display: flex;
        _height: 2.35em;
        _box-sizing: content-box;
        _padding-top: .1em;
        padding-left: 1.15em;
        _padding-bottom: .38em;
    }
    x-shell-search {
        margin-right:-9em!important;
    }    
</style>

<template>
    <div class="top">

        <x-menuitem icon="x-menu" class="button" href="x-shell:menu-main"></x-menuitem>

        <a class="logo" x-if="state.logo" x-attr:href="state.logo.href" x-attr:title="state.logo.title" x-attr:target="state.logo.target">
            <img x-attr:src="state.logo.src" />
        </a>



        <div class="center">
            <x-shell-breadcrumb x-if="!state.search" x-attr:url="state.url"></x-shell-breadcrumb>
            <x-shell-search x-else x-on:close="closeSearch"></x-shell-search>
        </div>

        <x-shell-menu class="horizontal" menu="shortcuts" x-attr:url="state.url"></x-shell-menu>

    </div>

    <x-shell-menu-inner class="tabs" menuitem-class="tab" level="1" x-attr:url="state.url"></x-shell-menu-inner>

</template>
