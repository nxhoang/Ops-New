function eventClickOnButton() {

    $('#btnSearchMtopPkg').click(function () {
        let arrDateRange = $("#txtDateRange").val().split('-');

        let plnStartDate = arrDateRange[0].trim().replace(new RegExp('/', 'g'), '');
        let plnEndDate = arrDateRange[1].trim().replace(new RegExp('/', 'g'), '');

        let aoNo = $("#txtAONo").val();
        let factory = $("#drpFactory").val();
        let buyer = $("#drpBuyer").val();
        let styleInfo = $("#txtBuyerInfo").val();

        //Check searching condition before searching mtop package
        if (!checkSearchMtopPkgCondition(factory, plnStartDate, plnEndDate, buyer, styleInfo, aoNo)) return;

        //Reload grid mtop package
        reloadGridMtopPackage(factory, plnStartDate, plnEndDate, buyer, styleInfo, aoNo);
    });

    $('#btnRequestJig').click(function () {

        let mtopPkg = GetSelectedOneRowData('#tbMtopPackage');

        //Check selected production package
        if (isEmpty(mtopPkg.PrdPkg)) {
            ShowMessage("Select package", "Please select production package", ObjMessageType.Info);
            return;
        }
                
        //Check factory id
        if (isEmpty($('#drpFactory').val())) {
            ShowMessage("Send Jig request", "Please select factory", ObjMessageType.Info);
            return;
        }

        $('#lblMtopPkgMdl').text(mtopPkg.PrdPkg);
        $('#txtAO').val(mtopPkg.AoNo);
        $('#txtAOQty').val(mtopPkg.PlanQty);
        $('#txtStyleCode').val(mtopPkg.StyleCode);
        
        ShowModalDragable('mdlRequestJig');
    });

    $('#btnSendRequestJig').click(function () {

        //Check jig request data before sending
        if (!checkJigRequestData()) return;

        let prdPkg = $('#lblMtopPkgMdl').text();

        let objJigRequest = {
            RequestId: $('#lblRequestJigId').text(),
            PrdPkg: prdPkg ,
            JigCode: $('#txtJigCode').val(),
            JigName: $('#txtJigName').val(),
            AO: $('#txtAO').val(),
            AOQty: $('#txtAOQty').val(),
            StyleCode: $('#txtStyleCode').val(),
            JigQty: $('#txtJigQty').val()
        };

        //Create object production readiness
        let objPrrd = {
            PRDPKG: prdPkg,
            FACTORY: $('#drpFactory').val(),
            JIG: JIG_STATUS.Requested,
            MOULD: '1',
            SOP: '1'
        };

        var config = ObjectConfigAjaxPost(
            "../PackageReadiness/SendJigRequest"
            , false
            , JSON.stringify({
                prrd: objPrrd
            })
        );
        AjaxPostCommon(config, function (resData) {
            if (resData.IsSuccess) {

                //Send request to TPM
                sendJigRequestToTpm(objJigRequest);

                ShowMessage("Send Jig request", resData.Result, ObjMessageType.Info);

                //Hide modal
                $("#mdlRequestJig").modal('hide');

            } else {

                ShowMessage("Send Jig request", resData.Log, ObjMessageType.Info);
            }

        });       

    });
}

function sendJigRequestToTpm(objJigRequest) {
    console.log(objJigRequest);

    //var config = ObjectConfigAjaxPost(
    //    "../PackageReadiness/SendRequestJig"
    //    , false
    //    , JSON.stringify({
    //        prrd: objJigRequest
    //    })
    //);
    //AjaxPostCommon(config, function (result) {
    //    console.log(result);
    //});
}

function checkSearchMtopPkgCondition(factory, plnStartDate, plnEndDate, buyer, styleInfo, aoNo) {
    //Check date range
    if (isEmpty(plnStartDate) || isEmpty(plnEndDate)) {
        ShowMessage("Search style", "Please select date range", ObjMessageType.Info);
        return false;
    }

    //Check factory
    if (isEmpty(factory)) {
        ShowMessage("Search style", "Please select factory", ObjMessageType.Info);
        return false;
    }

    //Check factory
    if (isEmpty(buyer)) {
        ShowMessage("Search style", "Please select buyer", ObjMessageType.Info);
        return false;
    }

    return true;
}