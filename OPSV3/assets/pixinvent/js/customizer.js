(function (window, document, $) {
    'use strict';
    $(document).ready(function () {

        /********************************
        *           Customizer          *
        ********************************/
        /*$('#cz-compact-menu').prop('checked', true);*/
        var body = $('body'),
            default_bg_color = $('.app-sidebar').attr('data-background-color'),
            default_tl_color = $('.color-customize').attr('data-background-color'),
            default_bg_image = $('.app-sidebar').attr('data-image');

        // Customizer toggle & close button click events  [Remove customizer code from production]
        $('.customizer-toggle').on('click', function () {
            $('.customizer').toggleClass('open');
        });
        $('.customizer-close').on('click', function () {
            $('.customizer').removeClass('open');
        });
        if ($('.customizer-content').length > 0) {
            $('.customizer-content').perfectScrollbar({
                theme: "dark"
            });
        }

        // Change Sidebar Background Color
        $('.cz-bg-color span').on('click', function () {
            var $this = $(this),
                bgColor = $this.attr('data-bg-color');

            $this.closest('.cz-bg-color').find('span.selected').removeClass('selected');
            $this.addClass('selected');

            //START MOD - SON) 26/Dec/2020 - don't check event back ground color
            //if(bgColor == 'white'){
            //    $('.logo-img img').attr('src','assets/img/logo-dark.png');
            //}
            //else{
            //    if($('.logo-img img').attr('src') == 'assets/img/logo-dark.png'){
            //        $('.logo-img img').attr('src','assets/img/logo.png');
            //    }
            //}
            //END MOD - SON) 26/Dec/2020
        });
        // Oanh Change Title Background Color (6 Jan 2021)
        $('.cz-bg-tl-color span').on('click', function () {
            var $this = $(this),
                bgColor = $this.attr('data-bg-color');

            $this.closest('.cz-bg-tl-color').find('span.selected').removeClass('selected');
            $this.addClass('selected');

        });

        // Change Background Image
        $('.cz-bg-image img').on('click', function () {
            var $this = $(this),
                src = $this.attr('src');

            $('.sidebar-background').css('background-image', 'url(' + src + ')');
            $this.closest('.cz-bg-image').find('.selected').removeClass('selected');
            $this.addClass('selected');
            localStorage.lc_bg_Image = src;
        });

        $('.cz-bg-image-display').on('click', function () {
            var $this = $(this);
            if ($this.prop('checked') === true) {
                $('.sidebar-background').css('display', 'block');
            }
            else {
                $('.sidebar-background').css('display', 'none');
            }
        });


        $('.cz-compact-menu').on('click', function () {
            $('.nav-toggle').trigger('click');
            if ($(this).prop('checked') === true) {
                $('.app-sidebar').trigger('mouseleave');
                $('.user-settings-wrap').addClass('d-none');
            }
            else {
                $('.user-settings-wrap').removeClass('d-none');
            }
        });

        $('.cz-sidebar-width').on('change', function () {
            var $this = $(this),
                width_val = this.value,
                wrapper = $('.wrapper');

            if (width_val === 'small') {
                $(wrapper).removeClass('sidebar-lg').addClass('sidebar-sm');
            }
            else if (width_val === 'large') {
                $(wrapper).removeClass('sidebar-sm').addClass('sidebar-lg');
            }
            else {
                $(wrapper).removeClass('sidebar-sm sidebar-lg');
            }

        });

    });
})(window, document, jQuery);