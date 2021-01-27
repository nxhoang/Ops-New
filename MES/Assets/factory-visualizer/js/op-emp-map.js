var isSyncEmployee = false, opGroupTbEmp, cvEmpPlugin, isCheckAttendance = false, CurrentEmps, firstSelFilter = false,
    CurrentAssignedEmp = [], CurrentRects = [], CurrentTeams;
const btnFilterEmp = "btnFilterEmp",
    divFilterEmp = "divFilterEmp",
    btnOkeFilterEmp = "btnOkeFilterEmp",
    ddlDeptTeam = "ddlDeptTeam",
    ddlCorp = "ddlCorp",
    selEmp = "selEmp",
    selSkill = "selSkill",
    chkAttendEmp = "chkAttendEmp",
    divAttDate = "divAttDate",
    drpFactory = "drpFactory",
    selectedFactorySpan = "select2-drpFactory-container",
    SelSection = "selSection",
    TxtAttendDate = "txtAttendDate",
    BtnSaveOpEmpChanges = "btnOpEmpSaveChanges",
    FacWorkerDivEmp = "facWorkerDivEmp",
    getDeptTeamsByCorpUrl = "/MesLineAllocation/GetDeptTeamByCorpApi",
    getCorpsUrl = "/MesLineAllocation/GetCorpsApi",
    getAttendEmpsUrl = "/MesLineAllocation/GetAttendEmpsApi",
    getOpEmpsUrl = "/MesLineAllocation/GetOpEmps",
    getOpEmpsByTeamsUrl = "/MesLineAllocation/GetOpEmpsByTeams",
    UrlGetAttEmps = "/MesLineAllocation/GetAttEmps",
    UrlGetDeptCodesByFactory = "/MesLineAllocation/GetDeptCodesByFactory",
    UrlBulkInsertDeptTeam = "/MesLineAllocation/BulkInsertDeptTeam",
    UrlBulkInsertAttEmps = "/MesLineAllocation/BulkInsertAttEmp",
    UrlSaveEmpOpChanges = "/MesLineAllocation/SaveOpEmpChanges",
    UrlGetEmpImageUrlConfig = "/MesLineAllocation/GetEmpImageUrlConfig",
    AjaxWaitMes = "<h3>Please wait...</h3>", // blockUI message
    AjaxLoadingImgMes = "<h3>Loading employee images...</h3>", // blockUI message
    bumLoadEmp = "<h3>Loading workers...</h3>",
    empBgColor = "#FAF7F8",
    empBorderColor = "#5588ee",
    empTextColor = "#5588ee",
    prcBorderColor = "#3b3b3b",
    prcTextColor = "#3b3b3b";

(() => {
    opWorkerMapTabClick();

    opGroupTbEmp = $("#divDisplayOpGroupEmp").opGroupTablePlugin({
        btnDisplayTableId: "btnDisplayOpGroupEmp",
        iconDisplayId: "iDopSignEmp",
        opGroupTableDivId: "divOpGroupTableEmp",
        opGroupTbodyTableId: "tBodyDisplayGroupEmp",
        isShowTable: false,
        opGroups: []
    });

    cvEmpPlugin = $("#facWorkerDivEmp").humanMapPlugin({
        CanvasWidth: 3000,
        CanvasHeight: 6000,
        DragOk: false,
        RectArray: [],
        EmpImgDir: "/Assets/jquery.human-task-map/img",
        onRefreshCanvas: (e, isChange) => {
            if (!isChange && CurrentTeams && CurrentTeams.length > 0) reloadAll(CurrentTeams);
        },
        onSaveCanvas: (e, rects) => {
            saveEmpOp(rects);
        }
    });

    //getCorps();
    document.getElementById("searchEmpForm").addEventListener("submit", submitSearchEmpForm);
    //getAttendEmps();

    checkedAttendEvent();

    // Initialize employee dropdown list
    initMultiSelect({
        selId: selEmp,
        buttonWidth: 300,
        buttonHeight: 300
    }, (checked, checkedOpt) => {
        console.log(checkedOpt);

        // Hiding all of employees
        if (!firstSelFilter) {
            cvEmpPlugin[0].getData().map(x => x.Type === "emp" ? x.IsShow = false : x);
            firstSelFilter = true;
        }

        // Then showing checked employees
        const emp = cvEmpPlugin[0].getData().find(x => x.Type === "emp" && x.Data.EmployeeCode === checkedOpt);
        if (emp) emp.IsShow = checked;

        cvEmpPlugin[0].draw();
    }, (isCheck) => {
        const searchInput = document.querySelectorAll(`#divSelEmp input`)[0];

        if (searchInput && searchInput.value && searchInput.value !== "") {
            // Searching
            console.log(searchInput.value);
        } else {
            cvEmpPlugin[0].getData().map(x => x.Type === "emp" ? x.IsShow = isCheck : x);
            cvEmpPlugin[0].draw();
        }
    });

    initMultiSelect({
        selId: selSkill,
        buttonWidth: 190,
        buttonHeight: 300
    }, (checked, selectedOpt) => {
        if (checked) {
            console.log(`selected: ${selectedOpt}`);
        } else {
            console.log(`unselected: ${selectedOpt}`);
        }
    }, (isCheck) => {
        //console.log(isCheck);
    });

    initMultiSelect({
        selId: SelSection,
        buttonWidth: 190,
        buttonHeight: 300
    }, (checked, selectedOpt) => {
        console.log(selectedOpt);

        if ($(`#${SelSection} option:selected`).length >= 6) {
            $(`#${SelSection} option`).each((i, v) => {
                if ($(v).is(':selected')) {
                    console.log(v.value);
                } else {
                    const input = $(`input[value="${v.value}"]`);

                    input.prop('disabled', true);
                    input.parent('li').addClass('disabled');
                }
            });
        }
        else {
            // Enable all checkboxes.
            $(`#${SelSection} option`).each((i, v) => {
                const input = $(`input[value="${v.value}"]`);
                input.prop('disabled', false);
                input.parent('li').addClass('disabled');
            });
        }
    }, (isCheck) => {
        console.log(isCheck);
    }, false);

    formatDateYYYYMMDD();
    attendanceDateOnchanged();
})();

function attendanceDateOnchanged() {
    document.getElementById(TxtAttendDate).addEventListener("change",
        (e) => {
            if (e.currentTarget.value && e.currentTarget.value !== "") {
                if (new Date(e.currentTarget.value) > new Date()) {
                    MsgInform("Error", "Attendance date is not future", "error", false, true);
                    e.currentTarget.value = (new Date()).yyyymmddhyphen();
                }
            }
        });
}

function getDeptCodesByFactory(factories) {
    ///<summary>Getting departments by current factory
    /// factory: current factory that selected at top dropdown list for searching package
    ///</summary >

    $.blockUI({ message: AjaxWaitMes });

    $.post(UrlGetDeptCodesByFactory, { factories }).
        done((res) => {
            if (res.IsSuccess && res.Result) insertOptToSectionSel(res.Result);
        }).fail((xhr, status, err) => {
            HandleException(xhr, status, err);
        }).always(() => {
            $.unblockUI();
        });
}

// Inserting options to section select control
function insertOptToSectionSel(data) {
    const ops = [];
    for (var e of data) {
        const op = {
            label: e.Section,
            title: e.FullName,
            value: e.DeptCode
        };
        ops.push(op);
    }

    $(`#${SelSection}`).multiselect('dataprovider', ops);
}

// Inserting options to employee select control
function insertOptToEmpSel(data) {
    const ops = [];
    for (var e of data) {
        let genderIcon;
        switch (e.Gender) {
            case "Male":
                genderIcon = ' ♂️ M';
                break;
            case "Female":
                genderIcon = ' ♀ F';
                break;
            default:
                genderIcon = '';
                break;
        }

        const op = {
            label: `${e.EmployeeCode}-${e.Name} ${genderIcon}`,
            title: `${e.EmployeeCode}-${e.Name}`,
            value: e.EmployeeCode
        };
        ops.push(op);
    }

    $(`#${selEmp}`).multiselect('dataprovider', ops);
}

// Filtering list of employees by attendance
function checkedAttendEvent() {
    document.getElementById(chkAttendEmp).addEventListener("click", () => {
        isCheckAttendance = this.checked;
        $(`#${divAttDate}`).toggle("slow");
    });
}

function BulkInsertAttendEmp(factory, selectedTeams, attDate, isPresent) {
    ///<summary>
    ///1. Loading list of attendance employees. If there are no data
    ///2. Bulk inserting list of attendance employees from K - API to database 
    ///then loading list of attendance employees</summary >

    $.blockUI({ message: AjaxWaitMes });
    //console.log("Beginning bulk inserting attendance employees.");

    const config = new AjaxConfig(UrlBulkInsertAttEmps, true, JSON.stringify({ factory, selectedTeams, attDate, isPresent }));

    AjaxPostCommon(config, (res) => {
        if (!res.IsSuccess) {
            if (res.ErrorCode === 1) MsgInform("Info", res.Log, "info", false, true);
            console.log(res.Log);
        }
        if (res.IsSuccess && res.Result) {
            insertOptToEmpSel(res.Result);

            getEmpImagePath().done((imgPathRes) => {
                console.log(imgPathRes);
                if (imgPathRes.IsSuccess) {
                    getOp().done((getOpRs) => {
                        if (getOpRs.opdts && getOpRs.opdts.nodes) {
                            const loadImgPromise = [],
                                empRects = createEmpRect(res.Result, loadImgPromise, imgPathRes.Result),
                                prcRects = createPrcRect(getOpRs.opdts, empRects.assignedEmps),
                                rects = prcRects.concat(empRects.rects);

                            // Waiting for loading images then drawing.
                            $.when.apply($, loadImgPromise).done(() => {
                                cvEmpPlugin[0].bindData(rects);
                            });
                        }
                    });
                }
            });
        }
        console.log(res);
    })
        .fail((err) => {
            console.log(`${err.statusText} ${err.status}`);
        })
        .always(() => {
            //console.log("Done beginning bulk inserting attendance employees.");
            $.unblockUI();
        });
}

//function BulkInsertAttendEmp(factory, selectedTeams, attDate, isPresent) {
//    ///<summary>
//    ///1. Loading list of attendance employees. If there are no data
//    ///2. Bulk inserting list of attendance employees from K - API to database 
//    ///then loading list of attendance employees</summary >

//    $.blockUI({ message: AjaxWaitMes });
//    //console.log("Beginning bulk inserting attendance employees.");

//    $.getJSON(UrlBulkInsertAttEmps, JSON.stringify({ factory, selectedTeams, attDate, isPresent })).done((res) => {
//        if (!res.IsSuccess) {
//            if (res.ErrorCode === 1) MsgInform("Info", res.Log, "info", false, true);
//            console.log(res.Log);
//        }
//        if (res.IsSuccess && res.Result) {
//            insertOptToEmpSel(res.Result);
//            const empRects = createEmpRect(res.Result);
//            cvEmpPlugin[0].reloadEmp(empRects.rects);
//            cvEmpPlugin[0].draw();
//        }
//        console.log(res);
//    })
//        .fail((err) => {
//            console.log(`${err.statusText} ${err.status}`);
//        })
//        .always(() => {
//            //console.log("Done beginning bulk inserting attendance employees.");
//            $.unblockUI();
//        });
//}

// Get list of attend employees from K-API.
function getAttendEmps() {
    $.blockUI({ message: AjaxLoadMdMes });

    $.getJSON(getAttendEmpsUrl, {}).done((result) => {
        if (result && result.items) {
            console.log(result.items);
        }
    })
        .fail((err) => {
            console.log(`${err.statusText} ${err.status}`);
        })
        .always(() => {
            $.unblockUI();
        });
}

// Get list of corporations from K-API.
function getCorps() {
    $.getJSON(getCorpsUrl).done((result) => {
        if (result && result.items) {
            const ops = [];
            let isPk2 = false;
            $.each(result.items, (key, item) => {
                if (item && item.FULLNAME && item.FULLNAME.trim() !== "") {
                    const op = {
                        label: item.FULLNAME,
                        title: item.SHORTNAME,
                        value: item.DEPTCODE
                    };
                    ops.push(op);
                    if (item.DEPTCODE === "1002") isPk2 = true;
                }
            });

            $(`#${ddlCorp}`).multiselect('dataprovider', ops);
            if (isPk2) {
                $(`#${ddlCorp}`).multiselect('deselect', result.items[0].DEPTCODE);
                $(`#${ddlCorp}`).multiselect('select', ['1002']);
            }

            const selectedCorp = $(`#${ddlCorp} option:selected`).val();
            //console.log(lastSelected);

            getDeptTeamsByCorp(selectedCorp);
        }
    })
        .fail((err) => {
            console.log(`${err.statusText} ${err.status}`);
        })
        .always((err) => {
            console.log(err);
        });
}

// Get list of departments by corporation from API.
function getDeptTeamsByCorp(corp) {
    $.blockUI({ message: AjaxLoadMdMes });

    $.getJSON(getDeptTeamsByCorpUrl, {
        corp: corp
    }).done((result) => {
        if (result && result.items) {
            const ops = [];
            $.each(result.items, (key, item) => {
                if (item && item.TEAM_NAME && item.TEAM_NAME.trim() !== "") {
                    const op = {
                        label: item.TEAM_NAME,
                        title: item.TEAM_NAME,
                        value: item.TEAM_CODE
                    };
                    ops.push(op);
                }
            });

            $(`#${ddlDeptTeam}`).multiselect('dataprovider', ops);
        }
    })
        .fail((err) => {
            console.log(`${err.statusText} ${err.status}`);
        })
        .always((err) => {
            $.unblockUI();
        });
}

function clickOkSearchEmp() {
    document.getElementById("btnOkeFilterEmp").addEventListener("click",
        (e) => {
            //$("#lineCreatureForm").submit(function (e) {
            e.preventDefault();
            console.log("Searching worker...");
            const selectedCorp = $(`#${ddlDeptTeam} option:selected`).val(),
                selectedDate = document.getElementById(TxtAttendDate).value;

            console.log(selectedCorp);
            console.log(selectedDate);
        });
}

// Submitting search employee form
function submitSearchEmpForm(e) {
    e.preventDefault();

    console.log("Searching employee...");

    const isCheckAttendance = document.getElementById(chkAttendEmp).checked,
        selectedDate = document.getElementById(TxtAttendDate).value.replace(/-/g, ''), // Replacing all of hyphens by nothing
        selectedLines = $(`#${SelSection} option:selected`),
        selectedTeams = [];

    selectedLines.map((i, v) => { selectedTeams.push(v.value); });
    console.log(selectedTeams);

    CurrentTeams = selectedTeams;
    console.log(document.getElementById(TxtAttendDate).value);

    if (isCheckAttendance) {
        // Going to server-side to get data
        const fs = document.getElementById(selectedFactorySpan);

        if (fs && fs.title) {
            const factory = document.getElementById(selectedFactorySpan).title.replace(/-/g, '').split(" ")[1],
                deptCode = document.getElementById(SelSection).value;

            console.log(factory);
            console.log(selectedDate);
            console.log(deptCode);
            // console.log(new Date());

            if (selectedDate) {
                if (selectedTeams.length > 0) {
                    BulkInsertAttendEmp(factory, selectedTeams, selectedDate, true);
                } else {
                    MsgInform("Error", "Please select team.", "error", false, true);
                }
            } else {
                MsgInform("Error", "Please select date.", "error", false, true);
            }
        } else {
            MsgInform("Error", "Please select a factory.", "error", false, true);
        }
    } else {
        //cvEmpPlugin[0].draw();
        //console.log(selectedLines);
        reloadAll(selectedTeams);
    }
}

function getAttendanceEmps(factory, attDate) {
    ///<summary>
    /// 1. Loading attendance employees from API by deptcode and attendance date.
    /// 2. Saving to database
    /// 3. Loading list of attendance employees from database
    ///</summary >

    $.blockUI({ message: "<h3>Please wait...</h3>" });
    return $.post(UrlGetAttEmps, {
        factory,
        attDate
    }).
        done((res) => {
            console.log(res);

            $.unblockUI();
        }).fail((xhr, status, err) => {
            HandleException(xhr, status, err);
        });
}

// Initialize department dropdown list
function initDeptTeamDdl() {
    $(`#${ddlDeptTeam}`).multiselect({
        includeSelectAllOption: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: 180,
        maxHeight: 300,
        onChange: (option, checked) => {
            //onChangedEvent(option, checked);
            console.log(option);
            console.log(checked);
        },
        onSelectAll: (isCheck) => {
            //checkAllEvent(isCheck);
            console.log(isCheck);
        }
    });
}

// Initialize corporation dropdown list
function initMultiSelect(select, onChange, onSelectAll, selectAllOpt) {
    $(`#${select.selId}`).multiselect({
        includeSelectAllOption: selectAllOpt === false ? false : true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: select.buttonWidth,
        maxHeight: select.maxHeight,
        onChange: (option, checked) => {
            onChange(checked, option[0].value);
        },
        onSelectAll: (isCheck) => {
            onSelectAll(isCheck);
        }
    });
}

// Initialize corporation dropdown list
function initCorpDdl() {
    $(`#${ddlCorp}`).multiselect({
        includeSelectAllOption: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: 180,
        maxHeight: 300,
        onChange: (option, checked) => {
            //onChangedEvent(option, checked);
            if (checked) {
                console.log(option[0].value);
                getDeptTeamsByCorp(option[0].value);
            }
        },
        onSelectAll: (isCheck) => {
            //checkAllEvent(isCheck);
            console.log(isCheck);
        }
    });
}

// Getting list of employees from API
function getEmployeesFromApi() {
    console.log("Beginning loading list of employees then inserting to database.");

    const uriLine = `/MesLineAllocation/BulkInsertEmployee`;
    $.getJSON(uriLine).done((result) => {
        console.log(result);

        console.log("Done loading list of employees and inserting to database.");
    })
        .fail((err) => {
            console.log(err);
        })
        .always((err) => {
            console.log(err);
        });
}

function opWorkerMapTabClick() {
    ///<summary>
    /// OP Worker Mapping tab clicking event.
    ///</summary >

    $("#opEmployeeMappingLink").on("click", () => {
        $(".multiselect-container.dropdown-menu").draggable();

        const fs = document.getElementById(selectedFactorySpan);

        if (fs && fs.title) {
            //const factory = document.getElementById(selectedFactorySpan).title.replace(/-/g, '');
            const factory = $("#drpFactory").val();
            console.log(factory);

            getDeptCodesByFactory([factory]);
        } else {
            console.error("No selected factory");
        }

        cvEmpPlugin[0].bindData([]);
        //cvEmpPlugin[0].clearCanvas(); // clearing everything the canvas
    });
}

function getImage(url) {
    return new Promise(function (resolve, reject) {
        const img = new Image();
        img.onload = function () {
            resolve(img);
        };
        img.onerror = function (e) {
            reject(e);
        };
        img.src = url;
    });
}

function imageExists(imageUrl) {
    const http = new XMLHttpRequest();
    http.open('HEAD', imageUrl, false);
    http.send();
    return http.status !== 404;
}

function createEmpRect(emps, loadImgPromise, empImagePath) {
    ///<summary>Creating employee rectangle object
    ///emps: employee array
    ///return an object includes assigned employees and rectangle objects</summary >

    const assignedEmps = [],
        rects = [];

    let x = 30,
        y = 30,
        n = 0,
        r = 1, // row
        c = 3; // column

    for (var i = 0; i < emps.length; i++) {
        // Cutting off if length of name is greater than 17
        if (emps[i].Name.length > 17) emps[i].Name = cutOffEmpName(emps[i].Name);

        console.log(`${empImagePath}/${emps[i].ImageName}`);

        if (emps[i].ImageName && imageExists(`${empImagePath}/${emps[i].ImageName}`)) {

            emps[i].ImageUrl = `${empImagePath}/${emps[i].ImageName}`;

            const userImg = new Image();
            //userImg.crossOrigin = "Anonymous";
            userImg.src = emps[i].ImageUrl;

            console.log(emps[i].ImageUrl);
            //console.log(userImg);

            emps[i].UserImg = userImg;
            const p = getImage(emps[i].ImageUrl);
            console.log(p);

            //console.log(p);
            loadImgPromise.push(p);
        } else {
            emps[i].ImageUrl = "";
        }

        // If employee is assigned to process, insert the employee to array.
        if (emps[i].StyleCode) {
            assignedEmps.push(emps[i]);
        } else {
            // Drawing processes as 3 columns

            //console.log(cvEmpPlugin[0]);

            rects.unshift(cvEmpPlugin[0].createRect("emp", x, y, 100, 120, empBorderColor, empBgColor, empTextColor, false, true, emps[i]));

            // If getting 3 columns, will go to next row.
            if (c * r - 1 === n) { // ex: 3 * 1 - 1 = 2, index i = 2 at the end of row
                r++; // increasing row
                y += 135;
                x = 30;
            } else {
                x += 130;
            }
            n++;
        }
    }

    return {
        assignedEmps,
        rects
    };
}

function createPrcRect(opdts, assignedEmps) {
    ///<summary> Creating process rectangle object for canvas.
    /// opdts: list of operation detail</summary >

    const rects = [];

    //console.log(cvEmpPlugin[0]);

    let x = 500,
        y = 30,
        r = 1, // row
        c = 6; // column

    for (var i = 0; i < opdts.nodes.length; i++) {
        // Looking for employee who is assigned to process.
        const empData = assignedEmps.filter(x => x.OpSerial === opdts.nodes[i].OpSerial.toString());
        if (empData && empData.length > 0 && empData[0]) opdts.nodes[i].Emp = empData[0];

        let bgColor = opdts.nodes[i].DisplayColor ? `#${opdts.nodes[i].DisplayColor.substring(3, 9)}` : "#FAF7F8";
        bgColor = bgColor.length === 4 || bgColor.length === 7 ? bgColor : "#fff";

        if (opdts.nodes[i].XPos === 0 && opdts.nodes[i].YPos === 0) {
            // Drawing processes as 6 columns
            rects.unshift(cvEmpPlugin[0].createRect("prc", x, y, 100, 120, "#3b3b3b", bgColor, "#3b3b3b", false, true, opdts.nodes[i]));

            // If getting 6 columns, will go to next row.
            if (c * r - 1 === i) { // ex: 6 * r - 1 = 5, index i = 5 at the end of row
                r++; // increasing row
                y += 135;
                x = 500;
            } else {
                x += 130;
            }
        } else {
            rects.unshift(cvEmpPlugin[0].createRect("prc", opdts.nodes[i].XPos, opdts.nodes[i].YPos, 100, 120, "#3b3b3b", bgColor, "#3b3b3b", false, true, opdts.nodes[i]));
        }
    }

    return rects;
}

function getEmpImagePath() {
    return $.getJSON(UrlGetEmpImageUrlConfig).done((res) => {
        console.log(res);
    })
        .fail((err) => { console.log(`${err.statusText} ${err.status}`); })
        .always(() => {
            //$.unblockUI();
        });
}

function reloadAll(teams) {
    ///<summary>Reloading data and canvas</summary>

    const opmt = new OpMaster(CurrentOpmt.StyleCode,
        CurrentOpmt.StyleColorSerial,
        CurrentOpmt.StyleSize,
        CurrentOpmt.RevNo,
        CurrentOpmt.OpRevNo),

        d1 = getOp(),

        // By default getting list of employees who are working in production factory and PK2
        d2 = getOpEmps("Production", teams, opmt),
        d3 = getEmpImagePath();

    $.when(d1, d2, d3).done((v1, v2, v3) => {
        console.log(v3);

        // Creating rectangle of employee
        if (v2[1] === "success" && v2[0].IsSuccess === true && v2[0].Result) {
            const loadImgPromise = [];
            const empRects = createEmpRect(v2[0].Result, loadImgPromise, v3[0].Result);
            //const empRects = createEmpRect(v2[0].Result, loadImgPromise, "/Assets/hrm/worker/img/1002");

            // Creating rectangle of process
            if (v1[1] === "success" && v1[0].opdts && v1[0].opdts.nodes) {
                const prcRects = createPrcRect(v1[0].opdts, empRects.assignedEmps),
                    rects = prcRects.concat(empRects.rects);

                $.blockUI({ message: AjaxLoadingImgMes });

                // Waiting for loading images then drawing.
                $.when.apply($, loadImgPromise).done(() => {
                    cvEmpPlugin[0].bindData(rects);
                    $.unblockUI();
                });
            }
        }
    });
}

function cutOffEmpName(name) {
    ///<summary>If name is so long, cutoff it as abbreviation name (ex: TRẦN THỊ BÍCH LÀNH to TRẦN T B LÀNH)
    ///name: employee name</summary >

    const res = name.split(" ");
    let abbName = res[0];
    for (var i = 0; i < res.length; i++) {
        if (i > 0 && res[i].trim() !== "") {
            abbName += " " + res[i].substring(0, 1);
        }
        if (i === res.length - 1) abbName += " " + res[i];
    }
    return abbName;
}

// Getting opmt and list of opdts
function getOp() {
    ///<summary>
    /// Reloading (refreshing) right-canvas.
    ///</summary >

    $.blockUI({ message: "<h3>Loading...</h3>" });
    return $.post(UrlGetOpdts, {
        opsMaster: CurrentOpmt,
        groupMode: CurrentGroupMode,
        languageId: currentLang
    }).
        done((res) => {
            console.log(res);

            if (res && res.opdts) {
                opGroupTbEmp[0].bindOpGroup(res.opdts.groups);
            } else {
                console.log(res);
            }

            $.unblockUI();
        }).fail((xhr, status, err) => {
            HandleException(xhr, status, err);
        });
}

// Get list of attend employees from K-API.
//function getOpEmps(dept, teams, opmt) {
//    $.blockUI({ message: bumLoadEmp });
//    const data = {
//        dept, teams, styleCode: opmt.StyleCode, styleSize: opmt.StyleSize, styleColorSerial: opmt.StyleColorSerial,
//        revNo: opmt.RevNo, opRevNo: opmt.OpRevNo, mxPackage: CurrentMesPackage.MxPackage
//    };

//    return $.getJSON(getOpEmpsUrl, JSON.stringify(data)).done((res) => {
//        if (res.IsSuccess) {
//            // Inserting options to employee select control
//            insertOptToEmpSel(res.Result);
//        } else {
//            console.log(res.Log);
//        }
//    })
//        .fail((err) => {
//            console.log(`${err.statusText} ${err.status}`);
//        })
//        .always(() => {
//            $.unblockUI();
//        });
//}

function getOpEmps(dept, teams, opmt) {
    $.blockUI({ message: bumLoadEmp });
    opmt.MxPackage = CurrentMesPackage.MxPackage;

    const data = { dept, teams, opmt },
        config = new AjaxConfig(getOpEmpsByTeamsUrl, true, JSON.stringify(data));

    return AjaxPostCommon(config, (res) => {
        if (res.IsSuccess) {
            // Inserting options to employee select control
            insertOptToEmpSel(res.Result);
        } else {
            console.log(res.Log);
        }
    }).always(() => {
        $.unblockUI();
    });
}

function toggleSearchEmp() {
    document.getElementById("btnFilterEmp").addEventListener("click",
        () => {
            $(`#${btnOkeFilterEmp}`).toggle();
            //$(`#${divFilterEmp}`).slideToggle("slow", 'easeOutExpo');
            $(`#${divFilterEmp}`).slideToggle("slow", 'easeOutBounce');
        });
}

function opWorkerDivMouseUp() {
    document.getElementById(FacWorkerDivEmp).addEventListener("click", () => {
        if (cvEmpPlugin[0]) document.getElementById(BtnSaveOpEmpChanges).disabled = !cvEmpPlugin[0].isLayoutChange();
    });
}

function opEmpSaveChanges() {
    document.getElementById(BtnSaveOpEmpChanges).addEventListener("click", () => {
        console.log("saving ...");

        //console.log(mapRectToOpdt(cvEmpPlugin[0].getData()));
        saveEmpOp();
    });
}

function saveEmpOp(rects) {
    ///<summary>
    /// Saving list of rectangles inside canvas to db
    ///<param name="rects">List of rectangles</param>
    ///</summary >

    const opdts = mapRectToOpdt(rects),
        config = {
            Message: AjaxSavingMes,
            Url: UrlSaveEmpOpChanges,
            Data: {
                opdts
            }
        };
    AjaxPostShortHand(config, (res) => {
        console.log(res);

        if (res.IsSuccess) {
            MsgInform("Inform", "Saved", "info", false, true);
            document.getElementById(BtnSaveOpEmpChanges).disabled = true;
            cvEmpPlugin[0].setIsChange(false);
        }
    });
}

function mapRectToOpdt(rects) {
    ///<summary>Mapping canvas rectangle object to operation detail</summary>
    ///<param name="rects">rectangles</param>

    const processes = [];

    for (var p of rects) {
        if (p.Type === "prc") {
            //console.log(p);
            const empCode = p.Data.Emp && p.Data.Emp.EmployeeCode ? p.Data.Emp.EmployeeCode : null,
                opdt = new WorkerOpdt(p.Data.StyleCode,
                    p.Data.StyleColorSerial,
                    p.Data.StyleSize,
                    p.Data.RevNo,
                    p.Data.OpRevNo,
                    p.Data.OpSerial,
                    empCode,
                    p.X,
                    p.Y);

            processes.push(opdt);
        }
    }
    return processes;
}
