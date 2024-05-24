<script type="module">
    import XElement from 'x-element';

    // class
    export default XElement.define('x-shell-loader', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                loading: false
            };
        }

        //props
        static get observedAttributes() { return ["loading"]; }

        //methods
        attributeChangedCallback(name, oldValue, newValue) {
            if (name == "loading") {
                this.state.loading = (newValue != null ? true : false);                        
            }
        }
    });
</script>

               
<style>
    :host > div {
        position: fixed;
        left: 0;
        top: 0;
        right: 0;
        z-index: 100;
        width: 100%;
        height: .2em;
        overflow: hidden;
    }
    :host > div :after {
        content: '';
        position: absolute;
        left: 0;
        width: 0;
        height: 100%;
        border-radius: 4px;
        box-shadow: 0 0 5px rgba(0, 0, 0, .2);
        animation: loadinganimationbar-animation 5s infinite;
        animation-delay: 0s;
    }
    @keyframes loadinganimationbar-animation {
        0% {width: 0;background: red;}
        25% {width: 40%;background: red;}
        50% {width: 60%;background: red;}
        75% {width: 75%;background: red;}
        100% {width: 100%;background: red;}
    }
</style>

<template>
    <div x-if="state.loading"><div class="track"></div></div>
</template>




