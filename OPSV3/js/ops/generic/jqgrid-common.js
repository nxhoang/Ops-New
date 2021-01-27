arrButtonName = {
    edittext: 'Edit',
    addtext: 'Add',
    deltext: 'Delete',
    searchtext: 'Search',
    refreshtext: 'Refresh'
};
arrPopup = {
    captionEdit: 'Edit',
    submitEdit: 'Summit',
    cancel: 'Cancel'
};
arrTitle = {
    textAnd: 'And',
    textOr: 'Or',
    coppyRow: 'Coppy'
};
arrSession = {
    deleted: 'true',
    user: 'Admin'
};
arrButtonAction = {
    all: 'All',
    save: 'Save',
    deleted: 'delete'
};
function SetPaging(gridTableId, navPageName) {
    var rowtext = $('#pg_' + navPageName).find('.ui-pg-selbox  option:selected').text();
    var rownum = $('#pg_' + navPageName).find('.ui-pg-selbox').val();
    if (rowtext === arrButtonAction.all) {
        gridTableId.jqGrid('setGridParam', { scroll: 1, page: 1, rowNum: 20, scrollrows: false });
        $("#" + navPageName + "_center table tbody tr td").css('display', 'none');
        $("#" + navPageName + "_center table tbody tr td").last().show();
    }
    else {
        gridTableId.jqGrid('setGridParam', { scroll: false, page: 1, rowNum: rownum, scrollrows: true });
        $(".ui-jqgrid-bdiv").children().css('height', 'auto');
        $("#" + navPageName + "_center table tbody tr td").show();
    }
    gridTableId.trigger('reloadGrid');
}
function SearchFilter(grid) {
    grid.jqGrid("filterToolbar", {
        stringResult: true, searchOnEnter: false,
        defaultSearch: "cn", ignoreCase: true, enableCstringResult: true, autoencode: false
    });
}