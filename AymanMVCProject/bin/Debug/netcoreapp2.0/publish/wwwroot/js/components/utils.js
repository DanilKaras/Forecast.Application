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
        negative: 2,
        superPositive: 3
    };

    var logs = {
        positive: 'Positive',
        neutral: 'Neutral',
        negative: 'Negative',
        strongPositive: 'StrongPositive',
        zeroRezults: 'ZeroRezults'
    };
    
    var fixedOutput = function(number) {
        number = number.toString();
        var charNum = number.indexOf(".");
        var beforePoint = number.substring(0, charNum);
        var afterPoint = number.substring(charNum, charNum + 8);
        return beforePoint+ afterPoint;
    };
    
    var symbolsList = $('#symbols-list-link').data('request-url');
    var updateAssets =  $('#update-assets-link').data('request-url');
    var manualForecast = $('#manual-forecast-link').data('request-url');
    var requestForToday = $('#requests-today-link').data('request-url');
    var autoForecastPost = $('#auto-forecast-link').data('request-url');
    var getForecastParts = $('#show-forecast-elements').data('request-url');
    var getLatestAssets = $('#get-latest-assets').data('request-url');
    var instantForecast = $('#instant-forecast-link').data('request-url');
    var modalWindow = $('#btc-modal');
    
    return {
        loaderShow: loaderShow, 
        loaderHide: loaderHide,
        group: group,
        indicators: indicators,
        symbolsList: symbolsList,
        updateAssets: updateAssets,
        manualForecast: manualForecast,
        requestForToday: requestForToday,
        autoForecastPost: autoForecastPost,
        getForecastParts: getForecastParts,
        getLatestAssets: getLatestAssets,
        logs: logs,
        fixedOutput: fixedOutput,
        instantForecast: instantForecast,
        modalWindow: modalWindow
    };
})();
