
function convertArrayOfObjectsToCSV(args) {
    var result, ctr, keys, columnDelimiter, lineDelimiter, data;
    data = args.data || null;
    if (data == null || !data.length) {
        return null;
    }
    columnDelimiter = args.columnDelimiter || ',';
    lineDelimiter = args.lineDelimiter || '\n';
    keys = Object.keys(data[0]);
    result = '';
    result += keys.join(columnDelimiter);
    result += lineDelimiter;
    data.forEach(function (item) {
        ctr = 0;
        keys.forEach(function (key) {
            if (ctr > 0) result += columnDelimiter;
            result += "\"" + CheckNullData(item[key]) + "\"";
            ctr++;
        });
        result += lineDelimiter;
    });
    return result;
}

function CheckNullData(value) {
    if (isEmpty(value)) return "";
    else return value;
}


function DowloadProcessToExcel() {
    var objOpMaster = GetSelectedOneRowData(gridOpsTableId);
    
    //var objOpsKey = {
    //    styleCode: objOpMaster.StyleCode,
    //    styleSize: objOpMaster.StyleSize,
    //    styleColor: objOpMaster.StyleColorSerial,
    //    revNo: objOpMaster.RevNo,
    //    opRevNo: objOpMaster.OpRevNo,
    //    edition: objOpMaster.Edition,
    //    languageId: objOpMaster.Language
    //};

    if ($.isEmptyObject(objOpMaster)) {
        ShowMessageOk("001", SmsFunction.Update, MessageType.Warning, MessageContext.NoData, ObjMessageType.Alert);
        return;
    }   

    let lanId = $("#drpLanguages").val();

    console.log('language id: '+ lanId);

    //window.open(`/ExportExcel/ExportProcessDetailToExcel/?styleCode=${objOpMaster.StyleCode}&styleSize=${objOpMaster.StyleSize}&styleColorSerial=${objOpMaster.StyleColorSerial}&revNo=${objOpMaster.RevNo}&opRevNo=${objOpMaster.OpRevNo}&edition=${objOpMaster.Edition}&languageId=${objOpMaster.Language}`); 
    window.open(`/ExportExcel/ExportProcessDetailToExcel/?styleCode=${objOpMaster.StyleCode}&styleSize=${objOpMaster.StyleSize}&styleColorSerial=${objOpMaster.StyleColorSerial}&revNo=${objOpMaster.RevNo}&opRevNo=${objOpMaster.OpRevNo}&edition=${objOpMaster.Edition}&languageId=${lanId}`);//ADD - SON) 23/Mar/2020 - get language from dropdownlist

}

function downloadCSV(args) {
    var objOpsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));
    if ($.isEmptyObject(objOpsMaster)) {
        return;
    }

    var objOpsKey = {
        styleCode: objOpsMaster.StyleCode,
        styleSize: objOpsMaster.StyleSize,
        styleColor: objOpsMaster.StyleColorSerial,
        revNo: objOpsMaster.RevNo,
        opRevNo: objOpsMaster.OpRevNo,
        edition: objOpsMaster.Edition,
        languageId:  objOpsMaster.Language
    };

    //Get list operation plan detail
    var lstOpDetail = GetListOpsDetail(objOpsKey);
    
    //Filter data
    lstOpDetail = GetOpsDetailDataForExport(lstOpDetail);

    var data, filename, link;
    var csv = convertArrayOfObjectsToCSV({
        data: lstOpDetail //opsData
    });
    if (csv == null) return;
    filename = args.filename || 'export.csv';
    if (!csv.match(/^data:text\/csv/i)) {
        csv = 'data:text/csv;charset=utf-8,%EF%BB%BF' + encodeURI(csv); //format unicode
    }
    data = csv; 
    link = document.createElement('a');    
    link.setAttribute('href', data);
    link.setAttribute('download', filename);
    link.click();
}

//Filter data for exporting
function GetOpsDetailDataForExport(lstOpdts) {
    var newLstOpsDetails = [];
    if (ArrayListIsNull(lstOpdts)) {
        return [];
    }
    var newOpdt;
    for (var i = 0; i < lstOpdts.length; i++) {
        newOpdt = {
            OpRevNo: lstOpdts[i].OpRevNo,
            OpSerial: lstOpdts[i].OpSerial,
            OpType: lstOpdts[i].OpType,
            OpNum: lstOpdts[i].OpNum,
            OpGroup: lstOpdts[i].OpGroup,
            OpName: lstOpdts[i].OpName,
            MachineName: lstOpdts[i].MachineName,
            OpDesc: lstOpdts[i].OpDesc,
            OpTime: lstOpdts[i].OpTime,
            OpPrice: lstOpdts[i].OpPrice,
            OfferOpPrice: lstOpdts[i].OfferOpPrice,
            MachineCount: lstOpdts[i].MachineCount,
            Remarks: lstOpdts[i].Remarks,
            MaxTime: lstOpdts[i].MaxTime,
            ManCount: lstOpdts[i].ManCount
        }

        newLstOpsDetails.push(newOpdt);
    }
  
    return newLstOpsDetails;
}


