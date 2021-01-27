const SelFactory = "selFactory", SelBuyer = "selBuyer", UriApiFactory = `${document.location.origin}/api/ApiFactory`;
var currentCorp = null, currentFactories;

function getCorporations(uri) {
    ///<summary>
    /// List of corporations is storage in t_cm_cstp table that configure for each country.
    ///</summary >

    $.getJSON(uri).done((data) => {
        if (data && data.length > 0 && data[0].CorpCode) {
            currentCorp = data[0].CorpCode;
            getFactoriesByCorporation(UriApiFactory, data[0].CorpCode, 1);
        } else {
            console.log("There is not Corporation");
            console.log(data);
        }
    });
}

function insertOptToFacSel(data) {
    const ops = [];
    for (var e of data) {
        const op = {
            label: e.FactoryName,
            title: e.FactoryId,
            value: e.FactoryName
        };
        ops.push(op);
    }

    $(`#${SelFactory}`).multiselect('dataprovider', ops);
}

function getFactoriesByCorporation(uri, corporation, desDatabase) {
    const l = `${uri}/?corporation=${corporation}&tenantId=${desDatabase}`;
    $.getJSON(l).done(function (res) {
        if (res) {
            insertOptToFacSel(res);
            currentFactories = res;
        } else {
            console.log("There is no factory.");
        }
    });
}

function InitMultiSelect(select, onChange, onSelectAll) {
    ///<summary>
    /// Initialize multi-select control
    /// <param name="select">select id</param>
    /// <param name="onChange">onchange event</param>
    /// <param name="onSelectAll">select all event</param>
    ///</summary >

    $(`#${select.selId}`).multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: select.buttonWidth,
        maxHeight: select.maxHeight,
        onChange: (option, checked) => {
            onChange(checked, option[0].value);
        },
        onSelectAll: (isCheck) => {
            onSelectAll(isCheck);
        }
    });
}

function GetBuyer(mCode, codeStatus) {
    const config = ObjectConfigAjaxPost("../MasterData/GetMasterCodesMySql", true, JSON.stringify({ mCode: mCode, codeStatus: codeStatus }));
    AjaxPostCommon(config, (res) => {
        insertOptToBuyerSel(res);
    });
}

function insertOptToBuyerSel(data) {
    const ops = [];
    for (var e of data) {
        const op = {
            label: `${e.SubCode} - ${e.CodeName}`,
            title: e.CodeName,
            value: e.SubCode
        };
        ops.push(op);
    }

    $(`#${SelBuyer}`).multiselect('dataprovider', ops);
}