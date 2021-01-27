const eventClickOnButtonModuleTab = () => {
    $('#achAomtopModule').click(() => {
        CREATEMESQCO = false;

        $("#divQcoRanking").hide();
        $("#divAomtop").show();

        _currentTab = 3;

        //Show mes module scheduler
        $('#divMesModuleScheduler').show();
        $('#divMesScheduler').hide();
    });
}