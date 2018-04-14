var lessToggle = true;
var moreToggle = false;
var lessToggleSecond = true;
var moreToggleSecond = false;
var triggerFirst = $('#trigger-block');
var periodBlockFirst = $('#period-block');
var triggerSecond = $('#trigger-block-second');
var periodBlockSecond = $('#period-block-second');


$(document).ready(function () {
    builder.toastrConfig();
    $('.panel-collapse').collapse({
        toggle: true
    });
    
    setTimeout(utils.loaderHide(), 1000);
});

triggerFirst.find('.rb-less').click(function () {
    if (!lessToggle && moreToggle){
        seasonalityDisable();
        periodBlockFirst.find('.per-24').click();
    }
    lessToggle = true;
    moreToggle = false;
});

triggerFirst.find('.rb-more').click(function () {
    if (lessToggle && !moreToggle){
        seasonalityEnable();
        periodBlockFirst.find('.per-72').click();
    }
    lessToggle = false;
    moreToggle = true;
});

var seasonalityEnable = function () {
    var $daily = $('#seasonality-daily');
    var $hourly = $('#seasonality-houly');
    if (!$daily.is(':checked')){
        $daily.click();
    }
    if (!$hourly.is(':checked')){
        $hourly.click();
    }
};

var seasonalityDisable = function () {
    var $daily = $('#seasonality-daily');
    var $hourly = $('#seasonality-houly');
    if ($daily.is(':checked')){
        $daily.click();
    }
    if ($hourly.is(':checked')){
        $hourly.click();
    }
};

triggerSecond.find('.rb-less').click(function () {
    if (!lessToggleSecond && moreToggleSecond){
        seasonalityDisableSecond();
        periodBlockSecond.find('.per-24').click();
    }
    lessToggleSecond = true;
    moreToggleSecond = false;
});

triggerSecond.find('.rb-more').click(function () {
    if (lessToggleSecond && !moreToggleSecond){
        seasonalityEnableSecond();
        periodBlockSecond.find('.per-72').click();
    }
    lessToggleSecond = false;
    moreToggleSecond = true;
});

var seasonalityEnableSecond = function () {
    var $daily = $('#seasonality-daily-second');
    var $hourly = $('#seasonality-houly-second');
    if (!$daily.is(':checked')){
        $daily.click();
    }
    if (!$hourly.is(':checked')){
        $hourly.click();
    }
};

var seasonalityDisableSecond = function () {
    var $daily = $('#seasonality-daily-second');
    var $hourly = $('#seasonality-houly-second');
    if ($daily.is(':checked')){
        $daily.click();
    }
    if ($hourly.is(':checked')){
        $hourly.click();
    }
};

$('#btc-forecast').click(function(){
    requests.instantForecast();
});