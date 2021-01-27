function eventClickBtnSearchPp() {
    $("#btnSearchMtopPkg").click(function () {

        let isMESChange = isChangingMESPackage();
        if (isMESChange) {
            ShowConfirmYesNo(
                "Save MES package"
                , "Do you want to search production package without saving MES package?"
                , function () {
                    searchAOMTOPProductionPackage();
                }
                , function () { }
            );
        } else {
            searchAOMTOPProductionPackage();
        }
    });
}

function eventClickSearchMESPackage() {
    $("#btnSearchMes").click(function () {

        var arrDateRange = $("#txtDateRangeMes").val().split('-');

        var d1 = arrDateRange[0].trim();
        var d2 = arrDateRange[1].trim();

        var aoNo = $("#txtAoNumber").val();
        var fac = $("#drpFactoryMes").val();
        var buyer = $("#drpBuyerMtop").val();
        var styleInfo = $("#txtStyleInfo").val();

        //Check date range
        if (isEmpty(d1) || isEmpty(d2)) {
            ShowMessage("Search style", "Please select date range", ObjMessageType.Info);
            return;
        }

        if (isEmpty(fac)) {
            ShowMessage("Search style", "Please select factory", ObjMessageType.Info);
            return;
        }

        if (isEmpty(buyer)) {
            ShowMessage("Search style", "Please select buyer", ObjMessageType.Info);
            return;
        }

        let isMESChange = isChangingMESPackage();
        if (isMESChange) {
            ShowConfirmYesNo(
                "Search MES package"
                , "Do you want to search MES package without saving?"
                , function () {
                    searchAOMTOPProductionPackage();
                }
                , function () { }
            );
        } else {
            loadMESPackage();
        }

    });
}

/**
 * Search AOMTOP production package
 */
function searchAOMTOPProductionPackage() {
    var arrDateRange = $("#txtDateRangePp").val().split('-');

    var d1 = arrDateRange[0].trim();
    var d2 = arrDateRange[1].trim();

    $("#txtStartDate").val(d1);

    var aoNo = $("#txtAoNumber").val();
    var fac = $("#drpFactoryMtop").val();
    var buyer = $("#drpBuyerMtop").val();
    var styleInfo = $("#txtStyleInfo").val();

    //Check date range
    if (isEmpty(d1) || isEmpty(d2)) {
        ShowMessage("Search style", "Please select date range", ObjMessageType.Info);
        return;
    }

    if (isEmpty(fac)) {
        ShowMessage("Search style", "Please select factory", ObjMessageType.Info);
        return;
    }

    if (isEmpty(buyer)) {
        ShowMessage("Search style", "Please select buyer", ObjMessageType.Info);
        return;
    }

    loadProductionAndMesScheduler();
}

function createMESPackageByQCO() {
    if (!checkDataCreateMESPackage()) return;

    let lineNo = $("#drpLineNo").val();
    let dailyTarQty = parseInt($("#txtDailyTarQty").val());
    let numLines = lineNo.length;
    let factoryMes = $("#drpFactoryMes").val();
    let totalQty = parseInt($("#txtTotalQty").val());

    //Check production package whether available or not before schedule
    let selPrdPkg = $("#hdSelectedPrdPkg").text();
    if (!isProductionPackageAvailable(factoryMes, selPrdPkg)) {
        ShowMessage("Create MES package", "This production package was scheduled.", ObjMessageType.Info);

        return false;
    }

    //Get information of the first selected events
    let objEvent = SELECTEDROWQCO[0];
    let styleInf = objEvent.StyleInf;

    //Get production package
    let proPackageId = SELECTEDROWQCO[0].PrdPkg;

    //CREATE PACKAGE GROUP.
    //Get current year and month
    let d = new Date();
    let month = (d.getMonth() + 1).toString();
    let mm = month.length === 1 ? "0" + month : month;
    let yy = d.getFullYear().toString();
    let yymm = yy.substr(2, 2) + mm;

    let newPkgGroup;

    //Keep mes packge serial
    let mesSerial = 0;
    let seqMes = 0;

    //IF DISTRIBUTION OLD PRODUCTION PACKAGE THEN NO NEED TO CREATE NEW PACKAGE GROUP AND PRODUCTION PACKAGE GROUP
    let isDisNewPPPackage = true;
    let oldPPkgRemainQty = 0;
    //Checking production package to create new group package or not
    $.each(SELECTEDROWQCO, function (idx, selEvn) {
        let pkgGroup = LIST_PP_PACKAGE_GROUP.filter(x => x.PPackage === selEvn.PrdPkg); //.map(x => x.PackageGroup);

        if (pkgGroup.length !== 0) {
            isDisNewPPPackage = false;
            newPkgGroup = pkgGroup[0].PackageGroup;

            //Get information of production package group in list
            let ppGroup = LIST_PACKAGE_GROUP.filter(x => x.PackageGroup === newPkgGroup);
            oldPPkgRemainQty = ppGroup.length === 0 ? oldPPkgRemainQty : ppGroup[0].RemainQty;
            return false;
        }
    });

    //If do not exist production package group then create new package group
    if (isDisNewPPPackage) {
        let listNewPkgGroup = LIST_PACKAGE_GROUP.filter(x => x.IsNew === "Y");
        if (listNewPkgGroup.length === 0) {
            //Create new Package group
            newPkgGroup = GetMaxPackageGroup(factoryMes, yymm);

            //Get number of sequence package group.
            CUR_PACKAGE_SEQ = parseInt(newPkgGroup.substr(10, 9));
        } else {
            CUR_PACKAGE_SEQ++;
            newPkgGroup = factoryMes + "-" + yymm + "-" + ZeroPad(CUR_PACKAGE_SEQ, 9);

        }
    }

    //Get max MES serial in package group
    let pkgGroup = LIST_PACKAGE_GROUP.filter(x => x.PackageGroup === newPkgGroup);
    if (pkgGroup.length !== 0) {

        //START MOD) SON (2019.09.17) - 17 September 2019 - check maximum mes package sequence
        let listMesSeq = LIST_MES_SEQ.filter(x => x.PackageGroup === newPkgGroup);
        if (listMesSeq.length !== 0) {
            //Get min Seq at the first position
            let maxSeq = listMesSeq[0].Seq;
            //Find maximum mes sequence in the list
            $.each(listMesSeq, function (idx, mesSeq) {
                if (maxSeq < mesSeq.Seq) maxSeq = mesSeq.Seq;
            });
            //Set max MES sequence
            seqMes = maxSeq;
        } else {
            seqMes = pkgGroup[0].MaxSeq;
        }

        //seqMes = pkgGroup[0].MaxSeq;
        //END MOD) SON (2019.09.17) - 17 September 2019 
    } else {
        //mesSerial = 0;

        seqMes = 0;
    }

    let plnQty = 0;

    //Check selected event are same style or not        
    let totalPlnQty = 0;
    let seqPP = 0;
    //let isSameStyle = true;
    $.each(SELECTEDROWQCO, function (ind, ppInf) {

        if (isDisNewPPPackage) {
            //Sum plan quantity of selected events
            totalPlnQty += ppInf.PlanQty;

            //Add production package group
            seqPP++;
            let objProPPkgGroup = {
                PackageGroup: newPkgGroup,
                SeqNo: seqPP,
                PPackage: ppInf.PrdPkg,
                Factory: ppInf.Factory,
                AONo: ppInf.AoNo,
                OrdQty: ppInf.OrdQty,
                PlanQty: ppInf.PlanQty,
                IsNew: "Y"
            };
            //Add production package into package group
            LIST_PP_PACKAGE_GROUP.push(objProPPkgGroup);
        }
    });

    plnQty = isDisNewPPPackage ? totalPlnQty : oldPPkgRemainQty;

    //let startDateExe = "2015-07-10"; 
    let startDateExe = $("#txtStartDate").val().replace(/\//g, "-");

    let id = objEvent.id;
    let startDate = startDateExe;
    let endDate = startDateExe;
    let aoNo = objEvent.AoNo;
    let styleCode = objEvent.StyleCode;
    let styleSize = objEvent.StyleSize;
    let styleColorSerial = objEvent.StyleColorSerial;
    let revNo = objEvent.RevNo;

    let bgColor = objEvent.backColor;

    //Maximum the line can be distributed
    let maxLineDis = Math.ceil(plnQty / dailyTarQty);

    if (maxLineDis < numLines) {
        ShowMessage("Create MES package", "Please reduce distribution line.", ObjMessageType.Info);
        return;
    }

    //Distribute mes packages
    let remainQty = 0; //Remain quantity
    let totalDis = 0; //Total distribution
    let curPlan = plnQty < dailyTarQty ? plnQty : dailyTarQty;

    //START MOD) SON (2019.09.17) - 17 September 2019 - check the number day of distribution
    //If create all is not checked then day distribution is 1
    let dayDis = $("#chkCreateAll").is(":checked") === false ? 1 : Math.ceil(totalQty / dailyTarQty);

    //Get the number of days to distribute MES package
    //let dayDis = Math.ceil(totalQty / dailyTarQty);

    //START MOD) SON (2019.09.17) - 17 September 2019 

    let dayPiloStart = DayPilot.Date.parse(startDate, "yyyy-MM-dd");

    for (let i = 1; i <= dayDis; i++) {

        $.each(lineNo, function (index, value) {
            if (remainQty > 0 || totalDis === remainQty) {

                //mesSerial++;
                seqMes++;

                let newMxPackage = "M" + proPackageId.slice(1) + "_" + ZeroPad(seqMes, 2);

                let newId = newPkgGroup + "_" + (CUR_NUMBER_MES_PKG + 1);
                CUR_NUMBER_MES_PKG++;

                //Create event object for Mes daypilot
                let e = new DayPilot.Event({
                    start: dayPiloStart,
                    end: dayPiloStart, //Always distribute 1 day only
                    id: newMxPackage, //newId,
                    text: newMxPackage, //newId,
                    resource: value,
                    Factory: factoryMes,
                    LineNo: value,
                    AoNo: aoNo,
                    StyleCode: styleCode,
                    StyleSize: styleSize,
                    StyleColorSerial: styleColorSerial,
                    RevNo: revNo,
                    PlanQty: curPlan,
                    StyleInf: styleInf,

                    PackageGroup: newPkgGroup,
                    SeqNo: seqMes,
                    PrdPkg: proPackageId,
                    MxPackage: newMxPackage,
                    MxTarget: curPlan,
                    PlnStartDate: "",
                    PlnEndDate: "",
                    FinishedQty: 0,
                    TaktTime: 0,
                    WorkingHours: 0,

                    MxStatus: "RO",

                    resizeDisabled: true, //1 MES package produce for 1 date 

                    backColor: bgColor,

                    cssClass: "event-style", //"distributed",
                    barHidden: true,

                    IsNew: "Y"//Status 'Y' is yes, new event was created, 'N' is No, old event load from database
                });
                dpMes.events.add(e);

                totalDis += curPlan;
                //Calculate remain qty again
                remainQty = plnQty - totalDis;
                curPlan = remainQty < dailyTarQty ? remainQty : dailyTarQty;

                //START ADD) SON (2019.09.17) - 17 September 2019 - add max MES sequence
                //Keep list mes seq in temporary memory
                LIST_MES_SEQ.push({ PackageGroup: newPkgGroup, Seq: seqMes, MxPackage: newMxPackage });
                //START ADD) SON (2019.09.17) - 17 September 2019
            }
        });

        dayPiloStart = new DayPilot.Date(dayPiloStart).addDays(1);
    }

    if (isDisNewPPPackage) {
        //Create object package group.   
        //If production package existed in current list of Package Group then no need to create Package group.        
        let objPkgGroup = {
            PackageGroup: newPkgGroup,
            MesFactory: factoryMes,
            StyleCode: styleCode,
            StyleSize: styleSize,
            StyleColorSerial: styleColorSerial,
            RevNo: revNo,
            Buyer: styleCode.substring(0, 3),
            TargetQty: totalQty,
            Status: "RO",
            MesPlnStartDate: "",
            MesPlnEndDate: "",
            Priority: 1,
            MadeQty: 0,
            //Registrar: "",
            StyleInf: styleInf,
            RemainQty: remainQty,
            IsNew: "Y",
            //MesMaxSerial: mesSerial,
            SeqNo: seqMes,
            Registrar: $("#hdUserId").val()
        };
        LIST_PACKAGE_GROUP.push(objPkgGroup);

    } else {
        //Update remain quantity
        let objIndex = LIST_PACKAGE_GROUP.findIndex(obj => obj.PackageGroup === newPkgGroup);
        LIST_PACKAGE_GROUP[objIndex].RemainQty = remainQty;

        LIST_PACKAGE_GROUP[objIndex].SeqNo = seqMes;
    }

    //START MOD) SON (2019.09.17) - 19 September 2019 - update remain quantity on gridview
    $.each(SELECTEDROWQCO, function (idx, selEvn) {

        //Find production package and update remain quantity
        let qcoPkg = QCO_CURRENT_LIST.filter(x => x.PrdPkg === selEvn.PrdPkg);
        if (qcoPkg !== null) qcoPkg[0].RemainQty = remainQty;

        //Reload gridview QCO base on value of slider.
        ReloadQCOGridBySlider();

    });

    //Color production package background if remain qty is 0
    //if (remainQty === 0) {
    //    //Remove event and add again
    //    $.each(SELECTEDROWQCO, function (idx, selEvn) {

    //        let qcoPkg = QCO_CURRENT_LIST.filter(x => x.PrdPkg === selEvn.PrdPkg);
    //        if (qcoPkg !== null) qcoPkg[0].RemainQty = 0;

    //        ReloadJqGridLocal(tableQCOPPKGName, QCO_CURRENT_LIST);

    //    });
    //}
    //END MOD) SON (2019.09.17) - 19 September 2019

    //Hide modal after creating event
    $('#mdlPpDivide').modal('hide');
}

function eventDivideProPackage() {
    $("#btnCreateMesPkg").click(function () {        
        if (CREATEMESQCO) {
            createMESPackageByQCO();
        } else {
            let lineNo = $("#drpLineNo").val();
            let dailyTarQty = parseInt($("#txtDailyTarQty").val());
            let numLines = lineNo.length;
            let factoryMes = $("#drpFactoryMes").val();
            let totalQty = parseInt($("#txtTotalQty").val());

            //Check line no
            if ($.isEmptyObject(lineNo)) {
                ShowMessage("Create MES package", "Please choose line number.", ObjMessageType.Info);
                return;
            }

            //Check daily quantity
            if ($.isEmptyObject(dailyTarQty) || isNaN(dailyTarQty) || dailyTarQty === 0) {
                ShowMessage("Create MES package", "Please check daily target Qty.", ObjMessageType.Info);
                return;
            }

            //Check MES factory
            if ($.isEmptyObject(factoryMes)) {
                ShowMessage("Create MES package", "Please select Mes factory.", ObjMessageType.Info);
                return;
            }

            //Check production package whether available or not before schedule
            let selPrdPkg = $("#hdSelectedPrdPkg").text();
            if (!isProductionPackageAvailable(factoryMes, selPrdPkg)) {
                ShowMessage("Create MES package", "This production package was scheduled.", ObjMessageType.Info);

                return false;
            }

            //if (!checkDataCreateMESPackage()) return;


            //Set dragged object to selected list events on AOMTOP scheduler
            //if (OBJEVENTMTOPSEL.length === 0) {
            //    OBJEVENTMTOPSEL.push(OBJDRAGPROPKG);
            //}

            //Get information of the first selected events
            let objEvent;
            objEvent = OBJEVENTMTOPSEL[0].data;
            let styleInf = objEvent.StyleInf;

            //Get production package
            let proPackageId = objEvent.id;

            //CREATE PACKAGE GROUP.
            //Get current year and month
            let d = new Date();
            let month = (d.getMonth() + 1).toString();
            let mm = month.length === 1 ? "0" + month : month;
            let yy = d.getFullYear().toString();
            let yymm = yy.substr(2, 2) + mm;

            let newPkgGroup;

            //Keep mes packge serial
            let mesSerial = 0;
            let seqMes = 0;

            //IF DISTRIBUTION OLD PRODUCTION PACKAGE THEN DON'T NEED TO CREATE NEW PACKAGE GROUP AND PRODUCTION PACKAGE GROUP
            let isDisNewPPPackage = true;
            let oldPPkgRemainQty = 0;
            //Checking production package to create new group package or not
            $.each(OBJEVENTMTOPSEL, function (idx, selEvn) {
                let selObj = selEvn.data;
                //Find package group in current list package group
                let pkgGroup = LIST_PP_PACKAGE_GROUP.filter(x => x.PPackage === selObj.PrdPkg); //.map(x => x.PackageGroup);

                //If package existed then don't need to create new package group
                if (pkgGroup.length !== 0) {
                    isDisNewPPPackage = false;
                    newPkgGroup = pkgGroup[0].PackageGroup;

                    //Get information of production package group in list
                    let ppGroup = LIST_PACKAGE_GROUP.filter(x => x.PackageGroup === newPkgGroup);
                    oldPPkgRemainQty = ppGroup.length === 0 ? oldPPkgRemainQty : ppGroup[0].RemainQty;
                    return false;
                }
            });

            //If do not exist production package group then create new package group
            if (isDisNewPPPackage) {
                let listNewPkgGroup = LIST_PACKAGE_GROUP.filter(x => x.IsNew === "Y");
                //If the is no new package group in current list then get max package group from database
                if (listNewPkgGroup.length === 0) {
                    //Create new Package group by factory, year and month
                    newPkgGroup = GetMaxPackageGroup(factoryMes, yymm);

                    //Get number of sequence package group (get last 9 characters of package group) and set it to the current sequent variable
                    CUR_PACKAGE_SEQ = parseInt(newPkgGroup.substr(10, 9));
                } else {
                    //If in current list has new package group, just increase package group sequence from variable
                    //and create package group from this sequence
                    CUR_PACKAGE_SEQ++;
                    newPkgGroup = factoryMes + "-" + yymm + "-" + ZeroPad(CUR_PACKAGE_SEQ, 9);
                }
            }

            //Get max MES sequence in package group
            let pkgGroup = LIST_PACKAGE_GROUP.filter(x => x.PackageGroup === newPkgGroup);
            if (pkgGroup.length !== 0) {

                //START MOD) SON (2019.09.17) - 17 September 2019 - check maximum mes package sequence
                let listMesSeq = LIST_MES_SEQ.filter(x => x.PackageGroup === newPkgGroup);
                if (listMesSeq.length !== 0) {
                    //Get min Seq at the first position
                    let maxSeq = listMesSeq[0].Seq;
                    //Find maximum mes sequence in the list
                    $.each(listMesSeq, function (idx, mesSeq) {
                        if (maxSeq < mesSeq.Seq) maxSeq = mesSeq.Seq;
                    });
                    //Set max MES sequence
                    seqMes = maxSeq;
                } else {
                    seqMes = pkgGroup[0].MaxSeq;
                }

                //seqMes = pkgGroup[0].MaxSeq;

                //END MOD) SON (2019.09.17) - 17 September 2019 
            } else {

                seqMes = 0;
            }

            let plnQty = 0;
            let totalPlnQty = 0;
            let seqPP = 0;
            //let isSameStyle = true;
            $.each(OBJEVENTMTOPSEL, function (ind, obj) {
                //Production package information
                let ppInf = obj.data;

                //Create new object production package group if it is new.
                if (isDisNewPPPackage) {
                    //Sum plan quantity of selected events
                    totalPlnQty += obj.data.PlanQty;

                    //Add production package group
                    seqPP++;
                    let objProPPkgGroup = {
                        PackageGroup: newPkgGroup,
                        SeqNo: seqPP,
                        PPackage: ppInf.PrdPkg,
                        Factory: ppInf.Factory,
                        AONo: ppInf.AoNo,
                        OrdQty: ppInf.OrdQty,
                        PlanQty: ppInf.PlanQty,
                        IsNew: "Y"
                    };
                    //Add production package into package group
                    LIST_PP_PACKAGE_GROUP.push(objProPPkgGroup);
                }
            });

            //Get plan quantity
            plnQty = isDisNewPPPackage ? totalPlnQty : oldPPkgRemainQty;

            //let startDateExe = "2015-07-10"; 
            let startDateExe = $("#txtStartDate").val().replace(/\//g, "-");

            let id = objEvent.id;
            let startDate = startDateExe; //objEvent.start; //
            let endDate = startDateExe; //objEvent.end;
            let aoNo = objEvent.AoNo;
            let styleCode = objEvent.StyleCode;
            let styleSize = objEvent.StyleSize;
            let styleColorSerial = objEvent.StyleColorSerial;
            let revNo = objEvent.RevNo;
            //let styleInf = objEvent.StyleInf;
            //var planQty = objEvent.PlanQty;

            //var lineNoEvent = objEvent.LineNo;
            //var prdPkg = objEvent.PrdPkg;
            let bgColor = objEvent.backColor;

            //Maximum the line can be distributed
            let maxLineDis = Math.ceil(plnQty / dailyTarQty);

            if (maxLineDis < numLines) {
                ShowMessage("Create MES package", "Please reduce distribution line.", ObjMessageType.Info);
                return;
            }

            //Get style information from gridview package group
            //let pkgGroup = GetSelectedOneRowData(tableGroupPackageId).PackageGroup;

            //Distribute mes packages
            let remainQty = 0; //Remain quantity
            let totalDis = 0; //Total distribution
            let curPlan = plnQty < dailyTarQty ? plnQty : dailyTarQty;

            //START MOD) SON (2019.09.17) - 17 September 2019 - check the number day of distribution
            //If create all is not checked then day distribution is 1
            let dayDis = $("#chkCreateAll").is(":checked") === false ? 1 : Math.ceil(totalQty / dailyTarQty);

            //Get the number of days to distribute MES package
            //let dayDis = Math.ceil(totalQty / dailyTarQty);

            //START MOD) SON (2019.09.17) - 17 September 2019 

            //Set start date to distribute mes package
            let dayPiloStart = DayPilot.Date.parse(startDate, "yyyy-MM-dd");

            for (let i = 1; i <= dayDis; i++) {

                $.each(lineNo, function (index, value) {
                    if (remainQty > 0 || totalDis === remainQty) {

                        seqMes++;

                        let newMxPackage = "M" + proPackageId.slice(1) + "_" + ZeroPad(seqMes, 2);

                        //let newId = newPkgGroup + "_" + (CUR_NUMBER_MES_PKG + 1);
                        CUR_NUMBER_MES_PKG++;

                        //Create event object for Mes daypilot
                        let e = new DayPilot.Event({
                            start: dayPiloStart,
                            end: dayPiloStart, //Always distribute 1 day only
                            id: newMxPackage, //newId,
                            text: newMxPackage, //newId,
                            resource: value,
                            Factory: factoryMes,
                            LineNo: value,
                            AoNo: aoNo,
                            StyleCode: styleCode,
                            StyleSize: styleSize,
                            StyleColorSerial: styleColorSerial,
                            RevNo: revNo,
                            PlanQty: curPlan,
                            StyleInf: styleInf,

                            PackageGroup: newPkgGroup,
                            SeqNo: seqMes,
                            PrdPkg: proPackageId,
                            MxPackage: newMxPackage,
                            MxTarget: curPlan,
                            PlnStartDate: "",
                            PlnEndDate: "",
                            FinishedQty: 0,
                            TaktTime: 0,
                            WorkingHours: 0,

                            MxStatus: "RO",

                            resizeDisabled: true, //1 MES package produce for 1 date 

                            backColor: bgColor,

                            cssClass: "event-style", //"distributed",
                            barHidden: true,

                            IsNew: "Y"//Status 'Y' is yes, new event was created, 'N' is No, old event load from database
                        });
                        dpMes.events.add(e);

                        totalDis += curPlan;
                        //Calculate remain qty again
                        remainQty = plnQty - totalDis;
                        curPlan = remainQty < dailyTarQty ? remainQty : dailyTarQty;

                        //START ADD) SON (2019.09.17) - 17 September 2019 - add max MES sequence
                        //Keep list mes seq in temporary memory
                        LIST_MES_SEQ.push({ PackageGroup: newPkgGroup, Seq: seqMes, MxPackage: newMxPackage });
                        //START ADD) SON (2019.09.17) - 17 September 2019
                    }
                });

                dayPiloStart = new DayPilot.Date(dayPiloStart).addDays(1);
            }

            if (isDisNewPPPackage) {
                //Create object package group.   
                //If production package existed in current list of Package Group then no need to create Package group.        
                let objPkgGroup = {
                    PackageGroup: newPkgGroup,
                    MesFactory: factoryMes,
                    StyleCode: styleCode,
                    StyleSize: styleSize,
                    StyleColorSerial: styleColorSerial,
                    RevNo: revNo,
                    Buyer: styleCode.substring(0, 3),
                    TargetQty: totalQty,
                    Status: "RO",
                    MesPlnStartDate: "",
                    MesPlnEndDate: "",
                    Priority: 1,
                    MadeQty: 0,
                    //Registrar: "",
                    StyleInf: styleInf,
                    RemainQty: remainQty,
                    IsNew: "Y",
                    //MesMaxSerial: mesSerial,
                    SeqNo: seqMes,
                    Registrar: $("#hdUserId").val()
                };
                LIST_PACKAGE_GROUP.push(objPkgGroup);

            } else {
                //Update remain quantity
                let objIndex = LIST_PACKAGE_GROUP.findIndex(obj => obj.PackageGroup === newPkgGroup);
                LIST_PACKAGE_GROUP[objIndex].RemainQty = remainQty;

                //Update max MES serial
                LIST_PACKAGE_GROUP[objIndex].SeqNo = seqMes;
            }

            //Color production package background if remain qty is 0
            if (remainQty === 0) {
                //Remove event and add again
                $.each(OBJEVENTMTOPSEL, function (idx, selEvn) {

                    let pp = dpMtop.events.find(selEvn.data.id);

                    dpMtop.events.remove(pp);

                    //Place css and adding again
                    selEvn.data.cssClass = "distributed";
                    selEvn.data.rightClickDisabled = true;
                    dpMtop.events.add(selEvn);

                });
            }

            //Remove temporary MES package when drag production package
            //removeTemporaryMesPkg();            
                        
            //Copy style information
            CopyStyleInfomation(styleCode, styleSize, styleColorSerial, revNo);
            
            //Hide modal after creating event
            $('#mdlPpDivide').modal('hide');
        }
    });
}

//Redefine event selected row
function eventSelectedRowOnPackageGroupGrid(rowData) {
    var packageGroup = rowData.PackageGroup;

    //Reload production package and mes package
    var params = { packageGroup: packageGroup };

    ReloadJqGrid2LoCal(tableMesPackageName, params);

}

function eventAddMesPackage() {
    $("#btnAddMesPkg").click(function () {
        ISUPDATED = false;
        //loadProductionAndMesScheduler();

        ClearDataOnMTopMesScheduler();

        $("#mdlProSchedule").modal("show");
    });
}

function eventEditMesPacakge() {
    $("#btnEditMesPkg").click(function () {
        ISUPDATED = true;
        //loadProductionAndMesScheduler();

        ClearDataOnMTopMesScheduler();

        $("#mdlProSchedule").modal("show");
    });
}

function clickCancelModalDistributionMes() {
    $("#btnCanelMdlDistributeMes, #btnCloseModalDistributionMes").click(function () {
        removeTemporaryMesPkg();
    });
}

function eventSaveMesPackage() {
    $("#btnSaveMesPkg").click(function () {

        var r = confirm("Do you want to save!");
        if (r === false) {
            return;
        }

        //ShowConfirmYesNoMessage("001", SmsFunction.Update, MessageType.Confirm, MessageContext.UpdateConfirm, function () { /*code here*/}, function () { });

        var fac = $("#drpFactoryMes").val();
        if ($.isEmptyObject(fac)) {
            ShowMessage("Create MES package", "Please select MES factory.", ObjMessageType.Info);

            return false;
        }

        //Get style information from gridview package group
        //var pkgGroup = GetSelectedOneRowData(tableGroupPackageId).PackageGroup;

        var lstMesEve = dpMes.events.list;

        if (lstMesEve.length === 0 && LIST_DELETED_MES_PACKAGE.length === 0) {
            ShowMessage("Create MES package", "There is no MES package.", ObjMessageType.Info);

            return false;
        }

        let userId = $("#hdUserId").val();

        let lstMesPkg = [];
        let lstMpmt = [];
        let lstPpkg = [];
        let listMesUpdate = [];
        let listMpmtUpdate = [];

        //Get list new or updated MES package on schedule
        $.each(lstMesEve, function (idx, eve) {
            if (eve.IsNew === "Y") {//Get list new MES package
                //Get AOMTOP and Line Serial
                let mesResource = eve.resource.split('#');
                //If mesResource has 2 arrays
                if (mesResource.length === 2) {
                    eve.LineSerial = mesResource[0];
                    eve.LineNo = mesResource[1];
                } else {//If mesResource has greater than 2 arrrays
                    //Get position of '#'
                    let pos = eve.resource.indexOf("#");
                    //Get a frist string from 0 to '#' position
                    eve.LineSerial = eve.resource.slice(0, pos);
                    //Get remain string behide '#' position
                    eve.LineNo = eve.resource.slice(pos + 1);
                }

                eve.PlnStartDate = eve.start.toString("yyyyMMdd");
                eve.PlnEndDate = eve.start.toString("yyyyMMdd");
                //eve.LineNo = eve.resource;

                eve.Registrar = userId;
                eve.UpdatedId = userId;

                lstMesPkg.push(eve);
            } else if (eve.IsNew === "U") { //Get list MES package which need to update
                //Get AOMTOP and Line Serial
                let mesResource = eve.resource.split('#');
                //If mesResource has 2 arrays
                if (mesResource.length === 2) {
                    eve.LineSerial = mesResource[0];
                    eve.LineNo = mesResource[1];
                } else {//If mesResource has greater than 2 arrrays
                    //Get position of '#'
                    let pos = eve.resource.indexOf("#");
                    //Get a frist string from 0 to '#' position
                    eve.LineSerial = eve.resource.slice(0, pos);
                    //Get remain string behide '#' position
                    eve.LineNo = eve.resource.slice(pos + 1);
                }

                eve.PlnStartDate = eve.start.toString("yyyyMMdd");
                eve.PlnEndDate = eve.start.toString("yyyyMMdd");
                //eve.LineNo = eve.resource;

                eve.UpdatedId = userId;

                listMesUpdate.push(eve);
            }
        });

        //Get start date and end date of package group
        $.each(LIST_PACKAGE_GROUP, function (idx, pkgGroup) {
            let pkgStartDate = "00000000";
            let pkgEndDate = "00000000";
            let minDate = "00000000";
            $.each(lstMesEve, function (idx, eve) {

                if (pkgGroup.PackageGroup === eve.PackageGroup) {

                    //set the first date
                    if (pkgStartDate === "00000000") {
                        pkgStartDate = eve.PlnStartDate;
                    }

                    if (pkgStartDate > eve.PlnStartDate) {
                        pkgStartDate = eve.PlnStartDate;
                    }

                    if (eve.PlnEndDate > pkgEndDate) {
                        pkgEndDate = eve.PlnEndDate;
                    }
                }
            });

            pkgGroup.MesPlnStartDate = pkgStartDate;
            pkgGroup.MesPlnEndDate = pkgEndDate;
            pkgGroup.UpdatedId = userId;

            listMpmtUpdate.push(pkgGroup);
        });

        //Filter new package group and production package group to insert into database
        lstPpkg = LIST_PP_PACKAGE_GROUP.filter(x => x.IsNew === "Y");
        lstMpmt = LIST_PACKAGE_GROUP.filter(x => x.IsNew === "Y");

        if (!ISUPDATED) {
            let config = ObjectConfigAjaxPost("../Planning/SaveMesPackage", false
                , JSON.stringify({ lstMpmt: lstMpmt, lstPpkg: lstPpkg, lstMpdt: lstMesPkg }));
            AjaxPostCommon(config, function (resSave) {
                if (resSave === Success) {
                    if (CREATEMESQCO) {
                        loadQcoAndMesScheduler();
                    } else {
                        //Reload mes scheduler and production scheduler
                        loadProductionAndMesScheduler();
                    }

                    ShowMessage("Save MES package", "Save successfully", ObjMessageType.Info);
                } else {
                    ShowMessage("Save MES package", resSave, ObjMessageType.Info);
                }
            });
        } else {

            let config = ObjectConfigAjaxPost("../Planning/UpdateMesPackage", false
                , JSON.stringify({ lstMpmt: lstMpmt, lstPpkg: lstPpkg, lstMpdt: lstMesPkg, lstMesPkgUpd: listMesUpdate, listMesPkdDel: LIST_DELETED_MES_PACKAGE, listMpmt: listMpmtUpdate }));
            AjaxPostCommon(config, function (resSave) {
                if (resSave === Success) {
                    if (CREATEMESQCO) {
                        loadQcoAndMesScheduler();
                    } else {
                        //Reload mes scheduler and production scheduler
                        loadProductionAndMesScheduler();
                    }

                    ShowMessage("Save MES package", "Package saved", ObjMessageType.Info);
                } else {
                    ShowMessage("Save MES package", resSave, ObjMessageType.Info);
                }
            });
        }

    });
}

function loadProductionAndMesScheduler() {
    //Clear data
    OBJDRAGMESPKG = null;
    OBJEVENTMTOPSEL = [];
    LIST_PACKAGE_GROUP = [];
    LIST_PP_PACKAGE_GROUP = [];
    LIST_DELETED_MES_PACKAGE = [];
    LIST_MES_SEQ = []; //ADD) SON (2019.09.17) - 17 September 2019 - clear list mes sequence

    CUR_NUMBER_MES_PKG = 0;
    CUR_PACKAGE_SEQ = 0;

    //Get information for searching
    let arrDateRange = $("#txtDateRangePp").val().split('-');
    let scrollTo = arrDateRange[0].replace(new RegExp('/', 'g'), '-');
    let startDate = $.trim(arrDateRange[0].replace(new RegExp('/', 'g'), ''));
    let endDate = $.trim(arrDateRange[1].replace(new RegExp('/', 'g'), ''));
    let factoryId = $("#drpFactoryMtop").val();

    let buyer = $("#drpBuyerMtop").val();
    let styleInfo = $("#txtStyleInfo").val();
    let aoNo = $("#txtAoNumber").val();

    //Calculate number of days range
    let d1 = new Date(arrDateRange[0].trim());
    let d2 = new Date(arrDateRange[1].trim());
    let difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

    //Get production lines
    GetFactoryLines(factoryId, startDate, endDate, buyer, styleInfo, aoNo, function (newArrLine) {
        dpMtop.resources = newArrLine;
        //PPLINES = newArrLine;
    });

    //Get production packages
    GetProductionPackage(factoryId, startDate, endDate, buyer, styleInfo, aoNo, function (lstPp) {
        dpMtop.events.list = lstPp;
    });

    dpMtop.days = difference + 1;
    dpMtop.startDate = arrDateRange[0];
    dpMtop.update();
    dpMtop.scrollTo(scrollTo);

    //Get line for MES
    let mesLines = GetMESLinesByFactory(factoryId);
    dpMes.resources = mesLines;

    //Get production packages
    GetMesPackages(factoryId, startDate, endDate, factoryId, aoNo, buyer, styleInfo, function (listMes) {
        dpMes.events.list = listMes;
    });

    dpMes.days = difference + 1;
    dpMes.startDate = arrDateRange[0];
    dpMes.update();
    dpMes.scrollTo(scrollTo);

    //Fill MES line to dropdownlist
    FillDataMultipleSelectLineNoMdl("drpLineNo", mesLines, "id", "name");
}

function loadMESPackage() {

    //Clear data
    OBJDRAGMESPKG = null;
    OBJEVENTMTOPSEL = [];
    LIST_PACKAGE_GROUP = [];
    LIST_PP_PACKAGE_GROUP = [];
    LIST_DELETED_MES_PACKAGE = [];
    LIST_MES_SEQ = []; //ADD) SON (2019.09.17) - 17 September 2019 - clear list mes sequence

    CUR_NUMBER_MES_PKG = 0;
    CUR_PACKAGE_SEQ = 0;

    //Get information for searching
    let arrDateRange = $("#txtDateRangeMes").val().split('-');
    let scrollTo = arrDateRange[0].replace(new RegExp('/', 'g'), '-');
    let startDate = $.trim(arrDateRange[0].replace(new RegExp('/', 'g'), ''));
    let endDate = $.trim(arrDateRange[1].replace(new RegExp('/', 'g'), ''));
    let factoryId = $("#drpFactoryMtop").val();

    let buyer = $("#drpBuyerMtop").val();
    let styleInfo = $("#txtStyleInfo").val();
    let aoNo = $("#txtAoNumber").val();

    //Calculate number of days range
    let d1 = new Date(arrDateRange[0].trim());
    let d2 = new Date(arrDateRange[1].trim());
    let difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

    //Get line for MES
    let mesLines = GetMESLinesByFactory(factoryId);
    dpMes.resources = mesLines;

    //Get production packages
    GetMesPackages(factoryId, startDate, endDate, factoryId, aoNo, buyer, styleInfo, function (listMes) {
        dpMes.events.list = listMes;
    });

    dpMes.days = difference + 1;
    dpMes.startDate = arrDateRange[0];
    dpMes.update();
    dpMes.scrollTo(scrollTo);
}

function loadQcoAndMesScheduler() {
    //Clear data
    OBJDRAGMESPKG = null;
    OBJEVENTMTOPSEL = [];
    LIST_PACKAGE_GROUP = [];
    LIST_PP_PACKAGE_GROUP = [];
    LIST_DELETED_MES_PACKAGE = [];
    LIST_MES_SEQ = []; //ADD) SON (2019.09.17) - 17 September 2019 - clear list mes sequence

    CUR_NUMBER_MES_PKG = 0;
    CUR_PACKAGE_SEQ = 0;

    let qcoFactory = $("#drpFactoryQco").val();
    let qcoYear = $("#txtYearQco").val();
    let qcoWeekNo = "W" + $("#txtWeekQco").val();

    let startDate = getCurrentDate(0).replace(new RegExp('/', 'g'), '');
    let endDate = getCurrentDate(30).replace(new RegExp('/', 'g'), '');

    let buyer = $("#drpBuyerQco").val();
    let styleInf = $("#txtStyleInfQCO").val();
    let aoNo = $("#txtAoNumberQco").val();

    //let buyer = $("#drpBuyerMtop").val();
    //let styleInf = $("#txtStyleInfo").val();
    //let aoNo = $("#txtAoNumber").val();

    loadProductionPackageByQCO(qcoFactory, aoNo, buyer, styleInf, startDate, endDate, qcoYear, qcoWeekNo);

    loadMesPackageQco(qcoFactory, aoNo, buyer, styleInf, startDate, endDate);

}

function clickTabProductionpackage() {
    $("#achQcoRanking").click(function () {

        CREATEMESQCO = true;

        $("#divAomtop").hide();
        $("#divQcoRanking").show();

        //let factoryId = $("#drpFactoryQco").val();

        var curDate = new Date();
        $("#txtYearQco").val(curDate.getFullYear());

    });

    $("#achAomtop").click(function () {

        CREATEMESQCO = false;

        $("#divQcoRanking").hide();
        $("#divAomtop").show();
    });
}

function loadProductionPackageByQCO(qcoFactory, aoNo, buyer, styleInf, startDate, endDate, qcoYear, qcoWeekNo) {
    let data;
    GetProductionPackageByQco(qcoFactory, qcoYear, qcoWeekNo, buyer, aoNo, styleInf, function (listQco) { data = listQco; });
    ReloadJqGridLocal(tableQCOPPKGName, data);

    if (data.length !== 0) {
        //Get mix and max ranking in the list.
        let keyValue = 'QCORank';
        let minMaxQCO = minmax(data, keyValue);

        let sliderQCO = document.getElementById('sliderQCO');

        sliderQCO.noUiSlider.updateOptions({
            range: {
                'min': minMaxQCO.min,
                'max': minMaxQCO.max
            }
        });

        sliderQCO.noUiSlider.set([minMaxQCO.min, minMaxQCO.max]);
    }

}

const minmax = (arrayOfObjects, keyValue) => {
    const values = arrayOfObjects.map(value => value[keyValue]);
    return {
        min: Math.min.apply(null, values),
        max: Math.max.apply(null, values)
    };
};

function clickSearchQcoPackage() {
    $("#btnSearchQcoPkg").click(function () {

        let isMESChange = isChangingMESPackage();
        if (isMESChange) {
            ShowConfirmYesNo(
                "Save MES package"
                , "Do you want to search production package without saving MES package?"
                , function () {
                    searchProductionPackageByQCO();
                }
                , function () {

                }
            );
        } else {
            searchProductionPackageByQCO();
        }
    });
}

function searchProductionPackageByQCO() {
    //ClearDataOnMTopMesScheduler();

    ClearTemporaryScheduleMesPackage();

    let qcoFactory = $("#drpFactoryQco").val();
    let qcoYear = $("#txtYearQco").val();
    let qcoWeekNo = "W" + $("#txtWeekQco").val();

    if (isEmpty(qcoYear) || isEmpty(qcoWeekNo)) {
        ShowMessage("Search QCO", "Please enter year and week no", ObjMessageType.Info);
        return;
    }

    let startDate = getCurrentDate(0).replace(new RegExp('/', 'g'), '');
    let endDate = getCurrentDate(30).replace(new RegExp('/', 'g'), '');

    let aoNo = $("#txtAoNumberQco").val();
    let buyer = $("#drpBuyerQco").val();
    let styleInf = $("#txtStyleInfQCO").val();

    //reloadProductionPackageByQco(qcoFactory, qcoYear, qcoWeekNo);

    loadProductionPackageByQCO(qcoFactory, aoNo, buyer, styleInf, startDate, endDate, qcoYear, qcoWeekNo);

    loadMesPackageQco(qcoFactory, aoNo, buyer, styleInf, startDate, endDate);
}

function loadMesPackageQco(qcoFactory, aoNo, buyer, styleInf, startDate, endDate) {

    //Get line from MySQL with line combination between Line Serial and AOMTOP line
    let mesLines = GetMESLinesByFactory(qcoFactory);
    dpMes.resources = mesLines;
    FillDataMultipleSelectLineNoMdl("drpLineNo", mesLines, "id", "name");

    ////Get line from MES (MySql)
    //GetFactoryLinesByFactoryIdMySql(qcoFactory, function (lines) {
    //    //Daypilot MES
    //    dpMes.resources = lines;
    //    FillDataMultipleSelectLineNoMdl("drpLineNo", lines, "id", "name");

    //});

    //Get production packages
    GetMesPackages(qcoFactory, startDate, endDate, qcoFactory, aoNo, buyer, styleInf, function (listMes) {
        dpMes.events.list = listMes;
    });

    //arrDateRange[0].replace(new RegExp('/', 'g'), '-')
    dpMes.days = 30;
    dpMes.startDate = getCurrentDate(0);
    dpMes.update();
    dpMes.scrollTo(getCurrentDate(0));
}
