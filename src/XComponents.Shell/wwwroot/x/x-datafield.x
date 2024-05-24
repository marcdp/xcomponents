<script type="module">
    import XElement from 'x-element';

    // class
    export default XElement.define('x-datafield', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                description: "",
                format: "",
                label: "",
                type: "",
                value: "",
                
            };
        }

        //props
        static get observedAttributes() { return ["description", "format", "label", "placeholder", "type", "value"]; }

        //methods
        attributeChangedCallback(name, oldValue, newValue) {
            if (name === "selected") {
                this.state.selected = (newValue != null);
            } else {
                super.attributeChangedCallback(name, oldValue, newValue);
            }
        }
        

    });
</script>


<style>
    :host {
        display:block;
        border:1px red solid;
    }
</style>

<template>
    <label>{{label}}</label>
    <div>
        <input type="text" x-model="value" />
    </div>
</template>
