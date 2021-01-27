
function eventClickSearchStyle() {
    $("#btnSearchStyleMdl").click(function () {
        //Reload style grid view
        var aoNo = $("#txtAoNumberMdl").val();
        var buyer = $("#drpBuyerMdl").val();
        var factory = $("#drpFactoryMdl").val();
        var buyerInfo = $("#txtBuyerInfoMdl").val();

        reloadStyleGrid(aoNo, factory, buyer, buyerInfo);
       
    });

    $("#btnSearchStyle").click(function () {
        //Reload style grid view
        var aoNo = $("#txtAoNumber").val();
        var buyer = $("#drpBuyer").val();
        var factory = $("#drpFactory").val();
        var buyerInfo = $("#txtBuyerInfo").val();
        var lineCd = $("#drpLine").val();
        
        //Get value from page and set it on modal
        $("#drpFactoryMdl").val(factory).trigger('change'); 
        $("#drpLineMdl").val(lineCd).trigger('change'); 
        $("#drpBuyerMdl").val(buyer).trigger('change'); 
        $("#txtBuyerInfoMdl").val(buyerInfo);
        $("#txtAoNumberMdl").val(aoNo);

        reloadStyleGrid(aoNo, factory, buyer, buyerInfo);

        $('#mdlStyleAo').modal('show');

    });
}

function reloadStyleGrid(aoNo, factory, buyer, buyerInfo) {
   
    if ($.isEmptyObject(factory)) {
        ShowMessage("Search style", "Please select factory", ObjMessageType.Info);
        return;
    }

    if ($.isEmptyObject(aoNo)) {
        ShowMessage("Search style", "Please enter AO number", ObjMessageType.Info);
        return;
    }

    var params = { aoNo: aoNo, buyer: buyer, factory: factory, buyerInfo: buyerInfo };
    ReloadJqGrid2LoCal(tblStyleAoName, params);
}

function eventFactorySelected() {

    $("#drpFactory").change(function () {
        var curDate = getCurrentDate();
        var fac = $(this).val();
       
        GetLinesByFactoryDgs("drpLine", "P2C1", "2019-01-30");
        //GetLinesByFactoryDgs("drpLine", fac, curDate);
    });

    $("#drpFactoryMdl").change(function () {
        var curDate = getCurrentDate();
        var fac = $(this).val();

        GetLinesByFactoryDgs("drpLineMdl", "P2C1", "2019-01-30");
        //GetLinesByFactoryDgs("drpLineMdl", fac, curDate);
    });

    //$('#drpFactory').on('select2:select', function (e) {
    //    var data = e.params.data;
    //    console.log(data);

    //    var curDate = getCurrentDate();
    //    var fac = $(this).val();
    //    console.log(curDate + " - " + fac);

    //    GetLinesByFactoryDgs("drpLine", "P2C1", "2019-01-29");
    //});
}

