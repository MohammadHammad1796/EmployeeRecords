function DisplayToastNotification(message, time = "Now") {
    $("#notificationMessage").text(message);
    $("#notificationTime").text(time);
    $(".toast").toast("show");
}

$(window).on("load", function () {
    var windowLocationHref = window.location.href;
    windowLocationHref = windowLocationHref.endsWith("/")
        ? windowLocationHref.substr(0, windowLocationHref.length - 1)
        : windowLocationHref;
    var pageUrl = windowLocationHref.substr(
        windowLocationHref.lastIndexOf("/") + 1
    );
    pageUrl = pageUrl.split("?")[0];

    $("a.nav-link").each(function () {
        var thisPage = $(this).attr("href");
        thisPage = thisPage.endsWith("/")
            ? thisPage.substr(0, thisPage.length - 1)
            : thisPage;
        thisPage = thisPage.substr(thisPage.lastIndexOf("/") + 1);

        if (thisPage === pageUrl) $(this).addClass("active");
    });
    $(".bg_load").fadeOut("slow");

    $(".navbar-toggler").on("click", function () {
        $("#sidebarMenu").toggleClass("d-md-none d-md-block");
        $("main").toggleClass("col-md-12");
    });

    window.onscroll = function () {
        if (document.body.scrollTop > 20 || document.documentElement.scrollTop > 20)
            $(".back-to-top").show(200);
        else $(".back-to-top").hide(200);
    };

    $(".back-to-top").on("click", function (e) {
        e.preventDefault();
        document.body.scrollTop = 0;
        document.documentElement.scrollTop = 0;
    });
});