//#region Variables
var msgSave, msgError;
const ModalTable = $(ModalTableId);
//#endregion

//#region Ready
(() => {
    GetMessageById("001", SystemIdOps, MenuIdOpm, SmsFunction.Add, MessageType.Success, MessageContext.Update,
        (msg) => { msgSave = msg; });
    GetMessageById("002", SystemIdOps, MenuIdAom, SmsFunction.Generic, MessageType.Error, MessageContext.Error,
        (msg) => { msgError = msg; });

    var goJs = go.GraphObject.make;
    var facDiagram;
    let selectedTable;

    function tableStyle() {
        return [{
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
            rotatable: true
        },
        new go.Binding("angle").makeTwoWay(),
        { // what to do when a drag-over or a drag-drop occurs on a Node representing a table
            mouseDragEnter: function (e, node) {
                highlightSeats(node, node.diagram.selection, true);
            },
            mouseDragLeave: function (e, node) {
                highlightSeats(node, node.diagram.selection, false);
            },
            mouseDrop: function (e, node) {
                assignPeopleToSeats(node, node.diagram.selection, e.documentPoint);
            },
            doubleClick: (e, n) => {
                window.SubmitMode = NavMode.Update;
                selectedTable = n.data;
                SetValuesForTableForm(n.data);
                ModalTable.modal('show');
            }
        }];
    }

    ModalTable.on('shown.bs.modal', () => {
        ChangeMode();
    });

    // Create a seat element at a particular alignment relative to the table.
    function facSeat(number, align, focus, bgColor) {
        if (bgColor === null || bgColor === undefined) bgColor = "burlywood";
        if (typeof align === 'string') align = go.Spot.parse(align);
        if (!align || !align.isSpot()) align = go.Spot.Right;
        if (typeof focus === 'string') focus = go.Spot.parse(focus);
        if (!focus || !focus.isSpot()) focus = align.opposite();
        return goJs(go.Panel, "Spot", {
            name: number.toString(),
            alignment: align,
            alignmentFocus: focus
        },
            goJs(go.Shape, TableShape.Square, {
                name: "SEATSHAPE",
                desiredSize: new go.Size(SeatSize, SeatSize),
                fill: bgColor,
                stroke: "white",
                strokeWidth: 2
            },
                new go.Binding("fill")),
            goJs(go.TextBlock, number.toString(), { font: "10pt Verdana, sans-serif" },
                new go.Binding("angle", "angle", function (n) {
                    return -n;
                }))
        );
    }

    function initData() {
        // Initialize the main Diagram
        facDiagram = goJs(go.Diagram, "facDiagramDiv", {
            allowDragOut: true, // to facWorkers
            allowDrop: true, // from facWorkers
            allowClipboard: false,
            initialContentAlignment: go.Spot.Center,
            draggingTool: goJs(specialDraggingTool),
            rotatingTool: goJs(horizontalTextRotatingTool),
            // For this sample, automatically show the state of the diagram's model on the page
            "ModelChanged": function (e) {
                if (e.isTransactionFinished) {
                    //document.getElementById("savedModel").textContent = facDiagram.model.toJson();
                }
            },
            "undoManager.isEnabled": true
        });

        facDiagram.nodeTemplateMap.add("", // default template, for people
            goJs(go.Node, "Auto", {
                background: "transparent"
            }, // in front of all Tables
                // when selected is in foreground layer
                new go.Binding("layerName", "isSelected", function (s) {
                    return s ? "Foreground" : "";
                }).ofObject(), {
                    locationSpot: go.Spot.Center
                },
                new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
                new go.Binding("text", "key"), { // what to do when a drag-over or a drag-drop occurs on a Node representing a table
                    mouseDragEnter: function (e, node) {
                        highlightSeats(node, node.diagram.selection, true);
                    },
                    mouseDragLeave: function (e, node) {
                        highlightSeats(node, node.diagram.selection, false);
                    },
                    mouseDrop: function (e, node) {
                        assignPeopleToSeats(node, node.diagram.selection, e.documentPoint);
                    }
                },
                goJs(go.Shape, "Rectangle", { fill: "blanchedalmond", stroke: null }),
                goJs(go.Panel, "Viewbox", { desiredSize: new go.Size(100, 150) },
                    goJs(go.TextBlock, {
                        margin: 2,
                        desiredSize: new go.Size(100, 150),
                        font: "11pt Verdana, sans-serif",
                        textAlign: "start",
                        stroke: "red"
                    },
                        new go.Binding("text", "", function (data) {
                            var s = data.key;
                            if (data.plus) s += " +" + data.plus.toString();

                            return s;
                        }))
                )
            ));

        // what to do when a drag-drop occurs in the Diagram's background
        facDiagram.mouseDrop = function (e) {
            // when the selection is dropped in the diagram's background,
            // make sure the selected people no longer belong to any table
            e.diagram.selection.each(function (n) {
                if (isPerson(n)) unassignSeat(n.data);
            });

            document.getElementById("btnSaveChanges").disabled = false;
        };

        facDiagram.addDiagramListener("PartRotated", function () {
            document.getElementById("btnSaveChanges").disabled = false;
        });

        // initialize the Palette
        var facWorkers = goJs(go.Diagram, "facWorkerDiv", {
            layout: goJs(go.GridLayout, {
                sorting: go.GridLayout.Ascending // sort by Node.text value
            }),
            allowDragOut: true, // to facDiagram
            allowDrop: true // from facDiagram
        });

        // to simulate a "move" from the Palette, the source Node must be deleted.
        facDiagram.addDiagramListener("ExternalObjectsDropped", function (e) {
            // if any Tables were dropped, don't delete from facWorkers
            if (!e.subject.any(isTable)) {
                facWorkers.commandHandler.deleteSelection();
            }
        });

        // put deleted people back in the facWorkers diagram
        facDiagram.addDiagramListener("SelectionDeleted", function (e) {
            // no-op if deleted by facWorkers' ExternalObjectsDropped listener
            if (facDiagram.disableSelectionDeleted) return;
            // e.subject is the facDiagram.selection collection
            e.subject.each(function (n) {
                if (isPerson(n)) {
                    facWorkers.model.addNodeData(facWorkers.model.copyNodeData(n.data));
                }
            });
        });

        facWorkers.nodeTemplateMap = facDiagram.nodeTemplateMap;

        // specify the contents of the Palette
        facWorkers.model = new go.GraphLinksModel([{ key: "Jon Snow" },
        { key: "Stannis Baratheon" }]);

        facWorkers.model.undoManager = facDiagram.model.undoManager; // shared UndoManager!

        // To simulate a "move" from the Diagram back to the Palette, the source Node must be deleted.
        facWorkers.addDiagramListener("ExternalObjectsDropped", function (e) {
            // e.subject is the facWorkers.selection collection
            // if the user dragged a Table to the facWorkers diagram, cancel the drag
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
    }
    // end init

    function isPerson(n) {
        return n !== null && n !== undefined && n.category === "";
    }

    function isTable(n) {
        return n !== null && n !== undefined && n.category !== "";
    }

    // Highlight the empty and occupied seats at a "Table" Node
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

        var guests = node.data.guests;
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

    // Given a "Table" Node, assign seats for all of the people in the given collection of Nodes;
    // the optional Point argument indicates where the collection of people may have been dropped.
    function assignPeopleToSeats(node, coll, pt) {
        if (isPerson(node)) { // refer to the person's table instead
            node = node.diagram.findNodeForKey(node.data.table);
            if (node === null) return;
        }
        if (coll.any(isTable)) {
            // if dragging a Table, don't allow it to be dropped onto another table
            facDiagram.currentTool.doCancel();
            return;
        }
        // OK -- all Nodes are people, call assignSeat on each person data
        coll.each(function (n) {
            assignSeat(node, n.data, pt);
        });
        positionPeopleAtSeats(node);
    }

    // Given a "Table" Node, assign one guest data to a seat at that table.
    // Also handles cases where the guest represents multiple people, because guest.plus > 0.
    // This tries to assign the unoccupied seat that is closest to the given point in document coordinates.
    function assignSeat(node, guest, pt) {
        if (isPerson(node)) { // refer to the person's table instead
            node = node.diagram.findNodeForKey(node.data.table);
            if (node === null) return;
        }
        if (guest instanceof go.GraphObject) throw Error("A guest object must not be a GraphObject: " + guest.toString());
        if (!(pt instanceof go.Point) && node) pt = node.location;

        // in case the guest used to be assigned to a different seat, perhaps at a different table
        unassignSeat(guest);

        if (node === null || node === undefined) return;

        var model = node.diagram.model;
        var guests = node.data.guests;
        // iterate over all seats in the Node to find one that is not occupied
        var closestseatname = findClosestUnoccupiedSeat(node, pt);
        if (closestseatname) {
            model.setDataProperty(guests, closestseatname, guest.key);
            model.setDataProperty(guest, "table", node.data.key);
            model.setDataProperty(guest, "seat", parseFloat(closestseatname));
        }

        var plus = guest.plus;
        if (plus) { // represents several people
            // forget the "plus" info, since next we create N copies of the node/data
            guest.plus = undefined;
            model.updateTargetBindings(guest);
            for (var i = 0; i < plus; i++) {
                var copy = model.copyNodeData(guest);
                // don't copy the seat assignment of the first person
                copy.table = undefined;
                copy.seat = undefined;
                model.addNodeData(copy);
                assignSeat(node, copy, pt);
            }
        }
    }

    // Declare that the guest represented by the data is no longer assigned to a seat at a table.
    // If the guest had been at a table, the guest is removed from the table's list of guests.
    function unassignSeat(guest) {
        if (guest instanceof go.GraphObject) throw Error("A guest object must not be a GraphObject: " + guest.toString());
        var model = facDiagram.model;
        // remove from any table that the guest is assigned to
        if (guest.table) {
            var table = model.findNodeDataForKey(guest.table);
            if (table) {
                var guests = table.guests;
                if (guests) model.setDataProperty(guests, guest.seat.toString(), undefined);
            }
        }
        model.setDataProperty(guest, "table", undefined);
        model.setDataProperty(guest, "seat", undefined);
    }

    // Find the name of the unoccupied seat that is closest to the given Point.
    // This returns null if no seat is available at this table.
    function findClosestUnoccupiedSeat(node, pt) {
        if (isPerson(node)) { // refer to the person's table instead
            node = node.diagram.findNodeForKey(node.data.table);
            //if (node === null) return;
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

    // Position the nodes of all of the guests that are seated at this table
    // to be at their corresponding seat elements of the given "Table" Node.
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

    // Position a single guest Node to be at the location of the seat to which they are assigned.
    function positionPersonAtSeat(guest) {
        if (guest instanceof go.GraphObject) throw Error("A guest object must not be a GraphObject: " + guest.toString());
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

    // Automatically drag people Nodes along with the table Node at which they are seated.
    function specialDraggingTool() {
        go.DraggingTool.call(this);
        this.isCopyEnabled = false; // don't want to copy people except between Diagrams
    }
    go.Diagram.inherit(specialDraggingTool, go.DraggingTool);

    specialDraggingTool.prototype.computeEffectiveCollection = function (parts) {
        var map = go.DraggingTool.prototype.computeEffectiveCollection.call(this, parts);
        // for each Node representing a table, also drag all of the people seated at that table
        parts.each(function (table) {
            if (isPerson(table)) return; // ignore persons
            // this is a table Node, find all people Nodes using the same table key
            for (var nit = table.diagram.nodes; nit.next();) {
                var n = nit.value;
                if (isPerson(n) && n.data.table === table.data.key) {
                    if (!map.contains(n)) map.add(n, {
                        point: n.location.copy()
                    });
                }
            }
        });

        return map;
    };
    // end SpecialDraggingTool

    // Automatically move seated people as a table is rotated, to keep them in their seats.
    // Note that because people are separate Nodes, rotating a table Node means the people Nodes
    // are not rotated, so their names (TextBlocks) remain horizontal.
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

    window.GetTbspByFactory = function getTables(selectedFactory) {
        facDiagram.clear();

        if (selectedFactory === null || selectedFactory === undefined) return;

        const l = `${UriApiTableSpace}/?factory=${selectedFactory}`;
        $.blockUI();
        $.getJSON(l).done(function (tables) {
            if (tables) {
                tables.forEach((t) => {
                    createNewNode(t.TableId, t.TableName, t.TbLocation, t.Angle, t.VirtualWidth, t.VirtualLength,
                        t.ActualWidth, t.ActualLength, t.SeatTotal, t.SeatType.toString(), t.SeatDistance, t.BackgroundColor,
                        t.Workers, t.LineSerial, t.Rate);
                });
            }
        }).always(() => { $.unblockUI(); });
    };

    function mySqlCreateTable(tb) {
        $.blockUI();
        $.post(UriApiTableSpace, tb).done((t) => {
            if (t === null || t === undefined) {
                MsgInform("Inform", "Could not add the Table space", "error", false, true);
            } else {
                createNewNode(t.TableId, t.TableName, t.TbLocation, t.Angle, t.VirtualWidth, t.VirtualLength,
                    t.ActualWidth, t.ActualLength, t.SeatTotal, t.SeatType.toString(), t.SeatDistance, t.BackgroundColor,
                    t.Workers, t.LineSerial, t.Rate);
                if (LastSelectedFactoryId) getMesLineByFactory(LastSelectedFactoryId);

                ModalTable.modal('hide');
                tb.TableId = t.TableId;

                // Reload list of factories as order to summarize line and table
                const cbCorporation = GetSelectedOptions(CbCorporation)[0];
                getFactoriesByCorporation(UriApiFactory, cbCorporation.value, SelectedDatabase);

                oracleCreateTable(tb);
            }
        }).always(() => { $.unblockUI(); });
    }

    function oracleCreateTable(tb) {
        $.post(UriApiOracleTbsp, tb).done((t) => {
            if (t === null || t === undefined) {
                console.log("Could not add the Table space to PKMES.");
            } else {
                console.log("Added successfully the Table space to PKMES.");
            }
        });
    }

    submitTableForm();

    function submitTableForm() {
        $("#tableForm").submit(function (e) {
            if (LastSelectedFactoryId === null || LastSelectedFactoryId === undefined) {
                MsgInform("Inform", "Please select a Factory", "error", false, true);
                return;
            }
            const txtTotalSeat = document.getElementById("seatTotal");
            const txtSeatDis = document.getElementById("seatDis");
            const txtTbWidth = document.getElementById("tableWidth");
            const txtTbLength = document.getElementById("tableLength");
            const seatType = getSelectedRadioByName("seatType");
            const txtDisplayWidth = document.getElementById("txtDisplayWidth");
            const txtDisplayLength = document.getElementById("txtDisplayLength");

            if (validateNumber(txtTotalSeat) && validateNumber(txtSeatDis) && seatType && seatType.value &&
                validateNumber(txtDisplayWidth) && validateNumber(txtDisplayLength)) {
                const totalSeat = parseInt(txtTotalSeat.value);
                const tbWidth = parseFloat(txtTbWidth.value);
                const tbLength = parseFloat(txtTbLength.value);
                const seatDistance = parseInt(txtSeatDis.value);
                const displayWidth = parseFloat(txtDisplayWidth.value);
                const displayLength = parseFloat(txtDisplayLength.value);
                const tableName = document.getElementById("txtSeletedLine").value;
                const tb = new TableSpace(null, tableName, TbLocation, 0, displayWidth, displayLength, totalSeat,
                    seatType.value, seatDistance, null, null, LastSelectedFactoryId, null, TableTypes.TableOO, tbWidth,
                    tbLength, null);

                switch (SubmitMode) {
                    case NavMode.Create:
                        var selectedLineId = $(LineMesGrid).jqGrid('getGridParam', 'selrow');
                        var selectedLineRow = $(LineMesGrid).jqGrid("getRowData", selectedLineId);
                        var bgColorCode = selectedLineRow.BackgroundColor.substr(51, 7);
                        tb.BackgroundColor = bgColorCode;
                        tb.LineSerial = selectedLineRow.LineSerial;

                        mySqlCreateTable(tb);

                        break;
                    case NavMode.Update:
                        var sl = GetSelectedOptions(CbLineId)[0];
                        var bgColorTable = sl.title;
                        tb.LineSerial = sl.value;
                        tb.BackgroundColor = bgColorTable;
                        tb.TableId = selectedTable.key;
                        tb.Angle = selectedTable.angle;
                        tb.TbLocation = selectedTable.loc;
                        tb.TableName = sl.text;
                        mySqlUpdateTable(tb);
                        break;
                }
            }

            e.preventDefault();
        });
    }

    function createNewNode(tbKey, tbName, tbLocation, tbAngle, virtualWidth, virtualLength, actualWidth, actualLength,
        totalSeat, seatType, seatDistance, bgColorTable, workers, lineSerial, rate) {
        const seatAlign = 35 / virtualWidth;

        facDiagram.nodeTemplateMap.add(TableTypes.TableOO,
            goJs(go.Node, "Spot", tableStyle(),
                goJs(go.Panel, "Spot",
                    goJs(go.Shape, TableShape.Rectangle, {
                        name: "TABLESHAPE",
                        desiredSize: new go.Size(virtualWidth, virtualLength),
                        fill: bgColorTable,
                        stroke: null
                    },
                        new go.Binding("desiredSize", "size", go.Size.parse).makeTwoWay(go.Size.stringify),
                        new go.Binding("fill")),
                    goJs(go.TextBlock, {
                        editable: true,
                        font: "bold 11pt Verdana, sans-serif"
                    },
                        new go.Binding("text", "name").makeTwoWay()
                    )
                ),
                recSeats(totalSeat, seatAlign, seatType, seatDistance, bgColorTable)
            ));

        workers = workers === null || workers === undefined ? {} : workers;

        const table = {
            "key": tbKey, "category": TableTypes.TableOO, "name": tbName, "guests": workers, "loc": tbLocation,
            "angle": tbAngle, "VirtualWidth": virtualWidth, "VirtualLength": virtualLength, "ActualWidth": actualWidth,
            "ActualLength": actualLength, "TotalSeat": totalSeat, "SeatType": seatType, "SeatDistance": seatDistance,
            "BackgroundColor": bgColorTable, "LineSerial": lineSerial, "Rate": rate
        };

        facDiagram.startTransaction("Begin adding table transaction");
        facDiagram.model.addNodeData(table);
        facDiagram.commitTransaction("Commit adding table transaction");
    }

    function deleteTable() {
        const table = facDiagram.selection.first();
        if (table === null || table === undefined) {
            MsgInform("Inform", "Please select a table", "error", false, true);
            return;
        }
        ConfirmYesNo("Confirmation", "Are you sure to delete?", () => {
            const tbId = table.data.key;
            mySqlDeleteTable(tbId);
            oracleDeleteTable(tbId);
        });
    }

    window.HideTableByLine = function hideTableByLine(lineId) {
        for (const it = facDiagram.nodes.iterator; it.next();) {
            facDiagram.startTransaction("Begin visible tables transaction");
            if (it.value.data.LineSerial.toString() !== lineId) {
                it.value.visible = false;
            } else {
                it.value.visible = true;
            }
            facDiagram.commitTransaction("Commit visible tables transaction");
        }
    };

    function mySqlDeleteTable(tableId) {
        $.blockUI();
        $.post('/Factory/MySqlDeleteTable', { tableId: tableId }).done((response) => {
            if (response.error === null || response.error === undefined) {
                const delResult = response.result;
                if (delResult !== "false") {
                    facDiagram.allowDelete = true;
                    facDiagram.commandHandler.deleteSelection();
                    facDiagram.allowDelete = false;

                    if (LastSelectedFactoryId) getMesLineByFactory(LastSelectedFactoryId);

                    // Reload list of factories
                    const cbCorporation = GetSelectedOptions(CbCorporation)[0];
                    getFactoriesByCorporation(UriApiFactory, cbCorporation.value, SelectedDatabase);
                } else {
                    console.log("An error occurred, could not delete the table.");
                }
            } else {
                console.log(response.error);
            }
        }).always(() => { $.unblockUI(); });
    }

    function oracleDeleteTable(tableId) {
        $.post('/Factory/OracleDeleteTable', { tableId: tableId }).done((response) => {
            if (response.error === null || response.error === undefined) {
                const delResult = response.result;
                if (delResult !== "false") {
                    console.log("Delete successfully the table in PKMES.");
                } else {
                    console.log("An error occurred, could not delete the table in PKMES.");
                }
            } else {
                console.log(response.error);
            }
        });
    }

    function updateSeatEvent(ac) {
        const table = facDiagram.selection.first();

        if (table === null || table === undefined) {
            MsgInform("Inform", "Please select a table", "error", false, true);
            return;
        }

        const t = table.data;
        const seatTotal = ac === "add" ? parseInt(t.TotalSeat) + 1 : parseInt(t.TotalSeat) - 1;
        const tbWidth = CalculateTableWidth(seatTotal, t.SeatDistance, t.SeatType);

        const tb = new TableSpace(t.key, t.name, t.loc, t.angle, tbWidth, t.VirtualLength, seatTotal,
            t.SeatType.toString(), t.SeatDistance, t.BackgroundColor, null, null, t.LineSerial, t.category,
            t.ActualWidth, t.ActualLength, t.Rate);

        updateSeat(tb, ac);
    }

    function updateSeat(tb, ac) {
        mySqlUpdateSeat(tb, ac);
    }

    function mySqlUpdateSeat(tb, ac) {
        $.blockUI();
        $.post('/Factory/MySqlUpdateSeat', { table: tb, action: ac }).done((response) => {
            if (response.error === null || response.error === undefined) {
                const addResult = response.result;
                if (addResult === true) {
                    facDiagram.allowDelete = true;
                    facDiagram.commandHandler.deleteSelection();
                    facDiagram.allowDelete = false;

                    createNewNode(tb.TableId, tb.TableName, tb.TbLocation, tb.Angle, tb.VirtualWidth, tb.VirtualLength,
                        tb.ActualWidth, tb.ActualLength, tb.SeatTotal, tb.SeatType.toString(), tb.SeatDistance,
                        tb.BackgroundColor, tb.Workers, tb.LineSerial, tb.Rate);

                    oracleUpdateSeat(tb, ac);
                } else {
                    console.log("An error occurred, could not add a seat to the table.");
                }
            } else {
                console.log(response.error);
            }
        }).always(() => { $.unblockUI(); });
    }

    function oracleUpdateSeat(tb, ac) {
        const ajaxConfig = new AjaxConfig('/Factory/OracleUpdateSeat', true, JSON.stringify({ table: tb, action: ac }));

        AjaxPostCommon(ajaxConfig, (response) => {
            if (response.error === null || response.error === undefined) {
                const addResult = response.result;
                if (addResult === true) {
                    console.log("Updated successfully a seat to the table in PKMES.");
                } else {
                    console.log("An error occurred, could not add a seat to the table in PKMES.");
                }
            } else {
                console.log(response.error);
            }
        });
    }

    function mapNodeToTableSpace(n) {
        if (n) {
            if (n.hasOwnProperty("category")) {
                const tableSpace = new TableSpace(n.key, n.name, n.loc, n.angle, null, null, null, null, null, null,
                    n.guests, null, null, n.category, null, null, null);

                return tableSpace;
            }
            return null;
        }
        return null;
    }

    function saveChanges() {
        const nodeDataArr = JSON.parse(facDiagram.model.toJson()).nodeDataArray;
        const tbsps = [];

        for (let n of nodeDataArr) {
            const tb = mapNodeToTableSpace(n);
            if (tb !== null) tbsps.push(tb);
        }

        mySqlSaveChanges({ tbsps: tbsps });
    }

    function mySqlSaveChanges(tbsps) {
        $.blockUI();
        $.post(MsqlFacSaveChanges, tbsps).done((response) => {
            if (response.error === null || response.error === undefined) {
                if (response === true) {
                    const msg = getMsgByLang(msgSave);
                    MsgInform(msg.title, msg.value, ObjMessageType.Info, false, true);

                    oracleSaveChanges(tbsps);
                } else {
                    const msg = getMsgByLang(msgError);
                    ShowMessage(msg.title, msg.value, ObjMessageType.Error);
                }
            } else {
                console.log(response.error);
            }
        }).always(() => { $.unblockUI(); });
    }

    function oracleSaveChanges(tbsps) {
        $.post(OraFacSaveChanges, tbsps).done((response) => {
            if (response.error === null || response.error === undefined) {
                if (response === true) {
                    console.log("Saved successfully changes in PKMES");
                } else {
                    console.log("Failed saving changes in PKMES");
                }
            } else {
                console.log(response.error);
            }
        });
    }

    function addSeat() {
        updateSeatEvent("add");
    }

    function deleteSeat() {
        updateSeatEvent("del");
    }

    function mySqlUpdateTable(t) {
        $.blockUI();
        $.post('/Factory/MySqlUpdateTable', t).done((response) => {
            if (response.error === null || response.error === undefined) {
                const tb = response.result;
                if (tb !== false) {
                    facDiagram.allowDelete = true;
                    facDiagram.commandHandler.deleteSelection();
                    facDiagram.allowDelete = false;

                    createNewNode(t.TableId, t.TableName, t.TbLocation, t.Angle, t.VirtualWidth, t.VirtualLength,
                        t.ActualWidth, t.ActualLength, t.SeatTotal, t.SeatType.toString(), t.SeatDistance, t.BackgroundColor,
                        t.Workers, t.LineSerial, t.Rate);

                    if (LastSelectedFactoryId) getMesLineByFactory(LastSelectedFactoryId);

                    ModalTable.modal('hide');

                    // Need to update in PKMES (central db)
                    oracleUpdateTable(t);
                } else {
                    console.log("Could not update the table.");
                }
            } else {
                console.log(response.error);
            }
        }).always(() => { $.unblockUI(); });
    }

    function oracleUpdateTable(t) {
        $.post('/Factory/OracleUpdateTable', t).done((response) => {
            if (response.error === null || response.error === undefined) {
                const tb = response.result;
                if (tb !== false) {
                    console.log("Updated succesfully the table to PKMES db.");
                } else {
                    console.log("Could not update the table to PKMES db.");
                }
            } else {
                console.log(response.error);
            }
        });
    }

    document.getElementById("btnDelTable").addEventListener("click", deleteTable);
    document.getElementById("btnSaveChanges").addEventListener("click", saveChanges);
    document.getElementById("btnAddSeat").addEventListener("click", addSeat);
    document.getElementById("btnRemoveSeat").addEventListener("click", deleteSeat);
})();
//#endregion

//#region Functions
function ChangeMode() {
    let titleModal = "";
    let btnValue = "";
    switch (SubmitMode) {
        case NavMode.Create:
            titleModal = "Creating New Table";
            btnValue = "Create";
            document.getElementById("divSelectedLine").style.display = "none";
            document.getElementById("txtSeletedLine").style.display = "block";
            setDefaultValues(1, 8, 5, 550, 60, 50, 192);
            break;
        case NavMode.Update:
            titleModal = "Updating Table";
            btnValue = "Update";
            document.getElementById("txtSeletedLine").style.display = "none";
            document.getElementById("divSelectedLine").style.display = "block";
            break;
    }

    document.getElementById("titleTableModal").innerHTML = titleModal;
    document.getElementById("btnSubmitFormTable").innerHTML = btnValue;
}

function setDefaultValues(seatType, totalSeat, seatDistance, actualWidth, actualLength, virtualLength, virtualWidth) {
    $("input[name=seatType][value=" + seatType + "]").prop('checked', true);
    document.getElementById("seatTotal").value = totalSeat;
    document.getElementById("seatDis").value = seatDistance;
    document.getElementById("tableWidth").value = actualWidth;
    document.getElementById("tableLength").value = actualLength;
    document.getElementById("txtDisplayLength").value = virtualLength;
    document.getElementById("txtDisplayWidth").value = virtualWidth;
}

function SetValuesForTableForm(tb) {
    const sl = GetSelectedOptions(CbLineId)[0];
    const selectedLine = sl === null || sl === undefined ? tb.LineSerial : sl.value;
    $(CbLineId).multiselect('deselect', selectedLine);
    $(CbLineId).multiselect('select', [tb.LineSerial]);
    document.getElementById("txtSeletedLine").value = tb.name;

    setDefaultValues(tb.SeatType, tb.TotalSeat, parseInt(tb.SeatDistance), parseInt(tb.ActualWidth),
        parseInt(tb.ActualLength), parseInt(tb.VirtualLength), parseInt(tb.VirtualWidth));
}

function getMsgByLang(msgObj) {
    const lang = $("#flagSelected").attr("value");
    const msg = GetMsgByLang(msgObj, lang);

    return msg;
}
//#endregion