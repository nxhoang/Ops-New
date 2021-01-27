const TbEmp = "tbEmp", TbEmpPager = "#tbEmpPager", jqTbEmp = `#${TbEmp}`, PopupEmp = "popupEmp";
var currentClickedEmpRow;

function InitJqGrid() {
    jQuery(jqTbEmp).jqGrid({
        datatype: "local",
        height: 300,
        width: null,
        pager: TbEmpPager,
        viewrecords: true,
        shrinkToFit: false,
        rowNum: 10,
        rowList: [20, 50, 100],
        colNames: ["Id", "Name", "Gender", "Department", "Position", "Skill", "Skill Level", "Image", "ImageUrl", "CorporationCode"],
        colModel: [
            { name: "EmployeeCode", index: "EmployeeCode", width: 90, sorttype: "string", key: true },
            { name: "Name", index: "Name", width: 250, sorttype: "string" },
            { name: "Gender", index: "Gender", width: 100, align: "center", sorttype: "string" },
            { name: "Department", index: "Department", width: 150, align: "center", sorttype: "string" },
            { name: "Position", index: "Position", width: 150, sorttype: "string" },
            { name: "Skill", index: "Skill", width: 150, sorttype: "string" },
            { name: "SkillLevel", index: "SkillLevel", width: 80, sorttype: "string" },
            {
                name: "FullImageUrl", index: "FullImageUrl", width: 150, sorttype: "string", align: "center",
                formatter: (cellValue, opts, rowObj) => {
                    if (cellValue) {
                        return `<img style="width: 45px; height: 60px" src="${cellValue}" alt="Employee Image" />`;
                    } else {
                        return `<img style="width: 45px; height: 60px" src="/Assets/hrm/common/img/NoImg.PNG" alt="Employee Image" />`;
                    }
                }
            },
            { name: "FullImageUrl", index: "ImageUrl", width: 150, sorttype: "string", hidden: true },
            { name: "CorporationCode", index: "CorporationCode", width: 150, sorttype: "string", hidden: true }
        ],
        caption: "List of Workers",
        ondblClickRow: (id) => {
            currentClickedEmpRow = jQuery(jqTbEmp).jqGrid('getRowData', id);
            document.getElementById(PopupEmp).style.display = "block";

            console.log(currentClickedEmpRow);

            const appElement = document.querySelector("[ng-app=hrm-emp-app]"),
                $scope = angular.element(appElement).scope();
            $scope.$apply(() => {
                $scope.Employee = currentClickedEmpRow;
            });
        }
    });
}