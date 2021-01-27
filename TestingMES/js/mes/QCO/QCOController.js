function ToggleSearchBox(EleIcon, ClassEle) {
    $(EleIcon).hide(500);

    let $ToggleSearchBox = $(ClassEle);
    $ToggleSearchBox.animate({ left: "11px" });
}
 
$('.searchingBoxToggle').find('i').on('click', function () {
    let $this = $(this);
    let iconDIVClass = $('.searchingBoxToggle').data('targetdomclass');
    //console.log(iconDIVClass);

    $('.searchingBoxToggle').animate({ left: "-101%" })
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