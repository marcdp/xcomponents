<!--<script>
    import XElement from "x-element";

    //define web component
    export default XElement.define('x-page', class extends XElement {

        //ctor
        constructor() {
            super();
            this.state = {
                title: "",
                width: null                                               
            }
        }

        //props
        static get observedAttributes() { return ["title", "width"]; }

    });
</script>

<style>
    :host {
        display:block;
        border:1px blue solid;
    }
</style>

<h1>title {{state.title}}</h1>
<slot>

</slot>-->