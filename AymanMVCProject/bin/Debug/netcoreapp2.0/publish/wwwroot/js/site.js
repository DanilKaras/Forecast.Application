﻿    var lessToggle = true;
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
        utils.loaderShow();

        var hourlySeasonality = false;
        var dailySeasonality = false;
        var symbol = '';
        var selectedGroup = '';
        var dataHours = 0;
        var periods = 0;
        var postData = '';

        selectedGroup = $('input[name=radio]:checked').val();

        switch (selectedGroup) {
            case utils.group.useButtons:
                dataHours = $('input[name=toggle]:checked').val();
                break;
            case utils.group.useSlider:
                var $custom = $('#custom-slider').val();
                if ($custom && $custom !== 0) {
                    dataHours = $custom;
                }
                else {
                    dataHours = $('#ex13').slider('getValue');
                }
                break;
            default:
                break;
        }

        symbol = $('.selectpicker option:selected').val();
        hourlySeasonality = $('#seasonality-houly').is(':checked');
        dailySeasonality = $('#seasonality-daily').is(':checked');

        periods = $('input[name=period]:checked').val();

        postData = {
            symbol: symbol,
            dataHours: dataHours,
            periods: periods,
            hourlySeasonality: hourlySeasonality,
            dailySeasonality: dailySeasonality
        };
        //send
        requests.sendToServer(postData);
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
        if (!$('#seasonality-daily').is(':checked')){
            $('#seasonality-daily').click();
        }
        if (!$('#seasonality-houly').is(':checked')){
            $('#seasonality-houly').click();
        }
    };
    
    var seasonalityDisable = function () {
        if ($('#seasonality-daily').is(':checked')){
            $('#seasonality-daily').click();
        }
        if ($('#seasonality-houly').is(':checked')){
            $('#seasonality-houly').click();
        }
    };
    
    $('#python-test').click(function () {
        requests.testPython();
    });
    