
function selectedCategory() {
    $("#drpDefectCat").change(function () {
        //Reload defect code list by category
        var params = { defectCat: $(this).val() };
        ReloadJqGrid2LoCal("tbDefectCode", params);
    });
}