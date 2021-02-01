
var _listSideBarBackgroundColor = {
    gradient_aqua_marine: { BgImage: 'linear-gradient(45deg, #1488CC, #2B32B2)', FontColor: '#ffffff' },
    gradient_sublime_vivid: { BgImage: 'linear-gradient(45deg, #FC466B, #3F5EFB)', FontColor: '#ffffff' },
    gradient_crystal_clear: { BgImage: 'linear-gradient(45deg, #159957, #155799)', FontColor: '#ffffff' },
    gradient_timber: { BgImage: 'linear-gradient(45deg, #FC00FF, #00DBDE)', FontColor: '#ffffff' },
    bg_black: { BgImage: 'linear-gradient(45deg, #000000 , #000000 )', FontColor: '#ffffff' },
    bg_white: { BgImage: 'linear-gradient(45deg, #ffffff , #ffffff )', FontColor: '#01a2cb' },
    bg_primary: { BgImage: 'linear-gradient(45deg, #666EE8 , #666EE8 )', FontColor: '#ffffff' },
    bg_danger: { BgImage: 'linear-gradient(45deg, #FF4961 , #FF4961 )', FontColor: '#ffffff' },

    gradient_man_of_steel: { BgImage: 'linear-gradient(45deg, #780206, #061161)', FontColor: '#ffffff' },
    gradient_purple_bliss: { BgImage: 'linear-gradient(45deg, #360033, #0b8793)', FontColor: '#ffffff' },
    bg_navy_blue: { BgImage: 'linear-gradient(45deg, #0e5cad, #0e5cad)', FontColor: '#ffffff' },
    bg_gray: { BgImage: 'linear-gradient(45deg, #9e9e9e, #9e9e9e)', FontColor: '#ffffff' },
    bg_default: { BgImage: 'linear-gradient(45deg, #1296c3, #1296c3)', FontColor: '#ffffff' },
};
var _listCardBackgroundColor = {
    gradient_aqua_marine: { BgImage: 'linear-gradient(45deg, #2B32B2, #1488CC)', FontColor: '#ffffff' },
    gradient_sublime_vivid: { BgImage: 'linear-gradient(45deg, #3F5EFB, #FC466B)', FontColor: '#ffffff' },
    gradient_crystal_clear: { BgImage: 'linear-gradient(45deg, #155799, #159957)', FontColor: '#ffffff' },
    gradient_timber: { BgImage: 'linear-gradient(45deg, #00DBDE, #FC00FF)', FontColor: '#ffffff' },
    bg_black: { BgImage: 'linear-gradient(45deg, #000000 , #000000 )', FontColor: '#ffffff' },
    bg_white: { BgImage: 'linear-gradient(45deg, #ffffff , #ffffff )', FontColor: '#01a2cb' },
    bg_primary: { BgImage: 'linear-gradient(45deg, #666EE8 , #666EE8 )', FontColor: '#ffffff' },
    bg_danger: { BgImage: 'linear-gradient(45deg, #FF4961 , #FF4961 )', FontColor: '#ffffff' },

    gradient_man_of_steel: { BgImage: 'linear-gradient(45deg, #061161, #780206)', FontColor: '#ffffff' },
    gradient_purple_bliss: { BgImage: 'linear-gradient(45deg, #0b8793, #360033)', FontColor: '#ffffff' },
    bg_navy_blue: { BgImage: 'linear-gradient(45deg, #0e5cad, #0e5cad)', FontColor: '#ffffff' },
    bg_gray: { BgImage: 'linear-gradient(45deg, #9e9e9e, #9e9e9e)', FontColor: '#ffffff' },
    bg_default: { BgImage: 'linear-gradient(45deg, #1296c3, #1296c3)', FontColor: '#ffffff' },
};
function setBackgroundColorBySideBar(objColor) {

    //let jqgridTitleBar = localStorage.getItem('sideBarColor');
    $('.ui-jqgrid-view > .ui-jqgrid-titlebar').css({ "background-image": objColor.BgImage, "color": objColor.FontColor });
    //$('.ui-jqgrid-view > .ui-jqgrid-titlebar').css({ "color": objColor.FontColor });
    $('.modal.modal-custom .modal-header').css({ "background-image": objColor.BgImage, "color": objColor.FontColor });
    //$('.modal.modal-custom .modal-header').css({ "color": objColor.FontColor });
    $('.modal.modal-custom .modal-header .modal-title').css({ "color": objColor.FontColor });

    $('.ui-accordion .ui-accordion-header').css({ "background-image": objColor.BgImage, "color": objColor.FontColor });
    //$('.ui-accordion .ui-accordion-header').css({ "color": objColor.FontColor });

    //Oanh Add 29Jan2021
    $('#edithdMachineGrid').css({ "background-image": objColor.BgImage, "color": objColor.FontColor });

    localStorage.setItem('sideBarColor', objColor.BgImage);
    localStorage.setItem('sideBarFontColor', objColor.FontColor);

    localStorage.setItem('objSideBarColor', JSON.stringify(objColor));

    setBackgroundJsPlum(objColor);
}
// test change color layout page Oanh 23Jan2021
function setBackgroundColor_LayoutPage(objColor) {
    if (objColor) {
        $('.op__controls').css({ "background-image": objColor.BgImage });
        localStorage.setItem('bgColorLayout', JSON.stringify(objColor));
        // Oanh add 27Jan2021 (color for icon of Layout Page)
        $('.op__process-connect').css({ "color": objColor.FontColor });
        $('.op__control-icon svg').css({ "color": objColor.FontColor });
        // End Oanh add 27Jan2021 (color for icon of Layout Page)
    }
    else {
        localStorage.setItem('bgColorLayout', JSON.stringify({ BgImage: 'linear-gradient(45deg, #fc6586, #53bbfd)', FontColor: '#ffffff' }));
    }
}
function setBackgroundColor_Layout_Load() {
    let objColor_layout = JSON.parse(localStorage.getItem('bgColorLayout'));
    if (objColor_layout) {
        $('.op__controls').css({ "background-image": objColor_layout.BgImage });
        // Oanh add 27Jan2021 (color for icon of Layout Page)
        $('.op__process-connect').css({ "color": objColor_layout.FontColor });
        $('.op__control-icon svg').css({ "color": objColor_layout.FontColor });
        // End Oanh add 27Jan2021 (color for icon of Layout Page)
    }
    else {
        localStorage.setItem('bgColorLayout', JSON.stringify({ BgImage: 'linear-gradient(45deg, #fc6586, #53bbfd)', FontColor: '#ffffff' }));
    }
}
//End test change color layout page Oanh 23Jan2021
function setBackgroundColorJqGridModal() {
    let objColor = JSON.parse(localStorage.getItem('objSideBarColor'));
    if (objColor) {
        $('.ui-jqgrid-view > .ui-jqgrid-titlebar').css({ "background-image": objColor.BgImage, "color": objColor.FontColor });
        //$('.ui-jqgrid-view > .ui-jqgrid-titlebar').css({ "color": objColor.FontColor });
        $('.modal.modal-custom .modal-header').css({ "background-image": objColor.BgImage, "color": objColor.FontColor });
        //$('.modal.modal-custom .modal-header').css({ "color": objColor.FontColor });
        $('.modal.modal-custom .modal-header .modal-title').css({ "color": objColor.FontColor });

        $('.ui-jqdialog .ui-widget-header').css({ "background-image": objColor.BgImage, "color": objColor.FontColor });
        //$('.ui-jqdialog .ui-widget-header').css({ "color": objColor.FontColor });

        //$('.ui-jqgrid .ui-jqgrid-title').css({ "color": objColor.FontColor });

        $('.ui-accordion .ui-accordion-header').css({ "background-image": objColor.BgImage, "color": objColor.FontColor });
        //$('.ui-accordion .ui-accordion-header').css({ "color": objColor.FontColor });
    }
    else {
        localStorage.setItem('objSideBarColor', JSON.stringify({ BgImage: 'linear-gradient(45deg, #1296c3, #1296c3)', FontColor: '#ffffff' }));
    }
}

function setBackgroundJsPlum(objColor) {
    //change title JsPlum modal

    if (objColor) {
        $('.jsPanel-hdr').css({ "background": 'none', "background-color": 'transparent', "background-image": objColor.BgImage });
        //$('.jsPanel-hdr').css({ "background-color": 'transparent' });
        //$('.jsPanel-hdr').css({ "background-image": objColor.BgImage });

        $('.jsPanel-replacement').css({ "background-color": 'transparent', "background-image": objColor.BgImage, "color": objColor.FontColor });
        //$('.jsPanel-replacement').css({ "background-image": objColor.BgImage });
        //$('.jsPanel-title').css({ "color": objColor.FontColor });
    } else {
        localStorage.setItem('objSideBarColor', JSON.stringify({ BgImage: 'linear-gradient(45deg, #1296c3, #1296c3)', FontColor: '#ffffff' }));
    }

}

function eventJsGlyphMinimize(objSideBarColor) {
    $('.jsglyph.jsglyph-minimize').click(function () {
        setTimeout(function () {
            $('.jsPanel-hdr').css({ "background": 'none', "background-color": 'transparent', "background-image": objSideBarColor.BgImage });
            //$('.jsPanel-hdr').css({ "background-color": 'transparent' });
            //$('.jsPanel-hdr').css({ "background-image": objSideBarColor.BgImage });
        }, 0);

    });
}

function sideBarSelection() {

    $('#spGradientAquaMarine').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_aqua_marine);
        setBackground('aqua-marine');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_aqua_marine);
    });

    $('#spGradientSublimeVivid').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_sublime_vivid);
        setBackground('sublime-vivid');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_sublime_vivid);
    });

    $('#spGradientCrystalClear').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_crystal_clear);
        setBackground('crystal-clear');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_crystal_clear);
    });

    $('#spGradientTimber').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_timber);
        setBackground('timber');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_timber);
    });

    $('#spGradientBlack').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_black);
        setBackground('black');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_black);
    });

    $('#spGradientWhite').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_white);
        setBackground('white');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_white);
    });

    $('#spGradientPrimary').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_primary);
        setBackground('primary');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_primary);
    });

    $('#spGradientDanger').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_danger);
        setBackground('danger');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_danger);
    });
    $('#spGradientManOfSteel').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_man_of_steel);
        setBackground('man-of-steel');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_man_of_steel);
    });
    $('#spGradientPurpleBliss').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_purple_bliss);
        setBackground('purple-bliss');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_purple_bliss);
    });
    $('#spNavyBlue').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_navy_blue);
        setBackground('navy-blue');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_navy_blue);
    });
    $('#spGray').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_gray);
        setBackground('gray');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_gray);
    });

    // only change color for title not apply for menu Oanh add 22Jan2021
    $('#cl-aqua-marine').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_aqua_marine);
        setOnlyBackgroundTitle('aqua-marine');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_aqua_marine);
    });
    $('#cl-sublime-vivid').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_sublime_vivid);
        setOnlyBackgroundTitle('sublime-vivid');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_sublime_vivid);
    });
    $('#cl-default').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_default);
        setOnlyBackgroundTitle('default');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_default);
    });
    $('#cl-crystal-clear').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_crystal_clear);
        setOnlyBackgroundTitle('crystal-clear');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_crystal_clear);
    });
    $('#cl-timber').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_timber);
        setOnlyBackgroundTitle('timber');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_timber);
    });
    $('#cl-man-of-steel').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_man_of_steel);
        setOnlyBackgroundTitle('man-of-steel');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_man_of_steel);
    });
    $('#cl-purple-bliss').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.gradient_purple_bliss);
        setOnlyBackgroundTitle('purple-bliss');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.gradient_purple_bliss);
    });
    $('#cl-navy-blue').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_navy_blue);
        setOnlyBackgroundTitle('navy-blue');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_navy_blue);
    });
    $('#cl-gray').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_gray);
        setOnlyBackgroundTitle('gray');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_gray);
    });
    $('#cl-primary').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_primary);
        setOnlyBackgroundTitle('primary');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_primary);
    });
    $('#cl-danger').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_danger);
        setOnlyBackgroundTitle('danger');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_danger);
    });
    $('#cl-black').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_black);
        setOnlyBackgroundTitle('black');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_black);
    });
    $('#cl-white').click(() => {
        setBackgroundColorBySideBar(_listSideBarBackgroundColor.bg_white);
        setOnlyBackgroundTitle('white');
        setBackgroundColor_LayoutPage(_listCardBackgroundColor.bg_white);
    });
    //End only change color for title not apply for menu Oanh add 22Jan2021
}
function setBackground(name_bg) {
    $('.app-sidebar').attr('data-background-color', name_bg);
    //localStorage.setItem('lc_bg_Color', name_bg);
    localStorage.setItem('lc_bg_Menu_Color', name_bg);
    localStorage.setItem('lc_bg_Color', name_bg);
    $('.color-customize').attr('data-background-color', name_bg);
    //$('.cz-bg-color span[data-bg-color="' + name_bg + '"]').addClass('selected');
    var $this = $('.cz-bg-tl-color span[data-bg-color="' + name_bg + '"]');

    $this.closest('.cz-bg-tl-color').find('span.selected').removeClass('selected');
    $this.addClass('selected');

    //$('#cl-' + name_bg +' span').addClass('selected');
}
function setOnlyBackgroundTitle(name_bg) {
    localStorage.setItem('lc_bg_Color', name_bg);
    $('.color-customize').attr('data-background-color', name_bg);
}
function ChangeColor() {
    /*Oanh lc storageColor 12Jan2021*/
    var default_bg_Menu_color = $('.app-sidebar').attr('data-background-color'),
        default_bg_color = $('.color-customize').attr('data-background-color'),
        default_bg_image = $('.app-sidebar').attr('data-image');
    if (typeof (Storage) !== "undefined") {
        if (localStorage.lc_bg_Image || localStorage.lc_bg_Color) {

            default_bg_color = localStorage.lc_bg_Color;
            default_bg_Menu_color = localStorage.lc_bg_Menu_Color;
            default_bg_image = localStorage.lc_bg_Image;
            $('.app-sidebar').attr('data-background-color', default_bg_Menu_color);
            $('.color-customize').attr('data-background-color', default_bg_color);
            $('.sidebar-background').css('background-image', 'url(' + default_bg_image + ')');

        } else {
            $('.app-sidebar').attr('data-background-color', 'aqua-marine');
            $('.color-customize').attr('data-background-color', 'default');

            // Create a localStorage the first time
            localStorage.lc_bg_Color = 'default';
            localStorage.lc_bg_Menu_Color = 'aqua-marine';
            localStorage.lc_bg_Image = $('.app-sidebar').attr('data-image');

            default_bg_color = localStorage.lc_bg_Color;
            default_bg_image = localStorage.lc_bg_Image;
            default_bg_Menu_color = localStorage.lc_bg_Menu_Color;

        }
        $('.cz-bg-color span[data-bg-color="' + default_bg_Menu_color + '"]').addClass('selected');
        $('.cz-bg-tl-color span[data-bg-color="' + default_bg_color + '"]').addClass('selected');
        $('.cz-bg-image img[src$="' + default_bg_image + '"]').addClass('selected');
    } else {
        alert('Browser not support local storage');
    }
    /*End Oanh lc storageColor 12Jan2021*/
}
