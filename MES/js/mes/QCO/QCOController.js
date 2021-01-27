function ToggleSearchBox(EleIcon, ClassEle) {
    $(EleIcon).hide(500);
    let $ToggleSearchBox = $(ClassEle);
    $ToggleSearchBox.animate({ left: "18px" });
    let $ToggleSearchBoxBackgroup = $(ClassEle + 'Background');
    $ToggleSearchBoxBackgroup.css({
        "width": "101%",
        "height": "101%",
        "opacity": "0.5",
        "background-color": "black",
        "position": "fixed",
        "top": "0",
        "left": "0",
        "z-index":"1"
    });
}
$('.searchingBoxToggle').find('i').on('click', function () {
    let $this = $(this);
    let iconDIVClass = $('.searchingBoxToggle').data('targetdomclass');
    //console.log(iconDIVClass);
    $('.searchingBoxToggle').animate({ left: "-101%" })
    $('.searchingBoxToggleBackground').removeAttr('style');
    $('.searchingBoxToggleBackground').css({
        "width": "0",
        "height": "0"
    });
    $('.' + iconDIVClass).find('i').show(800);
});
$('.searchingBoxToggle').find('i').on('mouseover', function () {
    let $this = $(this);
    var d = 180;
    $this.css({
        '-moz-transform': 'rotate(' + d + 'deg)',
        '-webkit-transform': 'rotate(' + d + 'deg)',
        '-o-transform': 'rotate(' + d + 'deg)',
        '-ms-transform': 'rotate(' + d + 'deg)',
        'transform': 'rotate(' + d + 'deg)'
    });  
    AnimateRotate($this,d); 
}); 
function AnimateRotate(Ele, d) {
    var elem = Ele;
    $({ deg: 0 }).animate({ deg: d }, {
        duration: 500,
        step: function (now) {
            elem.css({
                transform: "rotate(" + now + "deg)"
            });
        }
    });
}