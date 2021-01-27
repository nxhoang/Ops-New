/*
Product Name: dhtmlxList
Version: 5.1.0
Edition: Standard
License: content of this file is covered by DHTMLX Commercial or enterpri. Usage outside GPL terms is prohibited. To obtain Commercial or Enterprise license contact sales@dhtmlx.com
Copyright UAB Dinamenta http://www.dhtmlx.com
 */

if (!window.dhtmlx) {
	dhtmlx = {}
}
dhtmlx.assert = function (b, a) {
	if (!b) {
		dhtmlx.error(a)
	}
};
dhtmlx.assert_enabled = function () {
	return false
};
dhtmlx.assert_event = function (f, c) {
	if (!f._event_check) {
		f._event_check = {};
		f._event_check_size = {}
	}
	for (var b in c) {
		f._event_check[b.toLowerCase()] = c[b];
		var e = -1;
		for (var d in c[b]) {
			e++
		}
		f._event_check_size[b.toLowerCase()] = e
	}
};
dhtmlx.assert_method_info = function (e, b, d, f) {
	var a = [];
	for (var c = 0; c < f.length; c++) {
		a.push(f[c][0] + " : " + f[c][1] + "\n   " + f[c][2].describe() + (f[c][3] ? "; optional" : ""))
	}
	return e.name + "." + b + "\n" + d + "\n Arguments:\n - " + a.join("\n - ")
};
dhtmlx.assert_method = function (c, a) {
	for (var b in a) {
		dhtmlx.assert_method_process(c, b, a[b].descr, a[b].args, (a[b].min || 99), a[b].skip)
	}
};
dhtmlx.assert_method_process = function (f, b, e, g, c, d) {
	var a = f[b];
	if (!d) {
		f[b] = function () {
			if (arguments.length != g.length && arguments.length < c) {
				dhtmlx.log("warn", "Incorrect count of parameters\n" + f[b].describe() + "\n\nExpecting " + g.length + " but have only " + arguments.length)
			} else {
				for (var h = 0; h < g.length; h++) {
					if (!g[h][3] && !g[h][2](arguments[h])) {
						dhtmlx.log("warn", "Incorrect method call\n" + f[b].describe() + "\n\nActual value of " + (h + 1) + " parameter: {" + (typeof arguments[h]) + "} " + arguments[h])
					}
				}
			}
			return a.apply(this, arguments)
		}
	}
	f[b].describe = function () {
		return dhtmlx.assert_method_info(f, b, e, g)
	}
};
dhtmlx.assert_event_call = function (c, b, a) {
	if (c._event_check) {
		if (!c._event_check[b]) {
			dhtmlx.log("warn", "Not expected event call :" + b)
		} else {
			if (dhtmlx.isNotDefined(a)) {
				dhtmlx.log("warn", "Event without parameters :" + b)
			} else {
				if (c._event_check_size[b] != a.length) {
					dhtmlx.log("warn", "Incorrect event call, expected " + c._event_check_size[b] + " parameter(s), but have " + a.length + " parameter(s), for " + b + " event")
				}
			}
		}
	}
};
dhtmlx.assert_event_attach = function (b, a) {
	if (b._event_check && !b._event_check[a]) {
		dhtmlx.log("warn", "Unknown event name: " + a)
	}
};
dhtmlx.assert_property = function (b, a) {
	if (!b._settings_check) {
		b._settings_check = {}
	}
	dhtmlx.extend(b._settings_check, a)
};
dhtmlx.assert_check = function (c, b) {
	if (typeof c == "object") {
		for (var a in c) {
			dhtmlx.assert_settings(a, c[a], b)
		}
	}
};
dhtmlx.assert_settings = function (h, e, d) {
	d = d || this._settings_check;
	if (d) {
		if (!d[h]) {
			return dhtmlx.log("warn", "Unknown propery: " + h)
		}
		var g = "";
		var b = "";
		var a = false;
		for (var c = 0; c < d[h].length; c++) {
			var f = d[h][c];
			if (typeof f == "string") {
				continue
			}
			if (typeof f == "function") {
				a = a || f(e)
			} else {
				if (typeof f == "object" && typeof f[1] == "function") {
					a = a || f[1](e);
					if (a && f[2]) {
						dhtmlx.assert_check(e, f[2])
					}
				}
			}
			if (a) {
				break
			}
		}
		if (!a) {
			dhtmlx.log("warn", "Invalid configuration\n" + dhtmlx.assert_info(h, d) + "\nActual value: {" + (typeof e) + "} " + e)
		}
	}
};
dhtmlx.assert_info = function (b, f) {
	var a = f[b];
	var e = "";
	var d = [];
	for (var c = 0; c < a.length; c++) {
		if (typeof rule == "string") {
			e = a[c]
		} else {
			if (a[c].describe) {
				d.push(a[c].describe())
			} else {
				if (a[c][1] && a[c][1].describe) {
					d.push(a[c][1].describe())
				}
			}
		}
	}
	return "Property: " + b + ", " + e + " \nExpected value: \n - " + d.join("\n - ")
};
if (dhtmlx.assert_enabled()) {
	dhtmlx.assert_rule_color = function (a) {
		if (typeof a != "string") {
			return false
		}
		if (a.indexOf("#") !== 0) {
			return false
		}
		if (a.substr(1).replace(/[0-9A-F]/gi, "") !== "") {
			return false
		}
		return true
	};
	dhtmlx.assert_rule_color.describe = function () {
		return "{String} Value must start from # and contain hexadecimal code of color"
	};
	dhtmlx.assert_rule_template = function (a) {
		if (typeof a == "function") {
			return true
		}
		if (typeof a == "string") {
			return true
		}
		return false
	};
	dhtmlx.assert_rule_template.describe = function () {
		return "{Function},{String} Value must be a function which accepts data object and return text string, or a sting with optional template markers"
	};
	dhtmlx.assert_rule_boolean = function (a) {
		if (typeof a == "boolean") {
			return true
		}
		return false
	};
	dhtmlx.assert_rule_boolean.describe = function () {
		return "{Boolean} true or false"
	};
	dhtmlx.assert_rule_object = function (a, b) {
		if (typeof a == "object") {
			return true
		}
		return false
	};
	dhtmlx.assert_rule_object.describe = function () {
		return "{Object} Configuration object"
	};
	dhtmlx.assert_rule_string = function (a) {
		if (typeof a == "string") {
			return true
		}
		return false
	};
	dhtmlx.assert_rule_string.describe = function () {
		return "{String} Plain string"
	};
	dhtmlx.assert_rule_htmlpt = function (a) {
		return !!dhtmlx.toNode(a)
	};
	dhtmlx.assert_rule_htmlpt.describe = function () {
		return "{Object},{String} HTML node or ID of HTML Node"
	};
	dhtmlx.assert_rule_notdocumented = function (a) {
		return false
	};
	dhtmlx.assert_rule_notdocumented.describe = function () {
		return "This options wasn't documented"
	};
	dhtmlx.assert_rule_key = function (b) {
		var a = function (c) {
			return b[c]
		};
		a.describe = function () {
			var d = [];
			for (var c in b) {
				d.push(c)
			}
			return "{String} can take one of next values: " + d.join(", ")
		};
		return a
	};
	dhtmlx.assert_rule_dimension = function (a) {
		if (a * 1 == a && !isNaN(a) && a >= 0) {
			return true
		}
		return false
	};
	dhtmlx.assert_rule_dimension.describe = function () {
		return "{Integer} value must be a positive number"
	};
	dhtmlx.assert_rule_number = function (a) {
		if (typeof a == "number") {
			return true
		}
		return false
	};
	dhtmlx.assert_rule_number.describe = function () {
		return "{Integer} value must be a number"
	};
	dhtmlx.assert_rule_function = function (a) {
		if (typeof a == "function") {
			return true
		}
		return false
	};
	dhtmlx.assert_rule_function.describe = function () {
		return "{Function} value must be a custom function"
	};
	dhtmlx.assert_rule_any = function (a) {
		return true
	};
	dhtmlx.assert_rule_any.describe = function () {
		return "Any value"
	};
	dhtmlx.assert_rule_mix = function (d, c) {
		var e = function (a) {
			if (d(a) || c(a)) {
				return true
			}
			return false
		};
		e.describe = function () {
			return d.describe()
		};
		return e
	}
}
dhtmlx.codebase = "./";
dhtmlx.copy = function (b) {
	var a = dhtmlx.copy._function;
	a.prototype = b;
	return new a()
};
dhtmlx.copy._function = function () {};
dhtmlx.extend = function (b, a) {
	for (var c in a) {
		b[c] = a[c]
	}
	if (dhtmlx.assert_enabled() && a._assert) {
		b._assert();
		b._assert = null
	}
	dhtmlx.assert(b, "Invalid nesting target");
	dhtmlx.assert(a, "Invalid nesting source");
	if (a._init) {
		b._init()
	}
	return b
};
dhtmlx.proto_extend = function () {
	var f = arguments;
	var c = f[0];
	var b = [];
	for (var e = f.length - 1; e > 0; e--) {
		if (typeof f[e] == "function") {
			f[e] = f[e].prototype
		}
		for (var d in f[e]) {
			if (d == "_init") {
				b.push(f[e][d])
			} else {
				if (!c[d]) {
					c[d] = f[e][d]
				}
			}
		}
	}
	if (f[0]._init) {
		b.push(f[0]._init)
	}
	c._init = function () {
		for (var g = 0; g < b.length; g++) {
			b[g].apply(this, arguments)
		}
	};
	c.base = f[1];
	var a = function (g) {
		this._init(g);
		if (this._parseSettings) {
			this._parseSettings(g, this.defaults)
		}
	};
	a.prototype = c;
	c = f = null;
	return a
};
dhtmlx.bind = function (b, a) {
	return function () {
		return b.apply(a, arguments)
	}
};
dhtmlx.require = function (a) {
	if (!dhtmlx._modules[a]) {
		dhtmlx.assert(dhtmlx.ajax, "load module is required");
		dhtmlx.exec(dhtmlx.ajax().sync().get(dhtmlx.codebase + a).responseText);
		dhtmlx._modules[a] = true
	}
};
dhtmlx._modules = {};
dhtmlx.exec = function (code) {
	if (window.execScript) {
		window.execScript(code)
	} else {
		window.eval(code)
	}
};
dhtmlx.methodPush = function (a, c, b) {
	return function () {
		var d = false;
		d = a[c].apply(a, arguments);
		return d
	}
};
dhtmlx.isNotDefined = function (b) {
	return typeof b == "undefined"
};
dhtmlx.delay = function (d, b, c, a) {
	setTimeout(function () {
		var e = d.apply(b, c);
		d = b = c = null;
		return e
	}, a || 1)
};
dhtmlx.uid = function () {
	if (!this._seed) {
		this._seed = (new Date).valueOf()
	}
	this._seed++;
	return this._seed
};
dhtmlx.toNode = function (a) {
	if (typeof a == "string") {
		return document.getElementById(a)
	}
	return a
};
dhtmlx.toArray = function (a) {
	return dhtmlx.extend((a || []), dhtmlx.PowerArray)
};
dhtmlx.toFunctor = function (str) {
	return (typeof(str) == "string") ? eval(str) : str
};
dhtmlx._events = {};
dhtmlx.event = function (d, c, a, b) {
	d = dhtmlx.toNode(d);
	var e = dhtmlx.uid();
	dhtmlx._events[e] = [d, c, a];
	if (b) {
		a = dhtmlx.bind(a, b)
	}
	if (d.addEventListener) {
		d.addEventListener(c, a, false)
	} else {
		if (d.attachEvent) {
			d.attachEvent("on" + c, a)
		}
	}
	return e
};
dhtmlx.eventRemove = function (b) {
	if (!b) {
		return
	}
	dhtmlx.assert(this._events[b], "Removing non-existing event");
	var a = dhtmlx._events[b];
	if (a[0].removeEventListener) {
		a[0].removeEventListener(a[1], a[2], false)
	} else {
		if (a[0].detachEvent) {
			a[0].detachEvent("on" + a[1], a[2])
		}
	}
	delete this._events[b]
};
dhtmlx.log = function (b, c, a) {
	if (window.console && console.log) {
		b = b.toLowerCase();
		if (window.console[b]) {
			window.console[b](c || "unknown error")
		} else {
			window.console.log(b + ": " + c)
		}
		if (a) {
			window.console.log(a)
		}
	}
};
dhtmlx.log_full_time = function (a) {
	dhtmlx._start_time_log = new Date();
	dhtmlx.log("Info", "Timing start [" + a + "]");
	window.setTimeout(function () {
		var b = new Date();
		dhtmlx.log("Info", "Timing end [" + a + "]:" + (b.valueOf() - dhtmlx._start_time_log.valueOf()) / 1000 + "s")
	}, 1)
};
dhtmlx.log_time = function (a) {
	var c = "_start_time_log" + a;
	if (!dhtmlx[c]) {
		dhtmlx[c] = new Date();
		dhtmlx.log("Info", "Timing start [" + a + "]")
	} else {
		var b = new Date();
		dhtmlx.log("Info", "Timing end [" + a + "]:" + (b.valueOf() - dhtmlx[c].valueOf()) / 1000 + "s");
		dhtmlx[c] = null
	}
};
dhtmlx.error = function (b, a) {
	dhtmlx.log("error", b, a)
};
dhtmlx.EventSystem = {
	_init: function () {
		this._events = {};
		this._handlers = {};
		this._map = {}
	},
	block: function () {
		this._events._block = true
	},
	unblock: function () {
		this._events._block = false
	},
	mapEvent: function (a) {
		dhtmlx.extend(this._map, a)
	},
	callEvent: function (c, e) {
		if (this._events._block) {
			return true
		}
		c = c.toLowerCase();
		dhtmlx.assert_event_call(this, c, e);
		var d = this._events[c.toLowerCase()];
		var a = true;
		if (dhtmlx.debug) {
			dhtmlx.log("info", "[" + this.name + "] event:" + c, e)
		}
		if (d) {
			for (var b = 0; b < d.length; b++) {
				if (d[b].apply(this, (e || [])) === false) {
					a = false
				}
			}
		}
		if (this._map[c] && !this._map[c].callEvent(c, e)) {
			a = false
		}
		return a
	},
	attachEvent: function (b, a, d) {
		b = b.toLowerCase();
		dhtmlx.assert_event_attach(this, b);
		d = d || dhtmlx.uid();
		a = dhtmlx.toFunctor(a);
		var c = this._events[b] || dhtmlx.toArray();
		c.push(a);
		this._events[b] = c;
		this._handlers[d] = {
			f: a,
			t: b
		};
		return d
	},
	detachEvent: function (d) {
		if (this._handlers[d]) {
			var b = this._handlers[d].t;
			var a = this._handlers[d].f;
			var c = this._events[b];
			c.remove(a);
			delete this._handlers[d]
		}
	}
};
dhtmlx.PowerArray = {
	removeAt: function (b, a) {
		if (b >= 0) {
			this.splice(b, (a || 1))
		}
	},
	remove: function (a) {
		this.removeAt(this.find(a))
	},
	insertAt: function (c, d) {
		if (!d && d !== 0) {
			this.push(c)
		} else {
			var a = this.splice(d, (this.length - d));
			this[d] = c;
			this.push.apply(this, a)
		}
	},
	find: function (a) {
		for (i = 0; i < this.length; i++) {
			if (a == this[i]) {
				return i
			}
		}
		return -1
	},
	each: function (a, c) {
		for (var b = 0; b < this.length; b++) {
			a.call((c || this), this[b])
		}
	},
	map: function (a, c) {
		for (var b = 0; b < this.length; b++) {
			this[b] = a.call((c || this), this[b])
		}
		return this
	}
};
dhtmlx.env = {};
if (navigator.userAgent.indexOf("Opera") != -1) {
	dhtmlx._isOpera = true
} else {
	dhtmlx._isIE = !!document.all;
	dhtmlx._isFF = !document.all;
	dhtmlx._isWebKit = (navigator.userAgent.indexOf("KHTML") != -1);
	if (navigator.appVersion.indexOf("MSIE 8.0") != -1 && document.compatMode != "BackCompat") {
		dhtmlx._isIE = 8
	}
	if (navigator.appVersion.indexOf("MSIE 9.0") != -1 && document.compatMode != "BackCompat") {
		dhtmlx._isIE = 9
	}
}
dhtmlx.env = {};
(function () {
	dhtmlx.env.transform = false;
	dhtmlx.env.transition = false;
	var a = {};
	a.names = ["transform", "transition"];
	a.transform = ["transform", "WebkitTransform", "MozTransform", "oTransform", "msTransform"];
	a.transition = ["transition", "WebkitTransition", "MozTransition", "oTransition"];
	var e = document.createElement("DIV");
	var c;
	for (var b = 0; b < a.names.length; b++) {
		while (p = a[a.names[b]].pop()) {
			if (typeof e.style[p] != "undefined") {
				dhtmlx.env[a.names[b]] = true
			}
		}
	}
})();
dhtmlx.env.transform_prefix = (function () {
	var a;
	if (dhtmlx._isOpera) {
		a = "-o-"
	} else {
		a = "";
		if (dhtmlx._isFF) {
			a = "-moz-"
		}
		if (dhtmlx._isWebKit) {
			a = "-webkit-"
		}
	}
	return a
})();
dhtmlx.env.svg = (function () {
	return document.implementation.hasFeature("http://www.w3.org/TR/SVG11/feature#BasicStructure", "1.1")
})();
dhtmlx.zIndex = {
	drag: 10000
};
dhtmlx.html = {
	create: function (b, a, c) {
		a = a || {};
		var d = document.createElement(b);
		for (var e in a) {
			d.setAttribute(e, a[e])
		}
		if (a.style) {
			d.style.cssText = a.style
		}
		if (a["class"]) {
			d.className = a["class"]
		}
		if (c) {
			d.innerHTML = c
		}
		return d
	},
	getValue: function (a) {
		a = dhtmlx.toNode(a);
		if (!a) {
			return ""
		}
		return dhtmlx.isNotDefined(a.value) ? a.innerHTML : a.value
	},
	remove: function (b) {
		if (b instanceof Array) {
			for (var a = 0; a < b.length; a++) {
				this.remove(b[a])
			}
		} else {
			if (b && b.parentNode) {
				b.parentNode.removeChild(b)
			}
		}
	},
	insertBefore: function (b, c, a) {
		if (!b) {
			return
		}
		if (c) {
			c.parentNode.insertBefore(b, c)
		} else {
			a.appendChild(b)
		}
	},
	locate: function (b, d) {
		b = b || event;
		var a = b.target || b.srcElement;
		while (a) {
			if (a.getAttribute) {
				var c = a.getAttribute(d);
				if (c) {
					return c
				}
			}
			a = a.parentNode
		}
		return null
	},
	offset: function (d) {
		if (d.getBoundingClientRect) {
			var g = d.getBoundingClientRect();
			var h = document.body;
			var b = document.documentElement;
			var a = window.pageYOffset || b.scrollTop || h.scrollTop;
			var e = window.pageXOffset || b.scrollLeft || h.scrollLeft;
			var f = b.clientTop || h.clientTop || 0;
			var j = b.clientLeft || h.clientLeft || 0;
			var k = g.top + a - f;
			var c = g.left + e - j;
			return {
				y: Math.round(k),
				x: Math.round(c)
			}
		} else {
			var k = 0,
			c = 0;
			while (d) {
				k = k + parseInt(d.offsetTop, 10);
				c = c + parseInt(d.offsetLeft, 10);
				d = d.offsetParent
			}
			return {
				y: k,
				x: c
			}
		}
	},
	pos: function (a) {
		a = a || event;
		if (a.pageX || a.pageY) {
			return {
				x: a.pageX,
				y: a.pageY
			}
		}
		var b = ((dhtmlx._isIE) && (document.compatMode != "BackCompat")) ? document.documentElement : document.body;
		return {
			x: a.clientX + b.scrollLeft - b.clientLeft,
			y: a.clientY + b.scrollTop - b.clientTop
		}
	},
	preventEvent: function (a) {
		if (a && a.preventDefault) {
			a.preventDefault()
		}
		dhtmlx.html.stopEvent(a)
	},
	stopEvent: function (a) {
		(a || event).cancelBubble = true;
		return false
	},
	addCss: function (b, a) {
		b.className += " " + a
	},
	removeCss: function (b, a) {
		b.className = b.className.replace(RegExp(a, "g"), "")
	}
};
(function () {
	var a = document.getElementsByTagName("SCRIPT");
	dhtmlx.assert(a.length, "Can't locate codebase");
	if (a.length) {
		a = (a[a.length - 1].getAttribute("src") || "").split("/");
		a.splice(a.length - 1, 1);
		dhtmlx.codebase = a.slice(0, a.length).join("/") + "/"
	}
})();
if (!dhtmlx.ui) {
	dhtmlx.ui = {}
}
dhtmlx.Destruction = {
	_init: function () {
		dhtmlx.destructors.push(this)
	},
	destructor: function (a) {
		this.destructor = function () {};
		this._htmlmap = null;
		this._htmlrows = null;
		if (this._html) {
			document.body.appendChild(this._html)
		}
		this._html = null;
		if (this._obj) {
			this._obj.innerHTML = "";
			this._obj._htmlmap = null
		}
		this._obj = this._dataobj = null;
		this.data = null;
		this._events = this._handlers = {};
		this.canvases = [];
		if (this.render) {
			this.render = function () {}
		}
	}
};
dhtmlx.destructors = [];
dhtmlx.event(window, "unload", function () {
	if (dhtmlx.destructors) {
		for (var c = 0; c < dhtmlx.destructors.length; c++) {
			dhtmlx.destructors[c].destructor(-1)
		}
		dhtmlx.destructors = []
	}
	for (var b in dhtmlx._events) {
		var d = dhtmlx._events[b];
		if (d[0].removeEventListener) {
			d[0].removeEventListener(d[1], d[2], false)
		} else {
			if (d[0].detachEvent) {
				d[0].detachEvent("on" + d[1], d[2])
			}
		}
		delete dhtmlx._events[b]
	}
});
dhtmlx.ajax = function (a, b, c) {
	if (arguments.length !== 0) {
		var d = new dhtmlx.ajax();
		if (c) {
			d.master = c
		}
		d.get(a, null, b)
	}
	if (!this.getXHR) {
		return new dhtmlx.ajax()
	}
	return this
};
dhtmlx.ajax.prototype = {
	getXHR: function () {
		if (dhtmlx._isIE) {
			return new ActiveXObject("Microsoft.xmlHTTP")
		} else {
			return new XMLHttpRequest()
		}
	},
	send: function (e, j, g) {
		var b = this.getXHR();
		if (typeof g == "function") {
			g = [g]
		}
		if (typeof j == "object") {
			var f = [];
			for (var c in j) {
				var h = j[c];
				if (h === null || h === dhtmlx.undefined) {
					h = ""
				}
				f.push(c + "=" + encodeURIComponent(h))
			}
			j = f.join("&")
		}
		if (j && !this.post) {
			e = e + (e.indexOf("?") != -1 ? "&" : "?") + j;
			j = null
		}
		b.open(this.post ? "POST" : "GET", e, !this._sync);
		if (this.post) {
			b.setRequestHeader("Content-type", "application/x-www-form-urlencoded")
		}
		var d = this;
		b.onreadystatechange = function () {
			if (!b.readyState || b.readyState == 4) {
				if (g && d) {
					for (var a = 0; a < g.length; a++) {
						if (g[a]) {
							g[a].call((d.master || d), b.responseText, b.responseXML, b)
						}
					}
				}
				d.master = null;
				g = d = null
			}
		};
		b.send(j || null);
		return b
	},
	get: function (a, c, b) {
		this.post = false;
		return this.send(a, c, b)
	},
	post: function (a, c, b) {
		this.post = true;
		return this.send(a, c, b)
	},
	sync: function () {
		this._sync = true;
		return this
	}
};
dhtmlx.AtomDataLoader = {
	_init: function (a) {
		this.data = {};
		if (a) {
			this._settings.datatype = a.datatype || "json";
			this._after_init.push(this._load_when_ready)
		}
	},
	_load_when_ready: function () {
		this._ready_for_data = true;
		if (this._settings.url) {
			this.url_setter(this._settings.url)
		}
		if (this._settings.data) {
			this.data_setter(this._settings.data)
		}
	},
	url_setter: function (a) {
		if (!this._ready_for_data) {
			return a
		}
		this.load(a, this._settings.datatype);
		return a
	},
	data_setter: function (a) {
		if (!this._ready_for_data) {
			return a
		}
		this.parse(a, this._settings.datatype);
		return true
	},
	load: function (a, b) {
		this.callEvent("onXLS", []);
		if (typeof b == "string") {
			this.data.driver = dhtmlx.DataDriver[b];
			b = arguments[2]
		} else {
			this.data.driver = dhtmlx.DataDriver[this._settings.datatype || "xml"]
		}
		if (window.dhx4) {
			return dhx4.ajax.get(a, dhtmlx.bind(function (d) {
					var c = d.xmlDoc;
					var f = c.responseText;
					var e = c.responseXML;
					if (this._onLoad) {
						this._onLoad.call(this, f, e, c)
					}
					if (b) {
						b.call(this, f, e, c)
					}
				}, this))
		} else {
			dhtmlx.ajax(a, [this._onLoad, b], this)
		}
	},
	parse: function (b, a) {
		this.callEvent("onXLS", []);
		this.data.driver = dhtmlx.DataDriver[a || "xml"];
		this._onLoad(b, null)
	},
	_onLoad: function (e, b, a) {
		var c = this.data.driver;
		var d = c.getRecords(c.toObject(e, b))[0];
		this.data = (c ? c.getDetails(d) : e);
		this.callEvent("onXLE", [])
	},
	_check_data_feed: function (b) {
		if (!this._settings.dataFeed || this._ignore_feed || !b) {
			return true
		}
		var a = this._settings.dataFeed;
		if (typeof a == "function") {
			return a.call(this, (b.id || b), b)
		}
		a = a + (a.indexOf("?") == -1 ? "?" : "&") + "action=get&id=" + encodeURIComponent(b.id || b);
		this.callEvent("onXLS", []);
		dhtmlx.ajax(a, function (d, c) {
			this._ignore_feed = true;
			this.setValues(dhtmlx.DataDriver.json.toObject(d)[0]);
			this._ignore_feed = false;
			this.callEvent("onXLE", [])
		}, this);
		return false
	}
};
dhtmlx.DataDriver = {};
dhtmlx.DataDriver.json = {
	toObject: function (data) {
		if (!data) {
			data = "[]"
		}
		if (typeof data == "string") {
			eval("dhtmlx.temp=" + data);
			return dhtmlx.temp
		}
		return data
	},
	getRecords: function (a) {
		if (a && a.data) {
			a = a.data
		}
		if (a && !(a instanceof Array)) {
			return [a]
		}
		return a
	},
	getDetails: function (a) {
		return a
	},
	getInfo: function (a) {
		return {
			_size: (a.total_count || 0),
			_from: (a.pos || 0),
			_key: (a.dhx_security)
		}
	}
};
dhtmlx.DataDriver.json_ext = {
	toObject: function (data) {
		if (!data) {
			data = "[]"
		}
		if (typeof data == "string") {
			var temp;
			eval("temp=" + data);
			dhtmlx.temp = [];
			var header = temp.header;
			for (var i = 0; i < temp.data.length; i++) {
				var item = {};
				for (var j = 0; j < header.length; j++) {
					if (typeof(temp.data[i][j]) != "undefined") {
						item[header[j]] = temp.data[i][j]
					}
				}
				dhtmlx.temp.push(item)
			}
			return dhtmlx.temp
		}
		return data
	},
	getRecords: function (a) {
		if (a && !(a instanceof Array)) {
			return [a]
		}
		return a
	},
	getDetails: function (a) {
		return a
	},
	getInfo: function (a) {
		return {
			_size: (a.total_count || 0),
			_from: (a.pos || 0)
		}
	}
};
dhtmlx.DataDriver.html = {
	toObject: function (b) {
		if (typeof b == "string") {
			var a = null;
			if (b.indexOf("<") == -1) {
				a = dhtmlx.toNode(b)
			}
			if (!a) {
				a = document.createElement("DIV");
				a.innerHTML = b
			}
			return a.getElementsByTagName(this.tag)
		}
		return b
	},
	getRecords: function (a) {
		if (a.tagName) {
			return a.childNodes
		}
		return a
	},
	getDetails: function (a) {
		return dhtmlx.DataDriver.xml.tagToObject(a)
	},
	getInfo: function (a) {
		return {
			_size: 0,
			_from: 0
		}
	},
	tag: "LI"
};
dhtmlx.DataDriver.jsarray = {
	toObject: function (data) {
		if (typeof data == "string") {
			eval("dhtmlx.temp=" + data);
			return dhtmlx.temp
		}
		return data
	},
	getRecords: function (a) {
		return a
	},
	getDetails: function (c) {
		var a = {};
		for (var b = 0; b < c.length; b++) {
			a["data" + b] = c[b]
		}
		return a
	},
	getInfo: function (a) {
		return {
			_size: 0,
			_from: 0
		}
	}
};
dhtmlx.DataDriver.csv = {
	toObject: function (a) {
		return a
	},
	getRecords: function (a) {
		return a.split(this.row)
	},
	getDetails: function (c) {
		c = this.stringToArray(c);
		var a = {};
		for (var b = 0; b < c.length; b++) {
			a["data" + b] = c[b]
		}
		return a
	},
	getInfo: function (a) {
		return {
			_size: 0,
			_from: 0
		}
	},
	stringToArray: function (b) {
		b = b.split(this.cell);
		for (var a = 0; a < b.length; a++) {
			b[a] = b[a].replace(/^[ \t\n\r]*(\"|)/g, "").replace(/(\"|)[ \t\n\r]*$/g, "")
		}
		return b
	},
	row: "\n",
	cell: ","
};
dhtmlx.DataDriver.xml = {
	toObject: function (b, a) {
		if (a && (a = this.checkResponse(b, a))) {
			return a
		}
		if (typeof b == "string") {
			return this.fromString(b)
		}
		return b
	},
	getRecords: function (a) {
		return this.xpath(a, this.records)
	},
	records: "/*/item",
	getDetails: function (a) {
		return this.tagToObject(a, {})
	},
	getInfo: function (a) {
		return {
			_size: (a.documentElement.getAttribute("total_count") || 0),
			_from: (a.documentElement.getAttribute("pos") || 0),
			_key: (a.documentElement.getAttribute("dhx_security"))
		}
	},
	xpath: function (d, k) {
		if (window.XPathResult) {
			var c = d;
			if (d.nodeName.indexOf("document") == -1) {
				d = d.ownerDocument
			}
			var h = [];
			var b = d.evaluate(k, c, null, XPathResult.ANY_TYPE, null);
			var j = b.iterateNext();
			while (j) {
				h.push(j);
				j = b.iterateNext()
			}
			return h
		} else {
			var g = true;
			try {
				if (typeof(d.selectNodes) == "undefined") {
					g = false
				}
			} catch (f) {}
			if (g) {
				return d.selectNodes(k)
			} else {
				var a = k.split("/").pop();
				return d.getElementsByTagName(a)
			}
		}
	},
	tagToObject: function (d, k) {
		k = k || {};
		var f = false;
		var c = d.childNodes;
		var j = {};
		for (var h = 0; h < c.length; h++) {
			if (c[h].nodeType == 1) {
				var g = c[h].tagName;
				if (typeof k[g] != "undefined") {
					if (!(k[g]instanceof Array)) {
						k[g] = [k[g]]
					}
					k[g].push(this.tagToObject(c[h], {}))
				} else {
					k[c[h].tagName] = this.tagToObject(c[h], {})
				}
				f = true
			}
		}
		var e = d.attributes;
		if (e && e.length) {
			for (var h = 0; h < e.length; h++) {
				k[e[h].name] = e[h].value
			}
			f = true
		}
		if (!f) {
			return this.nodeValue(d)
		}
		k.value = this.nodeValue(d);
		return k
	},
	nodeValue: function (a) {
		if (a.firstChild) {
			return a.firstChild.wholeText || a.firstChild.data
		}
		return ""
	},
	fromString: function (b) {
		if (window.DOMParser && !dhtmlx._isIE) {
			return (new DOMParser()).parseFromString(b, "text/xml")
		}
		if (window.ActiveXObject) {
			var a = new ActiveXObject("Microsoft.xmlDOM");
			a.loadXML(b);
			return a
		}
		dhtmlx.error("Load from xml string is not supported")
	},
	checkResponse: function (d, c) {
		if (c && (c.firstChild && c.firstChild.tagName != "parsererror")) {
			return c
		}
		var b = this.fromString(d.replace(/^[\s]+/, ""));
		if (b) {
			return b
		}
		dhtmlx.error("xml can't be parsed", d)
	}
};
dhtmlx.DataLoader = {
	_init: function (a) {
		a = a || "";
		this.name = "DataStore";
		this.data = (a.datastore) || (new dhtmlx.DataStore());
		this._readyHandler = this.data.attachEvent("onStoreLoad", dhtmlx.bind(this._call_onready, this))
	},
	load: function (a, b) {
		dhtmlx.AtomDataLoader.load.apply(this, arguments);
		if (!this.data.feed) {
			this.data.feed = function (d, c) {
				if (this._load_count) {
					return this._load_count = [d, c]
				} else {
					this._load_count = true
				}
				this.load(a + ((a.indexOf("?") == -1) ? "?" : "&") + "posStart=" + d + "&count=" + c, function () {
					var e = this._load_count;
					this._load_count = false;
					if (typeof e == "object") {
						this.data.feed.apply(this, e)
					}
				})
			}
		}
	},
	_onLoad: function (c, b, a) {
		this.data._parse(this.data.driver.toObject(c, b));
		this.callEvent("onXLE", []);
		if (this._readyHandler) {
			this.data.detachEvent(this._readyHandler);
			this._readyHandler = null
		}
	},
	dataFeed_setter: function (a) {
		this.data.attachEvent("onBeforeFilter", dhtmlx.bind(function (g, f) {
				if (this._settings.dataFeed) {
					var e = {};
					if (!g && !e) {
						return
					}
					if (typeof g == "function") {
						if (!f) {
							return
						}
						g(f, e)
					} else {
						e = {
							text: f
						}
					}
					this.clearAll();
					var b = this._settings.dataFeed;
					if (typeof b == "function") {
						return b.call(this, f, e)
					}
					var d = [];
					for (var c in e) {
						d.push("dhx_filter[" + c + "]=" + encodeURIComponent(e[c]))
					}
					this.load(b + (b.indexOf("?") < 0 ? "?" : "&") + d.join("&"), this._settings.datatype);
					return false
				}
			}, this));
		return a
	},
	_call_onready: function () {
		if (this._settings.ready) {
			var a = dhtmlx.toFunctor(this._settings.ready);
			if (a && a.call) {
				a.apply(this, arguments)
			}
		}
	}
};
dhtmlx.DataStore = function () {
	this.name = "DataStore";
	dhtmlx.extend(this, dhtmlx.EventSystem);
	this.setDriver("xml");
	this.pull = {};
	this.order = dhtmlx.toArray()
};
dhtmlx.DataStore.prototype = {
	setDriver: function (a) {
		dhtmlx.assert(dhtmlx.DataDriver[a], "incorrect DataDriver");
		this.driver = dhtmlx.DataDriver[a]
	},
	_parse: function (e) {
		this.callEvent("onParse", [this.driver, e]);
		if (this._filter_order) {
			this.filter()
		}
		var f = this.driver.getInfo(e);
		if (f._key) {
			dhtmlx.security_key = f._key
		}
		var d = this.driver.getRecords(e);
		var h = (f._from || 0) * 1;
		if (h === 0 && this.order[0]) {
			h = this.order.length
		}
		var b = 0;
		for (var c = 0; c < d.length; c++) {
			var a = this.driver.getDetails(d[c]);
			var g = this.id(a);
			if (!this.pull[g]) {
				this.order[b + h] = g;
				b++
			}
			this.pull[g] = a;
			if (this.extraParser) {
				this.extraParser(a)
			}
			if (this._scheme) {
				if (this._scheme.$init) {
					this._scheme.$update(a)
				} else {
					if (this._scheme.$update) {
						this._scheme.$update(a)
					}
				}
			}
		}
		for (var c = 0; c < f._size; c++) {
			if (!this.order[c]) {
				var g = dhtmlx.uid();
				var a = {
					id: g,
					$template: "loading"
				};
				this.pull[g] = a;
				this.order[c] = g
			}
		}
		this.callEvent("onStoreLoad", [this.driver, e]);
		this.refresh()
	},
	id: function (a) {
		return a.id || (a.id = dhtmlx.uid())
	},
	changeId: function (b, a) {
		dhtmlx.assert(this.pull[b], "Can't change id, for non existing item: " + b);
		this.pull[a] = this.pull[b];
		this.pull[a].id = a;
		this.order[this.order.find(b)] = a;
		if (this._filter_order) {
			this._filter_order[this._filter_order.find(b)] = a
		}
		this.callEvent("onIdChange", [b, a]);
		if (this._render_change_id) {
			this._render_change_id(b, a)
		}
	},
	get: function (a) {
		return this.item(a)
	},
	set: function (b, a) {
		return this.update(b, a)
	},
	item: function (a) {
		return this.pull[a]
	},
	update: function (b, a) {
		if (this._scheme && this._scheme.$update) {
			this._scheme.$update(a)
		}
		if (this.callEvent("onBeforeUpdate", [b, a]) === false) {
			return false
		}
		this.pull[b] = a;
		this.refresh(b)
	},
	refresh: function (a) {
		if (this._skip_refresh) {
			return
		}
		if (a) {
			this.callEvent("onStoreUpdated", [a, this.pull[a], "update"])
		} else {
			this.callEvent("onStoreUpdated", [null, null, null])
		}
	},
	silent: function (a) {
		this._skip_refresh = true;
		a.call(this);
		this._skip_refresh = false
	},
	getRange: function (d, c) {
		if (d) {
			d = this.indexById(d)
		} else {
			d = this.startOffset || 0
		}
		if (c) {
			c = this.indexById(c)
		} else {
			c = Math.min((this.endOffset || Infinity), (this.dataCount() - 1));
			if (c < 0) {
				c = 0
			}
		}
		if (this.min) {
			d = this.min
		}
		if (this.max) {
			c = this.max
		}
		if (d > c) {
			var b = c;
			c = d;
			d = b
		}
		return this.getIndexRange(d, c)
	},
	getIndexRange: function (d, c) {
		c = Math.min((c || Infinity), this.dataCount() - 1);
		var a = dhtmlx.toArray();
		for (var b = (d || 0); b <= c; b++) {
			a.push(this.item(this.order[b]))
		}
		return a
	},
	dataCount: function () {
		return this.order.length
	},
	exists: function (a) {
		return !!(this.pull[a])
	},
	move: function (a, d) {
		if (a < 0 || d < 0) {
			dhtmlx.error("DataStore::move", "Incorrect indexes");
			return
		}
		var c = this.idByIndex(a);
		var b = this.item(c);
		this.order.removeAt(a);
		this.order.insertAt(c, Math.min(this.order.length, d));
		this.callEvent("onStoreUpdated", [c, b, "move"])
	},
	scheme: function (a) {
		this._scheme = a
	},
	sync: function (e, d, a) {
		if (typeof d != "function") {
			a = d;
			d = null
		}
		if (dhtmlx.debug_bind) {
			this.debug_sync_master = e;
			dhtmlx.log("[sync] " + this.debug_bind_master.name + "@" + this.debug_bind_master._settings.id + " <= " + this.debug_sync_master.name + "@" + this.debug_sync_master._settings.id)
		}
		var c = e;
		if (e.name != "DataStore") {
			e = e.data
		}
		var b = dhtmlx.bind(function (h, f, g) {
				if (g != "update" || d) {
					h = null
				}
				if (!h) {
					this.order = dhtmlx.toArray([].concat(e.order));
					this._filter_order = null;
					this.pull = e.pull;
					if (d) {
						this.silent(d)
					}
					if (this._on_sync) {
						this._on_sync()
					}
				}
				if (dhtmlx.debug_bind) {
					dhtmlx.log("[sync:request] " + this.debug_sync_master.name + "@" + this.debug_sync_master._settings.id + " <= " + this.debug_bind_master.name + "@" + this.debug_bind_master._settings.id)
				}
				if (!a) {
					this.refresh(h)
				} else {
					a = false
				}
			}, this);
		e.attachEvent("onStoreUpdated", b);
		this.feed = function (g, f) {
			c.loadNext(f, g)
		};
		b()
	},
	add: function (e, a) {
		if (this._scheme) {
			e = e || {};
			for (var b in this._scheme) {
				e[b] = e[b] || this._scheme[b]
			}
			if (this._scheme) {
				if (this._scheme.$init) {
					this._scheme.$update(e)
				} else {
					if (this._scheme.$update) {
						this._scheme.$update(e)
					}
				}
			}
		}
		var f = this.id(e);
		var d = this.dataCount();
		if (dhtmlx.isNotDefined(a) || a < 0) {
			a = d
		}
		if (a > d) {
			dhtmlx.log("Warning", "DataStore:add", "Index of out of bounds");
			a = Math.min(this.order.length, a)
		}
		if (this.callEvent("onBeforeAdd", [f, e, a]) === false) {
			return false
		}
		if (this.exists(f)) {
			return dhtmlx.error("Not unique ID")
		}
		this.pull[f] = e;
		this.order.insertAt(f, a);
		if (this._filter_order) {
			var c = this._filter_order.length;
			if (!a && this.order.length) {
				c = 0
			}
			this._filter_order.insertAt(f, c)
		}
		this.callEvent("onafterAdd", [f, a]);
		this.callEvent("onStoreUpdated", [f, e, "add"]);
		return f
	},
	remove: function (c) {
		if (c instanceof Array) {
			for (var a = 0; a < c.length; a++) {
				this.remove(c[a])
			}
			return
		}
		if (this.callEvent("onBeforeDelete", [c]) === false) {
			return false
		}
		if (!this.exists(c)) {
			return dhtmlx.error("Not existing ID", c)
		}
		var b = this.item(c);
		this.order.remove(c);
		if (this._filter_order) {
			this._filter_order.remove(c)
		}
		delete this.pull[c];
		this.callEvent("onafterdelete", [c]);
		this.callEvent("onStoreUpdated", [c, b, "delete"])
	},
	clearAll: function () {
		this.pull = {};
		this.order = dhtmlx.toArray();
		this.feed = null;
		this._filter_order = null;
		this.callEvent("onClearAll", []);
		this.refresh()
	},
	idByIndex: function (a) {
		if (a >= this.order.length || a < 0) {
			dhtmlx.log("Warning", "DataStore::idByIndex Incorrect index")
		}
		return this.order[a]
	},
	indexById: function (b) {
		var a = this.order.find(b);
		return a
	},
	next: function (b, a) {
		return this.order[this.indexById(b) + (a || 1)]
	},
	first: function () {
		return this.order[0]
	},
	last: function () {
		return this.order[this.order.length - 1]
	},
	previous: function (b, a) {
		return this.order[this.indexById(b) - (a || 1)]
	},
	sort: function (f, b, a) {
		var c = f;
		if (typeof f == "function") {
			c = {
				as: f,
				dir: b
			}
		} else {
			if (typeof f == "string") {
				c = {
					by: f,
					dir: b,
					as: a
				}
			}
		}
		var e = [c.by, c.dir, c.as];
		if (!this.callEvent("onbeforesort", e)) {
			return
		}
		if (this.order.length) {
			var g = dhtmlx.sort.create(c);
			var d = this.getRange(this.first(), this.last());
			d.sort(g);
			this.order = d.map(function (h) {
					return this.id(h)
				}, this)
		}
		this.refresh();
		this.callEvent("onaftersort", e)
	},
	filter: function (e, d) {
		if (!this.callEvent("onBeforeFilter", [e, d])) {
			return
		}
		if (this._filter_order) {
			this.order = this._filter_order;
			delete this._filter_order
		}
		if (!this.order.length) {
			return
		}
		if (e) {
			var b = e;
			d = d || "";
			if (typeof e == "string") {
				e = dhtmlx.Template.fromHTML(e);
				d = d.toString().toLowerCase();
				b = function (h, g) {
					return e(h).toLowerCase().indexOf(g) != -1
				}
			}
			var c = dhtmlx.toArray();
			for (var a = 0; a < this.order.length; a++) {
				var f = this.order[a];
				if (b(this.item(f), d)) {
					c.push(f)
				}
			}
			this._filter_order = this.order;
			this.order = c
		}
		this.refresh();
		this.callEvent("onAfterFilter", [])
	},
	each: function (c, b) {
		for (var a = 0; a < this.order.length; a++) {
			c.call((b || this), this.item(this.order[a]))
		}
	},
	provideApi: function (d, b) {
		this.debug_bind_master = d;
		if (b) {
			this.mapEvent({
				onbeforesort: d,
				onaftersort: d,
				onbeforeadd: d,
				onafteradd: d,
				onbeforedelete: d,
				onafterdelete: d,
				onbeforeupdate: d
			})
		}
		var c = ["get", "set", "sort", "add", "remove", "exists", "idByIndex", "indexById", "item", "update", "refresh", "dataCount", "filter", "next", "previous", "clearAll", "first", "last", "serialize"];
		for (var a = 0; a < c.length; a++) {
			d[c[a]] = dhtmlx.methodPush(this, c[a])
		}
		if (dhtmlx.assert_enabled()) {
			this.assert_event(d)
		}
	},
	serialize: function () {
		var c = this.order;
		var a = [];
		for (var b = 0; b < c.length; b++) {
			a.push(this.pull[c[b]])
		}
		return a
	}
};
dhtmlx.sort = {
	create: function (a) {
		return dhtmlx.sort.dir(a.dir, dhtmlx.sort.by(a.by, a.as))
	},
	as: {
		"int": function (d, c) {
			d = d * 1;
			c = c * 1;
			return d > c ? 1 : (d < c ? -1 : 0)
		},
		string_strict: function (d, c) {
			d = d.toString();
			c = c.toString();
			return d > c ? 1 : (d < c ? -1 : 0)
		},
		string: function (d, c) {
			d = d.toString().toLowerCase();
			c = c.toString().toLowerCase();
			return d > c ? 1 : (d < c ? -1 : 0)
		}
	},
	by: function (b, a) {
		if (!b) {
			return a
		}
		if (typeof a != "function") {
			a = dhtmlx.sort.as[a || "string"]
		}
		b = dhtmlx.Template.fromHTML(b);
		return function (d, c) {
			return a(b(d), b(c))
		}
	},
	dir: function (b, a) {
		if (b == "asc") {
			return a
		}
		return function (d, c) {
			return a(d, c) * -1
		}
	}
};
dhtmlx.KeyEvents = {
	_init: function () {
		dhtmlx.event(this._obj, "keypress", this._onKeyPress, this)
	},
	_onKeyPress: function (b) {
		b = b || event;
		var a = b.which || b.keyCode;
		this.callEvent((this._edit_id ? "onEditKeyPress" : "onKeyPress"), [a, b.ctrlKey, b.shiftKey, b])
	}
};
dhtmlx.MouseEvents = {
	_init: function () {
		if (this.on_click) {
			dhtmlx.event(this._obj, "click", this._onClick, this);
			dhtmlx.event(this._obj, "contextmenu", this._onContext, this)
		}
		if (this.on_dblclick) {
			dhtmlx.event(this._obj, "dblclick", this._onDblClick, this)
		}
		if (this.on_mouse_move) {
			dhtmlx.event(this._obj, "mousemove", this._onMouse, this);
			dhtmlx.event(this._obj, (dhtmlx._isIE ? "mouseleave" : "mouseout"), this._onMouse, this)
		}
	},
	_onClick: function (a) {
		return this._mouseEvent(a, this.on_click, "ItemClick")
	},
	_onDblClick: function (a) {
		return this._mouseEvent(a, this.on_dblclick, "ItemDblClick")
	},
	_onContext: function (a) {
		var b = dhtmlx.html.locate(a, this._id);
		if (b && !this.callEvent("onBeforeContextMenu", [b, a])) {
			return dhtmlx.html.preventEvent(a)
		}
	},
	_onMouse: function (a) {
		if (dhtmlx._isIE) {
			a = document.createEventObject(event)
		}
		if (this._mouse_move_timer) {
			window.clearTimeout(this._mouse_move_timer)
		}
		this.callEvent("onMouseMoving", [a]);
		this._mouse_move_timer = window.setTimeout(dhtmlx.bind(function () {
					if (a.type == "mousemove") {
						this._onMouseMove(a)
					} else {
						this._onMouseOut(a)
					}
				}, this), 500)
	},
	_onMouseMove: function (a) {
		if (!this._mouseEvent(a, this.on_mouse_move, "MouseMove")) {
			this.callEvent("onMouseOut", [a || event])
		}
	},
	_onMouseOut: function (a) {
		this.callEvent("onMouseOut", [a || event])
	},
	_mouseEvent: function (g, f, b) {
		g = g || event;
		var a = g.target || g.srcElement;
		var c = "";
		var h = null;
		var d = false;
		while (a && a.parentNode) {
			if (!d && a.getAttribute) {
				h = a.getAttribute(this._id);
				if (h) {
					if (a.getAttribute("userdata")) {
						this.callEvent("onLocateData", [h, a, g])
					}
					if (!this.callEvent("on" + b, [h, g, a])) {
						return
					}
					d = true
				}
			}
			c = a.className;
			if (c) {
				c = c.split(" ");
				c = c[0] || c[1];
				if (f[c]) {
					return f[c].call(this, g, h || dhtmlx.html.locate(g, this._id), a)
				}
			}
			a = a.parentNode
		}
		return d
	}
};
dhtmlx.Settings = {
	_init: function () {
		this._settings = this.config = {}
	},
	define: function (b, a) {
		if (typeof b == "object") {
			return this._parseSeetingColl(b)
		}
		return this._define(b, a)
	},
	_define: function (b, a) {
		dhtmlx.assert_settings.call(this, b, a);
		var c = this[b + "_setter"];
		return this._settings[b] = c ? c.call(this, a) : a
	},
	_parseSeetingColl: function (c) {
		if (c) {
			for (var b in c) {
				this._define(b, c[b])
			}
		}
	},
	_parseSettings: function (c, a) {
		var b = dhtmlx.extend({}, a);
		if (typeof c == "object" && !c.tagName) {
			dhtmlx.extend(b, c)
		}
		this._parseSeetingColl(b)
	},
	_mergeSettings: function (a, c) {
		for (var b in c) {
			switch (typeof a[b]) {
			case "object":
				a[b] = this._mergeSettings((a[b] || {}), c[b]);
				break;
			case "undefined":
				a[b] = c[b];
				break;
			default:
				break
			}
		}
		return a
	},
	_parseContainer: function (b, a, c) {
		if (typeof b == "object" && !b.tagName) {
			b = b.container
		}
		this._obj = this.$view = dhtmlx.toNode(b);
		if (!this._obj && c) {
			this._obj = c(b)
		}
		dhtmlx.assert(this._obj, "Incorrect html container");
		this._obj.className += " " + a;
		this._obj.onselectstart = function () {
			return false
		};
		this._dataobj = this._obj
	},
	_set_type: function (a) {
		if (typeof a == "object") {
			return this.type_setter(a)
		}
		dhtmlx.assert(this.types, "RenderStack :: Types are not defined");
		dhtmlx.assert(this.types[a], "RenderStack :: Inccorect type name", a);
		this.type = dhtmlx.extend({}, this.types[a]);
		this.customize()
	},
	customize: function (a) {
		if (a) {
			dhtmlx.extend(this.type, a)
		}
		this.type._item_start = dhtmlx.Template.fromHTML(this.template_item_start(this.type));
		this.type._item_end = this.template_item_end(this.type);
		this.render()
	},
	type_setter: function (a) {
		this._set_type(typeof a == "object" ? dhtmlx.Type.add(this, a) : a);
		return a
	},
	template_setter: function (a) {
		return this.type_setter({
			template: a
		})
	},
	css_setter: function (a) {
		this._obj.className += " " + a;
		return a
	}
};
dhtmlx.Template = {
	_cache: {},
	empty: function () {
		return ""
	},
	setter: function (a) {
		return dhtmlx.Template.fromHTML(a)
	},
	obj_setter: function (b) {
		var a = dhtmlx.Template.setter(b);
		var c = this;
		return function () {
			return a.apply(c, arguments)
		}
	},
	fromHTML: function (a) {
		if (typeof a == "function") {
			return a
		}
		if (this._cache[a]) {
			return this._cache[a]
		}
		a = (a || "").toString();
		a = a.replace(/[\r\n]+/g, "\\n");
		a = a.replace(/\{obj\.([^}?]+)\?([^:]*):([^}]*)\}/g, '"+(obj.$1?"$2":"$3")+"');
		a = a.replace(/\{common\.([^}\(]*)\}/g, '"+common.$1+"');
		a = a.replace(/\{common\.([^\}\(]*)\(\)\}/g, '"+(common.$1?common.$1(obj):"")+"');
		a = a.replace(/\{obj\.([^}]*)\}/g, '"+obj.$1+"');
		a = a.replace(/#([a-z0-9_]+)#/gi, '"+obj.$1+"');
		a = a.replace(/\{obj\}/g, '"+obj+"');
		a = a.replace(/\{-obj/g, "{obj");
		a = a.replace(/\{-common/g, "{common");
		a = 'return "' + a + '";';
		return this._cache[a] = Function("obj", "common", a)
	}
};
dhtmlx.Type = {
	add: function (c, b) {
		if (!c.types && c.prototype.types) {
			c = c.prototype
		}
		if (dhtmlx.assert_enabled()) {
			this.assert_event(b)
		}
		var a = b.name || "default";
		this._template(b);
		this._template(b, "edit");
		this._template(b, "loading");
		c.types[a] = dhtmlx.extend(dhtmlx.extend({}, (c.types[a] || this._default)), b);
		return a
	},
	_default: {
		css: "default",
		template: function () {
			return ""
		},
		template_edit: function () {
			return ""
		},
		template_loading: function () {
			return "..."
		},
		width: 150,
		height: 80,
		margin: 5,
		padding: 0
	},
	_template: function (c, a) {
		a = "template" + (a ? ("_" + a) : "");
		var b = c[a];
		if (b && (typeof b == "string")) {
			if (b.indexOf("->") != -1) {
				b = b.split("->");
				switch (b[0]) {
				case "html":
					b = dhtmlx.html.getValue(b[1]).replace(/\"/g, '\\"');
					break;
				case "http":
					b = new dhtmlx.ajax().sync().get(b[1], {
							uid: (new Date()).valueOf()
						}).responseText;
					break;
				default:
					break
				}
			}
			c[a] = dhtmlx.Template.fromHTML(b)
		}
	}
};
dhtmlx.SingleRender = {
	_init: function () {},
	_toHTML: function (a) {
		return this.type._item_start(a, this.type) + this.type.template(a, this.type) + this.type._item_end
	},
	render: function () {
		if (!this.callEvent || this.callEvent("onBeforeRender", [this.data])) {
			if (this.data) {
				this._dataobj.innerHTML = this._toHTML(this.data)
			}
			if (this.callEvent) {
				this.callEvent("onAfterRender", [])
			}
		}
	}
};
dhtmlx.ui.Tooltip = function (a) {
	this.name = "Tooltip";
	if (dhtmlx.assert_enabled()) {
		this._assert()
	}
	if (typeof a == "string") {
		a = {
			template: a
		}
	}
	dhtmlx.extend(this, dhtmlx.Settings);
	dhtmlx.extend(this, dhtmlx.SingleRender);
	this._parseSettings(a, {
		type: "default",
		dy: 0,
		dx: 20
	});
	this._dataobj = this._obj = document.createElement("DIV");
	this._obj.className = "dhx_tooltip";
	dhtmlx.html.insertBefore(this._obj, document.body.firstChild)
};
dhtmlx.ui.Tooltip.prototype = {
	show: function (a, b) {
		if (this._disabled) {
			return
		}
		if (this.data != a) {
			this.data = a;
			this.render(a)
		}
		this._obj.style.top = b.y + this._settings.dy + "px";
		this._obj.style.left = b.x + this._settings.dx + "px";
		this._obj.style.display = "block"
	},
	hide: function () {
		this.data = null;
		this._obj.style.display = "none"
	},
	disable: function () {
		this._disabled = true
	},
	enable: function () {
		this._disabled = false
	},
	types: {
		"default": dhtmlx.Template.fromHTML("{obj.id}")
	},
	template_item_start: dhtmlx.Template.empty,
	template_item_end: dhtmlx.Template.empty
};
dhtmlx.AutoTooltip = {
	tooltip_setter: function (b) {
		var a = new dhtmlx.ui.Tooltip(b);
		this.attachEvent("onMouseMove", function (d, c) {
			a.show(this.get(d), dhtmlx.html.pos(c))
		});
		this.attachEvent("onMouseOut", function (d, c) {
			a.hide()
		});
		this.attachEvent("onMouseMoving", function (d, c) {
			a.hide()
		});
		return a
	}
};
dhtmlx.compat = function (a, b) {
	if (dhtmlx.compat[a]) {
		dhtmlx.compat[a](b)
	}
};
if (!dhtmlx.attaches) {
	dhtmlx.attaches = {}
}
dhtmlx.attaches.attachAbstract = function (b, a) {
	var e = document.createElement("DIV");
	e.id = "CustomObject_" + dhtmlx.uid();
	e.style.width = "100%";
	e.style.height = "100%";
	e.cmp = "grid";
	document.body.appendChild(e);
	this.attachObject(e.id);
	a.container = e.id;
	var d = this.vs[this.av];
	d.grid = new window[b](a);
	d.gridId = e.id;
	d.gridObj = e;
	d.grid.setSizes = function () {
		if (this.resize) {
			this.resize()
		} else {
			this.render()
		}
	};
	var c = "_viewRestore";
	return this.vs[this[c]()].grid
};
dhtmlx.attaches.attachDataView = function (a) {
	return this.attachAbstract("dhtmlXDataView", a)
};
dhtmlx.attaches.attachChart = function (a) {
	return this.attachAbstract("dhtmlXChart", a)
};
dhtmlx.compat.layout = function () {};
dhtmlx.ui.pager = function (a) {
	this.name = "Pager";
	if (dhtmlx.assert_enabled()) {
		this._assert()
	}
	dhtmlx.extend(this, dhtmlx.Settings);
	this._parseContainer(a, "dhx_pager");
	dhtmlx.extend(this, dhtmlx.EventSystem);
	dhtmlx.extend(this, dhtmlx.SingleRender);
	dhtmlx.extend(this, dhtmlx.MouseEvents);
	this._parseSettings(a, {
		size: 10,
		page: -1,
		group: 5,
		count: 0,
		type: "default"
	});
	this.data = this._settings;
	this.refresh()
};
dhtmlx.ui.pager.prototype = {
	_id: "dhx_p_id",
	on_click: {
		/*dhx_pager_item: function (a, b) {
			this.select(b)
		}*/
		
		
		dhx_pager_item: function (a, b) {
			this.select(b)
		}
		
	},
	select: function (a) {
		switch (a) {
		case "next":
			a = this._settings.page + 1;
			break;
		case "prev":
			a = this._settings.page - 1;
			break;
		case "first":
			a = 0;
			break;
		case "last":
			a = this._settings.limit - 1;
			break;
		default:
			break
		}
		if (a < 0) {
			a = 0
		}
		if (a >= this.data.limit) {
			a = this.data.limit - 1
		}
		if (this.callEvent("onBeforePageChange", [this._settings.page, a])) {
			this.data.page = a * 1;
			this.refresh();
			this.callEvent("onAfterPageChange", [a])
		}
	},
	types: {
		"default": {
			template: dhtmlx.Template.fromHTML("{common.pages()}"),
			pages: function (c) {
				var b = "";
				if (c.page == -1) {
					return ""
				}
				c.min = c.page - Math.round((c.group - 1) / 2);
				c.max = c.min + c.group - 1;
				if (c.min < 0) {
					c.max += c.min * (-1);
					c.min = 0
				}
				if (c.max >= c.limit) {
					c.min -= Math.min(c.min, c.max - c.limit + 1);
					c.max = c.limit - 1
				}
				for (var a = (c.min || 0); a <= c.max; a++) {
					b += this.button({
						id: a,
						index: (a + 1),
						selected: (a == c.page ? "_selected" : "")
					})
				}
				return b
			},
			page: function (a) {
				return a.page + 1
			},
			first: function () {
				return this.button({
					id: "first",
					index: " &lt;&lt; ",
					selected: ""
				})
			},
			last: function () {
				return this.button({
					id: "last",
					index: " &gt;&gt; ",
					selected: ""
				})
			},
			prev: function () {
				return this.button({
					id: "prev",
					index: "&lt;",
					selected: ""
				})
			},
			next: function () {
				return this.button({
					id: "next",
					index: "&gt;",
					selected: ""
				})
			},
			button: dhtmlx.Template.fromHTML("<li class='page-item'> <a dhx_p_id='{obj.id}' class='dhx_pager_item{obj.selected} page-link'>{obj.index}</a> </li>")  
			
			
			 
		}
	},
	refresh: function () {
		var a = this._settings;
		a.limit = Math.ceil(a.count / a.size);
		if (a.limit && a.limit != a.old_limit) {
			a.page = Math.min(a.limit - 1, a.page)
		}
		var b = a.page;
		if (b != -1 && (b != a.old_page) || (a.limit != a.old_limit)) {
			this.render();
			this.callEvent("onRefresh", []);
			a.old_limit = a.limit;
			a.old_page = a.page
		}
	},
	template_item_start: dhtmlx.Template.fromHTML("<div>"),
	template_item_end: dhtmlx.Template.fromHTML("</div>")
};
dhtmlx.DataProcessor = {
	_dp_init: function (b) {
		var a = "_methods";
		b[a] = ["setItemStyle", "", "changeId", "remove"];
		this.attachEvent("onAfterAdd", function (c) {
			b.setUpdated(c, true, "inserted")
		});
		this.data.attachEvent("onStoreLoad", dhtmlx.bind(function (d, c) {
				if (d.getUserData) {
					d.getUserData(c, this._userdata)
				}
			}, this));
		this.attachEvent("onBeforeDelete", function (d) {
			if (b._silent_mode) {
				return true
			}
			var c = b.getState(d);
			if (c == "inserted") {
				b.setUpdated(d, false);
				return true
			}
			if (c == "deleted") {
				return false
			}
			if (c == "true_deleted") {
				return true
			}
			b.setUpdated(d, true, "deleted");
			return false
		});
		this.attachEvent("onAfterEditStop", function (c) {
			b.setUpdated(c, true, "updated")
		});
		this.attachEvent("onBindUpdate", function (c) {
			window.setTimeout(function () {
				b.setUpdated(c.id, true, "updated")
			}, 1)
		});
		a = "_getRowData";
		b[a] = function (g, c) {
			var e = this.obj.data.get(g);
			var f = {};
			for (var d in e) {
				if (d.indexOf("_") === 0) {
					continue
				}
				f[d] = e[d]
			}
			return f
		};
		a = "_clearUpdateFlag";
		b[a] = function () {};
		this._userdata = {};
		b.attachEvent("insertCallback", this._dp_callback);
		b.attachEvent("updateCallback", this._dp_callback);
		b.attachEvent("deleteCallback", function (c, d) {
			this.obj.setUserData(d, this.action_param, "true_deleted");
			this.obj.remove(d)
		});
		dhtmlx.compat("dataProcessor", b)
	},
	_dp_callback: function (a, b) {
		this.obj.data.set(b, dhtmlx.DataDriver.xml.getDetails(a.firstChild));
		this.obj.data.refresh(b)
	},
	setItemStyle: function (c, a) {
		var b = this._locateHTML(c);
		if (b) {
			b.style.cssText += ";" + a
		}
	},
	changeId: function (b, a) {
		this.data.changeId(b, a);
		this.refresh()
	},
	setUserData: function (c, a, b) {
		if (c) {
			this.data.get(c)[a] = b
		} else {
			this._userdata[a] = b
		}
	},
	getUserData: function (b, a) {
		return b ? this.data.get(b)[a] : this._userdata[a]
	}
};
(function () {
	var a = "_dp_init";
	dhtmlx.DataProcessor[a] = dhtmlx.DataProcessor._dp_init
})();
dhtmlx.compat.dnd = function () {
	if (window.dhtmlDragAndDropObject) {
		var h = "_dragged";
		var d = dhtmlDragAndDropObject.prototype.checkLanding;
		dhtmlDragAndDropObject.prototype.checkLanding = function (k, l, j) {
			d.apply(this, arguments);
			if (!j) {
				var m = dhtmlx.DragControl._drag_context = dhtmlx.DragControl._drag_context || {};
				if (!m.from) {
					m.from = this.dragStartObject
				}
				dhtmlx.DragControl._checkLand(k, l, true)
			}
		};
		var b = dhtmlDragAndDropObject.prototype.stopDrag;
		dhtmlDragAndDropObject.prototype.stopDrag = function (l, j, k) {
			if (!k) {
				if (dhtmlx.DragControl._last) {
					dhtmlx.DragControl._active = g.dragStartNode;
					dhtmlx.DragControl._stopDrag(l, true)
				}
			}
			b.apply(this, arguments)
		};
		var g = new dhtmlDragAndDropObject();
		var e = dhtmlx.DragControl._startDrag;
		dhtmlx.DragControl._startDrag = function () {
			e.apply(this, arguments);
			var m = dhtmlx.DragControl._drag_context;
			if (!m) {
				return
			}
			var l = [];
			var k = [];
			for (var j = 0; j < m.source.length; j++) {
				l[j] = {
					idd: m.source[j]
				};
				k.push(m.source[j])
			}
			g.dragStartNode = {
				parentNode: {},
				parentObject: {
					idd: l,
					id: (k.length == 1 ? k[0] : k),
					treeNod: {
						object: m.from
					}
				}
			};
			g.dragStartNode.parentObject.treeNod[h] = l;
			g.dragStartObject = m.from
		};
		var f = dhtmlx.DragControl._checkLand;
		dhtmlx.DragControl._checkLand = function (k, l, j) {
			f.apply(this, arguments);
			if (!this._last && !j) {
				k = g.checkLanding(k, l, true)
			}
		};
		var a = dhtmlx.DragControl._stopDrag;
		dhtmlx.DragControl._stopDrag = function (k, j) {
			a.apply(this, arguments);
			if (g.lastLanding && !j) {
				g.stopDrag(k, false, true)
			}
		};
		var c = dhtmlx.DragControl.getMaster;
		dhtmlx.DragControl.getMaster = function (k) {
			var l = null;
			if (k) {
				l = c.apply(this, arguments)
			}
			if (!l) {
				l = g.dragStartObject;
				var m = [];
				var n = l[h];
				for (var j = 0; j < n.length; j++) {
					m.push(n[j].idd || n[j].id)
				}
				dhtmlx.DragControl._drag_context.source = m
			}
			return l
		}
	}
};
dhtmlx.DataMove = {
	_init: function () {
		dhtmlx.assert(this.data, "DataMove :: Component doesn't have DataStore")
	},
	copy: function (b, e, a, d) {
		var c = this.get(b);
		if (!c) {
			dhtmlx.log("Warning", "Incorrect ID in DataMove::copy");
			return
		}
		if (a) {
			dhtmlx.assert(a.externalData, "DataMove :: External object doesn't support operation");
			c = a.externalData(c)
		}
		a = a || this;
		return a.add(a.externalData(c, d), e)
	},
	move: function (c, g, b, f) {
		if (c instanceof Array) {
			for (var d = 0; d < c.length; d++) {
				var a = (b || this).indexById(this.move(c[d], g, b, c[d]));
				if (c[d + 1]) {
					g = a + (this.indexById(c[d + 1]) < a ? 0 : 1)
				}
			}
			return
		}
		nid = c;
		if (g < 0) {
			dhtmlx.log("Info", "DataMove::move - moving outside of bounds is ignored");
			return
		}
		var e = this.get(c);
		if (!e) {
			dhtmlx.log("Warning", "Incorrect ID in DataMove::move");
			return
		}
		if (!b || b == this) {
			this.data.move(this.indexById(c), g)
		} else {
			dhtmlx.assert(b.externalData, "DataMove :: External object doesn't support operation");
			nid = b.add(b.externalData(e, f), g);
			this.remove(c)
		}
		return nid
	},
	moveUp: function (b, a) {
		return this.move(b, this.indexById(b) - (a || 1))
	},
	moveDown: function (b, a) {
		return this.moveUp(b, (a || 1) * -1)
	},
	moveTop: function (a) {
		return this.move(a, 0)
	},
	moveBottom: function (a) {
		return this.move(a, this.data.dataCount() - 1)
	},
	externalData: function (a, c) {
		var b = dhtmlx.extend({}, a);
		b.id = c || dhtmlx.uid();
		b.$selected = b.$template = null;
		return b
	}
};
dhtmlx.DragControl = {
	_drag_masters: dhtmlx.toArray(["dummy"]),
	addDrop: function (b, c, a) {
		b = dhtmlx.toNode(b);
		b.dhx_drop = this._getCtrl(c);
		if (a) {
			b.dhx_master = true
		}
	},
	_getCtrl: function (b) {
		b = b || dhtmlx.DragControl;
		var a = this._drag_masters.find(b);
		if (a < 0) {
			a = this._drag_masters.length;
			this._drag_masters.push(b)
		}
		return a
	},
	addDrag: function (a, b) {
		a = dhtmlx.toNode(a);
		a.dhx_drag = this._getCtrl(b);
		dhtmlx.event(a, "mousedown", this._preStart, a)
	},
	_preStart: function (a) {
		if (dhtmlx.DragControl._active) {
			dhtmlx.DragControl._preStartFalse();
			dhtmlx.DragControl.destroyDrag()
		}
		dhtmlx.DragControl._active = this;
		dhtmlx.DragControl._start_pos = {
			x: a.pageX,
			y: a.pageY
		};
		dhtmlx.DragControl._dhx_drag_mm = dhtmlx.event(document.body, "mousemove", dhtmlx.DragControl._startDrag);
		dhtmlx.DragControl._dhx_drag_mu = dhtmlx.event(document.body, "mouseup", dhtmlx.DragControl._preStartFalse);
		dhtmlx.DragControl._dhx_drag_sc = dhtmlx.event(this, "scroll", dhtmlx.DragControl._preStartFalse);
		a.cancelBubble = true;
		return false
	},
	_preStartFalse: function (a) {
		dhtmlx.DragControl._dhx_drag_mm = dhtmlx.eventRemove(dhtmlx.DragControl._dhx_drag_mm);
		dhtmlx.DragControl._dhx_drag_mu = dhtmlx.eventRemove(dhtmlx.DragControl._dhx_drag_mu);
		dhtmlx.DragControl._dhx_drag_sc = dhtmlx.eventRemove(dhtmlx.DragControl._dhx_drag_sc)
	},
	_startDrag: function (a) {
		var b = {
			x: a.pageX,
			y: a.pageY
		};
		if (Math.abs(b.x - dhtmlx.DragControl._start_pos.x) < 5 && Math.abs(b.y - dhtmlx.DragControl._start_pos.y) < 5) {
			return
		}
		dhtmlx.DragControl._preStartFalse();
		if (!dhtmlx.DragControl.createDrag(a)) {
			return
		}
		dhtmlx.DragControl.sendSignal("start");
		dhtmlx.DragControl._dhx_drag_mm = dhtmlx.event(document.body, "mousemove", dhtmlx.DragControl._moveDrag);
		dhtmlx.DragControl._dhx_drag_mu = dhtmlx.event(document.body, "mouseup", dhtmlx.DragControl._stopDrag);
		dhtmlx.DragControl._moveDrag(a)
	},
	_stopDrag: function (a) {
		dhtmlx.DragControl._dhx_drag_mm = dhtmlx.eventRemove(dhtmlx.DragControl._dhx_drag_mm);
		dhtmlx.DragControl._dhx_drag_mu = dhtmlx.eventRemove(dhtmlx.DragControl._dhx_drag_mu);
		if (dhtmlx.DragControl._last) {
			dhtmlx.DragControl.onDrop(dhtmlx.DragControl._active, dhtmlx.DragControl._last, this._landing, a);
			dhtmlx.DragControl.onDragOut(dhtmlx.DragControl._active, dhtmlx.DragControl._last, null, a)
		}
		dhtmlx.DragControl.destroyDrag();
		dhtmlx.DragControl.sendSignal("stop")
	},
	_moveDrag: function (a) {
		var b = dhtmlx.html.pos(a);
		dhtmlx.DragControl._html.style.top = b.y + dhtmlx.DragControl.top + "px";
		dhtmlx.DragControl._html.style.left = b.x + dhtmlx.DragControl.left + "px";
		if (dhtmlx.DragControl._skip) {
			dhtmlx.DragControl._skip = false
		} else {
			dhtmlx.DragControl._checkLand((a.srcElement || a.target), a)
		}
		a.cancelBubble = true;
		return false
	},
	_checkLand: function (a, b) {
		while (a && a.tagName != "BODY") {
			if (a.dhx_drop) {
				if (this._last && (this._last != a || a.dhx_master)) {
					this.onDragOut(this._active, this._last, a, b)
				}
				if (!this._last || this._last != a || a.dhx_master) {
					this._last = null;
					this._landing = this.onDragIn(dhtmlx.DragControl._active, a, b);
					if (this._landing) {
						this._last = a
					}
					return
				}
				return
			}
			a = a.parentNode
		}
		if (this._last) {
			this._last = this._landing = this.onDragOut(this._active, this._last, null, b)
		}
	},
	sendSignal: function (a) {
		dhtmlx.DragControl.active = (a == "start")
	},
	getMaster: function (a) {
		return this._drag_masters[a.dhx_drag || a.dhx_drop]
	},
	getContext: function (a) {
		return this._drag_context
	},
	createDrag: function (f) {
		var c = dhtmlx.DragControl._active;
		var d = this._drag_masters[c.dhx_drag];
		var b;
		if (d.onDragCreate) {
			b = d.onDragCreate(c, f);
			b.style.position = "absolute";
			b.style.zIndex = dhtmlx.zIndex.drag;
			b.onmousemove = dhtmlx.DragControl._skip_mark
		} else {
			var g = dhtmlx.DragControl.onDrag(c, f);
			if (!g) {
				return false
			}
			var b = document.createElement("DIV");
			b.innerHTML = g;
			b.className = "dhx_drag_zone";
			b.onmousemove = dhtmlx.DragControl._skip_mark;
			document.body.appendChild(b)
		}
		dhtmlx.DragControl._html = b;
		return true
	},
	_skip_mark: function () {
		dhtmlx.DragControl._skip = true
	},
	destroyDrag: function () {
		var b = dhtmlx.DragControl._active;
		var c = this._drag_masters[b.dhx_drag];
		if (c && c.onDragDestroy) {
			c.onDragDestroy(b, dhtmlx.DragControl._html)
		} else {
			dhtmlx.html.remove(dhtmlx.DragControl._html)
		}
		dhtmlx.DragControl._landing = dhtmlx.DragControl._active = dhtmlx.DragControl._last = dhtmlx.DragControl._html = null
	},
	top: 5,
	left: 5,
	onDragIn: function (c, b, d) {
		var a = this._drag_masters[b.dhx_drop];
		if (a.onDragIn && a != this) {
			return a.onDragIn(c, b, d)
		}
		b.className = b.className + " dhx_drop_zone";
		return b
	},
	onDragOut: function (c, b, f, d) {
		var a = this._drag_masters[b.dhx_drop];
		if (a.onDragOut && a != this) {
			return a.onDragOut(c, b, f, d)
		}
		b.className = b.className.replace("dhx_drop_zone", "");
		return null
	},
	onDrop: function (c, b, g, f) {
		var a = this._drag_masters[b.dhx_drop];
		dhtmlx.DragControl._drag_context.from = dhtmlx.DragControl.getMaster(c);
		if (a.onDrop && a != this) {
			return a.onDrop(c, b, g, f)
		}
		b.appendChild(c)
	},
	onDrag: function (b, c) {
		var a = this._drag_masters[b.dhx_drag];
		if (a.onDrag && a != this) {
			return a.onDrag(b, c)
		}
		dhtmlx.DragControl._drag_context = {
			source: b,
			from: b
		};
		return "<div style='" + b.style.cssText + "'>" + b.innerHTML + "</div>"
	}
};
dhtmlx.DragItem = {
	_init: function () {
		dhtmlx.assert(this.move, "DragItem :: Component doesn't have DataMove interface");
		dhtmlx.assert(this.locate, "DragItem :: Component doesn't have RenderStack interface");
		dhtmlx.assert(dhtmlx.DragControl, "DragItem :: DragControl is not included");
		if (!this._settings || this._settings.drag) {
			dhtmlx.DragItem._initHandlers(this)
		} else {
			if (this._settings) {
				this.drag_setter = function (a) {
					if (a) {
						this._initHandlers(this);
						delete this.drag_setter
					}
					return a
				}
			}
		}
		if (this.dragMarker) {
			this.attachEvent("onBeforeDragIn", this.dragMarker);
			this.attachEvent("onDragOut", this.dragMarker)
		}
	},
	_initHandlers: function (a) {
		dhtmlx.DragControl.addDrop(a._obj, a, true);
		dhtmlx.DragControl.addDrag(a._obj, a)
	},
	onDragIn: function (d, c, f) {
		var h = this.locate(f) || null;
		var b = dhtmlx.DragControl._drag_context;
		var g = dhtmlx.DragControl.getMaster(d);
		var a = (this._locateHTML(h) || this._obj);
		if (a == dhtmlx.DragControl._landing) {
			return a
		}
		b.target = h;
		b.to = g;
		if (!this.callEvent("onBeforeDragIn", [b, f])) {
			b.id = null;
			return null
		}
		dhtmlx.html.addCss(a, "dhx_drag_over");
		return a
	},
	onDragOut: function (d, c, h, f) {
		var g = this.locate(f) || null;
		if (h != this._dataobj) {
			g = null
		}
		var b = (this._locateHTML(g) || (h ? dhtmlx.DragControl.getMaster(h)._obj : window.undefined));
		if (b == dhtmlx.DragControl._landing) {
			return null
		}
		var a = dhtmlx.DragControl._drag_context;
		dhtmlx.html.removeCss(dhtmlx.DragControl._landing, "dhx_drag_over");
		a.target = a.to = null;
		this.callEvent("onDragOut", [a, f]);
		return null
	},
	onDrop: function (c, b, g, f) {
		var a = dhtmlx.DragControl._drag_context;
		a.to = this;
		a.index = a.target ? this.indexById(a.target) : this.dataCount();
		a.new_id = dhtmlx.uid();
		if (!this.callEvent("onBeforeDrop", [a, f])) {
			return
		}
		if (a.from == a.to) {
			this.move(a.source, a.index)
		} else {
			if (a.from) {
				a.from.move(a.source, a.index, a.to, a.new_id)
			} else {
				dhtmlx.error("Unsopported d-n-d combination")
			}
		}
		this.callEvent("onAfterDrop", [a, f])
	},
	onDrag: function (c, f) {
		var g = this.locate(f);
		var d = [g];
		if (g) {
			if (this.getSelected) {
				var b = this.getSelected();
				if (dhtmlx.PowerArray.find.call(b, g) != -1) {
					d = b
				}
			}
			var a = dhtmlx.DragControl._drag_context = {
				source: d,
				start: g
			};
			a.from = this;
			if (this.callEvent("onBeforeDrag", [a, f])) {
				return a.html || this._toHTML(this.get(g))
			}
		}
		return null
	}
};
dhtmlx.EditAbility = {
	_init: function (a) {
		this._edit_id = null;
		this._edit_bind = null;
		dhtmlx.assert(this.data, "EditAbility :: Component doesn't have DataStore");
		dhtmlx.assert(this._locateHTML, "EditAbility :: Component doesn't have RenderStack");
		this.attachEvent("onEditKeyPress", function (c, d, b) {
			if (c == 13 && !b) {
				this.stopEdit()
			} else {
				if (c == 27) {
					this.stopEdit(true)
				}
			}
		});
		this.attachEvent("onBeforeRender", function () {
			this.stopEdit()
		})
	},
	isEdit: function () {
		return this._edit_id
	},
	edit: function (b) {
		if (this.stopEdit(false, b)) {
			if (!this.callEvent("onBeforeEditStart", [b])) {
				return
			}
			var a = this.data.get(b);
			if (a.$template) {
				return
			}
			a.$template = "edit";
			this.data.refresh(b);
			this._edit_id = b;
			this._save_binding(b);
			this._edit_bind(true, a);
			this.callEvent("onAfterEditStart", [b])
		}
	},
	stopEdit: function (c, e) {
		if (!this._edit_id) {
			return true
		}
		if (this._edit_id == e) {
			return false
		}
		var a = {};
		if (!c) {
			this._edit_bind(false, a)
		} else {
			a = null
		}
		if (!this.callEvent("onBeforeEditStop", [this._edit_id, a])) {
			return false
		}
		var b = this.data.get(this._edit_id);
		b.$template = null;
		if (!c) {
			this._edit_bind(false, b)
		}
		var d = this._edit_id;
		this._edit_bind = this._edit_id = null;
		this.data.refresh(d);
		this.callEvent("onAfterEditStop", [d, a]);
		return true
	},
	_save_binding: function (h) {
		var a = this._locateHTML(h);
		var c = "";
		var f = "";
		var e = [];
		if (a) {
			var d = a.getElementsByTagName("*");
			var g = "";
			for (var b = 0; b < d.length; b++) {
				if (d[b].nodeType == 1 && (g = d[b].getAttribute("bind"))) {
					c += "els[" + e.length + "].value=" + g + ";";
					f += g + "=els[" + e.length + "].value;";
					e.push(d[b]);
					d[b].className += " dhx_allow_selection";
					d[b].onselectstart = this._block_native
				}
			}
			d = null
		}
		c = Function("obj", "els", c);
		f = Function("obj", "els", f);
		this._edit_bind = function (k, j) {
			if (k) {
				c(j, e);
				if (e.length && e[0].select) {
					e[0].select()
				}
			} else {
				f(j, e)
			}
		}
	},
	_block_native: function (a) {
		(a || event).cancelBubble = true;
		return true
	}
};
dhtmlx.SelectionModel = {
	_init: function () {
		this._selected = dhtmlx.toArray();
		dhtmlx.assert(this.data, "SelectionModel :: Component doesn't have DataStore");
		this.data.attachEvent("onStoreUpdated", dhtmlx.bind(this._data_updated, this));
		this.data.attachEvent("onStoreLoad", dhtmlx.bind(this._data_loaded, this));
		this.data.attachEvent("onAfterFilter", dhtmlx.bind(this._data_filtered, this));
		this.data.attachEvent("onIdChange", dhtmlx.bind(this._id_changed, this))
	},
	_id_changed: function (c, a) {
		for (var b = this._selected.length - 1; b >= 0; b--) {
			if (this._selected[b] == c) {
				this._selected[b] = a
			}
		}
	},
	_data_filtered: function () {
		for (var a = this._selected.length - 1; a >= 0; a--) {
			if (this.data.indexById(this._selected[a]) < 0) {
				var c = this._selected[a];
				var b = this.item(c);
				if (b) {
					delete b.$selected
				}
				this._selected.splice(a, 1);
				this.callEvent("onSelectChange", [c])
			}
		}
	},
	_data_updated: function (c, b, a) {
		if (a == "delete") {
			this._selected.remove(c)
		} else {
			if (!this.data.dataCount() && !this.data._filter_order) {
				this._selected = dhtmlx.toArray()
			}
		}
	},
	_data_loaded: function () {
		if (this._settings.select) {
			this.data.each(function (a) {
				if (a.$selected) {
					this.select(a.id)
				}
			}, this)
		}
	},
	_select_mark: function (c, b, a) {
		if (!a && !this.callEvent("onBeforeSelect", [c, b])) {
			return false
		}
		this.data.item(c).$selected = b;
		if (a) {
			a.push(c)
		} else {
			if (b) {
				this._selected.push(c)
			} else {
				this._selected.remove(c)
			}
			this._refresh_selection(c)
		}
		return true
	},
	select: function (d, c, a) {
		if (!d) {
			return this.selectAll()
		}
		if (d instanceof Array) {
			for (var b = 0; b < d.length; b++) {
				this.select(d[b], c, a)
			}
			return
		}
		if (!this.data.exists(d)) {
			dhtmlx.error("Incorrect id in select command: " + d);
			return
		}
		if (a && this._selected.length) {
			return this.selectAll(this._selected[this._selected.length - 1], d)
		}
		if (!c && (this._selected.length != 1 || this._selected[0] != d)) {
			this._silent_selection = true;
			this.unselectAll();
			this._silent_selection = false
		}
		if (this.isSelected(d)) {
			if (c) {
				this.unselect(d)
			}
			return
		}
		if (this._select_mark(d, true)) {
			this.callEvent("onAfterSelect", [d])
		}
	},
	unselect: function (a) {
		if (!a) {
			return this.unselectAll()
		}
		if (!this.isSelected(a)) {
			return
		}
		this._select_mark(a, false)
	},
	selectAll: function (d, c) {
		var a;
		var b = [];
		if (d || c) {
			a = this.data.getRange(d || null, c || null)
		} else {
			a = this.data.getRange()
		}
		a.each(function (e) {
			var f = this.data.item(e.id);
			if (!f.$selected) {
				this._selected.push(e.id);
				this._select_mark(e.id, true, b)
			}
			return e.id
		}, this);
		this._refresh_selection(b)
	},
	unselectAll: function () {
		var a = [];
		this._selected.each(function (b) {
			this._select_mark(b, false, a)
		}, this);
		this._selected = dhtmlx.toArray();
		this._refresh_selection(a)
	},
	isSelected: function (a) {
		return this._selected.find(a) != -1
	},
	getSelected: function (a) {
		switch (this._selected.length) {
		case 0:
			return a ? [] : "";
		case 1:
			return a ? [this._selected[0]] : this._selected[0];
		default:
			return ([].concat(this._selected))
		}
	},
	_is_mass_selection: function (a) {
		return a.length > 100 || a.length > this.data.dataCount / 2
	},
	_refresh_selection: function (b) {
		if (typeof b != "object") {
			b = [b]
		}
		if (!b.length) {
			return
		}
		if (this._is_mass_selection(b)) {
			this.data.refresh()
		} else {
			for (var a = 0; a < b.length; a++) {
				this.render(b[a], this.data.item(b[a]), "update")
			}
		}
		if (!this._silent_selection) {
			this.callEvent("onSelectChange", [b])
		}
	}
};
dhtmlx.RenderStack = {
	_init: function () {
		dhtmlx.assert(this.data, "RenderStack :: Component doesn't have DataStore");
		dhtmlx.assert(dhtmlx.Template, "dhtmlx.Template :: dhtmlx.Template is not accessible");
		this._html = document.createElement("DIV")
	},
	_toHTML: function (a) {
		dhtmlx.assert((!a.$template || this.type["template_" + a.$template]), "RenderStack :: Unknown template: " + a.$template);
		this.callEvent("onItemRender", [a]);
		return this.type._item_start(a, this.type) + (a.$template ? this.type["template_" + a.$template] : this.type.template)(a, this.type) + this.type._item_end
	},
	_toHTMLObject: function (a) {
		this._html.innerHTML = this._toHTML(a);
		return this._html.firstChild
	},
	_locateHTML: function (a) {
		if (this._htmlmap) {
			return this._htmlmap[a]
		}
		this._htmlmap = {};
		var c = this._dataobj.childNodes;
		for (var b = 0; b < c.length; b++) {
			var d = c[b].getAttribute(this._id);
			if (d) {
				this._htmlmap[d] = c[b]
			}
		}
		return this._locateHTML(a)
	},
	locate: function (a) {
		return dhtmlx.html.locate(a, this._id)
	},
	show: function (b) {
		var a = this._locateHTML(b);
		if (a) {
			this._dataobj.scrollTop = a.offsetTop - this._dataobj.offsetTop
		}
	},
	render: function (f, d, c, e) {
		if (f) {
			var a = this._locateHTML(f);
			switch (c) {
			case "update":
				if (!a) {
					return
				}
				var b = this._htmlmap[f] = this._toHTMLObject(d);
				dhtmlx.html.insertBefore(b, a);
				dhtmlx.html.remove(a);
				break;
			case "delete":
				if (!a) {
					return
				}
				dhtmlx.html.remove(a);
				delete this._htmlmap[f];
				break;
			case "add":
				var b = this._htmlmap[f] = this._toHTMLObject(d);
				dhtmlx.html.insertBefore(b, this._locateHTML(this.data.next(f)), this._dataobj);
				break;
			case "move":
				this.render(f, d, "delete");
				this.render(f, d, "add");
				break;
			default:
				dhtmlx.error("Unknown render command: " + c);
				break
			}
		} else {
			if (this.callEvent("onBeforeRender", [this.data])) {
				this._dataobj.innerHTML = this.data.getRange().map(this._toHTML, this).join("");
				this._htmlmap = null
			}
		}
		this.callEvent("onAfterRender", [])
	},
	pager_setter: function (b) {
		this.attachEvent("onBeforeRender", function () {
			var d = this._settings.pager._settings;
			if (d.page == -1) {
				return false
			}
			this.data.min = d.page * d.size;
			this.data.max = (d.page + 1) * d.size - 1;
			return true
		});
		var a = new dhtmlx.ui.pager(b);
		var c = dhtmlx.bind(function () {
				this.data.refresh()
			}, this);
		a.attachEvent("onRefresh", c);
		this.data.attachEvent("onStoreUpdated", function (e) {
			var d = this.dataCount();
			if (d != a._settings.count) {
				a._settings.count = d;
				if (a._settings.page == -1) {
					a._settings.page = 0
				}
				a.refresh()
			}
		});
		return a
	},
	height_setter: function (a) {
		if (a == "auto") {
			this.attachEvent("onAfterRender", this._correct_height);
			dhtmlx.event(window, "resize", dhtmlx.bind(this._correct_height, this))
		}
		return a
	},
	_correct_height: function () {
		this._dataobj.style.overflow = "hidden";
		this._dataobj.style.height = "1px";
		var a = this._dataobj.scrollHeight;
		this._dataobj.style.height = a + "px";
		if (dhtmlx._isFF) {
			var b = this._dataobj.scrollHeight;
			if (b != a) {
				this._dataobj.style.height = b + "px"
			}
		}
		this._obj.style.height = this._dataobj.style.height
	},
	_getDimension: function () {
		var a = this.type;
		var b = (a.border || 0) + (a.padding || 0) * 2 + (a.margin || 0) * 2;
		return {
			x: a.width + b,
			y: a.height + b
		}
	},
	x_count_setter: function (b) {
		var c = this._getDimension();
		var a = dhtmlx.$customScroll ? 0 : 18;
		this._dataobj.style.width = c.x * b + (this._settings.height != "auto" ? a : 0) + "px";
		return b
	},
	y_count_setter: function (a) {
		var b = this._getDimension();
		this._dataobj.style.height = b.y * a + "px";
		return a
	}
};
dhtmlx.VirtualRenderStack = {
	_init: function () {
		dhtmlx.assert(this.render, "VirtualRenderStack :: Object must use RenderStack first");
		this._htmlmap = {};
		this._dataobj.style.overflowY = "scroll";
		dhtmlx.event(this._dataobj, "scroll", dhtmlx.bind(this._render_visible_rows, this));
		dhtmlx.event(window, "resize", dhtmlx.bind(function () {
				this.render()
			}, this));
		this.data._unrendered_area = [];
		this.data.getIndexRange = this._getIndexRange
	},
	_locateHTML: function (a) {
		return this._htmlmap[a]
	},
	show: function (c) {
		range = this._getVisibleRange();
		var b = this.data.indexById(c);
		var a = Math.floor(b / range._dx) * range._y;
		this._dataobj.scrollTop = a
	},
	_getIndexRange: function (e, d) {
		if (d !== 0) {
			d = Math.min((d || Infinity), this.dataCount() - 1)
		}
		var a = dhtmlx.toArray();
		for (var b = (e || 0); b <= d; b++) {
			var c = this.item(this.order[b]);
			if (this.order.length > b) {
				if (!c) {
					this.order[b] = dhtmlx.uid();
					c = {
						id: this.order[b],
						$template: "loading"
					};
					this._unrendered_area.push(this.order[b])
				} else {
					if (c.$template == "loading") {
						this._unrendered_area.push(this.order[b])
					}
				}
				a.push(c)
			}
		}
		return a
	},
	render: function (f, d, c, e) {
		if (f) {
			var a = this._locateHTML(f);
			switch (c) {
			case "update":
				if (!a) {
					return
				}
				var b = this._htmlmap[f] = this._toHTMLObject(d);
				dhtmlx.html.insertBefore(b, a);
				dhtmlx.html.remove(a);
				break;
			default:
				this._render_delayed();
				break
			}
		} else {
			if (this.callEvent("onBeforeRender", [this.data])) {
				this._htmlmap = {};
				this._render_visible_rows(null, true);
				this._wait_for_render = false;
				this.callEvent("onAfterRender", [])
			}
		}
	},
	_render_delayed: function () {
		if (this._wait_for_render) {
			return
		}
		this._wait_for_render = true;
		window.setTimeout(dhtmlx.bind(function () {
				this.render()
			}, this), 1)
	},
	_create_placeholder: function (a) {
		var b = document.createElement("DIV");
		b.className = "dhxdataview_placeholder";
		b.style.cssText = "height:" + a + "px; width:100%; overflow:hidden;";
		return b
	},
	_render_visible_rows: function (u, x) {
		this.data._unrendered_area = [];
		var s = this._getVisibleRange();
		if (!this._dataobj.firstChild || x) {
			this._dataobj.innerHTML = "";
			this._dataobj.appendChild(this._create_placeholder(s._max));
			this._htmlrows = [this._dataobj.firstChild]
		}
		var d = Math.max(s._from, 0);
		var r = (this.data.max || this.data.max === 0) ? this.data.max : Infinity;
		while (d <= s._height) {
			while (this._htmlrows[d] && this._htmlrows[d]._filled && d <= s._height) {
				d++
			}
			if (d > s._height) {
				break
			}
			var j = d;
			while (!this._htmlrows[j]) {
				j--
			}
			var a = this._htmlrows[j];
			var c = d * s._dx + (this.data.min || 0);
			if (c > r) {
				break
			}
			var f = Math.min(c + s._dx - 1, r);
			var l = this._create_placeholder(s._y);
			var g = this.data.getIndexRange(c, f);
			if (!g.length) {
				break
			}
			l.innerHTML = g.map(this._toHTML, this).join("");
			for (var o = 0; o < g.length; o++) {
				this._htmlmap[this.data.idByIndex(c + o)] = l.childNodes[o]
			}
			var q = parseInt(a.style.height, 10);
			var w = (d - j) * s._y;
			var n = (q - w - s._y);
			dhtmlx.html.insertBefore(l, w ? a.nextSibling : a, this._dataobj);
			this._htmlrows[d] = l;
			l._filled = true;
			if (w <= 0 && n > 0) {
				a.style.height = n + "px";
				this._htmlrows[d + 1] = a
			} else {
				if (w < 0) {
					dhtmlx.html.remove(a)
				} else {
					a.style.height = w + "px"
				}
				if (n > 0) {
					var k = this._htmlrows[d + 1] = this._create_placeholder(n);
					dhtmlx.html.insertBefore(k, l.nextSibling, this._dataobj)
				}
			}
			d++
		}
		if (this.data._unrendered_area.length) {
			var m = this.indexById(this.data._unrendered_area[0]);
			var b = this.indexById(this.data._unrendered_area.pop()) + 1;
			if (b > m) {
				if (!this.callEvent("onDataRequest", [m, b - m])) {
					return false
				}
				dhtmlx.assert(this.data.feed, "Data feed is missed");
				this.data.feed.call(this, m, b - m)
			}
		}
		if (dhtmlx._isIE) {
			var v = this._getVisibleRange();
			if (v._from != s._from) {
				this._render_visible_rows()
			}
		}
	},
	_getVisibleRange: function () {
		var b = dhtmlx.$customScroll ? 0 : 18;
		var g = this._dataobj.scrollTop;
		var a = this._dataobj.scrollWidth;
		var h = this._dataobj.offsetHeight;
		var k = this.type;
		var d = this._getDimension();
		var l = Math.floor(a / d.x) || 1;
		var c = Math.floor(g / d.y);
		var j = Math.ceil((h + g) / d.y) - 1;
		var e = this.data.max ? (this.data.max - this.data.min) : this.data.dataCount();
		var f = Math.ceil(e / l) * d.y;
		return {
			_from: c,
			_height: j,
			_top: g,
			_max: f,
			_y: d.y,
			_dx: l
		}
	}
};
dhtmlXDataView = function (a) {
	this.name = "DataView";
	if (dhtmlx.assert_enabled()) {
		this._assert()
	}
	dhtmlx.extend(this, dhtmlx.Settings);
	this._parseContainer(a, "dhx_dataview");
	dhtmlx.extend(this, dhtmlx.AtomDataLoader);
	dhtmlx.extend(this, dhtmlx.DataLoader);
	dhtmlx.extend(this, dhtmlx.EventSystem);
	dhtmlx.extend(this, dhtmlx.RenderStack);
	dhtmlx.extend(this, dhtmlx.SelectionModel);
	dhtmlx.extend(this, dhtmlx.MouseEvents);
	dhtmlx.extend(this, dhtmlx.KeyEvents);
	dhtmlx.extend(this, dhtmlx.EditAbility);
	dhtmlx.extend(this, dhtmlx.DataMove);
	dhtmlx.extend(this, dhtmlx.DragItem);
	dhtmlx.extend(this, dhtmlx.DataProcessor);
	dhtmlx.extend(this, dhtmlx.AutoTooltip);
	dhtmlx.extend(this, dhtmlx.Destruction);
	this.data.attachEvent("onStoreUpdated", dhtmlx.bind(function () {
			this.render.apply(this, arguments)
		}, this));
	this._parseSettings(a, {
		drag: false,
		edit: false,
		select: "multiselect",
		type: "default"
	});
	if (this._settings.height != "auto" && !this._settings.renderAll) {
		dhtmlx.extend(this, dhtmlx.VirtualRenderStack)
	}
	this.data.provideApi(this, true);
	if (this.config.autowidth) {
		this.attachEvent("onBeforeRender", function () {
			this.type.width = Math.floor((this._dataobj.scrollWidth) / (this.config.autowidth * 1 || 1)) - this.type.padding * 2 - this.type.margin * 2 - this.type.border * 2;
			this.type._item_start = dhtmlx.Template.fromHTML(this.template_item_start(this.type));
			this.type._item_end = this.template_item_end(this.type)
		});
		dhtmlx.event(window, "resize", function () {
			this.refresh()
		}, this)
	}
	if (dhtmlx.$customScroll) {
		dhtmlx.CustomScroll.enable(this)
	}
};
dhtmlXDataView.prototype = {
	bind: function () {
		dhtmlx.BaseBind.legacyBind.apply(this, arguments)
	},
	sync: function () {
		dhtmlx.BaseBind.legacySync.apply(this, arguments)
	},
	dragMarker: function (c, e) {
		var d = this._locateHTML(c.target);
		if (this.type.drag_marker) {
			if (this._drag_marker) {
				this._drag_marker.style.backgroundImage = "";
				this._drag_marker.style.backgroundRepeat = ""
			}
			if (d) {
				d.style.backgroundImage = "url(" + (dhtmlx.image_path || "") + this.type.drag_marker + ")";
				d.style.backgroundRepeat = "no-repeat";
				this._drag_marker = d
			}
		}
		if (d && this._settings.auto_scroll) {
			var a = d.offsetTop;
			var f = d.offsetHeight;
			var b = this._obj.scrollTop;
			var g = this._obj.offsetHeight;
			if (a - f >= 0 && a - f * 0.75 < b) {
				b = Math.max(a - f, 0)
			} else {
				if (a + f / 0.75 > b + g) {
					b = b + f
				}
			}
			this._obj.scrollTop = b
		}
		return true
	},
	_id: "dhx_f_id",
	on_click: {
		dhx_dataview_item: function (a, b) {
			if (this.stopEdit(false, b)) {
				if (this._settings.select) {
					if (this._settings.select == "multiselect") {
						this.select(b, a.ctrlKey || a.metaKey, a.shiftKey)
					} else {
						this.select(b)
					}
				}
			}
		}
	},
	on_dblclick: {
		dhx_dataview_item: function (a, b) {
			if (this._settings.edit) {
				this.edit(b)
			}
		}
	},
	on_mouse_move: {},
	types: {
		"default": {
			css: "default",
			template: dhtmlx.Template.fromHTML("<div style='padding:10px; white-space:nowrap; overflow:hidden;'>{obj.text}</div>"),
			template_edit: dhtmlx.Template.fromHTML("<div style='padding:10px; white-space:nowrap; overflow:hidden;'><textarea style='width:100%; height:100%;' bind='obj.text'></textarea></div>"),
			template_loading: dhtmlx.Template.fromHTML("<div style='padding:10px; white-space:nowrap; overflow:hidden;'>Loading...</div>"),
			width: 210,
			height: 115,
			margin: 0,
			padding: 10,
			border: 1
		}
	},
	template_item_start: dhtmlx.Template.fromHTML("<div dhx_f_id='{-obj.id}' class='dhx_dataview_item dhx_dataview_{obj.css}_item{-obj.$selected?_selected:}' style='width:{obj.width}px; height:{obj.height}px; padding:{obj.padding}px; margin:{obj.margin}px; float:left; overflow:hidden;'>"),
	template_item_end: dhtmlx.Template.fromHTML("</div>")
};
dhtmlx.compat("layout");
if (typeof(window.dhtmlXCellObject) != "undefined") {
	dhtmlXCellObject.prototype.attachDataView = function (a) {
		this.callEvent("_onBeforeContentAttach", ["dataview"]);
		var b = document.createElement("DIV");
		b.style.width = "100%";
		b.style.height = "100%";
		b.style.position = "relative";
		b.style.overflow = "hidden";
		this._attachObject(b);
		if (typeof(a) == "undefined") {
			a = {}
		}
		b.id = "DataViewObject_" + new Date().getTime();
		a.container = b.id;
		a.skin = this.conf.skin;
		this.dataType = "dataview";
		this.dataObj = new dhtmlXDataView(a);
		this.dataObj.setSizes = function () {
			this.render()
		};
		b = null;
		this.callEvent("_onContentAttach", []);
		return this.dataObj
	}
}
dhtmlXList = function (a) {
	this.name = "List";
	dhtmlx.extend(this, dhtmlx.Settings);
	this._parseContainer(a, "dhx_list");
	dhtmlx.extend(this, dhtmlx.AtomDataLoader);
	dhtmlx.extend(this, dhtmlx.DataLoader);
	dhtmlx.extend(this, dhtmlx.EventSystem);
	dhtmlx.extend(this, dhtmlx.RenderStack);
	dhtmlx.extend(this, dhtmlx.SelectionModel);
	dhtmlx.extend(this, dhtmlx.MouseEvents);
	dhtmlx.extend(this, dhtmlx.KeyEvents);
	dhtmlx.extend(this, dhtmlx.EditAbility);
	dhtmlx.extend(this, dhtmlx.DataMove);
	dhtmlx.extend(this, dhtmlx.DragItem);
	dhtmlx.extend(this, dhtmlx.DataProcessor);
	dhtmlx.extend(this, dhtmlx.AutoTooltip);
	dhtmlx.extend(this, dhtmlx.Destruction);
	this._getDimension = function () {
		var b = this.type;
		var c = (b.margin || 0) * 2;
		return {
			x: b.width + c,
			y: b.height + c
		}
	};
	this.data.attachEvent("onStoreUpdated", dhtmlx.bind(function () {
			this.render.apply(this, arguments)
		}, this));
	this._parseSettings(a, {
		drag: false,
		edit: false,
		select: "multiselect",
		type: "default"
	});
	this.data.provideApi(this, true);
	if (dhtmlx.$customScroll) {
		dhtmlx.CustomScroll.enable(this)
	}
};
dhtmlXList.prototype = {
	bind: function () {
		dhtmlx.BaseBind.legacyBind.apply(this, arguments)
	},
	sync: function () {
		dhtmlx.BaseBind.legacySync.apply(this, arguments)
	},
	dragMarker: function (c, e) {
		var d = this._locateHTML(c.target);
		if (d && this._settings.auto_scroll) {
			var a = d.offsetTop;
			var f = d.offsetHeight;
			var b = this._obj.scrollTop;
			var g = this._obj.offsetHeight;
			if (a - f >= 0 && a - f * 0.75 < b) {
				b = Math.max(a - f, 0)
			} else {
				if (a + f / 0.75 > b + g) {
					b = b + f
				}
			}
			this._obj.scrollTop = b
		}
		return true
	},
	_id: "dhx_f_id",
	on_click: {
		dhx_list_item: function (a, b) {
			if (this.stopEdit(false, b)) {
				if (this._settings.select) {
					if (this._settings.select == "multiselect") {
						this.select(b, a.ctrlKey || a.metaKey, a.shiftKey)
					} else {
						this.select(b)
					}
				}
			}
		}
	},
	on_dblclick: {
		dhx_list_item: function (a, b) {
			if (this._settings.edit) {
				this.edit(b)
			}
		}
	},
	on_mouse_move: {},
	types: {
		"default": {
			css: "default",
			template: dhtmlx.Template.fromHTML("<div style='padding:10px; white-space:nowrap; overflow:hidden;'>{obj.text}</div>"),
			template_edit: dhtmlx.Template.fromHTML("<div style='padding:10px; white-space:nowrap; overflow:hidden;'><textarea bind='obj.text'></textarea></div>"),
			template_loading: dhtmlx.Template.fromHTML("<div style='padding:10px; white-space:nowrap; overflow:hidden;'>Loading...</div>"),
			height: 50,
			margin: 0,
			padding: 10,
			border: 1
		}
	},
	template_item_start: dhtmlx.Template.fromHTML("<div dhx_f_id='{-obj.id}' class='dhx_list_item dhx_list_{obj.css}_item{-obj.$selected?_selected:}' style='width:100%; height:{obj.height}px; padding:{obj.padding}px; margin:{obj.margin}px; overflow:hidden;'>"),
	template_item_end: dhtmlx.Template.fromHTML("</div>")
};
dhtmlx.compat("layout");
if (typeof(window.dhtmlXCellObject) != "undefined") {
	dhtmlXCellObject.prototype.attachList = function (a) {
		this.callEvent("_onBeforeContentAttach", ["list"]);
		var b = document.createElement("DIV");
		b.style.width = "100%";
		b.style.height = "100%";
		b.style.position = "relative";
		b.style.overflowX = "hidden";
		this._attachObject(b);
		if (typeof(a) == "undefined") {
			a = {}
		}
		b.id = "ListObject_" + new Date().getTime();
		a.container = b.id;
		a.skin = this.conf.skin;
		this.dataType = "list";
		this.dataObj = new dhtmlXList(a);
		this.dataObj.setSizes = function () {
			this.render()
		};
		b = null;
		this.callEvent("_onContentAttach", []);
		return this.dataObj
	}
}
function dataProcessor(a) {
	this.serverProcessor = a;
	this.action_param = "!nativeeditor_status";
	this.object = null;
	this.updatedRows = [];
	this.autoUpdate = true;
	this.updateMode = "cell";
	this._tMode = "GET";
	this._headers = null;
	this._payload = null;
	this.post_delim = "_";
	this._waitMode = 0;
	this._in_progress = {};
	this._invalid = {};
	this.mandatoryFields = [];
	this.messages = [];
	this.styles = {
		updated: "font-weight:bold;",
		inserted: "font-weight:bold;",
		deleted: "text-decoration : line-through;",
		invalid: "background-color:FFE0E0;",
		invalid_cell: "border-bottom:2px solid red;",
		error: "color:red;",
		clear: "font-weight:normal;text-decoration:none;"
	};
	this.enableUTFencoding(true);
	dhx4._eventable(this);
	if (this.connector_init) {
		this.setTransactionMode("POST", true);
		this.serverProcessor += (this.serverProcessor.indexOf("?") != -1 ? "&" : "?") + "editing=true"
	}
	return this
}
dataProcessor.prototype = {
	url: function (a) {
		if (a.indexOf("?") != -1) {
			return "&"
		} else {
			return "?"
		}
	},
	setTransactionMode: function (b, a) {
		if (typeof b == "object") {
			this._tMode = b.mode || this._tMode;
			this._headers = this._headers || b.headers;
			this._payload = this._payload || b.payload
		} else {
			this._tMode = b;
			this._tSend = a
		}
		if (this._tMode == "REST") {
			this._tSend = false;
			this._endnm = true
		}
		if (this._tMode == "JSON") {
			this._tSend = false;
			this._endnm = true;
			this._headers = this._headers || {};
			this._headers["Content-type"] = "application/json"
		}
	},
	escape: function (a) {
		if (this._utf) {
			return encodeURIComponent(a)
		} else {
			return escape(a)
		}
	},
	enableUTFencoding: function (a) {
		this._utf = dhx4.s2b(a)
	},
	setDataColumns: function (a) {
		this._columns = (typeof a == "string") ? a.split(",") : a
	},
	getSyncState: function () {
		return !this.updatedRows.length
	},
	enableDataNames: function (a) {
		this._endnm = dhx4.s2b(a)
	},
	enablePartialDataSend: function (a) {
		this._changed = dhx4.s2b(a)
	},
	setUpdateMode: function (b, a) {
		this.autoUpdate = (b == "cell");
		this.updateMode = b;
		this.dnd = a
	},
	ignore: function (b, a) {
		this._silent_mode = true;
		b.call(a || window);
		this._silent_mode = false
	},
	setUpdated: function (d, c, e) {
		this._log("item " + d + " " + (c ? "marked" : "unmarked") + " [" + (e || "updated") + "]");
		if (this._silent_mode) {
			return
		}
		var b = this.findRow(d);
		e = e || "updated";
		var a = this.obj.getUserData(d, this.action_param);
		if (a && e == "updated") {
			e = a
		}
		if (c) {
			this.set_invalid(d, false);
			this.updatedRows[b] = d;
			this.obj.setUserData(d, this.action_param, e);
			if (this._in_progress[d]) {
				this._in_progress[d] = "wait"
			}
		} else {
			if (!this.is_invalid(d)) {
				this.updatedRows.splice(b, 1);
				this.obj.setUserData(d, this.action_param, "")
			}
		}
		if (!c) {
			this._clearUpdateFlag(d)
		}
		this.markRow(d, c, e);
		if (c && this.autoUpdate) {
			this.sendData(d)
		}
	},
	_clearUpdateFlag: function (a) {},
	markRow: function (f, c, e) {
		var d = "";
		var b = this.is_invalid(f);
		if (b) {
			d = this.styles[b];
			c = true
		}
		if (this.callEvent("onRowMark", [f, c, e, b])) {
			d = this.styles[c ? e : "clear"] + d;
			this.obj[this._methods[0]](f, d);
			if (b && b.details) {
				d += this.styles[b + "_cell"];
				for (var a = 0; a < b.details.length; a++) {
					if (b.details[a]) {
						this.obj[this._methods[1]](f, a, d)
					}
				}
			}
		}
	},
	getState: function (a) {
		return this.obj.getUserData(a, this.action_param)
	},
	is_invalid: function (a) {
		return this._invalid[a]
	},
	set_invalid: function (c, b, a) {
		if (a) {
			b = {
				value: b,
				details: a,
				toString: function () {
					return this.value.toString()
				}
			}
		}
		this._invalid[c] = b
	},
	checkBeforeUpdate: function (a) {
		return true
	},
	sendData: function (a) {
		if (a) {
			this._log("Sending: " + a)
		}
		if (this._waitMode && (this.obj.mytype == "tree" || this.obj._h2)) {
			return
		}
		if (this.obj.editStop) {
			this.obj.editStop()
		}
		if (typeof a == "undefined" || this._tSend) {
			return this.sendAllData()
		}
		if (this._in_progress[a]) {
			return false
		}
		this.messages = [];
		if (this.getState(a) !== "deleted") {
			if (!this.checkBeforeUpdate(a) && this.callEvent("onValidationError", [a, this.messages])) {
				return false
			}
		}
		this._beforeSendData(this._getRowData(a), a)
	},
	_beforeSendData: function (a, b) {
		if (!this.callEvent("onBeforeUpdate", [b, this.getState(b), a])) {
			return false
		}
		this._sendData(a, b)
	},
	serialize: function (d, e) {
		if (typeof d == "string") {
			return d
		}
		if (typeof e != "undefined") {
			return this.serialize_one(d, "")
		} else {
			var a = [];
			var c = [];
			for (var b in d) {
				if (d.hasOwnProperty(b)) {
					a.push(this.serialize_one(d[b], b + this.post_delim));
					c.push(b)
				}
			}
			a.push("ids=" + this.escape(c.join(",")));
			if (window.dhtmlx && dhtmlx.security_key) {
				a.push("dhx_security=" + dhtmlx.security_key)
			}
			return a.join("&")
		}
	},
	serialize_one: function (d, b) {
		if (typeof d == "string") {
			return d
		}
		var a = [];
		for (var c in d) {
			if (d.hasOwnProperty(c)) {
				if ((c == "id" || c == this.action_param) && this._tMode == "REST") {
					continue
				}
				a.push(this.escape((b || "") + c) + "=" + this.escape(d[c]))
			}
		}
		return a.join("&")
	},
	_applyPayload: function (a) {
		if (this._payload) {
			for (var b in this._payload) {
				a = a + (a.indexOf("?") === -1 ? "?" : "&") + this.escape(b) + "=" + this.escape(this._payload[b])
			}
		}
		return a
	},
	_sendData: function (e, f) {
		this._log("url: " + this.serverProcessor);
		this._log(e);
		if (!e) {
			return
		}
		if (!this.callEvent("onBeforeDataSending", f ? [f, this.getState(f), e] : [null, null, e])) {
			return false
		}
		if (f) {
			this._in_progress[f] = (new Date()).valueOf()
		}
		var l = this;
		var k = function (o) {
			var r = [];
			if (f) {
				r.push(f)
			} else {
				if (e) {
					for (var q in e) {
						r.push(q)
					}
				}
			}
			return l.afterUpdate(l, o, r)
		};
		var b = this.serverProcessor + (this._user ? (this.url(this.serverProcessor) + ["dhx_user=" + this._user, "dhx_version=" + this.obj.getUserData(0, "version")].join("&")) : "");
		var n = this._applyPayload(b);
		if (this._tMode == "GET") {
			dhx4.ajax.query({
				url: n + ((n.indexOf("?") != -1) ? "&" : "?") + this.serialize(e, f),
				method: "GET",
				headers: this._headers,
				callback: k
			})
		} else {
			if (this._tMode == "POST") {
				dhx4.ajax.query({
					url: n,
					method: "POST",
					headers: this._headers,
					callback: k,
					data: this.serialize(e, f)
				})
			} else {
				if (this._tMode == "JSON") {
					var g = e[this.action_param];
					var j = {};
					for (var m in e) {
						j[m] = e[m]
					}
					delete j[this.action_param];
					delete j.id;
					delete j.gr_id;
					dhx4.ajax.query({
						url: n,
						method: "POST",
						headers: this._headers,
						callback: k,
						data: JSON.stringify({
							id: f,
							action: g,
							data: j
						})
					})
				} else {
					if (this._tMode == "REST") {
						var d = this.getState(f);
						var c = b.replace(/(\&|\?)editing\=true/, "");
						var h = c.split("?");
						if (h[1]) {
							h[1] = "?" + h[1]
						}
						var j = "";
						var a = "post";
						if (d == "inserted") {
							j = this.serialize(e, f)
						} else {
							if (d == "deleted") {
								a = "DELETE";
								c = h[0] + f + (h[1] || "")
							} else {
								a = "PUT";
								j = this.serialize(e, f);
								c = h[0] + f + (h[1] || "")
							}
						}
						this._applyPayload(c);
						dhx4.ajax.query({
							url: c,
							method: a,
							headers: this._headers,
							data: j,
							callback: k
						})
					}
				}
			}
		}
		this._waitMode++
	},
	sendAllData: function () {
		this._log("Sending all updated items");
		if (!this.updatedRows.length) {
			return
		}
		this.messages = [];
		var b = true;
		for (var a = 0; a < this.updatedRows.length; a++) {
			if (this.getState(this.updatedRows[a]) !== "deleted") {
				b &= this.checkBeforeUpdate(this.updatedRows[a])
			}
		}
		if (!b && !this.callEvent("onValidationError", ["", this.messages])) {
			return false
		}
		if (this._tSend) {
			this._sendData(this._getAllData())
		} else {
			for (var a = 0; a < this.updatedRows.length; a++) {
				if (!this._in_progress[this.updatedRows[a]]) {
					if (this.is_invalid(this.updatedRows[a])) {
						continue
					}
					this._beforeSendData(this._getRowData(this.updatedRows[a]), this.updatedRows[a]);
					if (this._waitMode && (this.obj.mytype == "tree" || this.obj._h2)) {
						return
					}
				}
			}
		}
	},
	_getAllData: function (d) {
		var b = {};
		var a = false;
		for (var c = 0; c < this.updatedRows.length; c++) {
			var e = this.updatedRows[c];
			if (this._in_progress[e] || this.is_invalid(e)) {
				continue
			}
			if (!this.callEvent("onBeforeUpdate", [e, this.getState(e), this._getRowData(e)])) {
				continue
			}
			b[e] = this._getRowData(e, e + this.post_delim);
			a = true;
			this._in_progress[e] = (new Date()).valueOf()
		}
		return a ? b : null
	},
	setVerificator: function (b, a) {
		this.mandatoryFields[b] = a || (function (c) {
				return (c !== "")
			})
	},
	clearVerificator: function (a) {
		this.mandatoryFields[a] = false
	},
	findRow: function (b) {
		var a = 0;
		for (a = 0; a < this.updatedRows.length; a++) {
			if (b == this.updatedRows[a]) {
				break
			}
		}
		return a
	},
	defineAction: function (a, b) {
		if (!this._uActions) {
			this._uActions = []
		}
		this._uActions[a] = b
	},
	afterUpdateCallback: function (b, g, f, e) {
		this._log("Action: " + f + " SID:" + b + " TID:" + g, e);
		var a = b;
		var d = (f != "error" && f != "invalid");
		if (!d) {
			this.set_invalid(b, f)
		}
		if ((this._uActions) && (this._uActions[f]) && (!this._uActions[f](e))) {
			return (delete this._in_progress[a])
		}
		if (this._in_progress[a] != "wait") {
			this.setUpdated(b, false)
		}
		var c = b;
		switch (f) {
		case "inserted":
		case "insert":
			if (g != b) {
				this.obj[this._methods[2]](b, g);
				b = g
			}
			break;
		case "delete":
		case "deleted":
			this.obj.setUserData(b, this.action_param, "true_deleted");
			this.obj[this._methods[3]](b);
			delete this._in_progress[a];
			return this.callEvent("onAfterUpdate", [b, f, g, e]);
			break
		}
		if (this._in_progress[a] != "wait") {
			if (d) {
				this.obj.setUserData(b, this.action_param, "")
			}
			delete this._in_progress[a]
		} else {
			delete this._in_progress[a];
			this.setUpdated(g, true, this.obj.getUserData(b, this.action_param))
		}
		this.callEvent("onAfterUpdate", [c, f, g, e])
	},
	enableDebug: function () {
		this._debug = true
	},
	_log: function () {
		if (this._debug && window.console && window.console.info) {
			window.console.info.apply(window.console, arguments)
		}
	},
	afterUpdate: function (j, h, a) {
		this._log("Server response received");
		if (window.JSON) {
			try {
				var n = JSON.parse(h.xmlDoc.responseText);
				var d = n.action || this.getState(a) || "updated";
				var b = n.sid || a[0];
				var c = n.tid || a[0];
				j.afterUpdateCallback(b, c, d, n);
				j.finalizeUpdate();
				return
			} catch (k) {}
		}
		var m = dhx4.ajax.xmltop("data", h.xmlDoc);
		if (!m || m.tagName == "DIV") {
			return this.cleanUpdate(a)
		}
		var l = dhx4.ajax.xpath("//data/action", m);
		if (!l.length) {
			return this.cleanUpdate(a)
		}
		for (var g = 0; g < l.length; g++) {
			var f = l[g];
			var d = f.getAttribute("type");
			var b = f.getAttribute("sid");
			var c = f.getAttribute("tid");
			j.afterUpdateCallback(b, c, d, f)
		}
		j.finalizeUpdate()
	},
	cleanUpdate: function (b) {
		if (b) {
			for (var a = 0; a < b.length; a++) {
				delete this._in_progress[b[a]]
			}
		}
	},
	finalizeUpdate: function () {
		if (this._waitMode) {
			this._waitMode--
		}
		if ((this.obj.mytype == "tree" || this.obj._h2) && this.updatedRows.length) {
			this.sendData()
		}
		this.callEvent("onAfterUpdateFinish", []);
		if (!this.updatedRows.length) {
			this.callEvent("onFullSync", [])
		}
	},
	init: function (a) {
		this.obj = a;
		if (a._dp_init) {
			a._dp_init(this)
		}
		if (this.connector_init) {
			a._dataprocessor = this
		}
	},
	setOnAfterUpdate: function (a) {
		this.attachEvent("onAfterUpdate", a)
	},
	setOnBeforeUpdateHandler: function (a) {
		this.attachEvent("onBeforeDataSending", a)
	},
	setAutoUpdate: function (c, b) {
		c = c || 2000;
		this._user = b || (new Date()).valueOf();
		this._need_update = false;
		this._update_busy = false;
		this.attachEvent("onAfterUpdate", function (d, f, g, e) {
			this.afterAutoUpdate(d, f, g, e)
		});
		this.attachEvent("onFullSync", function () {
			this.fullSync()
		});
		var a = this;
		window.setInterval(function () {
			a.loadUpdate()
		}, c)
	},
	afterAutoUpdate: function (a, c, d, b) {
		if (c == "collision") {
			this._need_update = true;
			return false
		} else {
			return true
		}
	},
	fullSync: function () {
		if (this._need_update == true) {
			this._need_update = false;
			this.loadUpdate()
		}
		return true
	},
	getUpdates: function (a, b) {
		if (this._update_busy) {
			return false
		} else {
			this._update_busy = true
		}
		dhx4.ajax.get(a, b)
	},
	_v: function (a) {
		if (a.firstChild) {
			return a.firstChild.nodeValue
		}
		return ""
	},
	_a: function (a) {
		var c = [];
		for (var b = 0; b < a.length; b++) {
			c[b] = this._v(a[b])
		}
		return c
	},
	loadUpdate: function () {
		var b = this;
		var a = this.obj.getUserData(0, "version");
		var c = this.serverProcessor + this.url(this.serverProcessor) + ["dhx_user=" + this._user, "dhx_version=" + a].join("&");
		c = c.replace("editing=true&", "");
		this.getUpdates(c, function (j) {
			var k = dhx4.ajax.xmltop("updates", j.xmlDoc);
			var f = dhx4.ajax.xpath("//userdata", k);
			b.obj.setUserData(0, "version", b._v(f[0]));
			var d = dhx4.ajax.xpath("//update", k);
			if (d.length) {
				b._silent_mode = true;
				for (var g = 0; g < d.length; g++) {
					var e = d[g].getAttribute("status");
					var l = d[g].getAttribute("id");
					var h = d[g].getAttribute("parent");
					switch (e) {
					case "inserted":
						b.callEvent("insertCallback", [d[g], l, h]);
						break;
					case "updated":
						b.callEvent("updateCallback", [d[g], l, h]);
						break;
					case "deleted":
						b.callEvent("deleteCallback", [d[g], l, h]);
						break
					}
				}
				b._silent_mode = false
			}
			b._update_busy = false;
			b = null
		})
	}
};
if (window.dataProcessor && !dataProcessor.prototype.init_original) {
	dataProcessor.prototype.connector_init = true
};
