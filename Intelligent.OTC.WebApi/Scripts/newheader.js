    var mbers = [];

    function onfinishRepeat() {

        init_navigate_bar();

        mbers = [$('#page_header > .topbar')
                    , $('#page_header > #banner')
                    , $('#page_header > .x_navigate_bar')
                    , $('#page_header > .submenu')
                    , $('.banner_small')
                    , $('#page_header > .x_navigate_bar')
                ];

        $('.x_menu').each(function () {

            $(this).find('>.x_menu_btn').each(function () {
                $(this).bind('click', function () { menu_click($(this)) });
                $(this).bind('mouseover', function () { menu_mouseover($(this)) });
                $(this).bind('mouseout', function () { menu_mouseout($(this)) });
            });
        });
        $('.submenu').bind('mouseover', function () { isMenuInUse = true; });
        $('.submenu').bind('mouseout', function () { isMenuInUse = false; menu_mouseout() });
        $('#banner').bind('mouseover', function () { isMenuInUse = true; });
        $('#banner').bind('mouseout', function () { isMenuInUse = false; menu_mouseout() });


        //$('.home').click();3000229326

        show_headers('0,1,2');

        setCurrentPage();
        //setFooterPosition();
        //$(window).scroll(function () {
        //    setFooterPosition();
        //}).resize(function () {
        //    setFooterPosition();
        //});
        //setInterval(setFooterPosition, 200);

        $(".toolkit-bar i").click(function () {
            var w = $("#tookit-content").css("min-width");
            var r = $("body").width() + 1;
            $("#tookit-content").height($(document).height());
            $(".toolkit-bar i").removeClass("selected");
            $(this).addClass("selected");
            if ($(".page_content").is(".left")) {
                $(".page_content").animate({ "margin-left": "+=" + w });
                $(".page_content").removeClass("left");

                $("#tookit-content").animate({ "left": "+" + r });
            } else {
                $(".page_content").animate({ "margin-left": "-=" + w });
                //$("#tookit-content").animate({"right":"-"+w});
                $("#tookit-content").animate({ "left": $(".toolkit-bar").width() });
                $(".page_content").addClass("left");
            }
        });
        $(".tookit-wrap").click(function (e) {
            e.stopPropagation();
        });
        $(".navigator-bg i").click(function (e) {
            e.stopPropagation();
        });
        $("body").click(function () {
            hideSlide($(".tookit-wrap"));
        });
    }




    function toggleSlide(pn) {
        if ($(pn).is(".open")) {
            hideSlide(pn);
        } else {
            showSlidepn(pn);
        }
    }

    function showSlidepn(pn) {
        if ($(pn).is(".open")) {
            return false;
        }
        var pnWidth = $(pn).width();
        $(pn).animate({ right: "+=" + pnWidth + "px" });
        $(this).animate({ right: "+=" + pnWidth + "px" });
        $(pn).addClass("open");
    }
    function hideSlide(pn) {
        if ($(pn).is(".open") == false) {
            return false;
        }
        var pnWidth = $(pn).width();
        $(pn).animate({ right: "-=" + pnWidth + "px" });
        $(this).animate({ right: "-=" + pnWidth + "px" });
        $(pn).removeClass("open");
    }
    function reset() {
        $(".tookit-search input:text").each(function () {
            $(this).val("");
        });
        hideSlide($(".tookit-wrap"));
    }

    function setCurrentPage() {
        var parentCss = $("a[href='@Request.Url.LocalPath']").first().parent("div").attr("id");
        try {
            move_pointer($("." + parentCss));
        } catch (e) { }
        //var left=$("."+parentCss).offset().left-$(".banner").offset().left;
        //$(".pointer").css("left",left+"px");
    }

    //function setFooterPosition() {

    //    if ($("body").height() > $(window).height()) {
    //        if ($(".page_footer").css("position") != "relative") {
    //            $(".page_footer").css("position", "relative");
    //        }
    //    } else {
    //        if ($(".page_footer").css("position") == "relative") {
    //            $(".page_footer").css({ "position": "fixed", left: "0", bottom: "0", width: "100%" });
    //        }
    //    }
    //}


    var fun_show_or_hid = function (jq, speed, callback) {
        if (!speed) {
            speed = 300;
        }

        if (callback) {
            callback.call()
        }
        else {
        }
        if (jq) {
            if ($(jq).is(":visible")) {
                $(jq).hide();
                if (callback) {
                    callback();
                }

            } else {
                $(jq).show();
                if (callback) {
                    callback();
                }
            }
            //$(jq).toggle(0, callback);
            //if ($(jq).is(":visible")) {
            //    $(jq).animate({ "height": "0px" }, callback);
            //}
            //else {
            //    $(jq).animate({ "height": "200px" }, callback);
            //}
            // jq.toggle('blind', { direction: 'up' }, speed, callback);
        }
    }

    var fun_reset_height = function (f_idx, callback) {
        var speed = 300;
        var speed_2 = 300;
        if (f_idx.length < 2) {
            speed = 300;
            speed_2 = 300;
        }

        for (var i = 0; i < f_idx.length; i++) {
            if (mbers[f_idx[i]].css('display') == 'none') {
                fun_show_or_hid(mbers[f_idx[i]], speed, null)//turn_small_on)
            }
        }

        var h_height = 0;
        for (var i = 0; i < f_idx.length; i++) {
            h_height = h_height + mbers[f_idx[i]].height();
        }
        h_height = h_height - 52;
        $('#pg_placeholder').animate(
    {
        height: h_height
        , duration: 1
    }, 0, "linear", callback);
    }

    var turn_small_on = function (f_idx, callback) {
        if (f_idx[0] == '1' && !$('#banner').hasClass('banner_small')) {
            $('#banner').switchClass('banner', "banner_small", 1, callback);
        } else if (!$("#banner").hasClass('banner')) {
            $('#banner').switchClass('banner_small', "banner", 1, callback);
            $('#banner').css({ display: 'block' });
        } else {
            callback.call();
        }
    }

    function show_headers(f_index) {
        var speed = 300;
        var speed_2 = 500;
        if ($('#page_header').is(":animated")) {
            return;
            //speed = 0;
            //speed_2 =0;
        }

        var f_idx = f_index.split(',');


        if (f_idx.length < 2) {
            speed = 300;
            speed_2 = 300;
        }


        for (var i = 0; i < mbers.length; i++) {
            var idx = i;
            for (var j = 0; j < f_idx.length; j++) {
                if (i == f_idx[j] && f_idx[j] != '') {
                    idx = null;
                }
            }

            if (idx != null && mbers[idx].css('display') != 'none') {
                fun_show_or_hid(mbers[idx], speed, function () { })//turn_small_on)
            }
        }

        if (f_idx[0] === '') {
            return;
        }
        turn_small_on(f_idx, function () { fun_reset_height(f_idx) });

    }

    function init_navigate_bar() {
        var has_navgate_bar = false;
        $('.x_navigate_bar_data > *').each(function () {
            has_navgate_bar = true;
            $('.x_navigate_contant').append($($(this)[0].outerHTML));
            if ($(this).next()[0]) {
                $('.x_navigate_contant').append($("<span>&nbsp;/&nbsp;</span>"));
            }
        });
    }

    var isMenuInUse = false;

    function show_submenu(jq) {
        $('.submenu div.sub-item').hide();
        $('.submenu div.sub-item').each(function () {
            if (jq.hasClass($(this).attr('id'))) {
                $(this).show();
                //show_headers('0,1,2,3');
            }
        });
    }

    function move_pointer(jq) {
        if (jq.parent().find(">.pointer").is(":animated")) {
            jq.parent().find(">.pointer").stop(true, true);
        }


        if (jq.currentTarget) {
            jq = $(jq.currentTarget);
        } 

        var left = jq.offset().left - jq.parent().offset().left;
        var color = jq.attr('color');

        jq.parent().find(">.pointer").each(function () {
            $(this).animate(
        {
            left: left
            , 'background-color': color
            , easing: 'easeOutCirc'
            , duration: 500
        }
        , 350
    );
        });
    }

    function menu_click(jq) {
        if (jq.hasClass('home')) {
            window.location = '/Site/#/dashboard';
        }
     
    }

    function menu_mouseout(jq) {
        var fun = function () {
            if (isMenuInUse) {
                return;
            }
            show_headers('0,1,2');
            isMenuInUse = false;
        }
        setTimeout(fun, 500);
    }

    function menu_mouseover(jq) {
        if (jq.currentTarget) {
            jq = $(jq.currentTarget);
        }

        show_submenu(jq);

        if (jq.hasClass('home') && !$('#home').length) {
            show_headers('0,1,2');
        } else {
            show_headers('0,1,2,3');
        }

        move_pointer(jq);
        return;
    }


    var isMiddleShown = true;
    var isRunning = false;
    function showMiddle() {
        isRunning = true;
        if (isMiddleShown) {
            isRunning = false;
            return;
        }
        isMiddleShown = true;
        isflashOffed = true;
        show_headers('0,1,2');

        if ($('.x_navigate_bar_data > *').length == 0) {
            //$('.x_navigate_bar').hide();
            show_headers('0,1,5');
        }
    }

    function showSmall() {
        isRunning = true;
        if (!isMiddleShown) {
            isRunning = false;
            return;
        }
        isMiddleShown = false;
        //$('.pointer').hide();
        show_headers('1,5');
    }

    var wheelmenu = function (event, delta) {
        //setFooterPosition();
        var o = ''; //, id = event.currentTarget.id;
        //o = '#' + id + ':';pg_placeholder

        if (delta > 0) {
            o += ' up (' + delta + ')';
            setTimeout(showMiddle, 10);
        }
        else if (delta < 0) {
            o += ' down (' + delta + ')';
            setTimeout(showSmall, 10);
        }
    };

    String.prototype.endWith = function (str) {
        if (str == null || str == "" || this.length == 0 || str.length > this.length)
            return false;
        if (this.substring(this.length - str.length) == str)
            return true;
        else
            return false;
        return true;
    }
    String.prototype.startWith = function (str) {
        if (str == null || str == "" || this.length == 0 || str.length > this.length)
            return false;
        if (this.substr(0, str.length) == str)
            return true;
        else
            return false;
        return true;
    }
    Date.prototype.format = function (format) {
        var o = {
            "M+": this.getMonth() + 1, //month
            "d+": this.getDate(), //day
            "h+": this.getHours(), //hour
            "m+": this.getMinutes(), //minute
            "s+": this.getSeconds(), //second
            "q+": Math.floor((this.getMonth() + 3) / 3), //quarter
            "S": this.getMilliseconds() //millisecond
        }
        if (/(y+)/.test(format)) format = format.replace(RegExp.$1,
    (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        for (var k in o) if (new RegExp("(" + k + ")").test(format))
            format = format.replace(RegExp.$1,
        RegExp.$1.length == 1 ? o[k] :
        ("00" + o[k]).substr(("" + o[k]).length));
        return format;
    }
