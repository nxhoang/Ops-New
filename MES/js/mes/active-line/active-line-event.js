function eventClickButton() {
    //#region Active Line
    $("#btnSearchLine").click(function () {
        
        let recDataVal = $("#drpReceiveData").val();
        if ($.isEmptyObject(recDataVal)) {
            ShowMessage("", "Select receive data type");
            return;
        }
        $("#drpReceiveData").val(recDataVal).trigger('change');
    }); 
    //#endregion

    //#region Mes Active Line
    $("#btnDisplayActiveLine").click(function () {
        $("#divLine").html("");

        let mapBy = $("#drpShowBy").val();
        if (mapBy === "1") {
            //Mes line mapping IoT
            //Get completed quantity from MES package (MX_IOT_Completed)
            mesLineTarget();
        } else {//Display option is 2: Mes line mapping by Operation Plan
            let disOptVal = $("#drpDisplayOption").val();
            if (disOptVal === "1") {
               //Target: Get completed quantity of each process from t_mx_opdt table(column LAST_IOT_DATA)
                mesLineMappingByIot();
            } else {
                //Worker: Show mapped processes to Seat number, connected Iot
                mesLineMappingSeat();
            }
        }
               
    }); 
     //#endregion
    
}

//#region Mes Active Line
function mesLineMappingByIot() {
    let factoryId = $("#drpFactory").val();
    let plnStartDate = $("#txtDate").val().replace(/\//g, "");

    $("#divLine").html("");

    //Hide alias
    $("#divProAlias").hide();

    //Display target
    GetMesPackagesById(factoryId, plnStartDate, function (listMpdt) {
        if (listMpdt !== null) {
            $.each(listMpdt, function (idx, pkg) { 
                if (pkg.IsActive === "Y") {
                    $("#divLine").append(createLine(pkg.LineName, ACTIVE_IMG_NAME, pkg.MxTarget, pkg.MX_IOT_Completed)); 
                } else {
                    $("#divLine").append(createLine(pkg.LineName, DEACTIVE_IMG_NAME, 0, 0));
                }
            });
        }
    });
}

function mesLineMappingSeat() {

    $("#divLine").html("");

    let factoryId = $("#drpFactory").val();
    let plnStartDate = $("#txtDate").val().replace(/\//g, "");

    //Show alias
    $("#divProAlias").show();
    //Display workers
    GetMappingSeats(factoryId, plnStartDate, function (listLines) {
        //console.log(listLines);
        $.each(listLines, function (idx, line) {
            //if (line.MappingSeats !== 0) {
            if (line.IsActive === "Y") {
                $("#divLine").append(createMappingSeat(line.LineName, ACTIVE_IMG_NAME, line.ConnectedIot, line.Workers, line.MappingSeats));
            } else {
                $("#divLine").append(createMappingSeat(line.LineName, DEACTIVE_IMG_NAME, line.ConnectedIot, line.Workers, line.MappingSeats));
            }
        });
    });
}

function mesLineTarget() {
    //console.log('inside mesLineTarget()');

    $("#divLine").html("");

    let factoryId = $("#drpFactory").val();
    let plnStartDate = $("#txtDate").val().replace(/\//g, "");

    //Display target
    GetListMesPkg(factoryId, plnStartDate, "0", function (listMpdt) {
        //console.log('inside GetListMesPkg()');
        if (listMpdt !== null) {
            $.each(listMpdt, function (idx, pkg) {
                //console.log(pkg);
                if (pkg.IsActive === "Y") {
                    var displayValue = pkg.MX_IOT_Completed > pkg.MX_IOT_COMPLETED_DGS ? pkg.MX_IOT_Completed : pkg.MX_IOT_COMPLETED_DGS; //2020-12-15 Tai Le(Thomas)
                    //$("#divLine").append(createLine(pkg.LineName, ACTIVE_IMG_NAME, pkg.MxTarget, pkg.MX_IOT_Completed, pkg.MxPackage));
                    $("#divLine").append(createLine(pkg.LineName, ACTIVE_IMG_NAME, pkg.MxTarget, displayValue, pkg.MxPackage));
                } else {
                    $("#divLine").append(createLine(pkg.LineName, DEACTIVE_IMG_NAME, 0, 0,''));
                }
            });
        }
    });
}

//#endregion

//#region Active Line
function mesLineReceiveData() {
    //console.log('inside mesLineReceiveData()');

    //let receiveVal = this.value;
    let receiveVal = $("#drpReceiveData").val();
    let factoryId = $("#drpFactory2").val();
    let plnStartDate = $("#txtDate").val().replace(/\//g, "");
    $("#divLine").html("");

    //Display target            
    GetListMesPkg(factoryId, plnStartDate, receiveVal, function (listMpdt) {
        if (listMpdt !== null) {
            $.each(listMpdt, function (idx, pkg) {
                if (pkg.IsActive === "Y") {
                    $("#divLine").append(createLine(pkg.LineName, ACTIVE_IMG_NAME, pkg.MxTarget, pkg.MX_IOT_Completed, pkg.MxPackage ));
                } else {
                    if (pkg.MX_IOT_Completed !== 0) {
                        //Deactive line but still have completed Iot data
                        $("#divLine").append(createLine(pkg.LineName, DEACTIVE_IMG_NAME, pkg.MxTarget, pkg.MX_IOT_Completed, pkg.MxPackage));
                    } else {
                        $("#divLine").append(createLine(pkg.LineName, DEACTIVE_IMG_NAME, 0, 0, pkg.MxPackage));
                    }

                }
            });
        }
    });
}
//#endregion

function eventOnChange() {

    //#region Active Line

    $("#drpFactory2").change(function () {
        mesLineReceiveData();
    });

    $("#drpReceiveData").change(function () {
        mesLineReceiveData();
    });
    //#endregion

    //#region Mes Active Line

    $("#drpFactory").change(function () {
        
        let mapBy = $("#drpShowBy").val();
        if (mapBy === "1") {
            //Mes line mapping IoT 
            //Get completed quantity from MES package (MX_IOT_Completed)
            mesLineTarget();
        } else {//Display option is 2: Mes line mapping by Operation Plan
            let disOptVal = $("#drpDisplayOption").val();
            if (disOptVal === "1") {
                //Target: Get completed quantity of each process from t_mx_opdt table(column LAST_IOT_DATA)
                mesLineMappingByIot();
            } else {
                //Worker: Show mapped processes to Seat number, connected Iot
                mesLineMappingSeat();
            }
        }
    });

    $("#drpDisplayOption").change(function () {

        //$("#divLine").html("");

        let mapBy = $("#drpShowBy").val();
        if (mapBy === "2") {
            //Display option is 2: Mes line mapping by Operation Plan
            let disOptVal = $("#drpDisplayOption").val();
            if (disOptVal === "1") {
                //Target: Get completed quantity of each process from t_mx_opdt table(column LAST_IOT_DATA)
                mesLineMappingByIot();
            } else {
                //Worker: Show mapped processes to Seat number, connected Iot
                mesLineMappingSeat();
            }
        } 

        //let disPlaVal = this.value;
        //if (disPlaVal === "1") {            
        //    //Target
        //    mesLineTarget();
        //} else { //Workers
        //    mesLineMappingSeat();
        //}
        
    });

    $("#drpShowBy").change(function () {
        //let factoryId = $("#drpFactory").val();
        //let plnStartDate = $("#txtDate").val().replace(/\//g, "");

        //$("#divLine").html("");

        let showBy = this.value;
        //Mes line mapping by Iot
        if (showBy === "1") {
            //Hide alias
            $("#divProAlias").hide();
            //Hide display option
            $("#lblDisplayOption, #divDisplayOption").hide();

            mesLineTarget();            

        } else { //Show mes active line by operation plan
            //Show display option
            $("#lblDisplayOption, #divDisplayOption").show();

            let disOptVal = $("#drpDisplayOption").val();
            if (disOptVal === "1") {
                //Target
                mesLineMappingByIot();
            } else {
                //Worker
                mesLineMappingSeat();
            }
        }
    });
    //#endregion
}