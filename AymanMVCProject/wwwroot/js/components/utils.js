var utils = (function () {
    
    var loaderShow = function () {
        $('#loader').show();
        $('body').append('<div id="overlay"></div>');
    };


    var loaderHide  = function () {
        $('#loader').hide();
        $('#overlay').remove();
    };
    
    var group = {
        useButtons: "useButtons",
        useSlider: "useSlider"
    };
    
    var indicators = {
        positive: 0,
        neutral: 1,
        negatine: 2
    };
    
    var symbolsList = $('#symbols-list-link').data('request-url');
    var updateAssets =  $('#update-assets-link').data('request-url');
    var manualForecast = $('#manual-forecast-link').data('request-url');
    var requestForToday = $('#requests-today-link').data('request-url');
    var autoForecastPost = $('#auto-forecast-link').data('request-url');
    
    return {
        loaderShow: loaderShow, 
        loaderHide: loaderHide,
        group: group,
        indicators: indicators,
        symbolsList: symbolsList,
        updateAssets: updateAssets,
        manualForecast: manualForecast,
        requestForToday: requestForToday,
        autoForecastPost: autoForecastPost
    };
})();
