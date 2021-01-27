(function () {

    var noop = function () { };

    var link = function (fn) {
        return function (scope, element) {
            fn.apply(fn, arguments);
            var resync = function () {
                if (scope.group) {
                    var target = element[0].querySelector("[jtk-group-content]") || element[0];
                    if (element[0]._jtkGroupProcessed !== true && element[0]._jtkGroupNodes) {

                        for (var i = 0; i < element[0]._jtkGroupNodes.length; i++) {
                            target.appendChild(element[0]._jtkGroupNodes[i].el);
                        }

                        delete element[0]._jtkGroupNodes;
                    }
                    target.setAttribute("jtk-group-content", "true");
                    element[0]._jtkGroupProcessed = true;
                }

                // post event to toolkit controller upstream
                scope.$emit("refresh", scope.node || scope.group);
            };
            scope.$watch(null, resync);  // for the first paint
            scope.$watchCollection(scope.node ? "node" : "group", resync);  // for subsequent data changes.
        };
    };

    var inherit = function (scope, prop) {
        var pn = scope.$parent, value = scope[prop];
        while (pn != null && value == null) {
            value = pn[prop];
            pn = pn.$parent;
        }
        if (value != null) {
            scope[prop] = value;
        }
    };

    angular.module("$jsPlumb", [])

        .factory("jsPlumbFactory", ["$compile", "jsPlumbService", "$timeout", function ($compile, jsPlumbService, $timeout) {

            return {
                /**
                 * Create an instance of the jsPlumb Toolkit.
                 * @method instance
                 * @param params
                 *
                 */
                instance: function (params) {
                    params = params || {};
                    return {
                        restrict: "E",
                        template: params.template || "<div class='jtk-angular-directive' style='height:100%;' ng-transclude></div>",
                        transclude: true,
                        scope: {
                            renderParams: "=",
                            params: "=",
                            data: "=",
                            format: "=",
                            jtkId: "@",
                            surfaceId: "@",
                            init: "="
                        },
                        replace: params.replace,
                        controller: ["$scope", "$attrs", function ($scope, $attrs) {
                            this.jtk = jsPlumbService.getToolkit($attrs.jtkId, $scope.params);
                            this.data = $scope.data;
                            $scope.toolkit = this.jtk;
                            var self = this;
                            $scope.$on("refresh", function (e, node) {
                                self.jtk.updateNode(node);
                            });

                            params.controller && params.controller.apply(this, arguments);
                        }],
                        controllerAs: params.controllerAs,
                        link: function (scope, element, attrs, controller) {
                            var jtk = controller.jtk, args = arguments;
                            var p = jsPlumb.extend({}, scope.renderParams);
                            // must configure a miniview using the directive when using angular. otherwise
                            // there is a clash.
                            delete p.miniview;
                            var dataFormat = attrs.format;
                            var deferredEdgesPainted = 0;
                            jsPlumb.extend(p, {
                                container: params.replace ? element[0] : element[0].childNodes[0],
                                templateRenderer: function (directiveId, data, toolkit, objectType) {
                                    var newScope = scope.$new();
                                    newScope[objectType] = data;
                                    newScope.toolkit = scope.toolkit;
                                    newScope.surface = scope.surface;
                                    var newNode = angular.element("<" + directiveId + " " + objectType + '="' + objectType + '" toolkit="toolkit" surface="surface"></' + directiveId + ">");
                                    $compile(newNode)(newScope);

                                    return newNode;
                                },
                                id: attrs.surfaceId
                            });
                            // create an id for this renderer if one does not exist.
                            p.id = p.id || jsPlumbToolkitUtil.uuid();

                            // in angular, disable enhanced views; it interfers with the two way data binding because
                            // it makes a copy of the original data.
                            p.enhancedView = false;

                            p.connectionHandler = function (edge, connectFn) {
                                deferredEdgesPainted++;
                                $timeout(connectFn);
                            };

                            // configure miniview if supplied as an attribute on the directive.
                            if (attrs.miniview != null) {
                                p.miniview = {
                                    container: attrs.miniview
                                };
                            }

                            jsPlumbToolkit.ready(function () {
                                // write the Surface into the controller's scope
                                scope.surface = jtk.render(p);
                                // register on the service
                                jsPlumbService.addSurface(p.id, scope.surface);

                                scope.surface.bind("nodeAdded", function (data) {
                                    if (data.node.group) {
                                        var groupEl = scope.surface.getRenderedGroup(data.node.group.id);
                                        if (groupEl) {
                                            if (groupEl._jtkGroupProcessed !== true) {
                                                groupEl._jtkGroupNodes = groupEl._jtkGroupNodes || [];
                                                groupEl._jtkGroupNodes.push(data);
                                            }
                                        }
                                    }
                                });

                                // bind to nodeUpdated event and apply the scope, to get changes
                                // through to the view layer.
                                scope.toolkit.bind("nodeUpdated", function () {
                                    $timeout(function () { scope.$apply(); });
                                });

                                scope.toolkit.bind("dataLoadStart", function () {
                                    deferredEdgesPainted = 0;
                                });

                                scope.toolkit.bind("dataLoadEnd", function () {
                                    if (deferredEdgesPainted > 0) {
                                        $timeout(scope.surface.getJsPlumb().getGroupManager().refreshAllGroups);
                                    }
                                    deferredEdgesPainted = 0;
                                });

                                // if user supplied a link function, call it.
                                params.link && params.link.apply(this, args);

                                if (scope.data) {
                                    jtk.load({ data: scope.data, type: dataFormat });
                                }

                                if (scope.init) {
                                    $timeout(function () {
                                        scope.init.apply(scope, args);
                                    });
                                }
                            });

                            params.link && params.link.apply(this, arguments);
                        }
                    };
                },
                node: function (params) {
                    var out = {};
                    jsPlumb.extend(out, params || {});
                    out.restrict = "E";
                    out.scope = out.scope || {};
                    out.scope.node = "=";
                    out.scope.toolkit = "=";
                    out.scope.surface = "=";
                    out.link = link(out.link || noop);
                    if (params.inherit) {
                        if (out.controller == null) {
                            out.controller = ["$scope", function ($scope) {
                                for (var i = 0; i < params.inherit.length; i++) {
                                    inherit($scope, params.inherit[i]);
                                }
                            }];
                        }
                        else if (out.controller.length != null) {
                            // an array. check if the array contains $scope.
                            var idx = out.controller.indexOf("$scope"), scopeAdded = false;
                            if (idx === -1) {
                                out.controller.unshift("$scope");
                                idx = 0;
                                scopeAdded = true;
                            }

                            var fn = out.controller.pop();
                            out.controller.push(function () {
                                for (var i = 0; i < params.inherit.length; i++) {
                                    inherit(arguments[idx], params.inherit[i]);
                                }
                                fn.apply(this, scopeAdded ? arguments.slice(1) : arguments);
                            });
                        }
                        else {
                            throw new TypeError("Controller spec must be in strict format to use inherit parameter with jsPlumb Angular integration");
                        }
                    }
                    return out;
                },

                group: function (params) {
                    var out = {};
                    jsPlumb.extend(out, params || {});
                    out.restrict = "E";
                    out.scope = out.scope || {};
                    out.scope.group = "=";
                    out.scope.toolkit = "=";
                    out.scope.surface = "=";
                    out.link = link(out.link || noop);
                    if (params.inherit) {
                        if (out.controller == null) {
                            out.controller = ["$scope", function ($scope) {
                                for (var i = 0; i < params.inherit.length; i++) {
                                    inherit($scope, params.inherit[i]);
                                }
                            }];
                        }
                        else if (out.controller.length != null) {
                            // an array. check if the array contains $scope.
                            var idx = out.controller.indexOf("$scope"), scopeAdded = false;
                            if (idx === -1) {
                                out.controller.unshift("$scope");
                                idx = 0;
                                scopeAdded = true;
                            }

                            var fn = out.controller.pop();
                            out.controller.push(function () {
                                for (var i = 0; i < params.inherit.length; i++) {
                                    inherit(arguments[idx], params.inherit[i]);
                                }
                                fn.apply(this, scopeAdded ? arguments.slice(1) : arguments);
                            });
                        }
                        else {
                            throw new TypeError("Controller spec must be in strict format to use inherit parameter with jsPlumb Angular integration");
                        }
                    }
                    return out;
                },

                miniview: function () {
                    return {
                        restrict: "AE",
                        scope: {
                            surfaceId: "@"
                        },
                        replace: true,
                        template: "<div></div>",
                        link: function (scope, element, attrs) {
                            var init = function () {
                                jsPlumbService.addMiniview(attrs.surfaceId, {
                                    container: element
                                });
                            };
                            scope.$watch(null, init);  // workaround angular async paint.
                        }
                    };
                }
            };
        }])

        .service("jsPlumbService", ["$templateCache", "$timeout", function ($templateCache, $timeout) {

            var eg = new jsPlumbUtil.EventGenerator(),
                toolkits = {},
                newToolkit = function (id, params) {
                    var tk = jsPlumbToolkit.newInstance(params || {});
                    tk._ngId = id;
                    toolkits[id] = tk;
                    eg.fire("ready", { id: id, toolkit: tk });
                    return tk;
                },
                surfaces = {},
                workQueues = {},
                handlers = {
                    "palette": function (surface, params) {
                        surface.registerDroppableNodes({
                            droppables: params.selector(params.element),
                            dragOptions: params.dragOptions,
                            typeExtractor: params.typeExtractor,
                            dataGenerator: params.dataGenerator,
                            locationSetter: params.locationSetter,
                            onDrop: params.onDrop
                        });
                    },
                    "miniview": function (surface, params) {
                        var miniview = surface.createMiniview({
                            container: params.container
                        });
                        surface.getToolkit().bind("dataLoadEnd", function () {
                            $timeout(miniview.invalidate);
                        });

                        surface.getToolkit().bind("nodeAdded", function (params) {
                            $timeout(function () { miniview.invalidate(params.node.id); });
                        });
                    }
                },
                addToWorkQueue = function (surfaceId, params, handler) {
                    var s = surfaces[surfaceId];
                    if (s) {
                        handler(s, params);
                    }
                    else {
                        workQueues[surfaceId] = workQueues[surfaceId] || [];
                        workQueues[surfaceId].push([params, handler]);
                    }
                };

            this.bind = function (event, toolkitId, callback) {
                eg.bind(event, function (p) {
                    if (p.id === toolkitId)
                        callback(p.toolkit, toolkitId, event);
                });
            };

            this.getToolkit = function (id, params) {
                id = id || jsPlumbToolkitUtil.uuid();
                if (toolkits[id] != null) return toolkits[id];
                else {
                    return newToolkit(id, params);
                }
            };

            this.resetToolkit = function (id) {
                var tk = toolkits[id];
                if (tk) {
                    tk.clear();
                    var rs = tk.getRenderers();
                    if (rs != null)
                        for (var r in rs)
                            if (rs.hasOwnProperty(r))
                                delete surfaces[rs[r]._ngId];

                    delete toolkits[id];
                }
            };

            this.addSurface = function (id, surface) {
                surfaces[id] = surface;
                surface._ngId = id;
                if (workQueues[id]) {
                    for (var i = 0; i < workQueues[id].length; i++) {
                        try {
                            workQueues[id][i][1](surface, workQueues[id][i][0]);
                        }
                        catch (e) {
                            if (typeof console != "undefined")
                                console.log("Cannot create component " + e);
                        }
                    }
                }
                delete workQueues[id];
            };

            this.getSurface = function (id) {
                return surfaces[id];
            };

            this.addComponent = function (surfaceId, params, type) {
                addToWorkQueue(surfaceId, params, handlers[type]);
            };

            this.addPalette = function (surfaceId, params) {
                this.addComponent(surfaceId, params, "palette");
            };

            this.addMiniview = function (surfaceId, params) {
                this.addComponent(surfaceId, params, "miniview");
            };

        }])

        .directive("jsplumbMiniview", ["jsPlumbFactory", function (jsPlumbFactory) {
            return jsPlumbFactory.miniview();
        }])

        .directive("jsplumbToolkit", ["jsPlumbFactory", function (jsPlumbFactory) {
            return jsPlumbFactory.instance();
        }])

        .directive("jsplumbPalette", ["jsPlumbService", "$timeout", function (jsPlumbService, $timeout) {
            return {
                restrict: "AE",
                scope: {
                    typeExtractor: "=",
                    generator: "=",
                    dragOptions: "=",
                    locationSetter: "=",
                    onDrop: "="
                },
                link: function ($scope, element, attrs) {
                    $timeout(function () {
                        var surface = jsPlumbService.getSurface(attrs.surfaceId);
                        if (surface) {
                            $scope.droppablesHandler = surface.registerDroppableNodes({
                                source: element[0],
                                selector: attrs.selector,
                                dragOptions: $scope.dragOptions || {
                                    zIndex: 50000,
                                    cursor: "move",
                                    clone: true
                                },
                                typeExtractor: $scope.typeExtractor,
                                dataGenerator: $scope.generator,
                                locationSetter: $scope.locationSetter,
                                onDrop: $scope.onDrop
                            });
                        }
                    });
                }
            };
        }]);

}).call(typeof window !== "undefined" ? window : this);