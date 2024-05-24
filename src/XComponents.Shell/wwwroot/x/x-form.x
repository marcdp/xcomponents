<script>
    import XElement from "x-element";

    //define web component
    export default XElement.define('x-form', class extends XElement {

        //ctor
        constructor() {
            super();
            this.state = {
                name: "Marcus",
                edad: 18,
                handsome: true,
                color: "red",
                time: "12:00",
                date: "2024-07-01",
                datetime: "2024-05-17T13:15",
                email: "pepe@terra.es",
                month: "2023-07",
                week: "2024-W16",
                password: "1234",
                radio: "a",
                range: 75,
                tel: "972365555",
                file: null                                               
            }
        }

    });
</script>

<template>
    <hr />

    <div x-show="state.edad == 19">
        ALERT LINE    ee
    </div>

    <hr />


    <input type="text" x-model="state.name" />
    <input type="number" x-model="state.edad" />
    <input type="checkbox" x-model="state.handsome" />
    <input type="range" x-model="state.range" min="0" max="100" value="90" step="10" />

    <input type="radio" name="radio" value="a" x-model="state.radio" /> aa
    <input type="radio" name="radio" value="b" x-model="state.radio" /> bb
    <input type="radio" name="radio" value="c" x-model="state.radio" /> cc

    <input type="date" x-model="state.date" />
    <input type="datetime-local" x-model="state.datetime" />

    <input type="file" x-model="state.file" />

    <!-- file -->
    {{state.toJson()}}

    <div>
        <div x-once>
            {{state.edad}} {{renderCount}}
        </div>
    </div>

    <hr />
</template>