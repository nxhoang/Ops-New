/*jQuery operation plan group table rows
Version: 1.0
Author: Nguyen Xuan Hoang
Released date: 27-Mar-2020
 */
(function ($) {
    $.fn.opGroupTablePlugin = function (options) {
        const settings = $.extend({
            btnDisplayTableId: null,
            iconDisplayId: null,
            opGroupTableDivId: null,
            opGroupTbodyTableId: null,
            isShowTable: true,
            opGroups: []
        }, options);

        return this.each((i, e) => {
            // Clearing html content
            e.innerHTML = "";
            const displayTable = settings.isShowTable ? "block" : "none";

            if (settings.btnDisplayTableId && settings.iconDisplayId && settings.opGroupTableDivId) {
                e.innerHTML =
                    `<button id="${settings.btnDisplayTableId}" style="position: absolute; top: 0; right: 0">
                        <i id="${settings.iconDisplayId}" class="glyphicon glyphicon-plus-sign"></i>
                    </button>
                    <div id="${settings.opGroupTableDivId}" class="row" style="display: ${displayTable}; margin: 24px 0 0 0;">
                        <table id="${settings.opGroupTableDivId}Tb" class="table" style="border: 1px solid #ccc; margin-bottom: 0; background: rgb(248, 248, 255, 0.9)">
                            <thead>
                                <tr>
                                    <th class="col-lg-3" style="border-top: unset">Color</th>
                                    <th class="col-lg-6" style="border-top: unset">Group Name</th>
                                    <th class="col-lg-3" style="border-top: unset">TotalTime</th>
                                </tr>
                            </thead>
                            <tbody id="${settings.opGroupTbodyTableId}"></tbody>
                        </table>
                    </div>`;

                displayOpGroup();
                e.bindOpGroup = addGroupToTable;
            } else {
                if (settings.btnDisplayTableId === null ||
                    settings.btnDisplayTableId === undefined ||
                    settings.btnDisplayTableId === "") {
                    console.error("btnDisplayTableId can't be null or empty.");
                }
                if (settings.iconDisplayId === null ||
                    settings.iconDisplayId === undefined ||
                    settings.iconDisplayId === "") {
                    console.error("iconDisplayId can't be null or empty.");
                }
                if (settings.opGroupTableDivId === null ||
                    settings.opGroupTableDivId === undefined ||
                    settings.opGroupTableDivId === "") {
                    console.error("opGroupTableDivId can't be null or empty.");
                }
            }

            window.addEventListener("click", () => {
                // Closing table and return plus sign if clicking outside of table
                $(`#${settings.iconDisplayId}`).addClass("glyphicon-plus-sign").removeClass("glyphicon-minus-sign");
                $(`#${settings.opGroupTableDivId}`).hide("slow", 'easeOutBounce');
            });
        });

        function displayOpGroup() {
            if (settings.btnDisplayTableId && settings.iconDisplayId && settings.opGroupTableDivId) {
                $(`#${settings.btnDisplayTableId}`).on("click", (e) => {
                    e.preventDefault();
                    e.stopPropagation();

                    $(`#${settings.iconDisplayId}`).toggleClass("glyphicon-plus-sign glyphicon-minus-sign");
                    $(`#${settings.opGroupTableDivId}`).slideToggle("slow", 'easeOutBounce');
                });

                $(`#${settings.opGroupTableDivId}Tb`).on("click", (e) => {
                    e.preventDefault();
                    e.stopPropagation();

                    // do nothing to prevent window click event that close table
                });
            }
        }

        function addGroupToTable(opGroups) {
            document.getElementById(settings.opGroupTbodyTableId).innerHTML = "";
            if (opGroups) {
                const trRows = [];
                for (let g of opGroups) {
                    const r =
                        `<tr><td><div style="width: 50px; height: 25px; background-color: ${g.BackgroundColor}"></div></td>` +
                        `<td>${g.CodeName}</td><td>${g.OpTime}</td></tr>`;
                    trRows.push(r);
                }
                $(`#${settings.opGroupTbodyTableId}`).html(trRows);
            } else {
                console.error("List of op-groups can't be null or empty.");
            }
        }
    };
}(jQuery));