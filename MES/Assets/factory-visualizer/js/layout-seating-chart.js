//#region Variables
const BtnSaveChanges = "btnSaveChanges";
var CurrentFactory, CurrentProcessViewMode = 2, CurrentLineSerial, isShowModuleModal = false, opGroupTb;
//#endregion

//#region Ready
(() => {
    var goJs = go.GraphObject.make, facDiagram, tlModuleDiagram, facWorkers;

    function tableStyle(tbsp) {
        return [{ background: "transparent" }, { layerName: "Background" }, // behind all Persons
        { locationSpot: go.Spot.Center, locationObjectName: "TABLESHAPE" },
        new go.Binding("location", "loc", go.Point.parse).makeTwoWay(go.Point.stringify),
        { rotatable: false, movable: false },
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
                document.getElementById(BtnSaveChanges).disabled = false;
                console.log("mouseDrop");
            },
            contextMenu: goJs("ContextMenu", goJs("ContextMenuButton", goJs(go.TextBlock, "Add time bar"),
                {
                    click: () => {
                        const tlb = findTimeBarByKey(`tlb${tbsp.TableId}`);

                        if (tlb) {
                            console.log(`Existed a timeline bar of table space (line: ${tbsp.TableName}).`);
                        } else {
                            const line = facDiagram.findNodeForKey(tbsp.TableId),
                                g = findMaxSeatAndCalOptime(line.data.guests);

                            if (g.maxSeat > 0) {
                                let maxStartTime = "07:30";
                                if (line.linksConnected.pb && line.linksConnected.pb.j) {
                                    maxStartTime = findMaxSWTime(line);
                                }

                                addTimeLineBar(tbsp, g.opTime, g.maxSeat, maxStartTime);
                            } else {
                                console.log("This line doesn't have any process.");
                            }
                        }
                    }
                }))
        }];
    }

    //#region Timeline bar
    function saveTimeLineBar(tlb, callBack) {
        $.blockUI({ message: "<h3>Saving timeline bar...</h3>" });
        $.post(FacSaveOpst, { opst: tlb }).done((response) => {
            var result;
            if (response.error === null || response.error === undefined) {
                result = response;
            } else {
                result = -1;
                console.log(response.error);
            }

            callBack(result);
        }).always(() => { $.unblockUI(); });
    }

    function getOpstsByMxPackage(mxPackage, callBack) {
        /// <summary>
        /// Get time line by mxPackage
        /// mxPackage: mes package id
        /// </summary >

        $.blockUI({ message: "<h3>Loading timeline bar...</h3>" });
        $.post(GetOpstsByMxPackageUrl, { mxPackage: mxPackage }).done((response) => {
            if (response.error === null || response.error === undefined) {
                callBack(response);
            } else {
                console.log(response.error);
            }
        }).always(() => { $.unblockUI(); });
    }
    //#endregion Timeline bar

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
        let opName = obj.part.data.guests[number.toString()];
        if (opName === undefined) opName = "";

        bindSeatDetail(number, line, opName);
        $("#seatDetailModal").modal('show');
    }

    function initData() {
        //#region Factory diagram (right canvas)
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
                {
                    // what to do when a drag-over or a drag-drop occurs on a Node representing a table
                    mouseDragEnter: function (e, node) {
                        highlightSeats(node, node.diagram.selection, true);
                    },
                    mouseDragLeave: function (e, node) {
                        highlightSeats(node, node.diagram.selection, false);
                    },
                    mouseDrop: function (e, node) {
                        assignPeopleToSeats(node, node.diagram.selection, e.documentPoint);

                        console.log("mouseDrop...");
                    }
                },
                goJs(go.Shape, "Rectangle",
                    { fill: "white", stroke: "#1a1b1c", strokeWidth: 0.2 },
                    new go.Binding("fill", "DisplayColor", function (p) { // DisplayColor is a property of process
                        let c = "white";
                        if (p && p.length === 9) c = `#${p.substring(3, 9)}`; // Because format of DisplayColor likes this "FF9ACD32", need to substring
                        if (p.length === 7) c = p;

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
                if (isTimeBar(n.part.data)) {
                    document.getElementById(BtnSaveChanges).disabled = false;
                    console.log("Mouse drop time line bar.");
                } else {
                    if (isPerson(n)) unassignSeat(n.data);
                }
            });
        };

        function isTimeBar(n) {
            return n.hasOwnProperty("StartTime");
        }

        facDiagram.addDiagramListener("PartRotated", function (n) {
            if (isTimeBar(n.subject.part.data)) {
                document.getElementById(BtnSaveChanges).disabled = false;
                console.log("Allow time line bar rotating.");
            } else {
                facDiagram.currentTool.doCancel();
            }
        });

        facDiagram.addDiagramListener("LinkDrawn", (e) => {
            const link = e.subject;

            // Set type for link as order to delete once the link is created.
            facDiagram.model.setDataProperty(link.data, "type", FlowType.Table);

            if (e && e.subject && e.subject.fromNode && e.subject.toNode && e.subject.fromNode.data && e.subject.toNode.data) {
                updateTimeByFlow(e.subject.fromNode.data.key, e.subject.toNode.data.key);

                // Saving links to database.
                const opls = new Opls(CurrentMesPackage.MxPackage, e.subject.fromNode.data.key, e.subject.toNode.data.key);
                saveLink(opls, (result) => {
                    if (result) {
                        console.log("Inserted successfully a line-link to database.");
                    } else {
                        console.log("Could not insert a line-link to database.");
                    }
                });
            }
        });

        // initialize the Palette
        facWorkers = goJs(go.Diagram, "facWorkerDiv",
            {
                layout: goJs(go.GridLayout, { sorting: go.GridLayout.Ascending }),// sort by Node.text value
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

                if (ev.subject.part.data.seat) {
                    bindSeatDetail(ev.subject.part.data.seat, tb.data.name, ev.ws.Ob);
                    $("#seatDetailModal").modal('show');
                }
            }
        });

        facWorkers.nodeTemplateMap = facDiagram.nodeTemplateMap;

        window.GetProcesses = function getProcesses(ps) {
            facDiagram.clear();
            facWorkers.clear();
            const mxPackageRowId = $('#tbMesPackage').jqGrid('getGridParam', 'selrow');
            const rowData = $('#tbMesPackage').jqGrid('getRowData', mxPackageRowId);
            CurrentLineSerial = rowData.LineSerial;
            GetTbspByFactory(CurrentFactory, ps);
            const pList = [], links = [];

            for (let pc of window.MesProcesses) {
                pc.key = `p${pc.OpSerial}`;
                if (pc.NextOp && pc.NextOp !== null) {
                    links.push({ from: pc.key, to: `p${pc.NextOp}` });
                }
                if (pc.TableId === 0 && pc.SeatNo === 0) {
                    pList.push(pc);
                }
            }

            //console.log(links);
            // specify the contents of the Palette
            facWorkers.model = new go.GraphLinksModel(pList);
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

        facDiagram.linkTemplate = goJs(go.Link, {
            contextMenu: goJs("ContextMenu", goJs("ContextMenuButton", goJs(go.TextBlock, "Remove flow"),
                {
                    click: (a, b) => {
                        if (b.part && b.part.data && b.part.data.type === FlowType.Table) {
                            const flow = b.part.data;
                            ConfirmYesNo("Confirmation", "Are you sure to delete this flow?", () => {
                                // Deleting line-flow.
                                const opls = new Opls(CurrentMesPackage.MxPackage, flow.from, flow.to);
                                deleteLineFlow(opls, (rs) => {
                                    if (rs) {
                                        MsgInform(msg.title, "The link is deleted.", ObjMessageType.Info, false, true);
                                        const node = facDiagram.findNodeForKey(flow.to);
                                        console.log(node.data);

                                        facDiagram.disableSelectionDeleted = true;
                                        facDiagram.commandHandler.deleteSelection();
                                        facDiagram.disableSelectionDeleted = false;

                                        let maxStartTime = "07:30";
                                        if (node.linksConnected.pb && node.linksConnected.pb.j) {
                                            maxStartTime = findMaxSWTime(node);
                                        }

                                        adjustTimeBarByTime(`tlb${flow.to}`, maxStartTime);
                                    } else {
                                        console.log("Could not delete.");
                                    }
                                });
                            }, () => { console.log("Cancelled deleting line-flow."); });
                        }
                    }
                }))
        }, { routing: go.Link.AvoidsNodes },
            goJs(go.Shape), goJs(go.Shape, { toArrow: "Standard" })
        );

        facWorkers.mouseDrop = (e) => {
            e.diagram.selection.each(function (n) {
                console.log(n.part.data.key);
            });

            for (const it = facWorkers.nodes.iterator; it.next();) {
                console.log(it.value.data.key);
                console.log(it.value.data);
            }
        };
        //#endregion Factory diagram (right canvas)

        //#region Module-Timeline diagram
        tlModuleDiagram = goJs(go.Diagram, "divTlModal", {
            allowDragOut: false, allowDrop: true, allowClipboard: false,
            initialContentAlignment: go.Spot.Center,
            draggingTool: goJs(SpecialDraggingTool),
            rotatingTool: goJs(horizontalTextRotatingTool),
            "undoManager.isEnabled": true
        });

        tlModuleDiagram.nodeTemplate =
            goJs(go.Node, "Auto", goJs(go.Shape, "RoundedRectangle", new go.Binding("fill", "BackgroundColor")),
                new go.Binding("width", "Width"), new go.Binding("height", "Height"),
                new go.Binding("angle", "Angle").makeTwoWay(),
                new go.Binding("location", "Location", go.Point.parse).makeTwoWay(go.Point.stringify),
                {
                    linkValidation: (fromNode) => {
                        const linkIt = fromNode.findLinksOutOf();
                        var isValid = !linkIt.next();

                        return isValid;
                    },
                    contextMenu: goJs("ContextMenu", goJs("ContextMenuButton", goJs(go.TextBlock, "Add time bar"),
                        {
                            click: (a, b) => {
                                const tbm = findMdTimeBarByKey(`tbm${b.part.data.GroupId}`);
                                if (tbm) {
                                    console.log("Existing module-timebar.");
                                } else {
                                    const currentModule = tlModuleDiagram.findNodeForKey(b.part.data.GroupId);
                                    if (currentModule) {
                                        let maxStartTime = "07:30";
                                        if (currentModule.linksConnected && currentModule.linksConnected.pb && currentModule.linksConnected.pb.j) {
                                            maxStartTime = findMdMaxSWTime(currentModule);
                                        }
                                        const endTime = calEndTimeByOpTime(b.part.data.OpTime, maxStartTime),
                                            tlbLoc = `${a.documentPoint.x - 30} ${a.documentPoint.y - 80}`,
                                            tlb = new Opst(`tbm${b.part.data.GroupId}`, 0, null, null, CurrentMesPackage.MxPackage, b.part.data.OpTime,
                                                maxStartTime, endTime, 140, 90, tlbLoc, 0, 120, b.part.data.BackgroundColor, TimelineType.Module, b.part.data.GroupId);

                                        saveTimeLineBar(tlb, (rs) => {
                                            if (rs !== -1) {
                                                tlb.TimeLineId = rs;
                                                tlModuleDiagram.add(insertTimelineBar(tlb));
                                            } else {
                                                console.log("Could not add time line bar.");
                                            }
                                        });
                                    } else {
                                        console.log("Something is wrong. Could not find module (group).");
                                    }
                                }
                            }
                        }))
                },
                goJs(go.TextBlock, { margin: 10 }, new go.Binding("text", "", (data) => {
                    let codeName = data.Name;
                    if (data.Name.length > 18) codeName = `${data.Name.substring(0, 15)}...`;
                    return `${codeName}\n${data.OpTime}`;
                })),
                goJs(go.Shape, { // Outside flow
                    alignment: new go.Spot(1, 0.5), width: 6, height: 6, portId: "tlmFromPortId", fromSpot: go.Spot.Right,
                    fromLinkable: true, fromMaxLinks: 1
                }),
                goJs(go.Shape, { // Into flow
                    alignment: new go.Spot(0, 0.5), width: 6, height: 6, portId: "tlmToPortId", toSpot: go.Spot.Left,
                    toLinkable: true
                }));

        tlModuleDiagram.linkTemplate = goJs(go.Link, {
            contextMenu: goJs("ContextMenu", goJs("ContextMenuButton", goJs(go.TextBlock, "Remove flow"),
                {
                    click: (a, b) => {
                        if (b.part && b.part.data) {
                            const flow = b.part.data;

                            ConfirmYesNo("Confirmation", "Are you sure to delete this flow?", () => {
                                // Updating NextModule as null.
                                const fromNode = tlModuleDiagram.findNodeForKey(flow.from);
                                tlModuleDiagram.model.setDataProperty(fromNode.data, "NextModule", null);

                                // Deleting module-flow.
                                tlModuleDiagram.disableSelectionDeleted = true;
                                tlModuleDiagram.commandHandler.deleteSelection();
                                tlModuleDiagram.disableSelectionDeleted = false;

                                const toNode = tlModuleDiagram.findNodeForKey(flow.to);

                                let maxStartTime = "07:30";
                                if (toNode.linksConnected.pb && toNode.linksConnected.pb.j) {
                                    maxStartTime = findMdMaxSWTime(toNode);
                                }
                                adjustMdTimeBarByTime(`tbm${flow.to}`, maxStartTime);
                            }, () => { console.log("Cancelled deleting module-flow."); });
                        }
                    }
                }))
        }, { routing: go.Link.AvoidsNodes },
            goJs(go.Shape), goJs(go.Shape, { toArrow: "Standard" })
        );

        window.LoadProcessGroup = function (opGroups) {
            const conf = new AjaxShortHandConfig(AjaxLoadMdMes, GetOpsmsByMxPackageUrl,
                { mxPackage: CurrentMesPackage.MxPackage });
            AjaxPostShortHand(conf, (rs) => {
                if (rs && rs.result) {
                    let x = 0, y = 0;

                    // Finding unsaved process groups.
                    const unsavedGroup = opGroups.filter(({ key: keyv }) => rs.result.every(({ GroupId: groupId }) => keyv !== groupId)).
                        map((v) => {
                            const w = v.Width ? v.Width : MdWidth, h = v.Height ? v.Height : MdHeight,
                                a = v.Angle ? v.Angle : MdAngle;
                            let loc;
                            if (v.Location) {
                                loc = v.Location;
                            } else {
                                loc = `${x} ${y}`;
                                x = x + 170;
                            }

                            const opsm = new Opsm(v.SubCode, v.Id, v.CodeName, v.SubCode, CurrentMesPackage.MxPackage,
                                CurrentFactory, v.OpTime, loc, w, h, a, v.BackgroundColor, null);
                            return opsm;
                        });

                    // Saving unsaved groups to database.
                    const sConf = new AjaxShortHandConfig(AjaxSaveMdMes, SaveOpsmsUrl, { opsms: unsavedGroup });
                    AjaxPostShortHand(sConf, (sRs) => {
                        if (sRs.error) {
                            console.log(sRs.error);
                        } else {
                            if (sRs) {
                                console.log("Saved successfully process groups (modules) to database.");
                            } else {
                                console.log("Failed saving process groups.");
                            }
                        }
                    });

                    // Combining unsaved group to opsm.
                    const modules = rs.result.concat(unsavedGroup);

                    // Fetching list of links.
                    const links = rs.result.filter(v => v.NextModule).map((v) => {
                        //const l = { from: v.Id, to: parseInt(v.NextModule) };
                        const l = { from: v.GroupId, to:v.NextModule };

                        return l;
                    });

                    tlModuleDiagram.model = new go.GraphLinksModel(modules, links);
                } else {
                    console.log(rs);
                    console.log("No data");
                }
            });
        };

        tlModuleDiagram.addDiagramListener("LinkDrawn", (e) => {
            if (e && e.subject && e.subject.fromNode && e.subject.toNode && e.subject.fromNode.data && e.subject.toNode.data) {
                updateModuleTimeByFlow(e.subject.fromNode.data.GroupId, e.subject.toNode.data.GroupId);
                tlModuleDiagram.model.setDataProperty(e.subject.fromNode.data, "NextModule", e.subject.toNode.data.GroupId);
            }
        });

        $('#modalTlModule').on('shown.bs.modal', function () {
            tlModuleDiagram.requestUpdate();
        });
        //#endregion Timeline diagram
    }

    function findMaxSWTime(node) {
        ///<summary>Finding maximum of started working time.
        /// node: current line (table space).
        ///</summary >

        let maxStartTime = "07:30";

        // Looping all of connected links to the line (node).
        for (let l of node.linksConnected.pb.j) {
            // Find all of toLink.
            if (l.data.to === node.part.data.key) {
                console.log(l.data);

                // Finding time line bar by from link.
                const tlb = findTimeBarByKey(`tlb${l.data.from}`);
                console.log(tlb);

                // Comparing to current maximum started working time.
                if (tlb && tlb.value && tlb.value.data && compareTime(tlb.value.data.EndTime, maxStartTime)) {
                    maxStartTime = tlb.value.data.EndTime;
                }
            }
        }

        return maxStartTime;
    }

    function findMdMaxSWTime(node) {
        ///<summary>
        /// Finding maximum of started working time.
        /// node: current module (process group).
        ///</summary >

        let maxStartTime = "07:30";

        // Looping all of connected links to the line (node).
        for (let l of node.linksConnected.pb.j) {
            // Find all of toLink.
            if (l.data.to === node.part.data.key) {
                console.log(l.data.from);

                // Finding time line bar by from link.
                const tlb = findMdTimeBarByKey(`tbm${l.data.from}`);

                // Comparing to current maximum started working time.
                if (tlb && tlb.value && tlb.value.data && compareTime(tlb.value.data.EndTime, maxStartTime)) {
                    maxStartTime = tlb.value.data.EndTime;
                }
            }
        }

        return maxStartTime;
    }

    function deleteLineFlow(opls, callBack) {
        $.blockUI({ message: "<h3>Deleting the link...</h3>" });
        $.post(FacDeleteLink, { link: opls }).done((response) => {
            var result;
            if (response.error === null || response.error === undefined) {
                result = response;
            } else {
                result = false;
                console.log(response.error);
            }

            callBack(result);
        }).always(() => { $.unblockUI(); });
    }

    function isPerson(n) {
        if (n && n.data && n.data.from) return false; // this is link

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
            if (n.data.hasOwnProperty("EndTime")) {
                console.log("This is time line bar.");
            } else {
                assignSeat(node, n.data, pt);
            }
        });
        positionPeopleAtSeats(node);
    }

    function findMaxSeatAndCalOptime(guests) {
        ///<summary>Finding maximum seat number and calculating OpTime
        ///guests: list of guests (processes)</summary>

        let maxSeat = 0, opTime = 0;
        for (let k in guests) {
            if (guests.hasOwnProperty(k)) {
                if (parseInt(k) > maxSeat && guests[k]) maxSeat = parseInt(k);
                const guestData = facDiagram.model.findNodeDataForKey(guests[k]);
                if (guestData && guestData.OpTime) opTime += guestData.OpTime;
            }
        }
        return { maxSeat, opTime };
    }

    function findTimeBarByKey(key) {
        for (const it = facDiagram.nodes.iterator; it.next();) {
            if (it.value.data.key === key) {
                return it;
            }
        }
        return null;
    }

    function findMdTimeBarByKey(key) {
        ///<summary>Finding module-timebar by key.</summary>

        for (const it = tlModuleDiagram.nodes.iterator; it.next();) {
            if (it.value.data.key === key) {
                return it;
            }
        }
        return null;
    }

    function adjustTimeBarByTime(key, st) {
        ///<summary>Adjusting start time of time bar by time
        ///key: time bar key (current line), st: start time</summary >

        for (const it = facDiagram.nodes.iterator; it.next();) {
            if (it.value.data.key === key) {
                const endTime = calEndTimeByOpTime(it.value.data.OpTime, st);
                facDiagram.startTransaction("Changing start/end working time");
                it.value.diagram.model.setDataProperty(it.value.data, "StartTime", st);
                it.value.diagram.model.setDataProperty(it.value.data, "EndTime", endTime);
                facDiagram.commitTransaction("Changing start/end working time");
            }
        }
    }

    function adjustMdTimeBarByTime(key, st) {
        ///<summary>
        /// Adjusting start time of module - timebar by time.
        /// key: time bar key (current module).
        /// st: start time.
        ///</summary >

        for (const it = tlModuleDiagram.nodes.iterator; it.next();) {
            if (it.value.data.key === key) {
                const endTime = calEndTimeByOpTime(it.value.data.OpTime, st);
                tlModuleDiagram.startTransaction("Changing start/end working time");
                it.value.diagram.model.setDataProperty(it.value.data, "StartTime", st);
                it.value.diagram.model.setDataProperty(it.value.data, "EndTime", endTime);
                tlModuleDiagram.commitTransaction("Changing start/end working time");
            }
        }
    }

    function scaleTimeBar(node, guest, isAssignSeat) {
        /// <summary>Scale time bar
        /// node: table
        /// guest: process
        /// isAssignSeat: true/false
        ///</summary >

        if (node && node.part && node.part.data && node.part.data.guests) {
            const seatNoOptime = findMaxSeatAndCalOptime(node.part.data.guests);

            if (seatNoOptime) {
                const barLength = calculateTimeBarLength(seatNoOptime.maxSeat, parseInt(node.part.data.SeatType));

                for (const it = facDiagram.nodes.iterator; it.next();) {
                    if (it.value.data.NextOp && it.value.data.NextOp !== "" && it.value.linksConnected.count < 1) {
                        facDiagram.model.startTransaction("Inserting a flow.");
                        const linkdata = { from: it.value.data.key, to: `p${it.value.data.NextOp}`, type: FlowType.Process };
                        facDiagram.model.addLinkData(linkdata);
                        facDiagram.model.commitTransaction("Inserting a flow.");
                    }

                    if (it.value.data.key === `tlb${node.part.data.key}`) {
                        const opTime = isAssignSeat === true ? guest.OpTime + seatNoOptime.opTime :
                            seatNoOptime.opTime - guest.OpTime,
                            st = it.value.data.StartTime,
                            endTime = `${calEndTimeByOpTime(opTime, st)}`;
                        if (seatNoOptime.maxSeat === 0) { // This means there is no process seated to table.
                            console.log(`MaxseatNo: ${seatNoOptime.maxSeat}`);
                            console.log(`BarLength: ${barLength}`);
                        } else {
                            facDiagram.startTransaction("Adjusting time bar");
                            it.value.diagram.model.setDataProperty(it.value.data, "OpTime", opTime);
                            it.value.diagram.model.setDataProperty(it.value.data, "Length", barLength);
                            it.value.diagram.model.setDataProperty(it.value.data, "Position", `${barLength + 5} 0`);
                            it.value.diagram.model.setDataProperty(it.value.data, "EndTime", endTime);
                            facDiagram.commitTransaction("Adjusted time bar");
                        }
                    }
                }
            }
        }
    }

    function addTimeLineBar(tbsp, opTime, maxSeatNo, startTime) {
        const loc = go.Point.parse(tbsp.TbLocation),
            endTime = `${calEndTimeByOpTime(opTime, startTime)}`,
            barLength = calculateTimeBarLength(maxSeatNo, parseInt(tbsp.SeatType));

        if (tbsp.Angle === 0) {
            loc.F = loc.F - 115;
        } else {
            loc.D = loc.D + 115;
        }
        const tlbLoc = `${loc.x} ${loc.y}`;
        const tlb = new Opst(`tlb${tbsp.TableId}`, 0, tbsp.LineSerial, tbsp.TableId, CurrentMesPackage.MxPackage, opTime,
            startTime, endTime, tbsp.VirtualWidth, 90, tlbLoc, tbsp.Angle, barLength, tbsp.BackgroundColor, TimelineType.Line);
        saveTimeLineBar(tlb, (rs) => {
            if (rs !== -1) {
                tlb.TimeLineId = rs;
                facDiagram.add(insertTimelineBar(tlb));
            } else {
                console.log("Could not add time line bar.");
            }
        });
    }

    function assignSeat(node, guest, pt) {
        console.log("assign seat...");

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

                const tlb = findTimeBarByKey(`tlb${node.data.key}`);
                if (tlb) scaleTimeBar(node, guest, true);
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

        document.getElementById(BtnSaveChanges).disabled = false;
    }

    function calculateTimeBarLength(maxSeatNo, seatType) {
        let seatNo = 0;
        switch (seatType) {
            case 1:
            case 2:
                if (maxSeatNo % 2 === 0) {
                    seatNo = maxSeatNo / 2;
                } else {
                    seatNo = (maxSeatNo + 1) / 2;
                }
                break;
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
                seatNo = maxSeatNo;
                break;
            case 9:
            case 10:
            case 11:
            case 12:
                if (maxSeatNo % 2 === 0) {
                    seatNo = maxSeatNo / 2 + 1;
                } else {
                    seatNo = (maxSeatNo + 1) / 2;
                }
                break;
        }
        const barLength = 35 * seatNo + 5 * (seatNo - 1);

        return barLength;
    }

    function calEndTimeByOpTime(opTime, startTime) {
        /// <summary>
        /// Calculate end time by OpTime of process and start working time (format hh:mm or hh:mm:ss)
        /// </summary>

        if (startTime && opTime) {
            const st = startTime.split(":");

            if (st && st.length > 1) {
                const sTemp = opTime + (parseInt(st[0]) * 3600 + parseInt(st[1]) * 60);
                let totalSecond = sTemp;
                if (st.length === 3) { // for hh:mm:ss
                    totalSecond = sTemp + parseInt(st[2]);
                }

                const days = Math.floor(totalSecond / (3600 * 24)),
                    hours = Math.floor((totalSecond - days * 3600 * 24) / 3600),
                    minutes = Math.floor((totalSecond - (days * 3600 * 24 + hours * 3600)) / 60),
                    seconds = totalSecond - (days * 3600 * 24 + hours * 3600 + minutes * 60),
                    h = hours < 10 ? `0${hours}` : hours,
                    m = minutes < 10 ? `0${minutes}` : minutes,
                    s = seconds < 10 ? `0${seconds}` : seconds,
                    t = days > 0 ? `${days}d ${h}:${m}:${s}` : `${h}:${m}:${s}`;

                return t;
            } else {
                console.log(st);
            }
        } else {
            console.log(`StartTime: ${startTime}`);
            console.log(`OpTime: ${opTime}`);
        }

        return "";
    }

    function insertTimelineBar(tlb) {
        /// <summary> Insert time line bar to the canvas.
        /// tlb: time line bar object.
        /// </summary >

        const n = goJs(go.Node, "Auto",
            [
                { locationSpot: go.Spot.Center, location: go.Point.parse(tlb.Location), locationObjectName: "TimeBar" },
                { selectionObjectName: "TimeBar" }, { rotatable: true, movable: true, angle: tlb.Angle },
                { width: tlb.TableWidth, height: tlb.Height },
                {
                    contextMenu: goJs("ContextMenu", goJs("ContextMenuButton", goJs(go.TextBlock, "Remove time bar"),
                        {
                            click: (a, b) => {
                                if (b.part && b.part.data) {
                                    const tlb = b.part.data;
                                    ConfirmYesNo("Confirmation",
                                        "Are you sure to remove?",
                                        () => {
                                            console.log(tlb);
                                            const conf = new AjaxShortHandConfig(AjaxDelMessage, FacDeleteTimeBar, { id: tlb.TimeLineId });
                                            AjaxPostShortHand(conf, (rs) => {
                                                if (rs) {
                                                    if (tlb.Type === TimelineType.Line) {
                                                        facDiagram.disableSelectionDeleted = true;
                                                        facDiagram.commandHandler.deleteSelection();
                                                        facDiagram.disableSelectionDeleted = false;
                                                    } else {
                                                        tlModuleDiagram.disableSelectionDeleted = true;
                                                        tlModuleDiagram.commandHandler.deleteSelection();
                                                        tlModuleDiagram.disableSelectionDeleted = false;
                                                    }
                                                } else {
                                                    console.log("Could not remove time line bar.");
                                                }
                                            });
                                        },
                                        () => { console.log("Cancelled deleting time bar."); });
                                } else {
                                    console.log(b.part);
                                }
                            }
                        }))
                }
            ],
            goJs(go.Panel, "Position", { name: "TimeBar", width: tlb.TableWidth, height: tlb.Height },
                goJs(go.TextBlock, { position: new go.Point(10, 0), editable: false, angle: 270, stroke: tlb.Color, font: "bold 11pt serif" }, {
                    doubleClick: (e, n) => {
                        $("#startTimeModal").modal('show');
                        submitStartTimeForm(n);
                    }
                },
                    new go.Binding("text", "StartTime").makeTwoWay()),
                goJs(go.Shape, "MinusLine", [{
                    position: new go.Point(12, 60), angle: 90, width: 30, height: 10, margin: 0, stroke: tlb.Color
                }]),
                goJs(go.Shape, "MinusLine", {
                    position: new go.Point(17, 72), height: 5, margin: 0, stroke: tlb.Color
                }, new go.Binding("width", "Length", (w) => { return w; })),
                goJs(go.TextBlock, { editable: false, angle: 270, stroke: tlb.Color, font: "bold 11pt serif" },
                    new go.Binding("position", "Position", go.Point.parse).makeTwoWay(go.Point.stringify),
                    new go.Binding("text", "EndTime", (d) => { return d; }).makeTwoWay())));

        const d = {
            "key": tlb.Key, "TimeLineId": tlb.TimeLineId, "LineSerial": tlb.LineSerial, "table": tlb.TableId,
            "StartTime": tlb.StartTime, "EndTime": tlb.EndTime, OpTime: tlb.OpTime, Height: tlb.Height,
            Position: `${tlb.Length + 5} 0`, Angle: tlb.Angle, Length: tlb.Length, Color: tlb.Color,
            TableWidth: tlb.TableWidth, "Type": tlb.Type
        };
        n.data = d; // Assign data to the timeline bar.

        return n;
    }

    function unassignSeat(guest) {
        console.log("unassign seat!!!");

        if (guest) {
            if (guest instanceof go.GraphObject)
                throw Error("A guest object must not be a GraphObject: " + guest.toString());
            const model = facDiagram.model;

            // remove from any table that the guest is assigned to
            if (guest.table) {
                const table = model.findNodeDataForKey(guest.table);
                if (table) {
                    const guests = table.guests;
                    if (guests && guest.seat) model.setDataProperty(guests, guest.seat.toString(), undefined);
                    document.getElementById(BtnSaveChanges).disabled = false;

                    const t = facDiagram.findNodeForKey(guest.table);
                    const tlb = findTimeBarByKey(`tlb${guest.table}`);
                    if (tlb) scaleTimeBar(t, guest, false);
                }
            }
            model.setDataProperty(guest, "table", undefined);
            model.setDataProperty(guest, "seat", undefined);
        }
    }

    function findClosestUnoccupiedSeat(node, pt) {
        if (isPerson(node)) { // refer to the person's table instead
            node = node.diagram.findNodeForKey(node.data.table);
        }

        const guests = node.data.guests;
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
        if (node && node.data) {
            if (isPerson(node)) { // refer to the person's table instead
                node = node.diagram.findNodeForKey(node.data.table);
                if (node === null) return;
            }
            if (node && node.data) {
                const guests = node.data.guests;
                const model = node.diagram.model;
                for (let seatname in guests) {
                    if (guests.hasOwnProperty(seatname)) {
                        const guestkey = guests[seatname];
                        const guestdata = model.findNodeDataForKey(guestkey);
                        positionPersonAtSeat(guestdata);
                    }
                }
            }
        }
    }

    function positionPersonAtSeat(guest) {
        if (guest instanceof go.GraphObject)
            throw Error("A guest object must not be a GraphObject: " + guest.toString());
        if (!guest || !guest.table || !guest.seat) return;
        const table = facDiagram.findPartForKey(guest.table);
        const person = facDiagram.findPartForData(guest);
        if (table && person) {
            const seat = table.findObject(guest.seat.toString());
            const loc = seat.getDocumentPoint(go.Spot.Center);
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
            for (const nit = table.diagram.nodes; nit.next();) {
                const n = nit.value;
                if (n && n.data && n.data.table && isPerson(n) && n.data.table === table.data.key) {
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
        ///<summary>
        ///Hiding tables by selected-line.
        ///isVisible: true/false.
        ///</summary >

        for (const it = facDiagram.nodes.iterator; it.next();) {
            facDiagram.startTransaction("Begin visible tables transaction");
            if (it && it.value && it.value.data && it.value.data.LineSerial.toString() === lineId) {
                it.value.visible = isVisible;
            }
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

    window.ShowHideTables = function showHideTables(isVisible) {
        ///<summary>
        /// Showing/hiding tables.
        /// isVisible: true/false.
        ///</summary >

        for (const it = facDiagram.nodes.iterator; it.next();) {
            facDiagram.startTransaction("Begin visible tables transaction");
            it.value.visible = isVisible;
            facDiagram.commitTransaction("Commit visible tables transaction");
        }
    };

    window.GetTbspByFactory = function getTables(selectedFactory, processes) {
        ///<summary>
        /// Getting table spaces(lines) by factory.
        /// selectedFactory: current factory.
        /// processes: list of processes.
        ///</summary >

        facDiagram.clear();
        facWorkers.clear();
        document.getElementById(BtnSaveChanges).disabled = true;

        if (selectedFactory === null || selectedFactory === undefined) return;

        const getLinesByFacUri = `${UriApiTableSpace}/?factory=${selectedFactory}`;
        const uriLine = `${UriApiLine}/?fac=${selectedFactory}`;
        $.getJSON(uriLine).done((result) => {
            bindDataCbMesLine(result);
        });

        $.getJSON(getLinesByFacUri).done((tables) => {
            if (tables) {
                let maxSeatNo = 0;

                tables.forEach((t, i) => {
                    const tbkey = parseInt(t.TableId), w = {};

                    for (let p of processes) {
                        if (p.TableId === tbkey) {
                            // Fill process name to seat by seat number.
                            w[`${p.SeatNo.toString()}`] = `p${p.OpSerial}`;

                            // Finding max of seat number as order to calculate width of time bar.
                            if (p.SeatNo > maxSeatNo) { maxSeatNo = p.SeatNo; };
                        }
                    }

                    const tbsp = new TableSpace(tbkey, t.TableName, t.TbLocation, t.Angle, t.VirtualWidth, t.VirtualLength,
                        t.SeatTotal, t.SeatType.toString(), t.SeatDistance, t.BackgroundColor, w, CurrentFactory, t.LineSerial);

                    createNewNode(tbsp);

                    if (i === tables.length - 1) {
                        seatProcesses(); // At the end of loop, will seat processes

                        getLinksByMxPackage(CurrentMesPackage.MxPackage, (links) => {
                            facDiagram.model.startTransaction("Inserting a flow.");
                            for (let l of links) {
                                facDiagram.model.addLinkData({ from: l.FromTable, to: l.ToTable, type: FlowType.Table });
                            }
                            facDiagram.model.commitTransaction("Inserting a flow.");
                        });

                        getOpstsByMxPackage(CurrentMesPackage.MxPackage, (rs) => {
                            for (let tlb of rs.result) {
                                if (tlb.LineSerial !== 0 && tlb.TableId !== 0) {
                                    tlb.Type = TimelineType.Line;
                                    tlb.Key = `tlb${tlb.TableId}`;
                                    facDiagram.add(insertTimelineBar(tlb));
                                } else {
                                    tlb.Key = `tbm${tlb.GroupId}`;
                                    tlModuleDiagram.add(insertTimelineBar(tlb));
                                }
                            }

                            // Show tables, processes by current line.
                            window.ToggleTablesByLine(CurrentLineSerial, true);
                        });
                    }
                });
            }
        });
    };

    function getLinksByMxPackage(mxPackage, callBack) {
        ///<summary>
        /// Getting flow-line by MES-package.
        /// mxPackage: mes-package, callBack: callback function.
        ///</summary >

        $.blockUI({ message: "<h3>Loading line-flows...</h3>" });
        $.post(FacGetLinksByMxPackage, { mxPackage: mxPackage }).done((response) => {
            if (response.error === null || response.error === undefined) {
                if (response.result) {
                    callBack(response.result);
                } else {
                    console.log(response.result);
                }
            } else {
                console.log(response.error);
            }
        }).always(() => { $.unblockUI(); });
    }

    function updateTimeByFlow(fromLine, toLine) {
        ///<summary>
        /// Updating timeline by line-flow.
        /// fromLine: from line (source)
        /// toLine: to line (destination)
        ///</summary >

        let fromTimeBar = null, toTimeBar = null;

        // Searching data for lines.
        for (const it = facDiagram.nodes.iterator; it.next();) {
            if (it && it.value && it.value.data && it.value.data.key) {
                if (it.value.data.key.toString() === `tlb${fromLine}`) {
                    //fromTimeBar = it.value.data;
                    fromTimeBar = it.value;
                }
                if (it.value.key.toString() === `tlb${toLine}`) {
                    //toTimeBar = it.value.data;
                    toTimeBar = it.value;
                }
            }
        }

        // Searching line node by searched line data above then updating time.
        if (fromTimeBar && toTimeBar) {
            for (const it = facDiagram.nodes.iterator; it.next();) {
                if (it && it.value && it.value.data) {
                    if (it.value.data.key === toTimeBar.data.key) {
                        if (compareTime(fromTimeBar.data.EndTime, toTimeBar.data.StartTime)) {
                            const endTime = calEndTimeByOpTime(toTimeBar.data.OpTime, fromTimeBar.data.EndTime);
                            facDiagram.startTransaction("Changing start/end working time");
                            it.value.diagram.model.setDataProperty(it.value.data, "StartTime", fromTimeBar.data.EndTime);
                            it.value.diagram.model.setDataProperty(it.value.data, "EndTime", endTime);
                            facDiagram.commitTransaction("Changing start/end working time");
                        }
                    }
                }
            }
        }
    }

    function updateModuleTimeByFlow(fromLine, toLine) {
        ///<summary>
        /// Updating timeline by module-flow.
        /// fromLine: from line (source)
        /// toLine: to line (destination)
        ///</summary >

        let fromTimeBar = null, toTimeBar = null;

        // Searching data for lines.
        for (const it = tlModuleDiagram.nodes.iterator; it.next();) {
            if (it && it.value && it.value.data && it.value.data.key) {
                if (it.value.data.key === `tbm${fromLine}`) {
                    fromTimeBar = it.value;
                }
                if (it.value.key === `tbm${toLine}`) {
                    toTimeBar = it.value;
                }
            }
        }

        // Searching line node by searched line data above then updating time.
        if (fromTimeBar && toTimeBar) {
            for (const it = tlModuleDiagram.nodes.iterator; it.next();) {
                if (it && it.value && it.value.data) {
                    if (it.value.data.key === toTimeBar.data.key) {
                        if (compareTime(fromTimeBar.data.EndTime, toTimeBar.data.StartTime)) {
                            const endTime = calEndTimeByOpTime(toTimeBar.data.OpTime, fromTimeBar.data.EndTime);
                            tlModuleDiagram.startTransaction("Changing start/end working time");
                            it.value.diagram.model.setDataProperty(it.value.data, "StartTime", fromTimeBar.data.EndTime);
                            it.value.diagram.model.setDataProperty(it.value.data, "EndTime", endTime);
                            tlModuleDiagram.commitTransaction("Changing start/end working time");
                        }
                    }
                }
            }
        }
    }

    function compareTime(ft, st) {
        ///<summary>
        ///If ft(first time) is greater than st(second time), return to true. 
        ///Else return to false
        ///</summary >

        const ftt = ft.split(":");
        const stt = st.split(":");

        if (parseInt(ftt[0]) > parseInt(stt[0])) {
            return true;
        } else {
            if (parseInt(ftt[0]) === parseInt(stt[0])) {
                if (parseInt(ftt[1]) > parseInt(stt[1])) {
                    return true;
                } else {
                    if (parseInt(ftt[1]) === parseInt(stt[1])) {
                        if (stt[2]) {
                            if (parseInt(ftt[2]) > parseInt(stt[2])) {
                                return true;
                            } else {
                                return false;
                            }
                        } else {
                            return false;
                        }
                    }
                }
            } else {
                return false;
            }
        }

        return false;
    }

    function createNewNode(tbsp) {
        ///<summary>
        ///Creating a node (table spaces(tbsp) is known as line).
        ///</summary >

        const seatAlign = 35 / tbsp.VirtualWidth;

        facDiagram.nodeTemplateMap.add(TableTypes.TableOO,
            goJs(go.Node, "Spot", tableStyle(tbsp),
                {
                    linkValidation: (fromNode) => {
                        const linkIt = fromNode.findLinksOutOf();
                        var isValid = !linkIt.next();

                        return isValid;
                    }
                },
                goJs(go.Panel, "Spot",
                    goJs(go.Shape, TableShape.Rectangle, {
                        name: "TABLESHAPE",
                        desiredSize: new go.Size(tbsp.VirtualWidth, tbsp.VirtualLength), fill: tbsp.BackgroundColor, stroke: null
                    },
                        new go.Binding("desiredSize", "size", go.Size.parse).makeTwoWay(go.Size.stringify),
                        new go.Binding("fill")),
                    goJs(go.TextBlock, { editable: false, font: "bold 11pt Verdana, sans-serif" },
                        new go.Binding("text", "name").makeTwoWay()),
                    goJs(go.Shape, { // Outside flow
                        alignment: go.Spot.Right, width: 6, height: 6, portId: `pFrom${tbsp.TableId}`, fromSpot: go.Spot.Right,
                        fromLinkable: true, fromMaxLinks: 1
                    }),
                    goJs(go.Shape, { // Into flow
                        alignment: go.Spot.Left, width: 6, height: 6, portId: `pTo${tbsp.TableId}`, toSpot: go.Spot.Left,
                        toLinkable: true
                    })
                ),
                recSeats(tbsp.SeatTotal, seatAlign, tbsp.SeatType, tbsp.SeatDistance, tbsp.BackgroundColor)
            ));

        // Preventing recursive links.
        facDiagram.validCycle = go.Diagram.CycleNotDirected;
        tlModuleDiagram.validCycle = go.Diagram.CycleNotDirected;

        let workers = {};
        if (tbsp.Workers) workers = tbsp.Workers;

        const table = {
            "key": tbsp.TableId, "category": TableTypes.TableOO, "name": tbsp.TableName, "guests": workers,
            "loc": tbsp.TbLocation, "angle": tbsp.Angle, "VirtualWidth": tbsp.VirtualWidth,
            "VirtualLength": tbsp.VirtualLength, "ActualWidth": tbsp.ActualWidth, "ActualLength": tbsp.ActualLength,
            "TotalSeat": tbsp.SeatTotal, "SeatType": tbsp.SeatType, "SeatDistance": tbsp.SeatDistance,
            "BackgroundColor": tbsp.BackgroundColor, "LineSerial": tbsp.LineSerial
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
        ///<summary>
        /// Get list of processes from canvas.
        ///</summary >

        const nodeDataArr = JSON.parse(facDiagram.model.toJson()).nodeDataArray,
            ps = [];

        console.log(nodeDataArr);

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
        ///<summary>
        ///Saves changes including lines, processes and timeline bar.
        ///</summary >

        const ps = getListProcesses(window.MesProcesses);
        mySqlSaveSeats(ps);
    }

    function getTimeBars() {
        ///<summary>
        ///Getting timeline bars from canvas.
        ///</summary >

        const allNodesIt = facDiagram.nodes, opsts = [];
        while (allNodesIt.next()) {
            const n = allNodesIt.value;
            if (n.data.hasOwnProperty("StartTime")) {
                const opst = new Opst(n.data.key, n.data.TimeLineId, n.data.LineSerial, n.data.table,
                    CurrentMesPackage.MxPackage, n.data.OpTime, n.data.StartTime, n.data.EndTime, n.data.TableWidth,
                    n.data.Height, `${n.location.x} ${n.location.y}`, n.angle, n.data.Length, n.data.Color);
                opsts.push(opst);
            }
        }
        return opsts;
    }

    function getModuleTimeBars() {
        ///<summary>
        ///Getting timeline bars from canvas.
        ///</summary >

        const allNodesIt = tlModuleDiagram.nodes, opsts = [];
        while (allNodesIt.next()) {
            const n = allNodesIt.value;
            if (n.data.hasOwnProperty("StartTime")) {
                const opst = new Opst(n.data.key, n.data.TimeLineId, n.data.LineSerial, n.data.table,
                    CurrentMesPackage.MxPackage, n.data.OpTime, n.data.StartTime, n.data.EndTime, n.data.TableWidth,
                    n.data.Height, `${n.location.x} ${n.location.y}`, n.angle, n.data.Length, n.data.Color);
                opsts.push(opst);
            }
        }
        return opsts;
    }

    function saveOpstChanges(opsts) {
        /// <summary>
        /// Saves Opst (timeline bar) changes
        /// opsts: List of operation simulation time
        ///</summary >

        $.blockUI({ message: "<h3>Saving timeline bar...</h3>" });
        $.post(SaveOpstChanges, { opsts: opsts }).done((response) => {
            if (response.error === null || response.error === undefined) {
                document.getElementById(BtnSaveChanges).disabled = true;
                const result = response;

                if (result === true) {
                    console.log("Saved successfully timeline bar.");
                } else {
                    //const msg = getMsgByLang(msgError);
                    //ShowMessage(msg.title, msg.value, ObjMessageType.Error);
                    console.log(ObjMessageType.Error);
                }
            } else {
                console.log(response.error);
            }
        }).always(() => { $.unblockUI(); });
    }

    function mySqlSaveSeats(opdts) {
        ///<summary>
        ///Saves seated processes to mysql database.
        ///opdts: list of operation detail - processes.
        ///</summary >

        $.blockUI({ message: "<h3>Saving placed processes...</h3>" });
        $.post(MsqlFacSaveSeatDetails, { opdts: opdts }).done((response) => {
            if (response.error === null || response.error === undefined) {
                document.getElementById(BtnSaveChanges).disabled = true;
                const result = response;

                if (result === true) {
                    console.log("Saved successfully placed process.");

                    const opsts = getTimeBars();
                    saveOpstChanges(opsts);

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

    function saveLink(link, callBack) {
        ///<summary>
        ///Saves a line-link to database.
        ///link: the new link.
        ///callBack: callback function.
        ///</summary >

        $.blockUI({ message: "<h3>Saving link...</h3>" });
        $.post(FacSaveLink, { link: link }).done((response) => {
            if (response.error === null || response.error === undefined) {
                if (response === true) {
                    callBack(true);
                } else {
                    callBack(false);
                }
            } else {
                console.log(response.error);
                callBack(false);
            }
        }).always(() => { $.unblockUI(); });
    }

    function oracleSaveSeats(opdts) {
        ///<summary>
        ///Saves a seated processes to oracle database.
        ///opdts: list of operation detail-processes.
        ///</summary >

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

    document.getElementById(BtnSaveChanges).addEventListener("click", saveChanges);

    document.getElementById("selOpView").addEventListener("change", (e) => {
        loadPalleteData(e);
    });

    window.SelectedRowPackageGroup = function impersonate(r) {
        CurrentFactory = r.MesFactory;
        document.getElementById("wid-op-layout").classList.add("hide");
    };

    function getOpGroup(opGroups, remainProcesses) {
        ///<summary>
        ///Get operation group by all operation group that was loaded from database and filter by un-seated processes.
        ///opGroups: all operation groups.
        ///remainProcesses: un-seated processes.
        ///</summary >

        const groups = [];
        for (let g of opGroups) {
            g.DisplayColor = g.BackgroundColor;
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
        ///<summary>
        /// Loading processes/process-groups to left-canvas (pallete).
        /// This function for viewing-mode dropdown-list event.
        ///</summary >

        const remainOpdts = JSON.parse(facWorkers.model.toJson()).nodeDataArray;

        facWorkers.clear();
        if (e.currentTarget.value) {
            const processes = [];
            let opGroups, groups;
            switch (e.currentTarget.value) {
                case "1":
                    for (let p of window.MesProcesses) {
                        p.key = `p${p.OpSerial}`;
                        if (!checkProcessBeforeDropping(p)) processes.push(p);
                    }
                    facWorkers.model = new go.GraphLinksModel(processes);
                    break;
                case "2":
                    console.log(window.ProcessGroups);
                    opGroups = getOpGroup(window.ProcessGroups, remainOpdts);
                    groups = sumProcessGroup(opGroups, window.MesProcesses);
                    facWorkers.model = new go.GraphLinksModel(groups);
                    break;
            }
        }
    }

    function sumProcessGroup(groups, processes) {
        ///<summary>
        /// Summarize processes by group.
        /// groups: list of process groups.
        /// processes: list of processes.
        ///</summary >

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
        ///<summary>Seat processes to line and add process-flows.</summary>

        for (let pc of window.MesProcesses) {
            const p = pc;

            facDiagram.model.commit(function (m) {
                if (pc.TableId && pc.SeatNo && pc.TableId !== 0 && pc.SeatNo !== 0) {
                    p.table = pc.TableId;
                    p.seat = pc.SeatNo;

                    if (pc.NextOp && pc.NextOp !== "") {
                        const linkdata = { from: pc.key, to: `p${pc.NextOp}`, type: FlowType.Process };
                        m.addLinkData(linkdata); // adding link to process
                    }

                    m.addNodeData(p);
                    positionPersonAtSeat(p);
                }
            }, "added guest");
        }
    }

    window.onbeforeunload = () => {
        const message = "Important: Please click on 'Save' button to leave this page.";
        const isDisable = document.getElementById(BtnSaveChanges).disabled;

        if (!isDisable) return message;
    };
    opLineMappingTabClick();

    function submitStartTimeForm(n) {
        ///<summary>
        /// Displaying the modal by double click. The modal for adjusting starting working time.
        /// This function catches the submit button of the modal.
        /// Adjust the starting working time and end working time.
        ///</summary >

        $("#startTimeForm").unbind().submit(function (e) {
            e.preventDefault();
            document.getElementById(BtnSaveChanges).disabled = false;

            const fSt = document.getElementById("fInputStartTime"),
                endTime = `${calEndTimeByOpTime(n.part.data.OpTime, fSt.value)}`;

            if (isShowModuleModal) {
                tlModuleDiagram.startTransaction("Changing module-start/end working time");
                n.diagram.model.setDataProperty(n.part.data, "StartTime", fSt.value);
                n.diagram.model.setDataProperty(n.part.data, "EndTime", endTime);
                tlModuleDiagram.commitTransaction("Changing module-start/end working time");
            } else {
                facDiagram.startTransaction("Changing line-start/end working time");
                n.diagram.model.setDataProperty(n.part.data, "StartTime", fSt.value);
                n.diagram.model.setDataProperty(n.part.data, "EndTime", endTime);
                facDiagram.commitTransaction("Changing line-start/end working time");
            }

            $("#startTimeModal").modal('hide');
        });
    }

    //displayOpGroup();

    $("#modalTlModule").on("shown.bs.modal", () => {
        isShowModuleModal = true;
    });

    $("#modalTlModule").on("hidden.bs.modal", () => {
        isShowModuleModal = false;
    });

    document.getElementById("btnToggleFreeze").addEventListener("click", () => {
        $('#iLock').toggleClass("fa-lock-open fa-lock");

        // To freeze the canvas.
        tlModuleDiagram.allowMove = !tlModuleDiagram.allowMove;
        document.getElementById("btnToggleFreeze").addEventListener("click", () => {
            $('#iLock').toggleClass("fa-lock-open fa-lock");

            // To freeze the canvas.
            tlModuleDiagram.allowMove = !tlModuleDiagram.allowMove;
        });
    });

    function getModulesInCanvas() {
        const nodeDataArr = JSON.parse(tlModuleDiagram.model.toJson()).nodeDataArray,
            opsms = [];

        for (let m of nodeDataArr) {
            const opsm = new Opsm(m.GroupId, m.Id, m.Name, m.GroupId, CurrentMesPackage.MxPackage, CurrentFactory, m.OpTime,
                m.Location, m.Width, m.Height, m.Angle, m.BackgroundColor, m.NextModule);
            opsms.push(opsm);
        }

        return opsms;
    }

    document.getElementById("btnSaveTlm").addEventListener("click", () => {
        const modules = getModulesInCanvas(),
            conf = new AjaxShortHandConfig(AjaxSavingMes, SaveOpsmsChanges, { opsms: modules });
        AjaxPostShortHand(conf, (rs) => {
            console.log(rs);
        });

        const opsts = getModuleTimeBars();
        saveOpstChanges(opsts);
    });

    // Initialize op-group table
    opGroupTb = $("#divDisplayOpGroup").opGroupTablePlugin({
        btnDisplayTableId: "btnDisplayOpGroup",
        iconDisplayId: "iDopSign",
        opGroupTableDivId: "divOpGroupTable",
        opGroupTbodyTableId: "tBodyDisplayGroup",
        isShowTable: false,
        opGroups: []
    });
})();
//#endregion

//#region Functions
function bindSeatDetail(seatNo, line, opName) {
    document.getElementById("lbSeatNo").innerHTML = seatNo.toString();
    document.getElementById("lbLine").innerHTML = line;
    document.getElementById("lbProcess").innerHTML = opName;
}

function reloadProcesses() {
    ///<summary>
    /// Reloading (refreshing) right-canvas.
    ///</summary >

    $.blockUI({ message: "<h3>Loading...</h3>" });
    $.post(UrlGetOpdts, { opsMaster: CurrentOpmt, groupMode: CurrentGroupMode, languageId: currentLang }).
        done((res) => {
            if (res && res.opdts) {
                window.MesProcesses = res.opdts.nodes;
                window.GetProcesses(window.MesProcesses);
                window.LoadProcessGroup(res.opdts.groups);

                console.log(res);

                //console.log(opGroupTb);
                opGroupTb[0].bindOpGroup(res.opdts.groups);

            } else {
                console.log(res);
            }

            $.unblockUI();
        }).fail((xhr, status, err) => {
            HandleException(xhr, status, err);
        });
}

function opLineMappingTabClick() {
    ///<summary>
    /// OP Line Mapping tab clicking event.
    ///</summary >

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
    ///<summary>
    /// Initialize dropdown-list.
    /// selId: selection identified.
    /// options: data option.
    /// onChangedEvent: changed event function.
    /// checkAllEvent: checked all event function.
    ///</summary >

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
    ///<summary>
    ///Binding data to mes-line dropdown list.
    ///</summary >

    const ops = [];
    $.each(data, (key, item) => {
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

function displayOpGroup() {
    $('#btnDisplayOpGroup').on("click", () => {
        $('#iDopSign').toggleClass("glyphicon-plus-sign glyphicon-minus-sign");
        $('#divTable').slideToggle("slow");
    });

    $('#btnDisplayOpGroupEmp').on("click", () => {
        $('#iDopSignEmp').toggleClass("glyphicon-plus-sign glyphicon-minus-sign");
        $('#divTableEmp').slideToggle("slow");
    });
}

function addGroupToTable(groups) {
    const trRows = [];
    for (let g of groups) {
        const r = `<tr><td><div style="width: 50px; height: 25px; background-color: ${g.BackgroundColor}"></div></td>` +
            `<td>${g.CodeName}</td><td>${g.OpTime}</td></tr>`;
        trRows.push(r);
    }
    $("#tbodyDisplayGroup").html(trRows);
    $("#tbodyDisplayGroupEmp").html(trRows);
}
//#endregion