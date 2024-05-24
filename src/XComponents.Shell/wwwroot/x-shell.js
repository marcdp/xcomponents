import XElement from 'x-element';


/*
 * Logger
 */
const logger = new class {
    log = (message, ...args) => {
        if (config.debug) console.log(this.getTime() + "[XShell] " + message, ...args);
    }
    warn = (message, ...args) => {
        console.warn(this.getTime() + "[XShell] " + message, ...args);
    }
    error = (message, ...args) => {
        console.error(this.getTime() + "[XShell] " + message, ...args);
    }
    getTime() {
        var date = new Date();
        return date.toTimeString().slice(0, 8) + "." + date.getMilliseconds() + " ";
    }
}


/*
 * Config
 */
const config = {
    copyright: `\u00A9 ${new Date().getFullYear()} DProjects`,
    debug: false,
    manifest: "",
    menus: [],
    logo: {
        src: "",
        target: "",
        title: "",
        href: "",
    },
    selector: "BODY",
    stylesheets: {},
    user: {
        avatar: "",
        username: "anonymous",
        name: "Anonymous user",
        roles: [],
        claims: {}
    }

}

const XPage = class {
    static define(name, handler) {
        debugger;
    }
}

const XShell = {
    //var
    _promises: [],
    //methods
    config(handler) {
        //config
        if (handler) handler(config);
        //add key to all menuitems
        var index = 0;
        var f = function (menuitems) {
            for (var menuitem of menuitems) {
                menuitem.key = "key" + (index++)
                if (menuitem.children) f(menuitem.children);
            }
        };
        for (var key in config.menus) {
            f(config.menus[key])
        }
        //load stylesheets
        for (let stylesheet in config.stylesheets) {
            this._promises.push(new Promise(function (resolve, reject) {
                var href = config.stylesheets[stylesheet];
                var media = "all";
                var link = document.createElement("link");
                link.setAttribute("rel", "stylesheet");
                link.setAttribute("type", "text/css");
                link.setAttribute("href", href);
                link.setAttribute("media", media);
                document.head.appendChild(link);
                link.addEventListener("load", function () {
                    resolve();
                });
                link.addEventListener("error", function () {
                    reject();
                });
            }));
        }
        //load manifest
        if (config.manifest) {
            this._promises.push(new Promise(async function (resolve, reject) {
                let resp = await fetch(config.manifest);
                config.manifest = await resp.json();
                resolve();
            }));
        }
    },
    async start() {
        const start = performance.now();
        logger.log(`**** Starting with configuration ...`, config);
        //start XElement
        this._promises.push(XElement.start());
        await Promise.all(this._promises);
        //log
        const end = performance.now();
        logger.log(`**** Started in ${end - start} ms`);
    },
    getSelectedMenuItems(url) {
        var menuitems = [];
        for (var menu in config.menus) {
            menuitems.push(...config.menus[menu])
        }
        var result = [];
        var f = function (menuitems) {
            var found = null;
            for (var menuitem of menuitems) {
                if (menuitem.href == url) {
                    found = menuitem;
                }
                if (menuitem.children) {
                    if (f(menuitem.children)) {
                        found = menuitem;
                    }
                }
            }
            if (found) result.push(found);
            return found;
        };
        f(menuitems)
        result.reverse();
        return result;
    }
}

// export
export default XShell;
export { config, logger, XPage };


// todo:
// x - xshell autoload used web components in template
// x - open search box -> cancel bubble --> useCapture al attachar els events;
// x - show submenu a top like github  https://github.com/marcdp
// x - x-page -> show breadcrumb in  x-shell-header
// x - x-model attribute
// x - x-page ?? --> OBJECTIU: que el que hi hagi dins de x-shell-page tingui un contexte de pagina !!!!!!!!!!!!!!!!! (no dins del ShadowDOM!!!!!)


// - element.x --> afegir tags <template>...</template> als SFC

// - x-page -> use page with specified in x-page



// icons library

// - x-datafield (label + input)
// - show menu at left with all the sub menuitems

// - generic error handler
// - generic file upload
// - generic i18n mechanism
// - autotranslate?

// - x-datafield multilanguage

// - search box > search in menus

// - allow push page to hash https://server/path/to/page#/path2/to/page2?var1=123#/path3/to/page3
// - allow show page in modal dialog
// - allow show page embedded in current page

// - menuitem -> embed page in menuitemposition

// - login page
// - load blazor page without frames

