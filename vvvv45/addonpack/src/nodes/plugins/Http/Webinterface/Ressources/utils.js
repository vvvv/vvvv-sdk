$.getDocHeight = function(){
    return Math.max(
        $(document).height(),
        $(window).height(),
        /* For opera: */
        document.documentElement.clientHeight
    );
};

$.getDocWidth = function(){
    return Math.max(
        $(document).width(),
        $(window).width(),
        /* For opera: */
        document.documentElement.clientWidth
    );
};