<script type="module">
    import XElement from 'x-element';

    // class
    export default XElement.define('x-shell-footer', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
            };
        }

        //props
        static get observedAttributes() { return []; }
    });
</script>

<style>
    :host {
        margin-left:auto;
        margin-right:auto;
        text-align:center;
        display:flex;
        justify-content:center;
        margin-top:2em;
        font-size:var(--text-small);
    }

</style>

<template>
    <slot></slot>
</template>
