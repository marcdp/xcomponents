
/*
 * State Class
 * Simple class that wraps an object and notifies a callback when a property is changed.
 */
class State {
    constructor(state, callback) {
        var proxy = new Proxy(this, {
            get(target, prop, receiver) {
                return state[prop];
            },
            set: (target, prop, value, receiver) => {
                state[prop] = value;
                callback();
                return true;
            }
        });
        proxy.toJson = () => JSON.stringify(state);
        return proxy;
    }
}
 

/*
 * Config
 */
const config = {
    // contains the debug flag
    debug: false,
    // contains the urls to load web components, indexed by prefix
    urls: {},
    // contains the selector that should be used to find web components
    selector: "BODY",
    // constains the name of web components that should be preloaded
    preload:[]
}


/*
 * Render Utils
 */
const renderUtils = new class {
    toArray = (value) => {
        if (Array.isArray(value)) return value;
        if (typeof (value) == "number") return Array.from({ length: value }, (v, i) => i + 1);
        if (typeof (value) == "string") return [...value];
        if (typeof (value) == "object") return Object.keys(value);
        return value;
    }
    toObject = (value) => {
        let result = {};
        for (let key in value) {
            let val = value[key];
            if (typeof (val) == "string") {
                result[key] = val;
            } else if (typeof (val) == "number") {
                result[key] = val;
            } else if (typeof (val) == "boolean") {
                if (val) result[key] = true;
            }
        }
        return result;
    }
    toDynamicArgument = (key, value) => {
        return { [key]: value };
    }
    toDynamicProperty = (key, value) => {
        return { [key]: value };
    }
}


/*
 * Logger
 */
const logger = new class {
    log = (message, ...args) => {
        if (config.debug) console.log(this.getTime() + "[XWebComponent] " + message, ...args);
    }
    warn= (message, ...args) => {
        console.warn(this.getTime() + "[XWebComponent] " + message, ...args);
    }
    error = (message, ...args) => {
        console.error(this.getTime() + "[XWebComponent] " + message, ...args);
    }
    getTime() {
        var date = new Date();
        return date.toTimeString().slice(0, 8) + "." + date.getMilliseconds()  + " ";        
    }
}


/*
 * ComponentLoader Class
 * Simple class that loads web components from config urls, depending on the component name, and compiles them to a js class
 */
const componentLoader = new class {
    loading = []
    load = async (name, usedBy) => {
        //if array
        if (Array.isArray(name)) {
            let promises = [];
            let names = name;
            for (let name of names) {
                promises.push(await componentLoader.load(name, usedBy));
            }
            return Promise.all(promises);
        }
        //check if component is already loaded
        let componentClass = window.customElements.get(name);
        if (componentClass) return componentClass;
        //check if component is loading
        if (this.loading.indexOf(name) != -1) {
            return await window.customElements.whenDefined(name);
        }
        //resolve url
        let aux = name;
        let item = null;
        while (true) {
            item = config.urls[aux];
            if (item) break;
            let i = aux.lastIndexOf("-");
            if (i == -1) break;
            aux = aux.substring(0, i);
        }
        if (item == null) logger.error(`Unable to load component: url not registered: ${name} (used by ${usedBy})`);
        //add to cache
        this.loading.push(name);
        //create from item
        const start = performance.now();
        if (typeof (item) == "string" && (item.endsWith(".js") || item.indexOf(".js?") != -1)) {
            //import and return web component class
            var url = item.replace("{name}", name);
            logger.log(`Loading web component: ${name} from ${url} (used by ${usedBy})`);
            var aModule = await import(url);
            componentClass = aModule.default;
        } else if (typeof (item) == "string" && (item.endsWith(".x") || item.indexOf(".x?") != -1)) {
            //load html and create web component class
            var url = item.replace("{name}", name);
            logger.log(`Loading web component: ${name} from ${url} (used by ${usedBy})`);
            var response = await fetch(url);
            var html = await response.text();
            componentClass = await this.compileFileToClass(name, url, html);
        } else {
            //invoke function to obtain web component class
            logger.error(`Unable to load web component: ${name}: no url defined to load this web component (used by ${usedBy})`);
        }
        //remove from loading
        this.loading.splice(this.loading.indexOf(name), 1);
        //log
        const end = performance.now();
        logger.log(`Loaded ${name} in ${end - start} ms !!!!`);
        //return class
        return componentClass;
    }
    async compileFileToClass(name, url, html) {
        let templateElement = null;
        if (typeof (html) == "string") {
            templateElement = document.createElement("template");
            templateElement.innerHTML = html;
        } else {
            templateElement = html;
        }
        let script = templateElement.content.querySelector("script") ?? document.createElement("script");
        let style = templateElement.content.querySelector("style") ?? document.createElement("style");
        let template = templateElement.content.querySelector("template") ?? document.createElement("template");
        //insert style as first child of template
        style = style.cloneNode(true)
        style.setAttribute("x-once", "");
        template.content.insertBefore(style, template.content.firstChild);
        //get referenced components
        var componentsToLoad = [];
        template.content.querySelectorAll("*").forEach((element) => {
            if (element.localName.indexOf("-") != -1 && componentsToLoad.indexOf(element.localName) == -1 && this.loading.indexOf(element.localName) == -1) {
                componentsToLoad.push(element.localName);
            }
        });
        //process
        var renderTemplate = template.innerHTML.trim();
        var js = "    " + script.textContent.trim();
        var i = js.indexOf("XWebComponent.define");
        if (i != -1) { 
            var j = js.indexOf("{", i);
            if (j != -1) {
                js = js.substring(0, j + 1) + "\n\n" +
                    "        //auto generated\n" +
                    "        render(state, renderUtils, renderCount) {;\n" +
                    "            " + componentLoader.compileTemplateToJs(name, url, renderTemplate).replaceAll("\n","\n            ") + "\n" +
                    "        }\n" +
                    js.substring(j + 1)
            }
        }
        //load referenced components
        if (componentsToLoad.length) {
            await this.load(componentsToLoad, name);
        }
        //create module
        var scriptElement = document.createElement("script");
        scriptElement.setAttribute("type", "module");
        scriptElement.setAttribute("data-x-name", name);
        scriptElement.setAttribute("data-x-src", url);
        //return promise that resolves when module is component is registered
        scriptElement.appendChild(document.createTextNode(js));
        return new Promise((resolve, reject) => {
            window.customElements.whenDefined(name).then((event) => {
                var aClass = customElements.get(name);
                resolve(aClass);
            });
            scriptElement.addEventListener("error", (event) => {
                reject();
            });
            document.head.appendChild(scriptElement);
        });

    }
    //compile html template into a function that creates a virtual dom object
    compileTemplateToJs = (name, url, html) => {
        var template = document.createElement("template");
        template.innerHTML = html.replaceAll("{{", "<x:text>").replaceAll("}}", "</x:text>").trim();
        var js = [];
        js.push("let _ifs = {};");
        js.push("return [");
        var index = 0;
        template.content.childNodes.forEach((subNode, subIndex) => {
            index += this._compileTemplateToJsRecursive(name, subNode, index, js, 0);
        });
        js.push("];");
        return js.join("\n");
    }
    compileTemplateToFunction = (name, url, html) => {
        var code = this.compileTemplateToJs(name, url, html);
        return new Function("state", "renderUtils", "renderCount", code);
    }
    //compile DOM node recursive into a function that creates virtual dom objects
    _compileTemplateToJsRecursive(name, node, index, js, level) {
        var indent = " ".repeat((level + 1) * 4);
        var incs = 0;
        if (node.nodeType == Node.TEXT_NODE) {
            //#text
            var textContent = node.textContent;
            var jsLine = [];
            jsLine.push(indent);
            jsLine.push("createVDOM(\"#text\", null, null, null, {index: " + index + "}, " + JSON.stringify(textContent) + "),");
            js.push(jsLine.join(""));
            incs++;
        } else if (node.nodeType == Node.COMMENT_NODE) {
            //#comment
            var textContent = node.textContent;
            var jsLine = [];
            jsLine.push(indent);
            jsLine.push("createVDOM(\"#comment\", null, null, null, {index: " + index + "}, " + JSON.stringify(textContent) + "),");
            js.push(jsLine.join(""));
            incs++;
        } else if (node.localName == "x:text") {
            //x:text
            var jsLine = [];
            jsLine.push(indent);
            jsLine.push("createVDOM(\"#text\", null, null, null, {index: " + index + "}, \"\" + " + node.textContent + "),");
            js.push(jsLine.join(""));
            incs++;
        } else if (node.localName.startsWith("x:")) {
            //error
            logger.error("Error compiling component ${name}: invalid node ${node.localName}: not implemented");
        } else {
            //element
            var jsLine = [];
            jsLine.push(indent);
            jsLine.push("createVDOM(\"" + node.tagName.toLowerCase() + "\"");
            var jsPostLine = [];
            var jsPost = [];
            var attrs = [];
            var props = [];
            var events = [];
            var options = [];
            var text = "";
            options.push("index:" + index);
            for (var i = 0; i < node.attributes.length; i++) {
                var attr = node.attributes[i];
                if (attr.name == "x-text") {
                    //...<span x-text="state.value"></span>...
                    if (node.childNodes.length) logger.error("Error compiling component ${name}: invalid x-text node: non empty");
                    text = "\"\" + " + attr.value;
                } else if (attr.name == "x-html") {
                    //...<span x-html="state.value"></span>...
                    if (node.childNodes.length) logger.error("Error compiling component ${name}: invalid x-html node: non empty");
                    options.push("format:\"html\"");
                    text = "\"\" + " + attr.value;
                } else if (attr.name == "x-attr" || attr.name == ":") {
                    //...<span x-attr="state.value"></span>...
                    //...<span :="state.value"></span>...
                    var attrValue = attr.value;
                    attrs.push("...renderUtils.toObject(" + attrValue + ")");
                } else if (attr.name.startsWith("x-attr:") || attr.name.startsWith(":")) {
                    //...<span x-attr:title="state.value"></span>...
                    var attrName = (attr.name.startsWith(":") ? attr.name.substring(1) : attr.name.substr(attr.name.indexOf(':') + 1));
                    var attrValue = attr.value;
                    if (attrName.startsWith("[") && attrName.endsWith("]")) {
                        attrs.push("...renderUtils.toDynamicArgument(" + attrName.substring(1, attrName.length - 1) + ", " + attrValue + ")");
                    } else {
                        attrs.push('"' + attrName + '":' + attrValue);
                    }
                } else if (attr.name == "x-prop" || attr.name == ".") {
                    //...<span x-prop="state.value"></span>...
                    //...<span .="state.value"></span>...
                    var propValue = attr.value;
                    attrs.push("..." + propValue + "");
                } else if (attr.name.startsWith("x-prop:") || attr.name.startsWith(".")) {
                    //...<input x-prop:value="state.value"></input>...
                    //...<input .value="state.value"></input>...
                    var propName = this._kebabToCamel((attr.name.startsWith(".") ? attr.name.substring(1) : attr.name.substr(attr.name.indexOf(':') + 1)));
                    var propValue = attr.value;
                    if (propName.startsWith("[") && propName.endsWith("]")) {
                        props.push("...renderUtils.toDynamicProperty(" + propName.substring(1, propName.length - 1) + ", " + propValue + ")");
                    } else {
                        props.push(propName + ":" + propValue);
                    }
                } else if (attr.name.startsWith("x-on:") || attr.name.startsWith("@")) {
                    //...<button x-on:click="this.onIncrement(event)">+1</button>...
                    //...<button x-on:click="onIncrement">+1</button>...
                    //...<button @click="onIncrement">+1</button>...
                    var eventName = (attr.name.startsWith("@") ? attr.name.substring(1) : attr.name.substr(attr.name.indexOf(':') + 1));
                    var eventHandler = attr.value;
                    if (eventHandler.indexOf("(") == -1 && eventHandler.indexOf(")") == -1 && eventHandler.indexOf(".") == -1) eventHandler = "this." + eventHandler + "(event)";
                    events.push("'" + eventName + "': (event) => " + eventHandler);
                } else if (attr.name == "x-if") {
                    //...<span x-if="state.value > 0">greather than 0</span>...
                    jsLine[0] = indent + "...((_ifs.c" + level + " = (" + attr.value + ")) ? [";
                    jsPostLine.push("] : []),");
                } else if (attr.name == "x-elseif") {
                    //...<span x-elseif="state.value < 0">less than 0</span>...
                    jsLine[0] = indent + "...(_ifs.c" + level + " ? [] : (_ifs.c" + level + " = (" + attr.value + ")) ? [";
                    jsPostLine.push("] : []),");
                } else if (attr.name == "x-else") {
                    //...<span x-else>is 0</span>...
                    jsLine[0] = indent + "...(!_ifs.c" + level + " ? [";
                    jsPostLine.push("] : []),");
                } else if (attr.name == "x-for") {
                    //...<li x-for="item in state.items" x-key="name">{{item.name + '(' + item.count + ') '}}</li>...
                    let keyName = node.getAttribute("x-key");
                    let forType = "";
                    if (keyName) {
                        forType = "key"
                    } else {
                        forType = "position"
                    }
                    //creates a comment virtual node that indicates the start of the for loop, and the type of the loop
                    js.push(indent + "createVDOM(\"#comment\", null, null, null, {index: " + index + ", forType:'" + forType + "'}, 'x-for-start'),");
                    //add loop
                    let parts = attr.value.split(" in ");
                    if (parts.length != 2) logger.error(`Error compiling template: invalid x-for attribute detected: ${attr.value}`);
                    let itemName = parts[0].trim();
                    let indexName = "index";
                    if (itemName.startsWith("(") && itemName.endsWith(")")) {
                        var arr = itemName.substring(1, itemName.length - 1).split(",");
                        itemName = arr[0].trim();
                        indexName = arr[1].trim();
                    }
                    let listName = parts[1].trim();
                    jsLine[0] = indent + "...(renderUtils.toArray(" + listName + ").map((" + itemName + ", " + indexName + ") => ";
                    jsPostLine.push(")),");
                    options.push("\"key\":" + itemName + "." + keyName);
                    //creates a comment end node that indicates the end of the for loop
                    jsPost.push(indent + "createVDOM(\"#comment\", null, null, null, {index: " + index + ", forType:'" + forType + "'}, 'x-for-end'),");
                } else if (attr.name == "x-key") {
                    //used in x-for
                } else if (attr.name == "x-show") {
                    //...<span x-show="state.value > 0">greather than 0</span>...
                    attrs.push("...(" + (attr.value) + " ? null : {style:'display:none'})");
                } else if (attr.name == "x-model") {
                    //...<input type="text" x-model="state.name"/>... --> <input type="text" x-prop:value="state.name" x-on:change="state.name = event.target.value"/>
                    //prop
                    let nodeName = node.tagName.toLowerCase();
                    let propertyName = "value";
                    let propValue = attr.value;
                    if (nodeName == "input") {
                        let type = node.getAttribute("type");
                        if (type == "range") {
                            propertyName = "valueAsNumber";
                        } else if (type == "checkbox") {
                            propertyName = "checked"
                        } else if (type == "radio") {
                            propertyName = "checked"
                            propValue += "=='" + node.getAttribute("value") + "'";
                        }
                    } else if (nodeName == "select") {
                    } else if (nodeName == "textarea") {
                    }
                    props.push(propertyName + ":" + propValue);
                    //event
                    let eventName = "change";
                    if (nodeName == "input") {
                        let type = node.getAttribute("type");
                        if (type == "number") {
                            propertyName = "valueAsNumber";
                        } else if (type == "range") {
                            propertyName = "valueAsNumber";
                        } else if (type == "checkbox") {
                            propertyName = "checked"
                        } else if (type == "radio") {
                            propValue = attr.value;
                            propertyName = "value";
                        }
                    } else if (nodeName == "select") {
                    } else if (nodeName == "textarea") {
                    }
                    events.push("'" + eventName + "': (event) => { " + propValue + " = event.target." + propertyName + "; this.invalidate(); }");

                } else if (attr.name == "x-once") {
                    //x-once: only render once
                    jsLine[0] = indent + "...((renderCount==0) ? [";
                    jsPostLine.push("] : [createVDOM('" + node.tagName.toLowerCase() + "', null, null, null, {once:true})]),");
                } else if (attr.name == "x-pre") {
                    //v-pre: skip childs
                    options.push("format:\"html\"");
                    text = JSON.stringify(node.innerHTML).replaceAll("<x:text>", "{{").replaceAll("</x:text>", "}}");
                } else if (attr.name.startsWith("x-")) {
                    //error
                    logger.error(`Error compiling template: invalid template attribute detected: ${attr.name}`);
                } else {
                    attrs.push("'" + attr.name + "':" + JSON.stringify(attr.value));
                }
            }
            jsLine.push(", " + (attrs.length ? '{' + attrs.join(',') + '}' : "null"));
            jsLine.push(", " + (props.length ? '{' + props.join(',') + '}' : "null"));
            jsLine.push(", " + (events.length ? '{' + events.join(',') + '}' : "null"));
            jsLine.push(", " + (options.length ? '{' + options.join(',') + '}' : "null"));
            if (text) {
                jsLine.push(", " + text + "");
                jsLine.push(")," + (jsPostLine.length ? jsPostLine.join('') : ''));
                js.push(jsLine.join(''));
            } else if (node.childNodes.length > 0) {
                jsLine.push(", [");
                js.push(jsLine.join(''));
                node.childNodes.forEach((subNode, subIndex) => {
                    this._compileTemplateToJsRecursive(name, subNode, subIndex, js, level + 1);
                });
                js.push(indent + "])," + (jsPostLine.length ? jsPostLine.join('') : ''));
            } else {
                jsLine.push(")," + (jsPostLine.length ? jsPostLine.join('') : ''));
                js.push(jsLine.join(''));
            }
            if (jsPost.length) js.push(jsPost.join(''));
            incs++;
        }
        return incs;
    }
    _kebabToCamel(str) {
        return str.split('-')
            .map((word, index) => index === 0 ? word : word.charAt(0).toUpperCase() + word.slice(1))
            .join('');
    }
}


/**
 * XWebComponent Class
 **/
class XWebComponent extends HTMLElement {

    // vars
    _shadowRoot = null;
    _vdom = null;
    _state = null;
    _connected = false;
    _renderDomTimeoutId = null;
    _renderCount = 0;
    _loaded = false;
    _refs = null;

    // ctor
    constructor(state) {
        super();
        //compile template to render function if not defined (this is be done only once per class)
        if (!this.constructor.prototype.render) {
            var template = this.constructor.template;
            if (!template) logger.error(`Unable to render component: template not found: ${this.name}`);
            this.constructor.prototype.render = componentLoader.compileTemplateToFunction(this.localName, "", template);
        }
        //create shadow root
        if (this.shadowRoot) {
            var id = this.getAttribute("x-id");
            var json = document.getElementById(id + "_state").textContent;
            this.state = JSON.parse(json);
            this._shadowRoot = this.shadowRoot;
            this._shadowRoot.innerHTML = "";

            //this._vdom = this._createVDOM();
        } else {
            this._shadowRoot = this.attachShadow({ 'mode': 'open' });
            this.state = state || {};
        }
    }

    // props
    get state() { return this._state; }
    set state(value) {
        this._state = new State(value, () => {
            this.invalidate();
        })
    }
    get refs() {
        if (!this._refs) {
            this._refs = new Proxy(this, {
                get(target, prop, receiver) {
                    return target._shadowRoot.querySelector("[ref='" + prop + "']");
                },
            });
        }
        return this._refs;
    }

    // methods
    onLoad() {
    }
    attributeChangedCallback(name, oldValue, newValue) {
        this.state[name] = newValue;
    }
    connectedCallback() {
        this._connected = true;
        this._renderDom();
        if (this.onLoad && !this._loadCalled) {
            this._loaded = true;
            setTimeout(() => this.onLoad(), 0);
        }

    }
    disconnectedCallback() {
        this.onUnload();
        this._connected = false;
    }
    invalidate() {
        if (this._connected) {
            if (!this._renderDomTimeoutId) {
                this._renderDomTimeoutId = window.requestAnimationFrame(() => {
                    this._renderDom();
                });
            }
        }
    }
    onUnload() { 
    }


    // static methods
    static async config(handler) {
        if (handler) handler(config);
    }
    static async start() {
        const start = performance.now();
        //log
        logger.log(`**** Starting with configuration ...`, config);
        //load components
        await Promise.all([
            await XWebComponent.loadComponents(config.preload, "start"),
            await XWebComponent.scanComponents(config.selector, "start")
        ]);        
        //log
        const end = performance.now();
        logger.log(`**** Started in ${end - start} ms`);
    }
    static async loadComponents(names, usedBy = "start") {
        await componentLoader.load(names, usedBy);
    }
    static async compileComponent(name, url, element) {
        await componentLoader.compileFileToClass(name, url, element);
    }
    static async scanComponents(selector, usedBy = "start") {
        if (selector) {
            logger.log(`Scanning for not defined web components in ${selector} ...`);
            let names = []
            document.querySelectorAll(config.selector + " *:not(:defined)").forEach((element) => {
                if (names.indexOf(element.localName) == -1) {
                    names.push(element.localName);
                }
            });
            await componentLoader.load(names, usedBy);
        }
    }
    static define(name, handler) {
        window.customElements.define(name, handler);
        return handler;
    }


    // private methods
    _createVDOM() {
        return this.render(this._state, renderUtils, this._renderCount++);        
    }
    _renderDom() {
        //count
        const start = performance.now();
        //cancel pending render
        if (this._renderDomTimeoutId) {
            window.cancelAnimationFrame(this._renderDomTimeoutId);
            this._renderDomTimeoutId = 0;
        };
        //render
        var vdom = this._createVDOM();
        //vdom to dom
        if (this._vdom == null) {
            var index = 0;
            for (var vNode of vdom) {
                while (index < vNode.options.index) {
                    var comment = document.createComment("");
                    this._shadowRoot.appendChild(comment);
                    index++;
                }
                var element = this._createDomElement(vNode);
                this._shadowRoot.appendChild(element);
                index++;
            }
            this._vdom = vdom;
        } else {
            this._diffDom(this._vdom, vdom, this._shadowRoot);
            this._vdom = vdom;
        }
        //log
        const end = performance.now();
        //logger.log(`Rendered ${this.localName}, in ${end-start} ms`);
    }
    _createDomElement(vNode) {
        //create element from vdom node
        if (vNode.tag == "#text") {
            return document.createTextNode(vNode.children);
        } else if (vNode.tag == "#comment") {
            return document.createComment(vNode.children);
        } else {
            let el = document.createElement(vNode.tag);
            for (let attr in vNode.attrs) {
                let attrValue = vNode.attrs[attr];
                if (typeof (attrValue) == "boolean") {
                    if (attrValue) {
                        el.setAttribute(attr, "");
                    }
                } else {
                    el.setAttribute(attr, attrValue);
                }
            }
            for (let prop in vNode.props) {
                let propValue = vNode.props[prop]
                el[prop] = propValue;
            }
            for (let event in vNode.events) {
                let eventHandler = vNode.events[event];
                let name = event;
                let options = {};
                if (name.indexOf(".") != -1) {
                    var modifiers = name.split(".").slice(1);
                    for (var modifier of modifiers) { //https://v2.vuejs.org/v2/guide/events
                        options[modifier] = true;
                    }
                    name = name.substring(0, name.indexOf("."));
                }
                if (typeof (eventHandler) == "string") eventHandler = new Function("event", eventHandler);                
                el.addEventListener(name, (event, ...args) => {
                    //mouse button
                    if (options.left && !event.button == 0) return false;
                    if (options.middle && !event.button == 1) return false;
                    if (options.right && !event.button == 2) return false;
                    //keys
                    if (options.alt && !event.altlKey) return false;
                    if (options.shift && !event.shiftKey) return false;
                    if (options.ctrl && !event.ctrlKey) return false;
                    if (name == "keydown" || name == "keypress" || name == "keyup") {
                        if (options.escape && event.key != "Escape") return false;
                        if (options.enter && event.key != "Enter") return false;
                        if (options.tab && event.key != "Tab") return false;
                        if (options.backspace && event.key != "Backspace") return false;
                        if (options.delete && event.key != "Delete") return false;
                        if (options.space && event.key != " ") return false;
                        if (options.up && event.key != "ArrowUp") return false;
                        if (options.down && event.key != "ArrowDown") return false;
                        if (options.left && event.key != "ArrowLeft") return false;
                        if (options.right && event.key != "ArrowRight") return false;
                    }
                    //invoke
                    var result = eventHandler.call(this, event, ...args);
                    //stop, prevent
                    if (options.stop) event.stopPropagation();
                    if (options.prevent) event.preventDefault();
                    //return
                    return result;
                }, options);
            }
            if (Array.isArray(vNode.children)) {
                var index = 0;
                for (let child of vNode.children) {
                    while (index < child.options.index) {
                        var comment = document.createComment("");
                        el.appendChild(comment);
                        index++;
                    }
                    let childElement = this._createDomElement(child);
                    el.appendChild(childElement);
                    index++;
                }
            } else if (typeof (vNode.children) == "string") {
                if (vNode.options.format == 'html') {
                    el.innerHTML = vNode.children;
                } else {
                    el.textContent = vNode.children;
                }
            }
            //if (config.debug) el.setAttribute("x-index", vNode.options.index);
            return el;
        }
    }
    _diffDom(vNodesOld, vNodesNew, parent) {
        //apply differences between two arrays of vdom elements
        var iold = 0;
        var inew = 0
        for (var i = 0; true ; i++) {
            var vNodeOld = vNodesOld[i + iold] ?? null;
            var vNodeNew = vNodesNew[i + inew] ?? null;
            if (vNodeOld == null && vNodeNew == null) {
                //nothing to do
                break;
            } else if (vNodeOld == null) {
                //append
                let element = this._createDomElement(vNodeNew);
                parent.appendChild(element);
            } else if (vNodeNew == null) {
                //remove
                parent.removeChild(parent.lastChild);
            } else if (vNodeOld.options.index < vNodeNew.options.index) {
                //remove old node
                let comment = document.createComment("");
                parent.replaceChild(comment, parent.childNodes[vNodeOld.options.index])
                inew--;
            } else if (vNodeOld.options.index > vNodeNew.options.index) {
                //replace node
                let element = this._createDomElement(vNodeNew);
                parent.replaceChild(element, parent.childNodes[vNodeNew.options.index])
                iold--;
            } else if (vNodeOld.tag == "#comment" && vNodeOld.options.forType == "key" && vNodeNew.tag == "#comment" && vNodeNew.options.forType == "key") {
                //for loop by key
                var oldStartIndex = i + iold;
                var oldEndIndex = oldStartIndex;
                while (vNodesOld[oldEndIndex].children != 'x-for-end') oldEndIndex++;
                var newStartIndex = i + inew;
                var newEndIndex = newStartIndex;
                while (vNodesNew[newEndIndex].children != 'x-for-end') newEndIndex++;
                this._diffDomListByKey(vNodesOld, oldStartIndex, oldEndIndex, vNodesNew, newStartIndex, newEndIndex, parent);
                iold += oldEndIndex - oldStartIndex;
                inew += newEndIndex - newStartIndex;
            } else if (vNodeOld.tag == "#comment" && vNodeOld.options.forType == "position" && vNodeNew.tag == "#comment" && vNodeNew.options.forType == "position") {
                //for loop by position
                var oldStartIndex = i + iold;
                var oldEndIndex = oldStartIndex;
                while (vNodesOld[oldEndIndex].children != 'x-for-end') oldEndIndex++;
                var newStartIndex = i + inew;
                var newEndIndex = newStartIndex;
                while (vNodesNew[newEndIndex].children != 'x-for-end') newEndIndex++;
                this._diffDomListByPosition(vNodesOld, oldStartIndex, oldEndIndex, vNodesNew, newStartIndex, newEndIndex, parent);
                iold += oldEndIndex - oldStartIndex;
                inew += newEndIndex - newStartIndex;
            } else if (vNodeOld.tag == "slot" && vNodeNew.tag == "slot") {
                //slot
            } else {
                //diff node
                let child = parent.childNodes[vNodeNew.options.index + inew];
                try {
                    this._diffDomElement(vNodeOld, vNodeNew, child);
                } catch (e) {
                    debugger;
                }
            }
        }
    }
    _diffDomElement(vNodeOld, vNodeNew, element) {
        if (vNodeNew.options.once) {
            return;
        }
        //attrs
        var validAttrs = [];
        for (let attr in vNodeNew.attrs) {
            var attrValue = vNodeNew.attrs[attr];
            if (attrValue != vNodeOld.attrs[attr]) {
                if (typeof (attrValue) == "boolean") {
                    if (attrValue) {
                        element.setAttribute(attr, "");
                    } else {
                        element.removeAttribute(attr);
                    }
                } else {
                    element.setAttribute(attr, attrValue);
                }
            }
            validAttrs.push(attr);
        }
        for (let attr in vNodeOld.attrs) {
            if (validAttrs.indexOf(attr) == -1) {
                element.removeAttribute(attr);
            }
        }
        //props  
        var validProps = [];
        for (let prop in vNodeNew.props) {
            var propValue = vNodeNew.props[prop];
            if (propValue != vNodeOld.props[prop]) {
                element[prop] = propValue;
            }
            validProps.push(prop);
        }
        //children
        if (Array.isArray(vNodeNew.children)) {
            this._diffDom(vNodeOld.children, vNodeNew.children, element);
            //} else if (typeof (vNodeNew.children) == "string") {
        } else {
            if (vNodeOld.children != vNodeNew.children) {
                if (vNodeNew.options.format == 'html') {
                    element.innerHTML = vNodeNew.children;
                } else if (vNodeNew.options.format == 'json') {
                    element.textContent = JSON.stringify(vNodeNew.children);
                } else {
                    element.textContent = vNodeNew.children;
                }
            }
        }
    }
    _diffDomListByPosition(vNodesOld, oldStartIndex, oldEndIndex, vNodesNew, newStartIndex, newEndIndex, parent) {
        //diff by position
        let oldLength = oldEndIndex - 1 - oldStartIndex;
        let newLength = newEndIndex - 1 - newStartIndex;
        for (let i = 0; i < newLength; i++) {
            let vNodeOld = vNodesOld[oldStartIndex + 1 + i];
            if (oldStartIndex + 1 + i >= oldEndIndex) vNodeOld = null;
            let vNodeNew = vNodesNew[newStartIndex + 1 + i];
            if (vNodeOld == null) {
                let element = this._createDomElement(vNodeNew);
                parent.insertBefore(element, parent.childNodes[newStartIndex + 1 + i]);
            } else {
                let element = parent.childNodes[newStartIndex + 1 + i];
                this._diffDomElement(vNodeOld, vNodeNew, element);
            }        
        }
        while (newLength < oldLength) {
            let element = parent.childNodes[newStartIndex + 1 + newLength];
            parent.removeChild(element);
            oldLength--;
        }        
    }
    _diffDomListByKey(vNodesOld, oldStartIndex, oldEndIndex, vNodesNew, newStartIndex, newEndIndex, parent) {
        //get oldKeys and newKeys
        let oldKeys = [];
        for (let i = oldStartIndex + 1; i < oldEndIndex; i++) {
            oldKeys.push(vNodesOld[i].options.key);
        }
        let newKeys = [];
        for (let i = newStartIndex + 1; i < newEndIndex; i++) {
            newKeys.push(vNodesNew[i].options.key);
        }       
        //check for duplicates
        const duplicates = newKeys.filter((item, index) => newKeys.indexOf(item) !== index);
        if (duplicates.length) {
            logger.warn("Duplicated keys detected in x-for loop: ", duplicates);
        }
        //remove old keys, and old DOM elements
        let removeKeys = [];
        for (let i = oldKeys.length - 1; i >= 0; i--) {
            let key = oldKeys[i];
            if (newKeys.indexOf(key) == -1) {
                parent.removeChild(parent.childNodes[newStartIndex + i + 1]);
                removeKeys.push(key);
                oldKeys.splice(i, 1);
            }
        }
        //add new keys, and new DOM elements
        let addedKeys = [];
        for (let i = 0; i < newKeys.length; i++) {
            let key = newKeys[i];
            if (oldKeys.indexOf(key) == -1) {
                let element = this._createDomElement(vNodesNew[newStartIndex + i + 1]);
                parent.insertBefore(element, parent.childNodes[newStartIndex + i + 1]);
                oldKeys.splice(i, 0, key);
                addedKeys.push(key);
            }
        }
        //reorder keys and DOM elements
        for (let i = 0; i < newKeys.length; i++) {
            let key = newKeys[i];
            let newIndex = i;
            let oldIndex = oldKeys.indexOf(key)
            if (newIndex != oldIndex) {
                parent.insertBefore(parent.childNodes[oldStartIndex + oldIndex + 1], parent.childNodes[oldStartIndex + newIndex + 1]);
                oldKeys.splice(oldIndex, 1);
                oldKeys.splice(newIndex, 0, key);

                var old = vNodesOld.splice(oldStartIndex + 1 + oldIndex, 1);
                vNodesOld.splice(oldStartIndex + 1 + newIndex, 0, old[0]);
            }
        }
        //for each element in list, apply the differences
        for (let i = 0; i < newKeys.length; i++) {
            let key = newKeys[i];
            if (addedKeys.indexOf(key) == -1) {
                let vNodeOld = vNodesOld[oldStartIndex + 1 + i];
                let vNodeNew = vNodesNew[newStartIndex + 1 + i];
                let element = parent.childNodes[newStartIndex + 1 + i];
                this._diffDomElement(vNodeOld, vNodeNew, element);
            }
        }
    }

}

//export
export default XWebComponent;

// todo
// x - options
// x - render should return an array of vdom elements? or a single vdom element with null value?
// x - VDOM: remove .div, .span, etc. and use VDOM.createElement("div", ...)
// x - componentLoader
// x - boolean Attributes
// x - if
// x - each / for / repeat ...
// x - define a format for .html files containing x components
// x - for item in state.items
// x - x-component.js -->  compile template only once!
// x - for (item,index) in state.items
// x - for (item,index) in number
// x - for (item,index) in string
// x - for (value,key,index) in object
// x - XWebComponent.scanAndLoadComponents("BODY");
// x - load referenced components recursive
// x - console.log milliseconds
// x x-on     --> x-on:, or @
// x x-prop -->  x-prop:
// x :attribute
// x v-show
// x v-model
// x v-on:....passive, .once, .self, .prevent, .stop
// x v-bind:xxxx.prop vs v-property:xxxxx
// x --> convert kebab-case attributes to camelCase state properties
// x v-attribute:[attrName]="state.attrValue"
// x x-prop camel case transformation!!
// x v-once: Render the element and component once only, and skip future updates.
// x v-pre: Skip compilation for this element and all its children.
