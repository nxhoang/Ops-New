const getSaleTeams = (callBack) => {
    var config = { url: "/DataMaster/GetSaleTeams", postData: null, async: true };
    AjaxGetCommon(config, function (response) { callBack(response); });
}

const getProductTeams = (callBack) => {
    var config = { url: "/DataMaster/GetProductTeams", postData: null, async: true };
    AjaxGetCommon(config, function (response) { callBack(response); });
}

const getBuyers = (callBack) => {
    var config = { url: "/DataMaster/GetBuyers", postData: JSON.stringify({ teamId: null }), async: true };
    AjaxPostCommon(config, function (response) { callBack(response); });
}

const getBuyersBySaleTeam = (teamId, callBack) => {
    var config = { url: "/DataMaster/GetBuyersByTeam", postData: JSON.stringify({ teamId: teamId }), async: true };
    AjaxPostCommon(config, function (response) { callBack(response); });
}

const getBuyersByFactory = (factoryId, callBack) => {
    var config = { url: "/DataMaster/GetBuyersByFactory", postData: JSON.stringify({ factoryId: factoryId }), async: true };
    AjaxPostCommon(config, function (response) { callBack(response); });
}

const getModuleColorsAsync = (callBack) => {
    var config = { url: "/DataMaster/GetModuleColors", postData: null, async: true };
    AjaxPostCommon(config, function (response) { callBack(response); });
}