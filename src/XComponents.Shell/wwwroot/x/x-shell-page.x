<script type="module">
    import XElement from 'x-element';
    import { logger } from 'x-shell';

    // class
    export default XElement.define('x-shell-page', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                loading: false,
                title: "Page Title",
                src: ""
            };
        }

        //props
        static get observedAttributes() { return ["src"]; }

        //methods
        onLoad() {
            this.initialize();
        }
        attributeChangedCallback(name, oldValue, newValue) {
            if (name === "src") {
                if (this._connected) {
                    this.navigate(newValue);
                } else {
                    this.state.src = newValue;
                }
            }
        }
        async navigate(src, options) {
            debugger;
            options ??= {};
            //check if already at the url
            if (this.state.src == src) {
                options.avoiPushState = true;
            }
            //start navigation
            const start = performance.now();
            logger.log(`Navigating to ${src}...`);
            //show loader
            this.state.loading = true;
            //load
            let response = await fetch(src);
            var html = await response.text();                                                        
            //get html
            debugger;
            var i = text.indexOf("<x-shell");
            var j = text.indexOf("</x-shell");
            if (i != -1 && j != -1) {
                i = text.indexOf(">", i) + 1;
                //replace inner html
                html = text.substring(i, j);
            }
            //set html
            this.innerHTML = html;
            //pushstate
            if (!options.avoiPushState) {
                history.pushState({}, "", src);
            }
            //hide loader
            this.state.loading = false;
            //set url
            this.state.src = src;
            //initialize
            this.initialize();
            //log
            const end = performance.now();
            logger.log(`Navigated in ${end - start} ms`);
        }
        async initialize() {
            //initialize
            console.log("INIT PAGEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        }
    });
</script>



<style>
    :host {
        display: block;
    }
</style>

<template>
    <x-shell-loader x-attr:loading="state.loading"></x-shell-loader>

    <slot>
    </slot>

</template>



