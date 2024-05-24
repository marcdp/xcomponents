<script type="module">
    import XElement from 'x-element';
    import { config } from 'x-shell';

    //cache
    const cache = {
    }


    // class
    export default XElement.define('x-icon', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                icon: null,
                svg: null
            };
        }

        //props
        static get observedAttributes() { return ["icon"]; }
        get icon() { return this.state.icon; }
        set icon(icon) {
            this.state.icon;
            this._loadIcon(icon);
        }

        //methods
        async attributeChangedCallback(name, oldVal, newVal) {
            if (name === "icon") this.icon = newVal;
        }
        async _loadIcon(icon) {
            this._icon = icon;
            if (icon) {
                let cacheItem = cache[icon];
                if (cacheItem) {
                    if (cacheItem.svg) {
                        //get from cache
                        this.state.svg = cacheItem.svg;
                    } else {
                        //wait until loaded
                        await cacheItem.promise;
                        this.state.svg = cacheItem.svg;
                    }
                } else {
                    //url
                    let src = "";
                    for (let prefix in config.icons) {
                        let url = config.icons[prefix];
                        if (prefix.length && icon.startsWith(prefix + "-")) {
                            src = url.replace("{name}", icon).replace("{name-unprefixed}", icon.substr(icon.indexOf("-")+1));
                            break;
                        }
                    }
                    //fetch
                    let cacheItem = {
                        svg: null,
                        promise: new Promise(async (resolve, reject) => {
                            let response = await fetch(src);
                            cacheItem.svg = (response.status == 200 ? await response.text() : "?");
                            resolve();
                        })
                    };
                    cache[icon] = cacheItem;
                    await cacheItem.promise;
                    this.state.svg = cacheItem.svg;
                }
            } else {
                this.state.svg = null;
            }
        }
    });
</script>


<style>
    :host {
        display: inline-block;
        width: 1.25em;
        height: 1.25em;
    }

    :host svg {
        width: 1.25em;
        height: 1.25em;
        fill: currentcolor
    }
</style>


<template>
    <span x-if="state.svg" x-html="state.svg"></span>
</template>


