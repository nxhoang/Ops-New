var dateTimeFormat = "YYYY/MM/DD";

function createGuid() {
    return 'xxxxxxxxxxxxxxxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

$(function () {
    $('*[data-format="DateRangeOpener"]').on('click', function () {
        let $this = $(this);
        let $correspondingID; /*= $this.attr('for');*/
         
        if ($this.attr('for'))
            $correspondingID = $('#' + $this.attr('for'));
        else
            $correspondingID = $this;
         
        var parentDIV = $correspondingID.closest('div');
         
        var _GUID = createGuid();
 
        var $DivDateRangeOpener = $("<div></div>", {
            "data-role": "DateRangeOpener"
            , "id": _GUID
            , "css": {
                "display": "block",
                "width": "fit-content",
                "border": "1px solid grey",
                "border-radius": "4px",
                "padding-left": "5px",
                "padding-bottom": "5px",
                "position": "absolute",
                "z-index": "3",
                "background-color": "black",
                "left": "13px",
                "top": "" + (parentDIV.height() + 2) +"px"
            }
        }).append(
            $("<div></div>", {
                "data-role": "DateRangeOpenerHeader",
                "css": { "display": "inline-block", "width": "99%" }
            }).append($("<i></i>", {
                "class": "fa fa-times fa-2x pull-right"
                , "data-role": "CloseDateRangeOpener"
                , "data-targetDOM": _GUID
                , "css": { "color": "white" }
            }))
        ).append(
            $("<div></div>", {
                "data-role": "DateRangeOpenerBody",
                "css": { "display": "flex" }
            }).append(new Pikaday({
                theme: 'pikaday-left',
                format: dateTimeFormat,
                onSelect: function (date) {
                    $correspondingID.val(moment(date).format(dateTimeFormat) + " -");
                    //$datepickerE.setMinDate(date);
                }
            }).el).append(new Pikaday({
                theme: 'pikaday-right',
                format: dateTimeFormat,
                onSelect: function (date) {
                    if ($correspondingID.val().length == 0)
                        $correspondingID.val("- " + moment(date).format(dateTimeFormat));
                    else {
                        if ($correspondingID.val().includes('-')) {
                            let array = $correspondingID.val().split('-');
                            //Compare 2 value before assign
                            if (moment(date) < moment(array[0].trim()))
                                $correspondingID.val(moment(date).format(dateTimeFormat) + ' - ' + array[0].trim());
                            else
                                $correspondingID.val(array[0].trim() + ' - ' + moment(date).format(dateTimeFormat));
                        }
                    }
                }
            }).el)
        );
        
        $DivDateRangeOpener.insertAfter($correspondingID); 
         
        var rect = $('div[data-role="DateRangeOpener"]')[0].getBoundingClientRect();  
        var Wrect = $(window).width(); 

        if (rect.x + rect.width > Wrect)
            $DivDateRangeOpener.animate({ left: (Wrect - rect.x - rect.width - 10) + 'px' });

    });

    $('*[data-format="DateSingleOpener"]').on('click', function () {
        let $this = $(this);
        let $correspondingID;

        if ($this.attr('for'))
            $correspondingID = $('#'+$this.attr('for'));
        else
            $correspondingID = $this;

        var _GUID = createGuid();

        var $DivDateRangeOpener = $("<div></div>", {
            "data-role": "DateSingleOpener"
            , "id": _GUID
            , "css": {
                "display": "block",
                "width": "fit-content",
                "border": "1px solid grey",
                "border-radius": "4px",
                "padding-left": "5px",
                "padding-bottom": "5px",
                "position": "relative",
                "z-index": "3",
                "background-color": " black",
                "left": "0",
                "top": "0"
            }
        }).append(
            $("<div></div>", {
                "data-role": "DateRangeOpenerHeader",
                "css": { "display": "inline-block", "width": "99%" }
            }).append($("<i></i>", {
                "class": "fa fa-times fa-2x pull-right"
                , "data-role": "CloseDateRangeOpener"
                , "data-targetDOM": _GUID
                , "css": { "color": "white" }
            }))
        ).append(
            $("<div></div>", {
                "data-role": "DateRangeOpenerBody",
                "css": { "display": "flex" }
            }).append(new Pikaday({
                format: dateTimeFormat
                , onSelect: function (date) {
                    $correspondingID.val(moment(date).format(dateTimeFormat));
                }
            }).el)
        );
        $DivDateRangeOpener.insertAfter($correspondingID);
      
        var rect = $('div[data-role="DateRangeOpener"]')[0].getBoundingClientRect();
        var Wrect = $(window).width();

        if (rect.x + rect.width > Wrect)
            $DivDateRangeOpener.animate({ left: (Wrect - rect.x - rect.width - 10) + 'px' });

    }); 
});


$(document).on('click', function (ev) {
    if ($(ev.target).is('i[data-role="CloseDateRangeOpener"]')) {
        let $i = $(ev.target);
        $i.closest('div[data-role="DateRangeOpener"]').remove();
        $i.closest('div[data-role="DateSingleOpener"]').remove();
    }
});
