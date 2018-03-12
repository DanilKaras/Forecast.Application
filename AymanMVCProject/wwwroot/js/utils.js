var utils = (function () {
    var domain = 'Home';
    
    var loaderShow = function () {
        $('.loader').show();
        $('body').append('<div id="overlay"></div>');
    };


    var loaderHide  = function () {
        $('.loader').hide();
        $('#overlay').remove();
    };
    
    var group = {
        useButtons: "useButtons",
        useSlider: "useSlider"
    };
    return {
        domain: domain,   
        loaderShow: loaderShow, 
        loaderHide: loaderHide,
        group: group
    };
})();
