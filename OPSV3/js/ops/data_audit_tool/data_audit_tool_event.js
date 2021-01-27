const eventSelectionDropdownlist = () => {
    $('#drpTeamGroup').change(() => {
        //get list teams
        const teamGroup = $('#drpTeamGroup').val();
        //if team group equal 1 then get sale team otherwise get product team.
        switch (teamGroup) {
            case '1':
                getSaleTeams(response => FillDataToDropDownlist("drpTeam", response, "RoleId", "RoleDesc"));
                break;
            case '2':
                getProductTeams(response => FillDataToDropDownlist("drpTeam", response, "FactoryId", "FactoryName"));
                break
            default:
            //clear data in dropdonwlist
        }
    });

    $('#drpTeam').change(() => {
        //get list buyer buy team
        const teamGroup = $('#drpTeamGroup').val();
        if (teamGroup === '1') getBuyerList();
    });

    $('#btnSearchStyle').click(() => {
        let startDate = '';
        let endDate = '';
        const dateRange = $('#txtDateRange').val();
        if (!isEmptyOrWhiteSpace(dateRange)) {
            const arrDateRange = dateRange.split(' - ')
            startDate = arrDateRange[0];
            endDate = arrDateRange[1];
        }
        const postData = { buyer: $('#drpBuyer').val(), startDate: startDate, endDate: endDate, aoNumber: $('#txtAoNumber').val(), styleInfo: $('#txtStyleInfo').val() };
        ReloadJqGrid2LoCal('tbStyle', postData);
    });
}