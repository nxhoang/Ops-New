
//Event click on package group row.
function eventSelectedRowOnPackageGroupGrid(rowdata) {
    var packageGroup = rowdata.PackageGroup;

    //Reload production package and mes package
    var params = { packageGroup: packageGroup };
    ReloadJqGrid2LoCal(tablePPackageName, params);

    ReloadJqGrid2LoCal(tableMesPackageName, params);

    //Check package group status
    if (rowdata.Status === "G") {
        //If status is going on then disable check list and reset buttons production readiness, 
        //button start package group
        DisableButtonsProductionReadiness(true);

        DisabledButton("btnResetProReadiness", true);
        DisabledButton("btnStartExecution", true);
    } else {
        DisableButtonsProductionReadiness(false);

        DisabledButton("btnResetProReadiness", false);
        DisabledButton("btnStartExecution", false);
    }
}

//function reloadGridPackageGroup(factory, plnStartDate, plnEndDate) {

//    var params = { factory: factory, plnStartDate: plnStartDate, plnEndDate: plnEndDate };
//    ReloadJqGrid2LoCal(tableGroupPackageName, params);
//}

function eventLineDetail() {

    $("#btnAddLineDetail").click(function () {

        //Create object line detail to insert to database.
        let objLineDt = createObjectLineDetail();
        
        //Don't let mxpackage, line serial or production date emtpy
        if (isEmptyOrWhiteSpace(objLineDt.MxPackage) || isEmptyOrWhiteSpace(objLineDt.LineSerial) || isEmptyOrWhiteSpace(objLineDt.ProDate)) {
            ShowMessage("Adding Line Detail", "Please select line serial or production date", ObjMessageType.Success);
            return;
        }

        //Check module and process whether are empty or not
        if (isEmptyOrWhiteSpace(objLineDt.ModuleId) && isEmptyOrWhiteSpace(objLineDt.ProcessGroup)) {
            ShowMessage("Adding Line Detail", "Please select Module or Process Group", ObjMessageType.Success);
            return;
        }

        //Get all rows of data on gridview.
        let allGrdRows = GetAllRowsDataJqGrid("#" + tableLineDetailId);
        //Find current line detail in current gridview
        let isExisting = allGrdRows.find(x => x.LineSerial === parseInt(objLineDt.LineSerial));
        //If line exist then return, don't insert it into DB
        if (!$.isEmptyObject(isExisting)) {
            ShowMessage("Adding Line Detail", "Line existed, please select another line", ObjMessageType.Alert);
            return;
        }

        let config = ObjectConfigAjaxPost("../MesManagement/AddLineDetail", false, JSON.stringify({ lineDt: objLineDt }));
        AjaxPostCommon(config, function (resAdd) {
            if (resAdd.IsSuccess) {
                //Reload line detail gridview
                reloadLineDetail(objLineDt.MxPackage);

                ShowMessage("Adding Line Detail", resAdd.Data, ObjMessageType.Info);
            } else {
                ShowMessage("Adding Line Detail", resAdd.Message, ObjMessageType.Error);
            }
        });
    });

    //Event delete line detail button
    $("#btnDeleteLineDt").click(function () {
        let selLineDt = GetSelectedOneRowData("#" + tableLineDetailId);

        if ($.isEmptyObject(selLineDt)) {
            ShowMessage("Delete Line Detail", "Please select line to delete", ObjMessageType.Alert);
            return;
        }

        ShowConfirmYesNo(
            "Delete Line Detail"
            , "Are you sure to delete line detail ?"
            , function () {
                let config = ObjectConfigAjaxPost("../MesManagement/DeleteLineDetail", false
                    , JSON.stringify({ mxPackage: selLineDt.MxPackage, lineSerial: selLineDt.LineSerial }));
                AjaxPostCommon(config, function (resDel) {
                    if (resDel.IsSuccess) {
                        //Reload line detail gridview
                        reloadLineDetail(selLineDt.MxPackage);

                        ShowMessage("Delete Line Detail", resDel.Data, ObjMessageType.Info);
                    } else {
                        ShowMessage("Delete Line Detail", resDel.Message, ObjMessageType.Error);
                    }
                });
            }
            , function () { }
        );
    });
}

//Event click button check list
function eventClickButtonCheckListReadiness() {
    $("#btnMesOpCheck").click(function () {
        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "Mes Operation Plan"
            , "Are you sure MES operation plan is ready?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.MESOP
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

    $("#btnBOMPatterns").click(function () {
        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "BOM Patterns"
            , "Are you sure BOM Patterns is ready?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.BOMPattern
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

    $("#btnPPMeetingCnf").click(function () {

        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "PP Meeting Confirm"
            , "Are you sure PP Meeting Confirm is readiness?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.PPMeeting
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

    $("#btnCheckMatReadiness").click(function () {
        var MESGPackage = GetSelectedOneRowData('#tbGroupPackage');
        //console.log(MESGPackage);

        var PPackage = $('#tbPPackage').jqGrid('getRowData');
        //console.log(PPackage);

        if (MESGPackage && PPackage.length > 0) {
            // create dynamic element
            $("<a></a>", {
                "data-role": "OpenQCODetail",
                "href": "#",
                "onclick": "OpenQCODetailDialog(this);",
                "data-ulr": '/QCO/QCODetailPop/?Factory=' + PPackage[0].Factory + '&LINENO=' + PPackage[0].LINENO + '&AONO=' + MESGPackage.AONo +
                    '&STYLECODE=' + MESGPackage.StyleCode + '&STYLESIZE=' + MESGPackage.StyleSize + '&STYLECOLORSERIAL=' + MESGPackage.StyleColorSerial + '&REVNO=' + MESGPackage.RevNo +
                    '&PRDPKG=' + PPackage[0].PPackage + '&QCOYEAR=' + PPackage[0].QCOYear + '&QCOWEEKNO=' + PPackage[0].QCOWeekNo,
                "data-target": MESGPackage.PackageGroup
            }).appendTo("body")
                // trigger `click` event on dynamically created element
                .click();

            setTimeout(function () {
                //remove the  dynamic element
                $('body').find('a[data-role="OpenQCODetail"]').remove();
            }, 2500);
        }

        return false;
         
    });

    $("#btnCuttingStatus").click(function () {
        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "Cutting Status"
            , "Are you sure Cutting Status is readiness?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.CuttingStatus
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

    $("#btnTreatments").click(function () {
        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "Treatments"
            , "Are you sure Treatments is readiness?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.Treatment
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

    $("#btnToolReadiness").click(function () {
        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "Tools"
            , "Are you sure Tools is readiness?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.Tools
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

    $("#btnMachineReadiness").click(function () {
        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "Machines"
            , "Are you sure Machines is readiness?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.Machines
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

    $("#btnJigRegistered").click(function () {
        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "JIG"
            , "Are you sure JIG is readiness?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.JIG
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

    $("#btnLineSetup").click(function () {
        var mxPackage = getSelectedMesPackage();
        if (mxPackage === null) {
            return;
        }

        ShowConfirmYesNo(
            "Line Setup"
            , "Are you sure Line Setup is readiness?"
            , function () {
                var mpcl = {
                    MxPackage: mxPackage,
                    CheckListId: ProductionReadiness.LineSetup
                };

                InsertReadinessCheckList(mpcl);
            }
            , function () { }
        );
    });

}

/*Modal: Confirm Material Readiness */
function ModalConfirmMatClick(Ele) {
    let $this = $(Ele);
    if ($this.attr('data-target') == null) {
        return;
    }

    ShowConfirmYesNo(
        "Material Readiness"
        , "Are you sure to Confirm Material Readiness ?"
        , function () {
            var mgcl = {
                PackageGroup: $this.attr('data-target'),
                CheckListId: ProductionReadiness.MaterialReadiness
            };

            InsertReadinessCheckList(mgcl, 'MGCL');
        }
        , function () { }
    );
}

/*Modal: Calculate Material Readiness */
function ModalCalcMatClick(Ele) {
    let $this = $(Ele);

    let ModalForm = $($this.closest('div.modal-content').find('form')); //.serialize();

    var qcoQueue = getFormData(ModalForm);
    console.log(qcoQueue);

    ShowConfirmYesNo(
        "Confirm"
        , "Are you sure to Re-calculate Material Readiness ?"
        , function () {
            RecalculateMaterialReadiness(qcoQueue);
        }
        , function () { }
    );

}

//Get mes package of gridview mes package
function getSelectedMesPackage() {
    //Get selected row mes package
    var objMesPkg = GetSelectedOneRowData(tableMesPackageId);

    if ($.isEmptyObject(objMesPkg)) {
        ShowMessage("Production Readiness", "Please select Mes package", ObjMessageType.Alert);
        return null;
    }

    return objMesPkg.MxPackage;
}

function eventResetStartProductionReadiness() {
    $("#btnResetProReadiness").click(function () {
        var objMesPackage = GetSelectedOneRowData(tableMesPackageId);
        if ($.isEmptyObject(objMesPackage)) {
            ShowMessage("Package Group Status", "Please select MES package", ObjMessageType.Alert);
            return;
        } else {

            if (!$.isEmptyObject(objMesPackage.PlnActStartDate.trim())) {
                ShowMessage("Reset package", "Cannot reset package because package started", ObjMessageType.Alert);
                return;
            }
        }

        ShowConfirmYesNo(
            "Reset Production Readiness"
            , "Do you want to reset production readiness of this package?"
            , function () {
                resetReadinessCheckList(objMesPackage.MxPackage);
            }
            , function () { }
        );
    });

    $("#btnStartExecution").click(function () {

        let objMesPackage = GetSelectedOneRowData(tableMesPackageId);
        let objMpmt = GetSelectedOneRowData(tableGroupPackageId);
        if ($.isEmptyObject(objMesPackage)) {
            ShowMessage("Package Group Status", "Please select MES package", ObjMessageType.Alert);
            return;
        } else {
            let lstReadiness;
            //Get list production readiness
            GetProductionReadinessCheckList(objMesPackage.MxPackage, function (lstRed) {
                lstReadiness = lstRed;
            });

            //Count production readiness if the list is 9 then production is ready for production
            if (lstReadiness.length !== 9) {
                ShowMessage("Production Readiness", "Please check readiness checklist", ObjMessageType.Alert);
                return;
            }

            //Check material readiness
            let listMgcl;
            //Get Material readiness
            GetMaterialReadinessCheckList(objMpmt.PackageGroup, function (resListMgcl) {
                listMgcl = resListMgcl;
            });

            //If list material different with 0 then material is ready
            if (listMgcl.Data.length === 0) {
                ShowMessage("Production Readiness", "Please check material readiness", ObjMessageType.Alert);
                return;
            }
        }

        ShowConfirmYesNo(
            "Start Package"
            , "Do you want to start this package?"
            , function () {
                //Start execution and update status and actual plan date of package group
                UpdateMesStartPlan(objMesPackage.PackageGroup, objMesPackage.SeqNo);
            }
            , function () { }
        );
    });

    $("#btnExcelWorkingProcess").click(function () {

        //Get current mes package
        let objMesPackage = GetSelectedOneRowData(tableMesPackageId);

        let mesPackage = objMesPackage.MxPackage;

        //Check mes package is empty or white space
        if (isEmptyOrWhiteSpace(mesPackage)) {
            ShowMessage("Export working process", "Please select MES package", ObjMessageType.Alert);
            return;
        }

        var config = ObjectConfigAjaxPost(
            "../ExcelMes/ExportWorkingProcess"
            , false
            , JSON.stringify({
                mesPackage: mesPackage
            })
        );

        AjaxPostCommon(config, function (resIns) {
            if (resIns.fileName !== "") {
                //Download excel file if export it successfully
                window.location.href = `../ExcelMes/Download/?file=${resIns.fileName}`;
            } else {
                ShowMessage("Export working process", resIns.errorMessage, ObjMessageType.Error);
            }
        });
    });
}
