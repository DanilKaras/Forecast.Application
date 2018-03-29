    var lessToggle = true;
    var moreToggle = false;
    $(document).ready(function () {
        $('#ex13').slider({
            ticks: [0, 240, 480, 720],
            ticks_labels: ['0', '240', '480', '720'],
            ticks_snap_bounds: 30
        });
        builder.toastrConfig();
        requests.selecter();
    });

    $('#trigger-block').click(function () {
        $('#use-buttons').click();
    });


    $('#custom-slider:text').on('input', function () {
        $('#use-slider').click();
        var $val = this.value;
        $('#ex13').slider('setValue', $val);

    });

    $('#ex13').on('change', function () {
        $('#use-slider').click();
        $('#custom-slider').val('');
    });

    $('#update-assets').click(function () {
        requests.updateAssets();
    });


    $('#make-forecast').click(function () {
        var valuesFromPage = builder.wrapData();
        
        var postData = {
            symbol: valuesFromPage.symbol,
            dataHours: valuesFromPage.dataHours,
            periods: valuesFromPage.periods,
            hourlySeasonality: valuesFromPage.hourlySeasonality,
            dailySeasonality: valuesFromPage.dailySeasonality
        };
        
        requests.sendToServerManual(postData);
    });
    
    
    $('#run-auto-forecast').click(function () {
        var valuesFromPage = builder.wrapData();
        
        var postData = {
            dataHours: valuesFromPage.dataHours,
            periods: valuesFromPage.periods,
            hourlySeasonality: valuesFromPage.hourlySeasonality,
            dailySeasonality: valuesFromPage.dailySeasonality
        };

        requests.sendToServerAuto(postData);
    });
    
    $('.rb-less').click(function () {
        if (!lessToggle && moreToggle){
            seasonalityDisable();
            $('.per-24').click();
        }
        lessToggle = true;
        moreToggle = false;
    });

    $('.rb-more').click(function () {
        if (lessToggle && !moreToggle){
            seasonalityEnable();
            $('.per-72').click();
        }
        lessToggle = false;
        moreToggle = true;
    });
    
    var onSuccessLoad = function (data) {
        if (data) {
            builder.table(data.table);
            builder.imgForecast(data.forecastPath);
            builder.imgComponents(data.componentsPath);
            builder.assetName(data.assetName);
            builder.toastrAlert(data.requestsPerDay);
            builder.indicator(data.indicator);
        }
    };


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
    
    $('#python-test').click(function () {
        requests.testPython();
    });
    