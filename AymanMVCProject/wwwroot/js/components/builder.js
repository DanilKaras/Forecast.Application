var builder = (function () {
    
    var table = function (table) {
        if (table){
            
            var $content = '';
            for(var i = 0; i < table.length; i++)
            {
                $content += "<tr><td>" +
                    table[i].id+"</td><td>"+
                    table[i].ds+"</td><td>"+
                    table[i].yhat+"</td><td>"+
                    table[i].yhatLower+"</td><td>"+
                    table[i].yhatUpper+"</td></tr>";
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
                class:'label label-success',
                html:'Positive'
            });
        }
        else if(indicator === utils.indicators.neutral){
            span = $('<span />',{
                class:'label label-default',
                html:'Neutral'
            });
        }
        else if(indicator === utils.indicators.negatine){
            span = $('<span />',{
                class:'label label-danger',
                html:'Negative'
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
    
    var showRequestForToday = function (data) {
        if(data){
            var message = "Requests: " + data.requestCount;
            $('#control-header').html(message);  
        }  
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
        wrapData: wrapData
    };
})();