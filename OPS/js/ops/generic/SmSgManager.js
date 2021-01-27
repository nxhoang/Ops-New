function GetMsg(contextSerial, sysId, menuId, funt, type, context, lang, handleLang) {
    var msgId = contextSerial + sysId + menuId + funt + type;
    var value = ReadBakeCookie(msgId);
    if (value) {
        handleLang(GetMsgByLang(value, lang));
    } else {
        //var result;
        var opToolConfig = {
            url: "/SmSg/GetMsgByItem",
            postData: JSON.stringify({ contextSerial: contextSerial, sysId: sysId, menuId: menuId, funt: funt, type: type, context: context })
        };
        AjaxPostCommon(opToolConfig, function (response) {
            //BakeCookie(msgId, response);
            var result = GetMsgByLang(response, lang);
            handleLang(result);
        });
        //return result;
    }
}

function GetMsgAsin(contextSerial, sysId, menuId, funt, type, context, lang) {
    var msgId = contextSerial + sysId + menuId + funt + type;
    var value = ReadBakeCookie(msgId);
    if (value) {
        return GetMsgByLang(value, lang);
    } else {
        var result;
        var opToolConfig = {
            url: "/SmSg/GetMsgByItem",
            async: false,
            postData: JSON.stringify({ contextSerial: contextSerial, sysId: sysId, menuId: menuId, funt: funt, type: type, context: context })
        };
        AjaxPostCommon(opToolConfig, function (response) {
            result = GetMsgByLang(response, lang);
        });
        return result;
    }
}


function GetMessageById(contextSerial, sysId, menuId, funt, type, context, callBack) {
    var opToolConfig = {
        url: "/SmSg/GetMsgByItem",
        postData: JSON.stringify({
            contextSerial: contextSerial, sysId: sysId, menuId: menuId, funt: funt, type: type, context: context
        })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        callBack(response);
    });
}

function GetMsgByLang(value, lang) {
    var result;
    var title = value.Title;
    if (!title) {
        title = value.Type;
    }
    switch (lang) {
        case "en":
            result = value.English;
            break;
        case "vi":
            result = value.Vietnamese;
            break;
        case "id":
            result = value.Indonesian;
            break;
        case "ko":
            result = value.Korean;
            break;
        case "my":
            result = value.Myanmar;
            break;
        case "et":
            result = value.Amharic;
            break;
        default:
            result = value.English;
            break;
    }
    if (!result) {
        result = "";
    }
    var msg = {
        title: title,
        value: result
    };
    return msg;
}

function ReplaceStr(s, d) {
    if (d) {
        return s.replace("{}", d);
    }
    return s.replace("{}", ' ');
}

function CreateCookie(name, value, hourse) {
    if (hourse) {
        var date = new Date();
        date.setTime(date.getTime() + hourse * 60 * 60 * 1000);
        var expires = '; expires=' + date.toGMTString();
    } else {
        expires = '';
    }
    document.cookie = name + '=' + encodeURIComponent(value) + expires + '; path=/';
}

function ReadCookie(name) {
    var nameEQ = name + '=';
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = decodeURIComponent(ca[i]);
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

//cookie object

function BakeCookie(name, value) {
    var cookie = [name, '=', JSON.stringify(value), '; path=/;'].join('');
    document.cookie = cookie;
}


function ReadBakeCookie(name) {
/*
    var result = document.cookie.match(new RegExp(name + '=([^;]+)'));
    result && (result = JSON.parse(result[1]));
    return result;
*/
    return false;
}

function DelBakeCookie(name) {
    document.cookie = [name, '=; expires=Thu, 01-Jan-1970 00:00:01 GMT; path=/;'].join('');
}