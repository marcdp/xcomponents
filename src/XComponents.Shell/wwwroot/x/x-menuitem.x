<script type="module">
    import XElement from 'x-element';

    // class
    export default XElement.define('x-menuitem', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                label: "",
                description:"",
                icon: "",
                img:"",
                disabled: "",
                href: "",
                target:"",
                selected: false
            };
        }

        //props
        static get observedAttributes() { return ["label", "description", "icon", "img", "disabled", "href", "target", "selected"]; }

        //methods
        attributeChangedCallback(name, oldValue, newValue) {
            if (name === "selected") {
                this.state.selected = (newValue != null);
            } else {
                super.attributeChangedCallback(name, oldValue, newValue);
            }
        }
        click(event) {
            if (this.state.target) {
                return false;
            }
            this.dispatchEvent(new CustomEvent("menuitem:clicked", { detail: { href: this.state.href }, composed: true, bubbles: true, cancelable: true }));
            event.stopPropagation();
            event.preventDefault();
        }   

    });
</script>


<style>
    :host {
        display:flex;
    }
    hr {
        border: var(--hr-border);
        border-top: none;
        margin-left:-.5em; margin-right:-.5em;
        width:100%;
    }
    a {
        display: flex;
        border-radius: var(--menuitem-border-radius);
        padding: 0.5em;
        text-decoration: none;
        color: var(--text-default);
        gap: .3em;
        flex: 1;
        flex-wrap: wrap;
    }
    a:hover {
        background: var(--menuitem-background-hover) !important;
    }
    a:active {
        background: var(--menuitem-background-active)!important;
    }
    a div {
        flex:1; 
        display:flex; 
        flex-direction:column
    }
    a span.label {
    }
    a span.description {
        color:var(--text-light); 
        flex-grow: 1;
        flex-basis: 100%;
    }
    a x-icon {
        width: 1.5em;
        text-align:center;
        color: gray;
        
    }
    a img {
        width: 1.2em;                
        margin:0;
        display:block;
    }
    a .target {
        font-size:var(--text-small);
        color:var(--text-light);
        opacity:.5;
    }
    :host(.button) a {
        border: var(--menuitem-border);
        padding-left: .4em;
        padding-right: .4em;
        margin-right: .65em;
        background: var(--menuitem-background);
    }
    :host(.tab) a {
        padding:.43em;
        padding-left: .4em;
        padding-right: .4em;
        margin-right: .65em;        
        position:relative;
    }
    :host(.tab) a.selected::before {
        content: "";
        background:#FD8C73;
        height:2px;
        position: absolute;
        bottom: -.6em;
        left:0;
        width: 100%;
    }
    :host(.breacrumb) a > x-icon:first-child {
        display: none;
    }
    :host(.breacrumb) .description {
        display: none;
    }
    :host(.left) {
        width: 19em;
        margin-top: 0 !important;
        margin-bottom: 0 !important;
        padding: 0;
        margin-right: 3.5em !important;
    }
    :host(.left) a {
        position: relative;
    }
    :host(.left) a.selected:before {
        content:"";
        border:.15em #0969DA solid;
        border-radius:.25em;
        position:absolute;
        left:-.55em; 
        top:.2em; 
        bottom:.2em;
    }

    :host(.avatar) a {
        width: 2.25em;
        padding: 0;
        flex: 1;
    }
    :host(.avatar) a img {
        width: 100%;
        display: block;
        object-fit: cover;
        border-radius: 50% !important;
    }
    :host(.avatar) a div {
        display:none;
    }
    :host(.hide-label) a div {
        display:none;
    }
    :host(.anchor) a:hover {
        text-decoration:underline!important;
        background:none!important;
    }
    :host(.anchor) a.selected {
        background: none !important;
    }
    :host a.selected {
        background: var(--menuitem-background-selected);
        font-weight: var(--menuitem-font-weight-selected);
    }
    :host a.selected x-icon {
        color: var(--menuitem-color-selected);
    }
    :host a.selected span.label {
        color: var(--menuitem-color-selected);
    }
    :host a.disabled {
        pointer-events:none;
    }

</style>


<template>
    <hr x-if="state.label=='-'" />

    <a x-else x-attr:href="state.href" @click="click" x-attr:class="(!state.href ? 'disabled' : (state.selected ? 'selected' : ''))" x-attr:target="state.target">

        <x-icon x-if="state.icon" x-attr:icon="state.icon"></x-icon>

        <img x-if="state.img" x-attr:src="state.img" />

        <div x-if="state.label || state.description">
            <span class="label" x-if="state.label">{{state.label}}</span>
            <span class="description" x-if="state.description">{{state.description}}</span>
        </div>

        <x-icon x-if="state.target" class="target" icon="x-url-open"></x-icon>

    </a>
</template>
