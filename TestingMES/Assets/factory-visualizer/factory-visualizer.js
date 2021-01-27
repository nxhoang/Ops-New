//#region Variables

//#endregion Variables

//#region Ready
(() => {
    getCorporations(UriApiCstp);
    initJqGridFactory([]);
    initJqGridAmtopLine([]);
    initJqGridMesLine([]);
    createMesLine();
    GetUserRole();
})();
//#endregion Ready

//#region Functions

//#region Common
function GetUserRole() {
    const getUrConfig = {
        url: "/Factory/GetUserRole",
        async: true
    };

    AjaxGetCommon(getUrConfig, (res) => {
        if (res.FlsRole) {
            $(DivLineAction).show();
            $("#divNav").show();
        } else {
            $(DivLineAction).hide();
            $("#divNav").hide();
        }
    });
}

function getCorporations(uri) {
    $.getJSON(uri).done(function (data) {
        const ops = [];
        $.each(data, function (key, item) {
            const op = {
                label: item.CorpName,
                title: item.CorpName,
                value: item.CorpCode
            };
            ops.push(op);
        });

        initSelect(CbCorporation, ops, (option) => {
            getFactoriesByCorporation(selFactory, $(option).val(), SelectedDatabase);
        });
        if (data && data.length > 0 && data[0].CorpCode) {
            getFactoriesByCorporation(UriApiFactory, data[0].CorpCode, SelectedDatabase);
        } else {
            console.log(data);
        }
    });
}

function GetSelectedOptions(selId) {
    const selectedOptions = $(`${selId} option:selected`);
    const selectedOpArr = [];

    selectedOptions.map((a, item) => {
        selectedOpArr.push(item);
    });

    return selectedOpArr;
}
//#endregion Common

//#region Factory
function initSelect(selId, options, onChange) {
    $(selId).multiselect({
        buttonWidth: '100%',
        enableFiltering: true,
        justVisible: false,
        maxHeight: 300,
        onChange: (option, checked) => {
            onChange(option, checked);
        }
    });

    $(selId).multiselect('dataprovider', options);
}

function initJqGridFactory(flsms) {
    const ajaxGridOption = { contentType: "application/json; charset=utf-8" },
        handleUpdateResponse = function (response, data) {
            const result = response.responseJSON.result;
            const currentDate = new Date();
            data.LastUpdated = currentDate.toLocaleDateString();
            if (result) return [true, "Successful", data];
            return [false, "An error occurred, please contact admin"];
        },
        updateSettings = {
            url: "UpdateFlsm",
            ajaxEditOptions: ajaxGridOption,
            reloadAfterSubmit: false,
            closeAfterEdit: true,
            serializeEditData: function (postData) {
                const flsm = {
                    FactoryId: postData.id,
                    Width: postData.Width,
                    Length: postData.Length
                };
                return JSON.stringify({ flsm: flsm, tenantId: SelectedDatabase });
            },
            afterSubmit: handleUpdateResponse
        };

    $(FactoryGrid).jqGrid({
        data: flsms,
        datatype: "local",
        loadonce: true,
        colNames: ["Factory", "FactoryName", "No of Lines", "Total Tables", "Total Machines", "Premises (W)", "Premises (L)",
            "Total Workers", "Last updated"],
        colModel: [
            {
                name: "FactoryId", index: "FactoryId", sorttype: "string", width: 100, align: "center", key: true, editable: false,
                searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "FactoryName", index: "FactoryName", sorttype: "string", width: 250, editable: false, align: "center",
                searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "NoOfLines", index: "NoOfLines", sorttype: "string", width: 100, editable: false, align: "center",
                editoptions: { maxlength: 50 }, searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "TotalTables", index: "TotalTables", sorttype: "int", width: 100, editable: false, align: "center",
                searchoptions: { sopt: ["eq", "ne", "le", "lt", "gt", "ge"] }
            },
            {
                name: "TotalMachines", index: "TotalMachines", sorttype: "int", width: 110, editable: false, align: "center",
                searchoptions: { sopt: ["eq", "ne", "le", "lt", "gt", "ge"] }
            },
            {
                name: "Width", index: "Width", sorttype: "int", width: 110, editable: true, align: "center",
                searchoptions: { sopt: ["eq", "ne", "le", "lt", "gt", "ge"] }, editoptions: { maxlength: 15 },
                editrules: { required: true, number: true }
            },
            {
                name: "Length", index: "Length", sorttype: "int", width: 110, editable: true, align: "center",
                searchoptions: { sopt: ["eq", "ne", "le", "lt", "gt", "ge"] }, editoptions: { maxlength: 15 },
                editrules: { required: true, number: true }
            },
            {
                name: "TotalWorkers", index: "TotalWorkers", sorttype: "int", width: 110, editable: false, align: "center",
                searchoptions: { sopt: ["eq", "ne", "le", "lt", "gt", "ge"] }
            },
            {
                name: "LastUpdated", index: "LastUpdated", sorttype: "date", formatter: "date", width: 120, editable: false,
                align: "center", searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            }
        ],
        pager: FactoryPager,
        rowNum: 200,
        rowList: [50, 100, 200],
        viewrecords: true,
        gridview: true,
        jsonReader: {
            repeatitems: false
        },
        caption: "Factories",
        width: null,
        shrinkToFit: false,
        height: "160",
        onSelectRow: function (id) {
            window.LastSelectedFactoryId = id;
            getMtopLineByFactory(id);
            getMesLineByFactory(id);

            GetTbspByFactory(id);
            document.getElementById("btnSaveChanges").disabled = true;
        }
    });

    $(FactoryGrid).jqGrid("navGrid", FactoryPager, { edit: true, add: false, del: false, search: true },
        updateSettings, null, null);
}

function getFactoriesByCorporation(uri, corporation, desDatabase) {
    const l = `${uri}/?corporation=${corporation}&tenantId=${desDatabase}`;
    $.getJSON(l).done(function (result) {
        if (result) {
            $(FactoryGrid).jqGrid("clearGridData").jqGrid("setGridParam", { data: result, datatype: "local" }).
                trigger("reloadGrid");
        }
    });
}
//#endregion Factory

//#region Line
function getMtopLineByFactory(factory) {
    $(LineAmtopGrid).closest(".ui-jqgrid").block({
        message: "<h1>Loading...</h1>",
        css: { border: "1px solid #ccc" }
    });
    const l = `${UriApiLine}/?factory=${factory}`;
    $.getJSON(l).done(function (result) {
        if (result) {
            $(LineAmtopGrid).jqGrid("clearGridData").jqGrid("setGridParam", { data: result, datatype: "local" }).
                trigger("reloadGrid");
            $(LineAmtopGrid).closest(".ui-jqgrid").unblock();
        }
    }).fail(function (jqxhr, textStatus, error) {
        switch (jqxhr.status) {
            case 500:
                $(LineAmtopGrid).closest(".ui-jqgrid").block({
                    message: "<h1>Could not connect to AMTOPS.</h1>",
                    css: { border: "1px solid #ccc" }
                });
                break;
            case 401:
                // handle unauthorized
                break;
            default:
                console.log(error);
                break;
        }
    });
}

function getMesLineByFactory(factory) {
    $(LineMesGrid).closest(".ui-jqgrid").block({
        message: "<h1>Loading...</h1>",
        css: { border: "1px solid #ccc" }
    });

    const l = `${UriApiLine}/?fac=${factory}`;
    $.getJSON(l).done(function (result) {
        if (result) {
            $(LineMesGrid).jqGrid("clearGridData").jqGrid("setGridParam", { data: result, datatype: "local" }).
                trigger("reloadGrid");
            $(LineMesGrid).closest(".ui-jqgrid").unblock();
        }

        bindDataCbMesLine(result);
    });
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

    initSelect(CbLineId, ops, () => { });
}

function openCreatureLineModal() {
    document.getElementById("txtLineName").value = "";
    if (LastSelectedFactoryId === null || LastSelectedFactoryId === undefined) {
        MsgInform("Inform", "Please select a Factory", "error", false, true);
        return;
    }
    window.SubmitLineMode = NavMode.Create;
    document.getElementById("titleModalLine").innerHTML = "Creating new Line";
    document.getElementById("txtAmtopLine").value = "";
    document.getElementById("txtLineName").value = "";
    document.getElementById("txtWorkerNo").value = "";
    document.getElementById("bgLineColor").value = "#deb887";
    document.getElementById("btnSubmitLine").innerHTML = "Create";

    $('#createLineModal').modal('show');
}

function updateLineModal() {
    const selectedLineId = $(LineMesGrid).jqGrid('getGridParam', 'selrow');
    if (selectedLineId === null || selectedLineId === undefined) {
        MsgInform("Inform", "Please select a Line", "error", false, true);
        return;
    }
    const lineRow = $(LineMesGrid).jqGrid("getRowData", selectedLineId);
    document.getElementById("txtAmtopLine").value = lineRow.LineNo;
    document.getElementById("txtLineName").value = lineRow.LineName;
    document.getElementById("txtWorkerNo").value = lineRow.LineMan;
    const bgColorCode = lineRow.BackgroundColor.substr(51, 7);
    document.getElementById("bgLineColor").value = bgColorCode;

    window.SubmitLineMode = NavMode.Update;
    document.getElementById("titleModalLine").innerHTML = "Updating a Line";
    document.getElementById("btnSubmitLine").innerHTML = "Update";

    $('#createLineModal').modal('show');
}

function createMesLine() {
    $("#lineCreatureForm").submit(function (e) {
        e.preventDefault();

        const lineName = document.getElementById("txtLineName").value;
        const lineNo = document.getElementById("txtAmtopLine").value;
        const workerNo = document.getElementById("txtWorkerNo").value;
        const lineBgColor = document.getElementById("bgLineColor").value;
        if (LastSelectedFactoryId === null || LastSelectedFactoryId === undefined) return;
        const lineRowId = $(LineAmtopGrid).jqGrid('getGridParam', 'selrow');
        const lineId = $(LineMesGrid).jqGrid('getGridParam', 'selrow');
        const mesLine = $(LineMesGrid).jqGrid("getRowData", lineId);
        const line = new LineEntity(null, lineName, LastSelectedFactoryId, lineNo, null, workerNo, lineBgColor, "1");

        // If updating ifself Line, no need to show message "duplicate Amtop Line".
        const isUpdateSameLine = SubmitLineMode === NavMode.Update && lineNo === mesLine.LineNo ? true : false;

        if (!isUpdateSameLine && isExistedLine(lineNo)) {
            ConfirmYesNo("Inform", "Duplicate Amtop Line. Are you sure to continute?",
                () => { submitLineForm(line, lineRowId, mesLine.LineSerial); }, () => { return; });
        } else {
            submitLineForm(line, lineRowId, mesLine.LineSerial);
        }
    });
}

function submitLineForm(line, lineRowId, lineSerial) {
    switch (SubmitLineMode) {
        case NavMode.Create:
            line.LineSerial = lineRowId;
            if (lineRowId === null || lineRowId === undefined) {
                ConfirmYesNo("Confirmation", "There's no selected Amtop Line. Are you sure to create as new Line?",
                    () => {
                        line.LineNo = null;
                        mySqlInsertLine(line);
                    },
                    () => { return; });
            } else {
                const lineRow = $(LineAmtopGrid).jqGrid("getRowData", lineRowId);
                line.LineNo = lineRow.LineNo;
                mySqlInsertLine(line);
            }
            break;
        case NavMode.Update:
            line.LineSerial = lineSerial;
            mySqlUpdateLine(line);
            break;
    }
}

function isExistedLine(line) {
    if (line.trim() === "") return false;

    const amtopLines = $(LineMesGrid).jqGrid("getCol", "LineNo");
    for (let l of amtopLines) {
        if (l === line) return true;
    }
    return false;
}

function mySqlInsertLine(line) {
    $.blockUI();
    $.post(UriApiLine, line).done((data) => {
        if (data === null || data === undefined) {
            alert("Could not add the Line");
        } else {
            const cbCorporation = GetSelectedOptions(CbCorporation)[0];
            getFactoriesByCorporation(UriApiFactory, cbCorporation.value, SelectedDatabase);
            getMesLineByFactory(line.Factory);
            $('#createLineModal').modal('hide');

            line.LineSerial = data.LineSerial;
            oracleInsertLine(line);
        }
    }).always(() => { $.unblockUI(); });
}

function oracleInsertLine(line) {
    $.post(UriApiOracleLine, line).done((data) => {
        if (data === null || data === undefined) {
            console.log("Could not add the Line to PKMES.");
        } else {
            console.log("Inserted successfully Line to PKMES.");
        }
    });
}

function mySqlUpdateLine(line) {
    $.blockUI();
    $.post("MySqlUpdateLine", line).done((res) => {
        if (res.result) {
            const cbCorporation = GetSelectedOptions(CbCorporation)[0];
            getFactoriesByCorporation(UriApiFactory, cbCorporation.value, SelectedDatabase);
            getMesLineByFactory(line.Factory);
            GetTbspByFactory(line.Factory);
            $('#createLineModal').modal('hide');

            // Need to update line in PKMES db.
            oracleUpdateLine(line);
        }
    }).always(() => { $.unblockUI(); });
}

function oracleUpdateLine(line) {
    $.post("OracleUpdateLine", line).done((res) => {
        if (res.result) {
            console.log("Updated successfully Line to PKMES.");
        }
    });
}

function initJqGridAmtopLine(lines) {
    $(LineAmtopGrid).jqGrid({
        data: lines,
        datatype: "local",
        loadonce: true,
        colNames: ["Line", "Description", "Capacity"],
        colModel: [
            {
                name: "LineNo", index: "LineNo", sorttype: "string", width: 100, align: "center", key: true,
                editable: false, searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "Description", index: "Description", sorttype: "string", width: 100, align: "center",
                editable: false, searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "Capacity", index: "Capacity", sorttype: "int", width: 100, align: "center",
                editable: false, searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            }
        ],
        pager: LineAmtopPager,
        rowNum: 200,
        rowList: [50, 100, 200],
        viewrecords: true,
        gridview: true,
        jsonReader: {
            repeatitems: false
        },
        caption: "AMTOPS Lines",
        width: null,
        shrinkToFit: false,
        height: "200",
        onSelectRow: function (id) {
            let lineName = "";
            if (id) {
                const lineRow = $(LineAmtopGrid).jqGrid("getRowData", id);
                if (lineRow) {
                    lineName = lineRow.LineNo;
                    document.getElementById("txtAmtopLine").value = lineName;
                }
            }
            document.getElementById("txtLineName").value = lineName;
        }
    });
}

function initJqGridMesLine(lines) {
    $(LineMesGrid).jqGrid({
        data: lines,
        datatype: "local",
        loadonce: true,
        colNames: ["LineSerial", "Line", "MTOPS Line", "Workers", "DisplayColor", "No of Tables"],
        colModel: [
            {
                name: "LineSerial", index: "LineSerial", sorttype: "int", width: 100, align: "center", key: true,
                editable: false, searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "LineName", index: "LineName", sorttype: "string", width: 150, align: "center", editable: false,
                searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "LineNo", index: "LineNo", sorttype: "string", width: 150, align: "center", editable: false,
                searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "LineMan", index: "LineMan", sorttype: "int", width: 100, editable: false, align: "center",
                searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            },
            {
                name: "BackgroundColor", index: "BackgroundColor", sorttype: "string", width: 120, editable: false, align: "center",
                searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }, formatter: (cellValue) => {
                    return `<div style="height: 32px; width: 70px; background: ${cellValue}"></div>`;
                }
            },
            {
                name: "TotalTables", index: "TotalTables", sorttype: "int", width: 100, editable: false, align: "center",
                searchoptions: { sopt: ["eq", "bw", "bn", "cn", "nc", "ew", "en"] }
            }
        ],
        pager: LineMesPager,
        rowNum: 200,
        rowList: [50, 100, 200],
        viewrecords: true,
        gridview: true,
        jsonReader: {
            repeatitems: false
        },
        caption: "Lines",
        width: null,
        shrinkToFit: false,
        height: "160",
        onSelectRow: function (id) {
            console.log(id);
            HideTableByLine(id);
        }
    });

    $(LineMesGrid).jqGrid("hideCol", ["LineSerial"]);
}
//#endregion Line

//#endregion Functions