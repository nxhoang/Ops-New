//#region Variables
const BtnSaveChanges = "btnSaveChanges";
var CurrentFactory;
var CurrentProcessViewMode = 2;

var CurrentLineSerial; /*2019-08-06 Tai Le (Thomas) */
//#endregion

//#region Ready
(() => {
    var goJs = go.GraphObject.make, facDiagram, facWorkers;

    function tableStyle() {
        return [
            {
                background: "transparent"
            },
            {
                layerName: "Background"
            }, // behind all Persons
            {
                locationSpot: go.Spot.Center,
                locationObjectName: "TABLESHAPE"
            },
            new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
            {
                rotatable: false,
                movable: false
            },
            new go.Binding("angle").makeTwoWay(),
            { // what to do when a drag-over or a drag-drop occurs on a Node representing a table
                mouseDragEnter: function (e, node, prev) {
                    highlightSeats(node, node.diagram.selection, true);
                },
                mouseDragLeave: function (e, node, next) {
                    highlightSeats(node, node.diagram.selection, false);
                },
                mouseDrop: function (e, node) {
                    assignPeopleToSeats(node, node.diagram.selection, e.documentPoint);
                    document.getElementById("btnSaveChanges").disabled = false;
                }
            }
        ];
    }

    // Create a seat element at a particular alignment relative to the table.
    function facSeat(number, align, focus, bgColor) {
        if (bgColor === null || bgColor === undefined) bgColor = "burlywood";
        if (typeof align === 'string') align = go.Spot.parse(align);
        if (!align || !align.isSpot()) align = go.Spot.Right;
        if (typeof focus === 'string') focus = go.Spot.parse(focus);
        if (!focus || !focus.isSpot()) focus = align.opposite();
        return goJs(go.Panel, "Spot",
            {
                name: number.toString(),
                alignment: align,
                alignmentFocus: focus
            },
            goJs(go.Shape, TableShape.Square,
                {
                    name: "SEATSHAPE",
                    desiredSize: new go.Size(SeatSize, SeatSize),
                    fill: bgColor,
                    stroke: "white",
                    strokeWidth: 2
                },
                {
                    mouseOver: (e, obj) => {
                        obj.strokeWidth = 4; obj.stroke = "dodgerblue";
                    },
                    mouseLeave: (e, obj) => {
                        obj.strokeWidth = 2; obj.stroke = "white";
                    },
                    doubleClick: (e, obj) => {
                        showSeatDetail(number, obj);
                    }
                },
                new go.Binding("fill")),
            goJs(go.TextBlock, number.toString(), { font: "10pt Verdana, sans-serif" },
                new go.Binding("angle", "angle", (n) => { return -n; })),
            {
                doubleClick: (e, obj) => {
                    showSeatDetail(number, obj);
                }
            }
        );
    }

    function showSeatDetail(number, obj) {
        const line = obj.part.data.name;
        console.log(obj.part.data);
        let opName = obj.part.data.guests[number.toString()];
        if (opName === undefined) opName = "";

        bindSeatDetail(number, line, opName);
        $("#seatDetailModal").modal('show');
    }

    function initData() {
        // Initialize the main Diagram
        facDiagram = goJs(go.Diagram, "facDiagramDiv", {
            allowDragOut: false, // to facWorkers
            allowDrop: true, // from facWorkers
            allowClipboard: false,
            initialContentAlignment: go.Spot.Center,
            draggingTool: goJs(SpecialDraggingTool),
            rotatingTool: goJs(horizontalTextRotatingTool),
            // For this sample, automatically show the state of the diagram's model on the page
            "ModelChanged": function (e) {
                if (e.isTransactionFinished) {
                    //document.getElementById("savedModel").textContent = facDiagram.model.toJson();
                }
            },
            "undoManager.isEnabled": true
        });

        facDiagram.nodeTemplateMap.add("",
            goJs(go.Node, "Auto", { background: "transparent" },
                new go.Binding("layerName", "isSelected", function (s) { return s ? "Foreground" : ""; }).ofObject(),
                { locationSpot: go.Spot.Center },
                new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
                new go.Binding("text", "key"),
                { // what to do when a drag-over or a drag-drop occurs on a Node representing a table
                    mouseDragEnter: function (e, node, prev) {
                        highlightSeats(node, node.diagram.selection, true);
                    },
                    mouseDragLeave: function (e, node, next) {
                        highlightSeats(node, node.diagram.selection, false);
                    },
                    mouseDrop: function (e, node) {
                        assignPeopleToSeats(node, node.diagram.selection, e.documentPoint);
                    }
                },
                goJs(go.Shape, "Rectangle",
                    { fill: "white", stroke: "#1a1b1c", strokeWidth: 0.2 },
                    new go.Binding("fill", "DisplayColor", function (p) { // DisplayColor is a property of process
                        let c = "white";
                        if (p) c = `#${p.substring(3, 9)}`; // Because format of DisplayColor likes this "FF9ACD32", need to substring

                        return c;
                    })),
                goJs(go.Panel, "Viewbox", { desiredSize: new go.Size(35, 37) },
                    goJs(go.TextBlock, {
                        margin: 0,
                        desiredSize: new go.Size(34, 36),
                        font: "3pt Verdana, sans-serif",
                        textAlign: "start",
                        stroke: "#1a1b1c"
                    },
                        new go.Binding("text", "", (data) => {
                            let s = "";
                            if (data.OpNum) { // This is process
                                switch (window.CurrentProcessViewMode) {
                                    case 1:
                                        s = data.Remarks !== "" ? `[${data.OpNum}] ${data.OpName} (${data.Remarks})` :
                                            `[${data.OpNum}] ${data.OpName}`;
                                        break;
                                    case 3:
                                        s = `[${data.OpNum}] ${data.Remarks}`;
                                        break;
                                    default:
                                        s = `[${data.OpNum}] ${data.OpName}`;
                                        break;
                                }
                            } else {
                                if (data.id) { // This is group
                                    s = `${data.title}\n»Processes: ${data.Processes}\n»TaktTime : ${data.Processes}` +
                                        `\n»LeadTime: ${data.LeadTime}\n»Machines : ${data.Machines}`;
                                    if (s === "") s = "(This is empty group)";
                                }
                            }

                            return s;
                        }))
                )
            ));

        // what to do when a drag-drop occurs in the Diagram's background
        facDiagram.mouseDrop = function (e) {
            e.diagram.selection.each(function (n) {
                if (isPerson(n)) unassignSeat(n.data);
            });
        };

        facDiagram.addDiagramListener("PartRotated", function () {
            facDiagram.currentTool.doCancel();
        });

        // initialize the Palette
        facWorkers = goJs(go.Diagram, "facWorkerDiv",
            {
                layout: goJs(go.GridLayout,
                    {
                        sorting: go.GridLayout.Ascending // sort by Node.text value
                    }),
                allowDragOut: true, // to facDiagram
                allowDrop: true // from facDiagram
            });

        // to simulate a "move" from the Palette, the source Node must be deleted.
        facDiagram.addDiagramListener("ExternalObjectsDropped", (e) => {
            let isGroup = false;
            e.subject.each(function (n) {
                if (n.data.title || n.data.title === "") {
                    isGroup = true;
                }
            });

            if (isGroup) {
                facDiagram.disableSelectionDeleted = true;
                facDiagram.commandHandler.deleteSelection();
                facDiagram.disableSelectionDeleted = false;
            }

            // if any Tables were dropped, don't delete from facWorkers
            if (!e.subject.any(isTable)) {
                facWorkers.disableSelectionDeleted = true;
                facWorkers.commandHandler.deleteSelection();
                facWorkers.disableSelectionDeleted = false;
            }
        });

        facDiagram.addDiagramListener("SelectionDeleted", (e) => {
            if (facDiagram.disableSelectionDeleted) return;
            e.subject.each(function (n) {
                if (isPerson(n)) {
                    if (n.data.title === null || n.data.title === undefined) {
                        facWorkers.model.addNodeData(facWorkers.model.copyNodeData(n.data));
                    }
                }
            });
        });

        facDiagram.addDiagramListener("ObjectDoubleClicked", (ev) => {
            if (ev.ws.Ob && typeof ev.ws.Ob === "string" && ev.subject.part.data.category !== "TableOO") {
                const tb = facDiagram.findNodeForKey(ev.subject.part.data.table);

                bindSeatDetail(ev.subject.part.data.seat, tb.data.name, ev.ws.Ob);
                $("#seatDetailModal").modal('show');
            }
        });

        facWorkers.nodeTemplateMap = facDiagram.nodeTemplateMap;

        window.GetProcesses = function getProcesses(ps) {
            facDiagram.clear();
            facWorkers.clear();
            if (window.CurrentOpmt && CurrentOpmt.ConfirmChk === "Y" && window.MesProcesses) {
                const mxPackageRowId = $('#tbMesPackage').jqGrid('getGridParam', 'selrow');
                const rowData = $('#tbMesPackage').jqGrid('getRowData', mxPackageRowId);
                CurrentLineSerial = rowData.LineSerial;
                GetTbspByFactory(CurrentFactory, ps);
                const pList = [];

                for (let pc of window.MesProcesses) {
                    if (pc.TableId === 0 && pc.SeatNo === 0) {
                        pList.push(pc);
                    }
                }

                // specify the contents of the Palette
                facWorkers.model = new go.GraphLinksModel(pList);
            } else {
                MsgInform("Inform", "Please confirm firstly the operation plan before mapping to seats.", "error", true, true);
            }
        };

        facWorkers.model.undoManager = facDiagram.model.undoManager;

        facWorkers.addDiagramListener("ExternalObjectsDropped", function (e) {
            if (e.subject.any(isTable)) {
                facDiagram.currentTool.doCancel();
                facWorkers.currentTool.doCancel();
                return;
            }
            facDiagram.selection.each(function (n) {
                if (isPerson(n)) unassignSeat(n.data);
            });
            facDiagram.disableSelectionDeleted = true;
            facDiagram.commandHandler.deleteSelection();
            facDiagram.disableSelectionDeleted = false;

            facWorkers.selection.each(function (n) {
                if (isPerson(n)) unassignSeat(n.data);
            });
        });

        // To scale the canvas as order to make size of process bigger
        facWorkers.scale = 3.5;
    }

    function isPerson(n) {
        return n !== null && n !== undefined && n.category === "";
    }

    function isTable(n) {
        return n !== null && n !== undefined && n.category !== "";
    }

    function highlightSeats(node, coll, show) {
        if (isPerson(node)) { // refer to the person's table instead
            node = node.diagram.findNodeForKey(node.data.table);
            if (node === null) return;
        }
        if (coll.any(isTable)) {
            // if dragging a Table, don't do any highlighting
            return;
        }
        if (node === null || node === undefined) return;

        const guests = node.data.guests;
        for (var sit = node.elements; sit.next();) {
            var seat = sit.value;
            if (seat.name) {
                var num = parseFloat(seat.name);
                if (isNaN(num)) continue;
                var seatshape = seat.findObject("SEATSHAPE");
                if (!seatshape) continue;
                if (show) {
                    if (guests[seat.name]) {
                        seatshape.stroke = "red";
                    } else {
                        seatshape.stroke = "green";
                    }
                } else {
                    seatshape.stroke = "white";
                }
            }
        }
    }

    function assignPeopleToSeats(node, coll, pt) {
        if (isPerson(node)) { // refer to the person's table instead
            node = node.diagram.findNodeForKey(node.data.table);
            if (node === null) return;
        }
        if (coll.any(isTable)) {
            facDiagram.currentTool.doCancel();
            return;
        }
        coll.each(function (n) {
            assignSeat(node, n.data, pt);
        });
        positionPeopleAtSeats(node);
    }

    function assignSeat(node, guest, pt) {
        if (isPerson(node)) {
            node = node.diagram.findNodeForKey(node.data.table);
            if (node === null) return;
        }
        if (guest instanceof go.GraphObject)
            throw Error("A guest object must not be a GraphObject: " + guest.toString());
        if (!(pt instanceof go.Point) && node) pt = node.location;

        // in case the guest used to be assigned to a different seat, perhaps at a different table
        unassignSeat(guest);

        if (node === null || node === undefined) return;

        const model = node.diagram.model;
        const guests = node.data.guests;
        const closestseatname = findClosestUnoccupiedSeat(node, pt);

        if (closestseatname) {
            if (guest.hasOwnProperty("title")) {
                console.log("this is group");
            } else {
                model.setDataProperty(guests, closestseatname, guest.key);
                model.setDataProperty(guest, "table", node.data.key);
                model.setDataProperty(guest, "seat", parseFloat(closestseatname));
            }
        }

        if (guest.id) {
            const groupCode = guest.id === "emptyGroup" ? "emptyGroup" : guest.id.substring(1);
            if (groupCode) {
                guest.id = undefined;
                model.updateTargetBindings(guest);
                let isFirst = false;
                for (let p of window.MesProcesses) {
                    let opGroup = p.OpGroup;
                    if (p.OpGroup === null && p.OpGroupName === "No Group") opGroup = "emptyGroup";
                    if (opGroup === groupCode) {
                        let process;

                        // Ignore first one as group because seat process only.
                        if (isFirst === false) {
                            process = {
                                key: `p${p.OpSerial}`, OpNum: p.OpNum, StyleCode: p.StyleCode,
                                StyleColorSerial: p.StyleColorSerial, StyleSize: p.StyleSize, RevNo: p.RevNo,
                                OpRevNo: p.OpRevNo, OpSerial: p.OpSerial, OpName: p.OpName, OpGroup: p.OpGroup,
                                table: guest.table, seat: guest.seat, loc: guest.loc, DisplayColor: p.DisplayColor,
                                OpTime: p.OpTime, NextOp: p.NextOp
                            };
                            isFirst = true;
                        } else {
                            process = {
                                key: `p${p.OpSerial}`, OpNum: p.OpNum, StyleCode: p.StyleCode,
                                StyleColorSerial: p.StyleColorSerial, StyleSize: p.StyleSize, RevNo: p.RevNo,
                                OpRevNo: p.OpRevNo, OpSerial: p.OpSerial, OpName: p.OpName, OpGroup: p.OpGroup,
                                table: undefined, seat: undefined, DisplayColor: p.DisplayColor, OpTime: p.OpTime,
                                NextOp: p.NextOp
                            };
                        }

                        if (!checkProcessBeforeDropping(process)) {
                            model.addNodeData(process);
                            assignSeat(node, process, pt);
                        }
                    }
                }
            }
        }

        document.getElementById("btnSaveChanges").disabled = false;
    }

    function unassignSeat(guest) {
        if (guest instanceof go.GraphObject)
            throw Error("A guest object must not be a GraphObject: " + guest.toString());
        const model = facDiagram.model;
        // remove from any table that the guest is assigned to
        if (guest.table) {
            const table = model.findNodeDataForKey(guest.table);
            if (table) {
                const guests = table.guests;
                if (guests) model.setDataProperty(guests, guest.seat.toString(), undefined);
                document.getElementById("btnSaveChanges").disabled = false;
            }
        }
        model.setDataProperty(guest, "table", undefined);
        model.setDataProperty(guest, "seat", undefined);
    }

    function findClosestUnoccupiedSeat(node, pt) {
        if (isPerson(node)) { // refer to the person's table instead
            node = node.diagram.findNodeForKey(node.data.table);
        }

        var guests = node.data.guests;
        var closestseatname = null;
        var closestseatdist = Infinity;
        // iterate over all seats in the Node to find one that is not occupied
        for (var sit = node.elements; sit.next();) {
            var seat = sit.value;
            if (seat.name) {
                var num = parseFloat(seat.name);
                if (isNaN(num)) continue; // not really a "seat"
                if (guests[seat.name]) continue; // already assigned
                var seatloc = seat.getDocumentPoint(go.Spot.Center);
                var seatdist = seatloc.distanceSquaredPoint(pt);
                if (seatdist < closestseatdist) {
                    closestseatdist = seatdist;
                    closestseatname = seat.name;
                }
            }
        }
        return closestseatname;
    }

    function positionPeopleAtSeats(node) {
        if (isPerson(node)) { // refer to the person's table instead
            node = node.diagram.findNodeForKey(node.data.table);
            if (node === null) return;
        }
        if (node === null || node === undefined) return;

        var guests = node.data.guests;
        var model = node.diagram.model;
        for (var seatname in guests) {
            if (guests.hasOwnProperty(seatname)) {
                var guestkey = guests[seatname];
                var guestdata = model.findNodeDataForKey(guestkey);
                positionPersonAtSeat(guestdata);
            }
        }
    }

    function positionPersonAtSeat(guest) {
        if (guest instanceof go.GraphObject)
            throw Error("A guest object must not be a GraphObject: " + guest.toString());
        if (!guest || !guest.table || !guest.seat) return;
        var diagram = facDiagram;
        var table = diagram.findPartForKey(guest.table);
        var person = diagram.findPartForData(guest);
        if (table && person) {
            var seat = table.findObject(guest.seat.toString());
            var loc = seat.getDocumentPoint(go.Spot.Center);
            person.location = loc;
        }
    }

    function SpecialDraggingTool() {
        go.DraggingTool.call(this);
        this.isCopyEnabled = false;
    }

    go.Diagram.inherit(SpecialDraggingTool, go.DraggingTool);

    SpecialDraggingTool.prototype.computeEffectiveCollection = function (parts) {
        var map = go.DraggingTool.prototype.computeEffectiveCollection.call(this, parts);
        // for each Node representing a table, also drag all of the people seated at that table
        parts.each(function (table) {
            if (isPerson(table)) return; // ignore persons
            // this is a table Node, find all people Nodes using the same table key
            for (var nit = table.diagram.nodes; nit.next();) {
                var n = nit.value;
                if (isPerson(n) && n.data.table === table.data.key) {
                    if (!map.has(n)) map.add(n, new go.DraggingInfo(n.location.copy()));
                }
            }
        });
        return map;
    };

    function horizontalTextRotatingTool() {
        go.RotatingTool.call(this);
    }

    go.Diagram.inherit(horizontalTextRotatingTool, go.RotatingTool);

    horizontalTextRotatingTool.prototype.rotate = function (newangle) {
        go.RotatingTool.prototype.rotate.call(this, newangle);
        var node = this.adornedObject.part;
        positionPeopleAtSeats(node);
    };
    // end horizontalTextRotatingTool

    initData();

    function recSeats(total, seatAlign, seatType, seatDistance, bgColorTable) {
        var seats = [];

        if (total > 0) {
            let px = seatAlign, py = 0, ox = 0, oy = 0;

            switch (seatType) {
                case "1":
                case "2":
                    var o = 1, z = 0;

                    if (seatType === "2") {
                        o = 0;
                        z = 1;
                    }

                    for (let i = 1; i <= total; i++) {
                        if (i % 2 === 0) {
                            seats.push(facSeat(i, new go.Spot(px, o, ox, oy), go.Spot.Center, bgColorTable));
                            px = px + seatAlign;
                            ox = ox + seatDistance;
                        } else {
                            seats.push(facSeat(i, new go.Spot(px, z, ox, oy), go.Spot.Center, bgColorTable));
                        }
                    }
                    break;
                case "3":
                case "4":
                    if (seatType === "3") py = 1;
                    for (let i = 1; i <= total; i++) {
                        seats.push(facSeat(i, new go.Spot(px, py, ox, oy), go.Spot.Center, bgColorTable));
                        px = px + seatAlign;
                        ox = ox + seatDistance;
                    }
                    break;
                case "5":
                case "6":
                    if (seatType === "5") py = 1;

                    for (let i = 1; i <= total; i++) {
                        let sn = i;
                        if (i > 1) sn = i + (i - 1);

                        seats.push(facSeat(sn, new go.Spot(px, py, ox, oy), go.Spot.Center, bgColorTable));
                        px = px + seatAlign;
                        ox = ox + seatDistance;
                    }
                    break;
                case "7":
                case "8":
                    if (seatType === "7") py = 1;

                    for (let i = 1; i <= total; i++) {
                        let sn = i + i;

                        seats.push(facSeat(sn, new go.Spot(px, py, ox, oy), go.Spot.Center, bgColorTable));
                        px = px + seatAlign;
                        ox = ox + seatDistance;
                    }
                    break;
                case "9":
                case "10":
                    var r = 1, e = 0;

                    if (seatType === "10") {
                        r = 0;
                        e = 1;
                    }

                    for (let i = 1; i <= total; i++) {
                        if (i === 1) {
                            seats.push(facSeat(i, new go.Spot(px, r, ox, oy), go.Spot.Center, bgColorTable));
                            px = px + seatAlign;
                            ox = ox + seatDistance;
                        }
                        if (i % 2 === 0) {
                            seats.push(facSeat(i, new go.Spot(px, r, ox, oy), go.Spot.Center, bgColorTable));
                        } else {
                            if (i !== 1) {
                                seats.push(facSeat(i, new go.Spot(px, e, ox, oy), go.Spot.Center, bgColorTable));
                                px = px + seatAlign;
                                ox = ox + seatDistance;
                            }
                        }
                    }
                    break;
                case "11":
                case "12":
                    var t = 1, b = 0;

                    if (seatType === "12") {
                        t = 0;
                        b = 1;
                    }

                    for (let i = 1; i <= total; i++) {
                        if (i % 2 === 0) {
                            seats.push(facSeat(i, new go.Spot(px, t, ox, oy), go.Spot.Center, bgColorTable));
                        } else {
                            seats.push(facSeat(i, new go.Spot(px, b, ox, oy), go.Spot.Center, bgColorTable));
                            px = px + seatAlign;
                            ox = ox + seatDistance;
                        }
                    }
                    break;
            }
        }

        return seats;
    }

    window.HideTableByLine = function hideTableByLine(lineId, isVisible) {
        for (const it = facDiagram.nodes.iterator; it.next();) {
            facDiagram.startTransaction("Begin visible tables transaction");
            if (it.value.data.LineSerial.toString() === lineId) {
                it.value.visible = isVisible;
            }
            facDiagram.commitTransaction("Commit visible tables transaction");
        }
    };

    window.ShowHideTables = function showHideTables(isVisible) {
        for (const it = facDiagram.nodes.iterator; it.next();) {
            facDiagram.startTransaction("Begin visible tables transaction");
            it.value.visible = isVisible;
            facDiagram.commitTransaction("Commit visible tables transaction");
        }
    };

    window.ToggleTablesByLine = function toggleTablesByLine(lineId, isVisible) {
        for (const it = facDiagram.nodes.iterator; it.next();) {
            facDiagram.startTransaction("Begin hiding tables transaction");
            if (it && it.value && it.value.data && it.value.data.LineSerial.toString() === lineId) {
                it.value.visible = isVisible; // Show/hide only the tables by the line.
            } else {
                it.value.visible = !isVisible; // Show/hide other tables
            }
            facDiagram.commitTransaction("Commit hiding tables transaction");
        }
    };

    window.GetTbspByFactory = function getTables(selectedFactory, processes) {
        facDiagram.clear();
        facWorkers.clear();
        document.getElementById("btnSaveChanges").disabled = true;

        if (selectedFactory === null || selectedFactory === undefined) return;

        const getLinesByFacUri = `${UriApiTableSpace}/?factory=${selectedFactory}`;
        const uriLine = `${UriApiLine}/?fac=${selectedFactory}`;
        $.getJSON(uriLine).done((result) => {
            bindDataCbMesLine(result);
        });

        $.getJSON(getLinesByFacUri).done((tables) => {
            if (tables) {
                tables.forEach((t, i) => {
                    const tbkey = parseInt(t.TableId);
                    const w = {};

                    for (let p of processes) {
                        if (p.TableId === tbkey) {
                            w[`${p.SeatNo.toString()}`] = p.OpName; // Seat processes to tables.
                        }
                    }

                    createNewNode(tbkey, t.TableName, t.TbLocation, t.Angle, t.VirtualWidth, t.VirtualLength, t.ActualWidth,
                        t.ActualLength, t.SeatTotal, t.SeatType.toString(), t.SeatDistance, t.BackgroundColor, w,
                        t.LineSerial, t.Rate);

                    if (i === tables.length - 1) {
                        seatProcesses(); // At the end of loop, will seat processes
                        window.ToggleTablesByLine(CurrentLineSerial, true); // Show tables, processes by curent line.
                    }
                });
            }
        });
    };

    function createNewNode(tbKey, tbName, tbLocation, tbAngle, virtualWidth, virtualLength, actualWidth, actualLength,
        totalSeat, seatType, seatDistance, bgColorTable, workers, lineSerial, rate) {
        const seatAlign = 35 / virtualWidth;

        facDiagram.nodeTemplateMap.add(TableTypes.TableOO,
            goJs(go.Node, "Spot", tableStyle(),
                goJs(go.Panel, "Spot",
                    goJs(go.Shape, TableShape.Rectangle,
                        {
                            name: "TABLESHAPE",
                            desiredSize: new go.Size(virtualWidth, virtualLength),
                            fill: bgColorTable,
                            stroke: null
                        },
                        new go.Binding("desiredSize", "size", go.Size.parse).makeTwoWay(go.Size.stringify),
                        new go.Binding("fill")),
                    goJs(go.TextBlock,
                        {
                            editable: false,
                            font: "bold 11pt Verdana, sans-serif"
                        },
                        new go.Binding("text", "name").makeTwoWay()
                    )
                ),
                recSeats(totalSeat, seatAlign, seatType, seatDistance, bgColorTable)
            ));

        workers = workers === null || workers === undefined ? {} : workers;

        const table = {
            "key": tbKey,
            "category": TableTypes.TableOO,
            "name": tbName,
            "guests": workers,
            "loc": tbLocation,
            "angle": tbAngle,
            "VirtualWidth": virtualWidth,
            "VirtualLength": virtualLength,
            "ActualWidth": actualWidth,
            "ActualLength": actualLength,
            "TotalSeat": totalSeat,
            "SeatType": seatType,
            "SeatDistance": seatDistance,
            "BackgroundColor": bgColorTable,
            "LineSerial": lineSerial,
            "Rate": rate
        };

        facDiagram.startTransaction("Begin adding table transaction");
        facDiagram.model.addNodeData(table);
        facDiagram.commitTransaction("Commit adding table transaction");
    }

    function checkProcessBeforeDropping(p) {
        const nodeDataArr = JSON.parse(facDiagram.model.toJson()).nodeDataArray;
        for (let n of nodeDataArr) {
            if (n.OpSerial === p.OpSerial) return true;
        }
        return false;
    }

    function getListProcesses(processes) {
        const nodeDataArr = JSON.parse(facDiagram.model.toJson()).nodeDataArray;
        const ps = [];
        //const lineSerial = nodeDataArr[0].LineSerial;

        for (let n of processes) {
            const newProcess = new MesOpdt("M", n.StyleCode, n.StyleColorSerial, n.StyleSize, n.RevNo, n.OpRevNo, n.OpSerial,
                n.LineSerial, n.TableId, n.SeatNo);
            const p = nodeDataArr.find(x => x.OpSerial === n.OpSerial);
            if (p) {
                newProcess.SeatNo = p.seat;
                newProcess.TableId = p.table;
                const g = nodeDataArr.find(x => x.category === "TableOO" && x.key === p.table);
                if (g) {
                    newProcess.LineSerial = g.LineSerial;
                } else {
                    newProcess.LineSerial = null;
                }
            } else {
                newProcess.SeatNo = null;
                newProcess.TableId = null;
                newProcess.LineSerial = null;
            }
            ps.push(newProcess);
        }

        return ps;
    }

    function saveChanges() {
        const ps = getListProcesses(window.MesProcesses);
        mySqlSaveSeats(ps);
    }

    function mySqlSaveSeats(opdts) {
        $.blockUI();
        $.post(MsqlFacSaveSeatDetails, { opdts: opdts }).done((response) => {
            if (response.error === null || response.error === undefined) {
                document.getElementById(BtnSaveChanges).disabled = true;
                const result = response;

                if (result === true) {
                    const msg = getMsgByLang(msgSave);
                    MsgInform(msg.title, msg.value, ObjMessageType.Info, false, true);

                    oracleSaveSeats(opdts);
                } else {
                    const msg = getMsgByLang(msgError);
                    ShowMessage(msg.title, msg.value, ObjMessageType.Error);
                }
            } else {
                console.log(response.error);
            }
        }).always(() => { $.unblockUI(); });
    }

    function oracleSaveSeats(opdts) {
        $.post(OraFacSaveSeatDetails, { opdts: opdts }).done((response) => {
            if (response.error === null || response.error === undefined) {
                if (response === true) {
                    console.log("Saved successfully to PKMES.");
                } else {
                    console.log("Failed saving to PKMES.");
                }
            } else {
                console.log(response.error);
            }
        });
    }

    document.getElementById("btnSaveChanges").addEventListener("click", saveChanges);

    document.getElementById("selOpView").addEventListener("change", (e) => {
        loadPalleteData(e);
    });

    window.SelectedRowPackageGroup = function impersonate(r) {
        CurrentFactory = r.MesFactory;
        document.getElementById("wid-op-layout").classList.add("hide");
    };

    function getOpGroup(opGroups, remainProcesses) {
        const groups = [];
        for (let g of opGroups) {
            g.DisplayColor = "white";
            if (g.id === "emptyGroup") {
                // If there are some processes not seated to Line, adding the group to the pallete (left canvas).
                const p = remainProcesses.findIndex(x => x.OpGroup === null);
                if (p >= 0) {
                    groups.push(g);
                }
            } else {
                // Format of group id is like "g123", so need to substring.
                // At layout, inserting "g" to group id for unique (process id is also same group id).
                const groupId = g.id.substring(1);

                // If there are some processes not seated to Line, adding the group to the pallete (left canvas).
                const p = remainProcesses.findIndex(x => x.OpGroup === groupId);
                if (p >= 0) {
                    groups.push(g);
                }
            }
        }

        return groups;
    }

    function loadPalleteData(e) {
        const remainOpdts = JSON.parse(facWorkers.model.toJson()).nodeDataArray;

        facWorkers.clear();
        if (e.currentTarget.value) {
            const processes = [];
            let opGroups, groups;
            switch (e.currentTarget.value) {
                case "1":
                    for (let p of window.MesProcesses) {
                        if (!checkProcessBeforeDropping(p)) processes.push(p);
                    }
                    facWorkers.model = new go.GraphLinksModel(processes);
                    break;
                case "2":
                    opGroups = getOpGroup(window.ProcessGroups, remainOpdts);
                    groups = sumProcessGroup(opGroups, window.MesProcesses);
                    facWorkers.model = new go.GraphLinksModel(groups);
                    break;
            }
        }
    }

    function sumProcessGroup(groups, processes) {
        for (let g of groups) {
            const groupId = g.id.substring(1); // Format of group id is like "g123" so need to be substring
            let processCount = 0, taktTime = 0, leadTime = 0, machines = 0;

            for (let p of processes) {
                if (p.OpGroup === groupId) {
                    processCount += 1;
                    const tt = p.ManCount < 1 || p.ManCount === 0 ? 0 : Math.round(p.OpTime / p.ManCount);
                    taktTime += tt;
                    leadTime += p.OpTime;
                    machines += p.MachineCount;
                }
            }
            g.Processes = processCount;
            g.TaktTime = taktTime;
            g.LeadTime = leadTime;
            g.Machines = machines;
        }
        return groups;
    }

    $('#mesOpTab a[data-toggle="tab"]').on('shown.bs.tab', (e) => {
        if ($(e.target).attr('href') === "#opLineMappingTab") {
            facWorkers.requestUpdate();
            facDiagram.requestUpdate();
        }
    });

    function seatProcesses() {
        for (let pc of window.MesProcesses) {
            const p = pc;

            facDiagram.model.commit(function (m) {
                if (pc.TableId && pc.SeatNo && pc.TableId !== 0 && pc.SeatNo !== 0) {
                    p.table = pc.TableId;
                    p.seat = pc.SeatNo;
                    m.addNodeData(p);
                    positionPersonAtSeat(p);
                }
            }, "added guest");
        }
    }

    window.onbeforeunload = function () {
        const message = "Important: Please click on 'Save' button to leave this page.";
        const isDisable = document.getElementById('btnSaveChanges').disabled;

        if (!isDisable) return message;
    };
    opLineMappingTabClick();
})();
//#endregion

//#region Functions
function bindSeatDetail(seatNo, line, opName) {
    document.getElementById("lbSeatNo").innerHTML = seatNo.toString();
    document.getElementById("lbLine").innerHTML = line;
    document.getElementById("lbProcess").innerHTML = opName;
}

function reloadProcesses() {
    $.blockUI({ message: "<h3>Loading...</h3>" });

    const config = {
              url: UrlGetOpdts,
              async: true,
              postData: JSON.stringify({ opsMaster: CurrentOpmt, groupMode: CurrentGroupMode, languageId: currentLang })
          },
        request = GenericAjaxPost(config);

    request.done((res) => {
        console.log(res);
        if (res && res.opdts) {
            window.MesProcesses = res.opdts.nodes;
            window.GetProcesses(window.MesProcesses);
        } else {
            console.log(res);
        }

        $.unblockUI();
    }).fail((xhr, status, err) => {
        HandleException(xhr, status, err);
    });
}

function opLineMappingTabClick() {
    $("#opLineMappingLink, #btnRefresh").on("click", () => {
        const isDisable = document.getElementById(BtnSaveChanges).disabled;

        if (isDisable === false) {
            const cf = confirm("Changes you made may not be saved.!");
            if (cf === true) reloadProcesses();
        } else {
            reloadProcesses();
        }
    });
}

function initDropdownlist(selId, options, onChangedEvent, checkAllEvent) {
    $(selId).multiselect({
        includeSelectAllOption: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: 155,
        maxHeight: 300,
        onChange: (option, checked) => {
            onChangedEvent(option, checked);
        },
        onSelectAll: (isCheck) => {
            checkAllEvent(isCheck);
        }
    });

    $(selId).multiselect('dataprovider', options);
    const selectedOptions = [];
    selectedOptions.push(CurrentLineSerial);
    $(selId).multiselect('select', selectedOptions);
}

function bindDataCbMesLine(data) {
    const ops = [];
    $.each(data, function (key, item) {
        const op = {
            label: item.LineName,
            title: item.BackgroundColor,
            value: item.LineSerial
        };
        ops.push(op);
    });

    initDropdownlist(LayoutCbLineId, ops, (option, checked) => {
        window.HideTableByLine(option[0].value, checked);
    }, (isCheckAll) => {
        window.ShowHideTables(isCheckAll);
    });
}
//#endregion