
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
 * Context
 */
class Context {
    renderCount = 0;
    constructor(renderCount) {
        this.renderCount = renderCount;
    }
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
        //create shadow root
        if (false && this.shadowRoot) {
            var id = this.getAttribute("x-id");
            var json = document.getElementById(id + "_state").textContent;
            this.state = JSON.parse(json);
            this._shadowRoot = this.shadowRoot;
            this._shadowRoot.innerHTML = "";
            //this._vdom = this.render(this._state, new Context(this._renderCount++));
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


    //// static methods
    //static async config(handler) {
    //    if (handler) handler(config);
    //}
    //static async start() { 
    //    const start = performance.now();
    //    //log
    //    logger.log(`**** Starting with configuration ...`, config);
    //    //load components
    //    await Promise.all([
    //        await XWebComponent.loadComponents(config.preload, "start"),
    //        await XWebComponent.scanComponents(config.selector, "start")
    //    ]);        
    //    //log
    //    const end = performance.now();
    //    logger.log(`**** Started in ${end - start} ms`);
    //}
    //static async loadComponents(names, usedBy = "start") {
    //    await componentLoader.load(names, usedBy);
    //}
    //static async compileComponent(name, url, element) {
    //    await componentLoader.compileFileToClass(name, url, element);
    //}
    //static async scanComponents(selector, usedBy = "start") {
    //    if (selector) {
    //        logger.log(`Scanning for not defined web components in ${selector} ...`);
    //        let names = []
    //        document.querySelectorAll(config.selector + " *:not(:defined)").forEach((element) => {
    //            if (names.indexOf(element.localName) == -1) {
    //                names.push(element.localName);
    //            }
    //        });
    //        await componentLoader.load(names, usedBy);
    //    }
    //}
    static define(name, handler) {
        window.customElements.define(name, handler);
        return handler;
    }


    // private methods
    _handleEvent(event, handlerArgs = []) {
        var handlerName = handlerArgs[0];
        logger.log(`Dispatching event to ${handlerName} ...`);
        var handler = this[handlerName];
        var result = handler.call(this, event, ...handlerArgs.splice(1));
        return result;
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
        var vdom = this.render(this._state, new Context(this._renderCount++));
        //vdom to dom
        if (this._vdom == null) {
            this._diffDom([], vdom, this._shadowRoot);
        } else {
            this._diffDom(this._vdom, vdom, this._shadowRoot);
        }
        this._vdom = vdom;
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
                    var result = this._handleEvent(event, eventHandler);
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
                    while (index < child.index) {
                        var comment = document.createComment("");
                        el.appendChild(comment);
                        index++;
                    }
                    let childElement = this._createDomElement(child);
                    el.appendChild(childElement);
                    index++;
                }
            } else if (typeof (vNode.children) == "string") {
                if (vNode.format == 'html') {
                    el.innerHTML = vNode.children;
                } else {
                    el.textContent = vNode.children;
                }
            }
            //if (config.debug) el.setAttribute("x-index", vNode.index);
            return el;
        }
    }
    _diffDom(vNodesOld, vNodesNew, parent) {
        //apply differences between two arrays of vdom elements
        var iold = 0;
        var inew = 0
        var incs = 0;
        for (var i = 0; true ; i++) {
            var vNodeOld = vNodesOld[i + iold] ?? null;
            var vNodeNew = vNodesNew[i + inew] ?? null;
            if (vNodeOld == null && vNodeNew == null) {
                //nothing to do
                break;
            } else if (vNodeOld == null) {
                //append
                if (vNodeNew.tag == "#comment" && vNodeNew.children == 'x-for-end') {
                    var newEndIndex = i + inew;
                    var newStartIndex = newEndIndex;
                    while (vNodesNew[newStartIndex].children != 'x-for-start') newStartIndex--;
                    incs += newEndIndex - newStartIndex;
                }
                while (parent.childNodes.length < vNodeNew.index + incs) {
                    parent.appendChild(document.createComment(""));
                }
                let element = this._createDomElement(vNodeNew);
                parent.appendChild(element);
            } else if (vNodeNew == null) {
                //remove
                parent.removeChild(parent.lastChild);
                throw new DOMException("Not implemented");
            } else if (vNodeOld.index < vNodeNew.index) {
                //remove old node
                //debugger;
                //let elementNew = document.createComment("");
                //let elementOld = parent.childNodes[vNodeOld.index + incs];
                //parent.replaceChild(elementNew, elementOld)
                //inew--;
                throw new DOMException("Not implemented");
            } else if (vNodeOld.index > vNodeNew.index) {
                //replace node
                //debugger;
                //let elementNew = this._createDomElement(vNodeNew);
                //let elementOld = parent.childNodes[vNodeNew.index + incs];
                //parent.replaceChild(elementNew, elementOld);
                //iold--;
                throw new DOMException("Not implemented");
            } else if (vNodeOld.tag == "#comment" && vNodeOld.forType == "key" && vNodeNew.tag == "#comment" && vNodeNew.forType == "key") {
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
            } else if (vNodeOld.tag == "#comment" && vNodeOld.forType == "position" && vNodeNew.tag == "#comment" && vNodeNew.forType == "position") {
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
                incs += newEndIndex - newStartIndex;
            } else if (vNodeOld.tag == "slot" && vNodeNew.tag == "slot") {
                //slot 
            } else if (vNodeOld.tag != vNodeNew.tag) {
                //replace node
                let elementNew = this._createDomElement(vNodeNew);
                let elementOld = parent.childNodes[vNodeNew.index + incs];
                parent.replaceChild(elementNew, elementOld);
            } else {
                //diff node
                let child = parent.childNodes[vNodeNew.index + incs];
                //let child = parent.childNodes[i + inew];
                try {
                    this._diffDomElement(vNodeOld, vNodeNew, child);
                } catch (e) {
                    debugger;
                }
            }
        }
    }
    _diffDomElement(vNodeOld, vNodeNew, element) {
        if (vNodeNew.once) {
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
        } else if (typeof (vNodeNew.children) == "undefined") {
            element.innerHTML = "";
        } else {
            if (vNodeOld.children != vNodeNew.children) {
                if (vNodeNew.format == 'html') {
                    element.innerHTML = vNodeNew.children;
                } else if (vNodeNew.format == 'json') {
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
            oldKeys.push(vNodesOld[i].key);
        }
        let newKeys = [];
        for (let i = newStartIndex + 1; i < newEndIndex; i++) {
            newKeys.push(vNodesNew[i].key);
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
