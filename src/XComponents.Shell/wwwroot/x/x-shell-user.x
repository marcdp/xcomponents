<script type="module">
    import XElement from 'x-element';
    import { config } from 'x-shell';

    // class
    export default XElement.define('x-shell-user', class extends XElement {

        // ctor
        constructor() {
            super();
            this.state = {
                user: config.user
            };
        }

        //props
        static get observedAttributes() { return []; }

    });
</script>

<style>
    :host {
        display: flex;
        flex: 1;
    }
    .avatar {
        border-radius: 50%;
        box-sizing: border-box;
        border: none;
        color: white;
        background: #222222;
        aspect-ratio: 1 / 1;
        text-align: center;
        font-weight: 500;
        cursor: pointer;
        padding: 0;
        display: block;
        font-size: 1.2em;
        margin-right: .5em;
    }
    b {
        font-weight: 500;
    }
    span {
        color: var(--text-light);
    }
</style>

<template>
    <img class="avatar" x-if="state.user.claims.avatar" x-attr:src="state.user.claims.avatar" />
    <span class="avatar" x-else>{{state.user.name.substring(0,1)}}</span>
    <div>
        <b>{{state.user.username}}</b><br />
        <span>{{state.user.name}}</span>
    </div>
</template>
