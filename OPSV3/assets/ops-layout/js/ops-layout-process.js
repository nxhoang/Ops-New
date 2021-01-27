// This function is copied from ops-layout.js file
function createSummary(sumType, data, language) {
    const sumModal = `#${sumType}`;
    const isExisted = $(sumModal).is(":visible");
    const curOpmt = GetSelectedOneRowData(gridOpsTableId);
    if ($.isEmptyObject(curOpmt)) return;

    // Get list of Operation name
    if (!isExisted) {
        $(".ops-node-tool").show();

        const config = {
            url: "/OpsLayout/SummarizeProcesses",
            async: true,
            postData: JSON.stringify({ opmt: curOpmt })
        };
        switch (sumType) {
            case "sumAllMachines":
                // vithv
                config.url = "/OpsLayout/SummarizeOpdtAllMachine";
                AjaxPostCommon(config, (response) => {
                    if (response !== null && response !== undefined) {
                        const processSums = response.machine;
                        let content = "";
                        for (let p of processSums) {
                            content += `<label>${p.ItemCode} - ${p.ItemName} - ${p.Total}</label><br />`;
                        }

                        $(".jtk-surface-canvas")
                            .append(`<div id="${sumType}" class="opsum" style="width: 350px; z-index:1900; position: relative; top: ${data.top}; left: ${data.left}">
                                <button class="opsum-btn-close" onclick="removeSummary($(${sumType}))">✖</button>
                                <fieldset class="opsum-machine scheduler-border sum-col-border">
                                <legend class="scheduler-border">Machines Link Summary</legend>
                                ${content}
                                </fieldset>
                            </div>`);

                        $(sumModal).draggable();
                    }
                });
                break;
            case "sumMainMachines":
                config.url = "/OpsLayout/SummarizeOpdtByMachine";
                AjaxPostCommon(config, (response) => {
                    if (response !== null && response !== undefined) {
                        const processSums = response;
                        let content = "";
                        for (let p of processSums) {
                            if (p.MachineType !== null) {
                                content += `<label>${p.MachineName} ${p.MachineType} - ${p.TotalMachines}</label><br />`;
                            }
                        }

                        $(".jtk-surface-canvas")
                            .append(`<div id="${sumType}" class="opsum" style="width: 350px; z-index:1900; position: relative; top: ${data.top}; left: ${data.left}">
                                <button class="opsum-btn-close" onclick="removeSummary($(${sumType}))">✖</button>
                                <fieldset class="opsum-machine scheduler-border sum-col-border">
                                <legend class="scheduler-border">Machines Summary</legend>
                                ${content}
                                </fieldset>
                            </div>`);

                        $(sumModal).draggable();
                    }
                });
                break;
            case "sumTool":
                config.url = "/OpsLayout/SummarizeOpdtByTools";
                AjaxPostCommon(config, (response) => {
                    const processSums = response.tools;
                    const processes = response.processes;
                    let content;

                    if (processSums.length !== 0 && processes.length !== 0) {
                        let i = 0;
                        let catId = processSums[0].CategId;
                        content = `<span><b>${processSums[0].Category}</b></span><br />`;

                        for (let p of processSums) {
                            i++;
                            if (catId !== p.CategId) {
                                content += `<span><b>${p.Category}</b></span><br />`;
                                catId = p.CategId;
                            }

                            const imgSrc = p.ImagePath === null ? "../img/no-image.png" :srcImg + p.ImagePath;
                            content += `<img onclick="ShowImgTools($(this))" class="sumtool-img" src="${imgSrc}" alt=" " /><span>${i}-${p.ItemName} - (${p.Total})</span><br />`;

                        // Set ordinal of tools for list of processes
                            for (let n of processes) {
                                if (p.ItemCode === n.ItemCode) n.Ordinal = i.toString();
                            }
                        }

                        // Loop list of nodes to assign ordinal of tools
                        toolkit.eachNode((index, node) => {
                            const tools = [];

                            processes.forEach((v) => {
                                if (v.OpSerial.toString() === node.data.id) {
                                    const tool = {
                                        ToolCode: v.ItemCode,
                                        Ordinal: v.Ordinal
                                    };

                                    tools.push(tool);
                                }
                            });
                            tools.sort((a, b) => {
                                return a.Ordinal > b.Ordinal;
                            });

                            let l = 4;
                            tools.forEach((v, i) => {
                                if (i === 0) {
                                    v.left = l;
                                } else {
                                    l = l + 13;
                                    v.left = l;
                                }
                            });

                            node.data.Tools = tools;
                            toolkit.updateNode(node);
                        });
                    }
                    else {
                        content = "<span>No tools for summary</span>";
                    }

                    $(".jtk-surface-canvas")
                        .append(`<div id="${sumType}" class="opsum" style="z-index:1900; position: relative; top: ${data.top}; left: ${data.left}">
                                <button class="opsum-btn-close" onclick="removeSummary($(${sumType}))">✖</button>
                                <fieldset class="opsum-machine scheduler-border sum-col-border">
                                <legend class="scheduler-border">Tools Summary</legend>
                                ${content}
                                </fieldset>
                            </div>`);

                    $(sumModal).draggable();
                });
                break;
            case "sumWorker":
                config.url = "/OpsLayout/SummarizeOpdtByWorker";
                AjaxPostCommon(config,
                    (response) => {
                        const workers = response.workers;
                        let content = "";
                        if (workers !== undefined && workers !== null) {
                            for (let w of workers) {
                                content += `<span>${w.ActionProcess} - ${w.TotalManCount}</span><br />`;
                            }
                            if (workers.length === 0) content += `<span>Data is empty</span>`;

                            $(".jtk-surface-canvas")
                                .append(`<div id="${sumType
                            }" class="opsum" style="z-index:1900; position: relative; top: ${data
                                    .top}; left: ${data.left}">
                                <button class="opsum-btn-close" onclick="removeSummary($(${sumType}))">✖</button>
                                <fieldset class="opsum-machine scheduler-border sum-col-border">
                                <legend class="scheduler-border">Worker Summary</legend>
                                ${content}
                                </fieldset>
                            </div>`);

                            $(sumModal).draggable();
                        }
                    });
                break;
            case "showPattern":
                {
                    var selOpmt = GetSelectedOneRowData(gridOpsTableId);
                    var languageId = language;
                    if ($.isEmptyObject(selOpmt)) return;
                    let configPat = {
                        url: "/OpsLayout/GetPatterns",
                        async: true,
                        postData: JSON.stringify({
                            edition: selOpmt.Edition,
                            styleCode: selOpmt.StyleCode,
                            styleSize: selOpmt.StyleSize,
                            styleColorSerial: selOpmt.StyleColorSerial,
                            revNo: selOpmt.RevNo,
                            opRevNo: selOpmt.OpRevNo,
                            languageId: languageId
                        })
                    };

                    AjaxPostCommon(configPat,
                        function (response) {
                            if (response.error !== "undefined") {
                                //alert(response[0].Url);
                                var contentTable = "";
                                var startTable = `<table class="table table-bordered">
                                            <thead>
                                                <tr>
                                                    <th class ="col-md-5">Process Name</th>
                                                    <th class ="col-md-1">Item Code</th>
                                                    <th class="col-md-3">Description</th>
                                                    <th class="col-md-2">Pattern</th>
                                                    <th class="col-md-1">Qty</th>
                                                </tr>
                                            </thead>`;
                                var endTable = "</table>";
                                $.each(response,
                                    function (i, value) {
                                        contentTable += `<tr>
                                    <td>${value.OpNameLan}</td>
                                    <td>${value.ItemCode}</td>
                                    <td>${value.Piece}</td>
                                    <td><img style="width: 40px; height: 20px;" onclick="ShowPatternImage('${value
                                            .Url}');" src="${value.UrlThumbnail}" ></td>
                                    <td>${value.PieceQty}</td>
                                </tr>`;
                                    });
                                var table = startTable + contentTable + endTable;
                                var content = `<div id="${sumType
                                }" class="opsum" style="width: 550px; z-index:1900; position: relative; top: ${data
                                    .top}; left: ${data.left}">
                         <button class="opsum-btn-close" onclick="removeSummary($(${sumType}))">✖</button>
                             <fieldset class="scheduler-border sum-col-border"  style="width: 550px">
                             <legend class="scheduler-border">Patterns</legend>
                             ${table}
                             </fieldset>
                            </div>`
                                $(".jtk-surface-canvas")
                                    .append(content);
                                $(sumModal).draggable();
                            } else {
                                ShowMessage("Showing pattern", "Something wrong.", MessageTypeError);
                                alert(response.error);
                            }
                        });
                }
                break;
            case "sumBom":
                if (curOpmt.Edition === "M") return;
                var configSumBom = {
                    url: "/OpsLayout/SummarizeBomByProcess", async: true,
                    postData: JSON.stringify({ opmt: curOpmt, language: language })
                };

                AjaxPostCommon(configSumBom, function (response) {
                    if (response.error === undefined || response.error === null) {
                        var contentTable = "";
                        var startTable = `<table class="table table-bordered" style="margin-bottom: 0">
                                            <thead>
                                                <tr>
                                                    <th>Process Name</th>
                                                    <th>Main Item Code</th>
                                                    <th>Item Code</th>
                                                    <th>Item Name</th>
                                                    <th>Color</th>
                                                    <th>Consumption</th>
                                                </tr>
                                            </thead>`;
                        var endTable = "</table>";
                        $.each(response.prots, function (i, value) {
                            const consumptionUnit = (value.UnitConsumption === undefined || value.UnitConsumption === null)
                            ? "0" : value.UnitConsumption;
                            const opName = (value.OpNameLan === undefined || value.OpNameLan === null) ? value.OpName :
                                value.OpNameLan;
                            contentTable += `<tr>
                                    <td>${opName}</td>
                                    <td>${value.MainItemCode}</td>
                                    <td>${value.ItemCode}</td>
                                    <td>${value.ItemName}</td>
                                    <td>${value.StyleColorways}</td>
                                    <td>${consumptionUnit} ${value.ConsumpUnit}</td>
                                </tr>`;
                        });
                        var table = startTable + contentTable + endTable;
                        var content = `<div id="${sumType}" class="opsum" style="width: 800px; z-index:1900;
                            position: relative; top: ${data.top}; left: ${data.left}">
                         <button class="opsum-btn-close" onclick="removeSummary($(${sumType}))">✖</button>
                             <fieldset class="scheduler-border sum-col-border" style="width: 800px">
                             <legend class="scheduler-border">Material Summary</legend>
                             ${table}
                             </fieldset>
                            </div>`;
                        $(".jtk-surface-canvas").append(content);
                        $(sumModal).draggable();
                    } else {
                        ShowMessage("Error", response.error, MessageTypeError);
                    }
                });
                break;
            case "jigSum":
                if (curOpmt.Edition === "M") return;
                {
                    const sumJigMode = "ModuleName";
                    const configSumJigFileByModule = {
                        url: "/OpsLayout/SummarizeJigFile", async: true,
                        postData: JSON.stringify({ opmt: curOpmt, language: language, sumJigMode: sumJigMode })
                    };

                    AjaxPostCommon(configSumJigFileByModule, function (response) {
                        if (response.error === undefined || response.error === null) {
                            const table = createTableForJigSummary(response, sumJigMode, curOpmt);
                            appendTableToSurface(table, sumType, sumModal, data);
                        } else {
                            ShowMessage("Error", response.error, MessageTypeError);
                        }
                    });
                }
                break;
            default:
                {
                    let factory = "";
                    const factoryId = (curOpmt.Factory === null || curOpmt.Factory === undefined) ? "" : curOpmt.Factory;
                    if (curOpmt.Edition === "A") {
                        factory = `<br /><label>Factory - ${factoryId}</label>`;
                    }
                    AjaxPostCommon(config, (response) => {
                        if (response.processSummary !== null && response.style !== null &&
                            response.processSummary !== undefined && response.style !== undefined) {
                            const style = response.style;
                            const processSum = response.processSummary;
                            let content = "";
                            let content1 = "";

                            switch (sumType) {
                                case "sumTitle":
                                    content = `<label><b>Style - ${style.styleCode} (${style.BuyerStyleCode} - ${style.buyerName})</b></label><br />
                                        <label style="width: 155px;">Color - ${style.styleColor}</label><br />
                                        <label style="width: 155px;">Size - ${style.styleSize}</label> <br />`;
                                    content1 = `<div class="opsum-img"><img class="main-summary-img" src="${style.picture}" alt=" " /></div>
                                        <p class="current-date">${formatDate(new Date())}</p>`;
                                    break;
                                case "sumEditable":
                                    content = `<textarea class="opsum-textarea" placeholder="Please enter comments / summary here."></textarea>`;
                                    break;
                                default:
                            }
                            $(".jtk-surface-canvas")
                                .append(`<div id="${sumType}" class="opsum" style="width: 550px; z-index:1900; position: relative; top: ${data.top}; left: ${data.left}">
                                <button class="opsum-btn-close" onclick="removeSummary($(${sumType}))">✖</button>
                                <div class="col-sum col-sum-1">
                                     <fieldset class="scheduler-border sum-first-col">
                                        <legend class="scheduler-border">Style</legend>
                                        ${content}
                                        <label>Revision - ${style.revNo}</label><br />
                                        <label>OpRevision - ${style.opRevNo}</label><br />
                                        <label>Edition - ${style.edition}</label>
                                        ${factory}
                                        ${content1}
                                     </fieldset>
                                </div><div class="col-sum col-sum-2">
                                     <fieldset class="scheduler-border sum-col-border">
                                        <legend class="scheduler-border">Summary</legend>
                                        <label>Process Count - ${processSum.ProcessCount}</label><br />
                                        <label>Operation Time - ${processSum.OperationTime}</label><br />
                                        <label>Machine Count - ${processSum.MachineCount}</label><br />
                                        <label>Worker Count - ${processSum.WorkerCount}</label><br />
                                        <label>Target per day - ${processSum.TargetPerDay} (pcs)</label><br />
                                        <label>Target per hour -${processSum.TargetPerHour} (pcs) </label><br />
                                        <label>Takt/Max Time - ${processSum.TaktTime} (s)</label>
                                     </fieldset>
                                </div>
                            </div>`);

                            $(sumModal).draggable();
                        } else {
                            if (response.error !== null && response.error !== null) {
                                ShowMessage("Error", response.error, MessageTypeError);
                            }
                        }
                    });
                }
        }
    }
}

function ShowPatternImage(url) {
    $("#imgPattern").attr("src", "");
    $("#imgPattern").attr("src", url);

    ShowModal("mdlPatternPreview");
}

function changedJigSummary() {
    const curOpmt = GetSelectedOneRowData(gridOpsTableId);
    if ($.isEmptyObject(curOpmt)) return;

    const language = $("#selected-flag").attr("value");
    const sumJigMode = document.getElementById("jigSumSelect").value;
    const configSumJigFileByModule = {
        url: "/OpsLayout/SummarizeJigFile", async: true,
        postData: JSON.stringify({ opmt: curOpmt, language: language, sumJigMode: sumJigMode })
    };

    AjaxPostCommon(configSumJigFileByModule, function (response) {
        if (response.error === undefined || response.error === null) {
            $("#jig-sum-table").remove();

            const table = sumJigMode !== "FileName" ? createTableForJigSummary(response, sumJigMode, curOpmt)
                : createDefaultTableForJigSummary(response, curOpmt);
            $("#jig-sum-div").append(table);
        }
    });
}

function appendTableToSurface(table, sumType, sumModal, data) {
    const content = `<div id="${sumType}" class="opsum" style="width: 700px; z-index:1900;
                                    position: relative; top: ${data.top}; left: ${data.left}">
                                    <button class="opsum-btn-close" onclick="removeSummary($(${sumType}))">✖</button>
                                <fieldset class="scheduler-border sum-col-border" style="width: 700px">
                                <legend class ="scheduler-border">Jig Summary</legend>
                                    <label>Summarize by: <select id="jigSumSelect" onchange="changedJigSummary()">
                                        <option value="ModuleName">Module</option>
                                        <option value="OpGroup">OpGroup</option>
                                        <option value="FileName">FileName</option>
                                    </select></label>
                                    <div id="jig-sum-div">${table}</div>
                                </fieldset></div>`;
    $(".jtk-surface-canvas").append(content);
    $(sumModal).draggable();
}

function createTableForJigSummary(response, sumType, curOpmt) {
    const beginTable = `<table id="jig-sum-table" class="table table-bordered" style="margin-bottom: 0">
        <thead><tr><th>Process Name</th><th>File Name</th></tr></thead>`;
    let contentTable = "";
    let sumBy;
    const ftpInfo = GetFtpLink(response.ftpInfo, curOpmt);

    $.each(response.result, function (i, value) {
        const opName = (value.OpNameLan === undefined || value.OpNameLan === null) ? value.OpName :
            value.OpNameLan;
        const opNum = (value.OpNum === undefined || value.OpNum === null) ? " " : value.OpNum;

        if (sumBy !== value.SumModeId) {
            sumBy = value.SumModeId;
            contentTable += `<tr><td colspan="2"><b>${sumType}: </b>${value.GroupName}</td></tr>`;
        };
        if (CheckImageURL(value.SysFileName.toLowerCase())) {
            const imgUrl = `${ftpInfo}/${value.OpSerial}/${value.SysFileName}`;
            const imgTag = `<img style="width:40px; height:20px;" onclick="ShowPatternImage('${imgUrl}');" src="${imgUrl}">`;

            contentTable += `<tr><td>[${opNum}] ${opName}</td><td>${imgTag} ${value.SysFileName}</td></tr>`;
        } else {
            contentTable += `<tr><td>[${opNum}] ${opName}</td><td>${value.SysFileName}</td></tr>`;
        }
    });
    const table = `${beginTable}${contentTable}</table>`;

    return table;
}

function createDefaultTableForJigSummary(response, curOpmt) {
    const beginTable = `<table id="jig-sum-table" class="table table-bordered" style="margin-bottom: 0">
        <thead><tr><th>File Name</th><th>Process Name</th></tr></thead>`;
    let contentTable = "";
    const ftpInfo = GetFtpLink(response.ftpInfo, curOpmt);

    $.each(response.result, function (i, value) {
        const opName = (value.OpNameLan === undefined || value.OpNameLan === null) ? value.OpName :
            value.OpNameLan;
        const opNum = (value.OpNum === undefined || value.OpNum === null) ? " " : value.OpNum;

        if (CheckImageURL(value.SysFileName.toLowerCase())) {
            const imgUrl = `${ftpInfo}/${value.OpSerial}/${value.SysFileName}`;

            const imgTag = `<img style="width:40px; height:20px;" onclick="ShowPatternImage('${imgUrl}');" src="${imgUrl}">`;
            contentTable += `<tr><td>${imgTag} ${value.SysFileName}</td><td>[${opNum}] ${opName}</td></tr>`;
        } else {
            contentTable += `<tr><td>[${opNum}] ${opName}</td><td>${value.SysFileName}</td></tr>`;
        }
    });
    const table = `${beginTable}${contentTable}</table>`;

    return table;
}