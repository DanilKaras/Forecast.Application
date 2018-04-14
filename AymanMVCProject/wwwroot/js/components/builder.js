var builder = (function () {
    
    var table = function (table) {
        if (table){
            
            var $content = '';
            for(var i = 0; i < table.length; i++)
            {
                $content += "<tr><td>" +
                    table[i].id+"</td><td>"+
                    table[i].ds+"</td><td>"+
                    utils.fixedOutput(table[i].yhat)+"</td><td>"+
                    utils.fixedOutput(table[i].yhatLower)+"</td><td>"+
                    utils.fixedOutput(table[i].yhatUpper)+"</td></tr>";
            }
            $('#table-content').html($content);
        }
        else{
            toastr.error("no data from server")
        }
    };
    
    var wrapData = function () {
        utils.loaderShow();

        var hourlySeasonality = false;
        var dailySeasonality = false;
        var symbol = '';
        var selectedGroup = '';
        var dataHours = 0;
        var periods = 0;
        var postData = '';

        selectedGroup = $('input[name=radio]:checked').val();
        if(selectedGroup){
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
        } else {
            dataHours = $('input[name=toggle]:checked').val();
        }
        

        symbol = $('.selectpicker option:selected').val();
        hourlySeasonality = $('#seasonality-houly').is(':checked');
        dailySeasonality = $('#seasonality-daily').is(':checked');

        periods = $('input[name=period]:checked').val();

        return {
            symbol: symbol,
            dataHours: dataHours,
            periods: periods,
            hourlySeasonality: hourlySeasonality,
            dailySeasonality: dailySeasonality
        }
    };
    
    var wrapForForecastElements = function (pickerName, indicatorVal) {
        var indicator = indicatorVal;
        var assetName = $('#'+pickerName).find('option:selected').val();
        var periods = $('input[name=period]:checked').val();
        var data = {
            indicator: indicator,
            assetName: assetName,
            periods: periods
        };
        var allow = true;
        for (var prop in data) {
            if(!data[prop] && data[prop]!== 0) allow = false;
        }
        if(allow){
            requests.showForecastElements(data);
        }
    };
    
    var imgForecast = function (picPath) {
        if(picPath){
            var imgForecast = $('<img />', {
                id: 'forecast',
                src: picPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            $('#forecast-place').html(imgForecast);
        }
        else{
            toastr.error("No forecast image")
        }
        
    };
    
    var imgComponents = function (picPath) {
        if(picPath){
            var imgComponents = $('<img />', {
                id: 'components',
                src: picPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            $('#components-place').html(imgComponents);
        } else {
            toastr.error("No components image");
        }
       
    };
    
    var assetName = function (assetName) {
        if(assetName){
            $('#asset-name').html(assetName);
        }else{
            toastr.error("No asset name");
        }
        
    };
    
    var indicator = function (indicator) {
        var span = '';
        if(indicator === utils.indicators.positive) {
            span = $('<span />',{
                class:'label label-info',
                html:'Positive'
            });
        }
        else if(indicator === utils.indicators.neutral){
            span = $('<span />',{
                class:'label label-default',
                html:'Neutral'
            });
        }
        else if(indicator === utils.indicators.negative){
            span = $('<span />',{
                class:'label label-danger',
                html:'Negative'
            });
        }
        else if (indicator === utils.indicators.superPositive){
            span = $('<span />',{
                class:'label label-success',
                html:'Strong Positive'
            });
        }
        $('#indicator-text').html(span);
    };
    
    
    var toastrAlert = function (requestNum) {
        if(requestNum){
            var $toastrMessage = 'You made '+ requestNum +' requests today!';
            if(requestNum < 800)
            {
                toastr.success($toastrMessage);
            }
            else if(800 <= requestNum < 950){
                toastr.warning($toastrMessage);
            }
            else{
                toastr.error($toastrMessage);
            }
        } else {
            toastr.warning("No number of requests per day")
        }
    };

    var toastrAlertUpdated = function () {
        toastr.success('Forecast by the asset has been successfully updated');
    };
    
    var showRequestForToday = function (data) {
        if(data){
            var message = "Requests: " + data.requestCount;
            $('#control-header').html(message);  
        }  
    };
    
    var indicatorPicker = function (data, pickerId) {
        var picker = $('#'+pickerId);
        picker.empty();
        if(data){
            var jsonData = data;
            for (var i = 0; i < jsonData.length; i++) {
                picker.append('<option value="' + jsonData[i] + '">' + jsonData[i] + '</option>')
            }
            picker.selectpicker('refresh');
        }    
    };
    
    var reportTable = function (data) {
        if (data){
            var $report = '';
            for(var i = 0; i < data.length; i++)
            {
                $report += '<tr>';
                
                if(data[i].log === utils.logs.negative){
                    $report += '<td class="danger">' + (i+1) + '</td>';
                    $report += '<td class="danger">' + data[i].assetName + '</td>';
                    $report += '<td class="danger">' + data[i].log + '</td>';
                    $report += '<td class="danger">' + data[i].rate + '</td>';
                } else if (data[i].log === utils.logs.positive){
                    $report += '<td class="info">' + (i+1) + '</td>';
                    $report += '<td class="info">' + data[i].assetName + '</td>';
                    $report += '<td class="info">' + data[i].log + '</td>';
                    $report += '<td class="info">' + data[i].rate + '</td>';
                } else if (data[i].log === utils.logs.strongPositive){
                    $report += '<td class="success">' + (i+1) + '</td>';
                    $report += '<td class="success">' + data[i].assetName + '</td>';
                    $report += '<td class="success">' + data[i].log + '</td>';
                    $report += '<td class="success">' + data[i].rate + '</td>';
                } else if (data[i].log === utils.logs.neutral){
                    $report += '<td class="active">' + (i+1) + '</td>';
                    $report += '<td class="active">' + data[i].assetName + '</td>';
                    $report += '<td class="active">' + data[i].log + '</td>';
                    $report += '<td class="active">' + data[i].rate + '</td>';
                } else if (data[i].log === utils.logs.zeroRezults){
                    $report += '<td class="warning">' + (i+1) + '</td>';
                    $report += '<td class="warning">' + data[i].assetName + '</td>';
                    $report += '<td class="warning">' + data[i].log + '</td>';
                    $report += '<td class="warning">' + data[i].rate + '</td>';
                }
                $report += '</tr>';
            }
            
            $('#table-report-content').html($report);
        }
    };
    
    var instantForecast = function (data) {
        
        utils.modalWindow.find('.modal-title').html(data.assetName);
        
        if(data.forecastPath){
            var imgForecast = $('<img />', {
                id: 'instant-forecast',
                src: data.forecastPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            $('#instant-forecast-place').html(imgForecast);
        }

        var indicator = data.indicator;
        var rate = 'Rate: ' +  data.rate;
        
        var span = '';
        if(indicator === utils.indicators.positive) {
            span = $('<span />',{
                class:'label label-info',
                html:'Positive'
            });
        }
        else if(indicator === utils.indicators.neutral){
            span = $('<span />',{
                class:'label label-default',
                html:'Neutral'
            });
        }
        else if(indicator === utils.indicators.negative){
            span = $('<span />',{
                class:'label label-danger',
                html:'Negative'
            });
        }
        else if (indicator === utils.indicators.superPositive){
            span = $('<span />',{
                class:'label label-success',
                html:'Strong Positive'
            });
        }
        
        $('#instant-indicator').html(span);
        $('#instant-rate').html(rate);
        
        utils.loaderHide();
        utils.modalWindow.modal('show');
    };
    
    var toastrConfig = function (){
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": false,
            "progressBar": false,
            "positionClass": "toast-bottom-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
    };
    
    
    return {
        table: table,
        imgForecast: imgForecast,
        imgComponents: imgComponents,
        assetName: assetName,
        toastrAlert: toastrAlert,
        toastrConfig: toastrConfig,
        indicator: indicator,
        showRequestForToday: showRequestForToday,
        wrapData: wrapData,
        indicatorPicker: indicatorPicker,
        toastrAlertUpdated: toastrAlertUpdated,
        wrapForForecastElements: wrapForForecastElements,
        reportTable: reportTable,
        instantForecast: instantForecast
    };
})();