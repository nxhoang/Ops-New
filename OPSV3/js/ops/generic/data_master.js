const getSaleTeams = (callBack) => {
    var config = { url: "/DataMaster/GetSaleTeams", postData: null, async: true };
    AjaxGetCommon(config, function (response) { callBack(response); });
}

const getProductTeams = (callBack) => {
    var config = { url: "/DataMaster/GetProductTeams", postData: null, async: true };
    AjaxGetCommon(config, function (response) { callBack(response); });
}

const getBuyers = (teamId, callBack) => {
    var config = { url: "/DataMaster/GetBuyers", postData: JSON.stringify({ teamId: teamId }), async: true };
    AjaxGetCommon(config, function (response) { callBack(response); });
}