const ACloseEmpPopup = "aCloseEmpPopup";

var hrmEmpApp = angular.module("hrm-emp-app", []);

hrmEmpApp.controller("HrmEmpAngCtr", ($scope) => {
    $scope.Employee = window.currentClickedEmpRow;
});

function CloseEmpPopup() {
    document.getElementById(ACloseEmpPopup).addEventListener("click", () => {
        document.getElementById(PopupEmp).style.display = "none";
    });
}