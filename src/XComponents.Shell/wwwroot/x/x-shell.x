<script type="module">
    import XElement from 'x-element';
    import { config, logger } from 'x-shell';


    // class
    export default XElement.define('x-shell', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                logo: config.logo,
                copyright: config.copyright,
                url: document.location.pathname + document.location.search,
                loading: false,
            };
        }


        //props
        static get observedAttributes() { return []; }


        //methods
        async onLoad() {
            
            window.addEventListener("popstate", (event) => {
                let url = document.location.pathname + document.location.search;
                this.navigate(url, { avoiPushState: true })
            });
            var template = this.getElementsByTagName("template");
            if (template.length > 0) {
                let url = document.location.pathname + document.location.search;
                debugger;
                var aaaa = await XElement.compileComponent("x-main", url, template[0]);
                debugger;
            }
        }
        expandMenu(menu) {
            this.refs[menu].toggle();
        }
        collapseMenus() {
            this.refs.main?.collapse();
            this.refs.profile?.collapse();
        }
        deepLink(url) {
            if (url == "x-shell:menu-profile") {
                this.expandMenu("profile");
            } else if (url == "x-shell:menu-main") {
                this.expandMenu("main");
            } else if (url == "x-shell:search") {
                this.refs.header.showSearch();
            } else {
                logger.error(`Unable to execute deeplink, not implemented: ${url}`);
            }
        }
        async navigate(url, options) {
            options ??= {};
            //handle deep link
            if (url.startsWith("x-shell:")) {
                this.deepLink(url);
                return false;
            }
            //check if already at the url
            if (this.state.url == url) {
                options.avoiPushState = true;
            }
            //start navigation
            const start = performance.now();
            logger.log(`Navigating to ${url}...`);
            //copllapse menus
            this.collapseMenus();
            //show loader
            this.state.loading = true;
            //load
            let response = await fetch(url + (url.indexOf("?")==-1 ? "?" : "&") + "skipLayout=true");
            let html = await response.text();
            //extract html
            var i = html.indexOf("<x-shell");
            var j = html.indexOf("</x-shell");
            if (i != -1 && j != -1) {
                i = html.indexOf(">", i) + 1;
                html = html.substring(i, j);
            }
            //set html
            this.innerHTML = html;
            //pushstate
            if (!options.avoiPushState) {
                history.pushState({}, "", url);
            }
            //hide loader
            this.state.loading = false;
            //set url
            this.state.url = url;
            //log
            const end = performance.now();
            logger.log(`Navigated in ${end - start} ms`);
        }

    });
</script>

<style>
    :host {
        display: block;
    }

    .overlay {
        display: none;
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: #8c959f;
        z-index: 5;
        transition: opacity 1s, display 1s;
        transition-behavior: allow-discrete;
        opacity: 0;

        @starting-style {
            opacity: .2;
        }
    }

    x-shell-panel.expanded ~ .overlay {
        display: block;
        opacity: .2;

        @starting-style {
            opacity: 0;
        }
    }

    .copyright {
        display: block;
        margin-top: 2em;
        color: var(--text-light);
        font-size: var(--text-small);
    }

    a.logo {
        margin-left: 1em;
        aspect-ratio: 1 / 1;
    }

    a.logo img {
        height: 100%;
    }

    x-shell-header:not(:defined) {
        margin-top: 10em
    }

    .main.small {
        max-width: 70em;
        margin-left: auto;
        margin-right: auto;
        padding: 2em;
    }

    .main.normal {
        max-width: 89em;
        margin-left: auto;
        margin-right: auto;
        padding: 2em;
    }

    .main.full {
        width: 100%;
        padding: 2em;
    }

    .main {
        display: flex;
    }

    .main > x-shell-page {
        flex: 1
    }
</style>

<template>
    <x-shell-header ref="header"
                    menu="shortcuts"
                    x-attr:url="state.url"
                    x-on:openmenu="this.expandMenu(event.detail)"
                    x-on:menuitem:clicked="this.navigate(event.detail.href)">
    </x-shell-header>

    <x-shell-panel ref="main">
        <img slot="header" class="logo" x-attr:src="state.logo.src" />
        <div slot="body">
            <x-shell-menu menu="main"
                          x-attr:url="state.url"
                          x-on:menuitem:clicked.capture="this.navigate(event.detail.href)">
            </x-shell-menu>
            <span class="copyright">{{state.copyright}}</span>
        </div>
    </x-shell-panel>


    <x-shell-panel class="right" ref="profile">
        <x-shell-user slot="header"></x-shell-user>
        <div slot="body">
            <x-shell-menu menu="profile"
                          x-attr:url="state.url"
                          x-on:menuitem:clicked.capture="this.navigate(event.detail.href)">
            </x-shell-menu>
        </div>
    </x-shell-panel>

    <div class="main normal">
        <x-shell-menu-inner level="2"
                            menuitem-class="left"
                            x-attr:url="state.url"
                            x-on:menuitem:clicked.capture="this.navigate(event.detail.href)"></x-shell-menu-inner>
        <div>
            <slot></slot>
        </div>
    </div>

    <x-shell-footer>
        <x-shell-menu menu="footer"
                      class="horizontal"
                      menuitem-class="anchor"
                      x-attr:url="state.url"
                      x-on:menuitem:clicked.capture="this.navigate(event.detail.href)">
        </x-shell-menu>
    </x-shell-footer>


    <div class="overlay" x-on:click="this.collapseMenus()">
    </div>

</template>

