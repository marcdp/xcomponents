

// Globals
let idCounter = 0;


// XComponents
class XComponentsClass {


    // vars
    _config = {
        _version: ""
    }
    _components = {};


    // ctor
    constructor() {
    }


    // config
    config(handler) {
        handler(this._config);
    }
    start() {
        this.resume(document.body);
    }
    async rpc(path, method, params) {
        var payload = {
            jsonrpc: "2.0",
            method: method,
            params: params || [],
            id: idCounter++
        }
        const response = await fetch(path, {
            body: JSON.stringify(payload),
            headers: {
                Accept: "application/json-rpc",
                "Content-Type": "application/json-rpc"
            },
            method: "POST"
        });
        var data = await response.text();
        //debugger;
        return false;
    }
    resume(element) {
        var names = [];
        // get not defined web component names
        element.querySelectorAll(":not(:defined)").forEach((wc) => {
            if (names.indexOf(wc.localName) == -1 && !this._components[wc.localName]) {
                names.push(wc.localName);
            }
        });
        // load components
        for (let name of names) {
            // define component
            let component = {
                name: name,
                url: "/_webcomponents/" + name + "?v=" + this._config.version,
                class: customElements.get(name),
                status: "loading"
            }
            this._components[name] = component;
            // load component
            if (!component.class) {
                let script = document.createElement("script");
                script.setAttribute("type", "module");
                script.setAttribute("src", component.url);
                script.addEventListener("load", () => {
                    component.status = "loaded";
                    component.class = customElements.get(name);
                });
                document.head.appendChild(script);
            }
        }
    }
}


// singleton
var XComponents = new XComponentsClass();
window.XComponents = XComponents;
