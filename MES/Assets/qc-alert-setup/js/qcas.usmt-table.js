﻿const TbUsmt = "tbUsmt", TbUsmtPager = "#tbUsmtPager", jqTbUsmt = `#${TbUsmt}`;
var currentClickedEmpRow;

function InitPkUserJqGrid() {
    jQuery(jqTbUsmt).jqGrid({
        datatype: "local",
        height: 300,
        width: null,
        pager: TbUsmtPager,
        viewrecords: true,
        shrinkToFit: false,
        rowNum: 10,
        rowList: [20, 50, 100],
        colNames: ["Id", "Name", "Email", "Phone", "Factory", "Buyer"],
        colModel: [
            { name: "Id", index: "Id", width: 90, sorttype: "string", hidden: true },
            { name: "Name", index: "Name", width: 250, sorttype: "string" },
            { name: "Email", index: "Email", width: 100, align: "center", sorttype: "string" },
            { name: "Phone", index: "Phone", width: 150, align: "center", sorttype: "string" },
            { name: "Factory", index: "Factory", width: 150, sorttype: "string" },
            { name: "Buyer", index: "Buyer", width: 150, sorttype: "string" }
        ],
        caption: "List of Users",
        ondblClickRow: (id) => {
            currentClickedEmpRow = jQuery(jqTbUsmt).jqGrid('getRowData', id);

            console.log(currentClickedEmpRow);
        }
    });
}