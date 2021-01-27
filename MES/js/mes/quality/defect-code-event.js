
function selectedCategory() {
    $("#drpDefectCat").change(function () {
        //Reload defect code list by category
        var params = { defectCat: $(this).val() };
        ReloadJqGrid2LoCal("tbDefectCode", params);
    });

    $("#drpLanguage").change(function () {
        
        //Hide all lang column jqgrid
        jQuery(tbDefectCodeId).jqGrid('hideCol', ["Vietnamese", "Bahasa", "Burmese", "Amharic"]);

        let languageId = $(this).val();
        switch (languageId) {
            case _languageId.Vietnamese:
                jQuery(tbDefectCodeId).jqGrid('showCol', ["Vietnamese"]);
                break;
            case _languageId.Bahasa:
                jQuery(tbDefectCodeId).jqGrid('showCol', ["Bahasa"]);
                break;
            case _languageId.Burmese:
                jQuery(tbDefectCodeId).jqGrid('showCol', ["Burmese"]);
                break;
            case _languageId.Amharic:
                jQuery(tbDefectCodeId).jqGrid('showCol', ["Amharic"]);
                break;
            default:
        }

        resizeDefectCodeJqgrid();
    });
    
}